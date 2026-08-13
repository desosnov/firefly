using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Everything about getting pixels to a device. None of this is a port — the C++
    /// only ever spoke to one board over one USB cable, so Serial had no abstraction
    /// above it. See Port Notes §3b.
    ///
    /// The wire format is defined here and mirrored in the receiver firmware.
    /// </summary>
    public abstract class ATransport
    {
        /// <summary>Whether pixels can be sent right now.</summary>
        public abstract bool Available();

        /// <summary>Send one full frame: 3 bytes per pixel, RGB order.</summary>
        public abstract int Write(byte[] buffer, int size);

        public abstract void Close();

        /// <summary>Short label for the connection UI.</summary>
        public abstract string Describe();
    }

    /// <summary>A Firefly found on the network, or offering its provisioning AP.</summary>
    public class FireflyDevice
    {
        public string name;
        public IPAddress address;
        public int pixelCount;
        public bool provisioned;   // false = sitting on its own SoftAP awaiting credentials

        public override string ToString()
        {
            return name + " (" + address + ")";
        }
    }

    /// <summary>
    /// Streams pixels to a Firefly over UDP.
    ///
    /// A frame is 4,320 bytes for 1,440 pixels, well over a ~1,500-byte MTU, so it's
    /// split into chunks with an 8-byte header the firmware reassembles:
    ///
    ///   0      'F'   magic
    ///   1      frame sequence, wraps at 256
    ///   2      chunk index
    ///   3      chunk count
    ///   4..5   first pixel index in this chunk, big endian
    ///   6..7   pixel count in this chunk, big endian
    ///   8..    RGB triples
    ///
    /// Sends are fire-and-forget, so unlike serial this does not throttle the render
    /// loop — hence SEND_INTERVAL, which holds output to a sane frame rate instead of
    /// flooding the device at several hundred FPS.
    /// </summary>
    public class WifiTransport : ATransport
    {
        public const int FIREFLY_PORT = 21324;
        public const byte PACKET_MAGIC = (byte)'F';
        public const int HEADER_SIZE = 8;
        public const int MAX_PIXELS_PER_CHUNK = 400;   // 400 * 3 + 8 = 1208 bytes
        public const double SEND_INTERVAL = 1.0 / 60.0;

        private UdpClient client;
        private IPEndPoint endPoint;
        private FireflyDevice device;
        private byte[] packet = new byte[HEADER_SIZE + MAX_PIXELS_PER_CHUNK * 3];
        private byte frameSeq = 0;
        private double lastSend = -1.0;

        public FireflyDevice Device { get { return device; } }

        public bool Connect(FireflyDevice target)
        {
            Close();
            try
            {
                device = target;
                endPoint = new IPEndPoint(target.address, FIREFLY_PORT);
                client = new UdpClient();
                client.Connect(endPoint);
                FireflyUtils.Log("[Wifi] Streaming to " + target);
                return true;
            }
            catch (Exception e)
            {
                client = null;
                FireflyUtils.Log("[Wifi] Could not open " + target + ". (" + e.Message + ")");
                return false;
            }
        }

        public override bool Available()
        {
            return client != null;
        }

        public override int Write(byte[] buffer, int size)
        {
            if (!Available()) return 0;

            double now = Time.realtimeSinceStartupAsDouble;
            if (lastSend > 0.0 && now - lastSend < SEND_INTERVAL) return 0;
            lastSend = now;

            int pixels = size / 3;
            int chunks = (pixels + MAX_PIXELS_PER_CHUNK - 1) / MAX_PIXELS_PER_CHUNK;

            try
            {
                for (int c = 0; c < chunks; c++)
                {
                    int first = c * MAX_PIXELS_PER_CHUNK;
                    int count = Math.Min(MAX_PIXELS_PER_CHUNK, pixels - first);

                    packet[0] = PACKET_MAGIC;
                    packet[1] = frameSeq;
                    packet[2] = (byte)c;
                    packet[3] = (byte)chunks;
                    packet[4] = (byte)(first >> 8);
                    packet[5] = (byte)(first & 0xFF);
                    packet[6] = (byte)(count >> 8);
                    packet[7] = (byte)(count & 0xFF);

                    Buffer.BlockCopy(buffer, first * 3, packet, HEADER_SIZE, count * 3);
                    client.Send(packet, HEADER_SIZE + count * 3);
                }
                frameSeq++;
                return size;
            }
            catch (Exception e)
            {
                FireflyUtils.Log("[Wifi] Send failed. (" + e.Message + ")");
                return 0;
            }
        }

        public override void Close()
        {
            if (client != null)
            {
                try { client.Close(); } catch (Exception) { }
                client = null;
            }
        }

        public override string Describe()
        {
            return device != null ? "Wifi: " + device.name : "Wifi";
        }
    }

    /// <summary>
    /// Finds Fireflies, and hands wifi credentials to unprovisioned ones.
    ///
    /// Two mechanisms, matching the two states a Firefly can be in:
    ///
    ///  - **On the network.** A UDP broadcast probe on the Firefly port; provisioned
    ///    devices answer with their name and pixel count. Simpler than mDNS and
    ///    adequate for a LAN, since both ends are ours.
    ///  - **Awaiting pairing.** An unprovisioned device brings up its own access
    ///    point. Once the desktop has joined it, the device answers on the SoftAP
    ///    gateway address over HTTP.
    ///
    /// Joining and leaving the device's access point is manual — Unity has no wifi
    /// APIs, and both Windows and macOS now gate SSID enumeration behind Location
    /// Services. See Port Notes §3b.
    /// </summary>
    public static class FireflyDiscovery
    {
        public const int DISCOVERY_TIMEOUT_MS = 600;
        public const string PROBE = "FIREFLY?";
        public const string REPLY_PREFIX = "FIREFLY!";

        /// <summary>ESP32 SoftAP gateway. The device's own AP always answers here.</summary>
        public const string SOFTAP_ADDRESS = "192.168.4.1";
        public const int SOFTAP_HTTP_PORT = 80;

        /// <summary>
        /// Broadcasts a probe and collects replies. Blocks for DISCOVERY_TIMEOUT_MS,
        /// which is short enough to sit on the main thread behind a button press.
        /// </summary>
        public static List<FireflyDevice> ScanNetwork()
        {
            List<FireflyDevice> found = new List<FireflyDevice>();
            UdpClient client = null;

            try
            {
                client = new UdpClient();
                client.EnableBroadcast = true;
                client.Client.ReceiveTimeout = DISCOVERY_TIMEOUT_MS;

                byte[] probe = Encoding.ASCII.GetBytes(PROBE);
                client.Send(probe, probe.Length, new IPEndPoint(IPAddress.Broadcast, WifiTransport.FIREFLY_PORT));

                DateTime deadline = DateTime.UtcNow.AddMilliseconds(DISCOVERY_TIMEOUT_MS);
                while (DateTime.UtcNow < deadline)
                {
                    try
                    {
                        IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = client.Receive(ref from);
                        FireflyDevice d = ParseReply(Encoding.ASCII.GetString(data), from.Address);
                        if (d != null && !found.Exists(x => x.address.Equals(d.address))) found.Add(d);
                    }
                    catch (SocketException)
                    {
                        break; // receive timeout — nothing more is answering
                    }
                }
            }
            catch (Exception e)
            {
                FireflyUtils.Log("[Wifi] Network scan failed. (" + e.Message + ")");
            }
            finally
            {
                if (client != null) { try { client.Close(); } catch (Exception) { } }
            }

            FireflyUtils.Log("[Wifi] Network scan found " + found.Count + " Firefly(s)");
            return found;
        }

        /// <summary>
        /// Asks the SoftAP gateway whether a Firefly is sitting there awaiting
        /// credentials. Only meaningful once the desktop has joined the device's AP.
        /// </summary>
        public static FireflyDevice ScanSoftAP()
        {
            try
            {
                string body = HttpGet("http://" + SOFTAP_ADDRESS + ":" + SOFTAP_HTTP_PORT + "/firefly");
                if (body == null) return null;

                // name|pixelCount
                string[] parts = body.Trim().Split('|');
                FireflyDevice d = new FireflyDevice();
                d.name = parts.Length > 0 ? parts[0] : "Firefly";
                d.pixelCount = parts.Length > 1 ? int.Parse(parts[1]) : 0;
                d.address = IPAddress.Parse(SOFTAP_ADDRESS);
                d.provisioned = false;

                FireflyUtils.Log("[Wifi] Found unprovisioned " + d.name + " on its access point");
                return d;
            }
            catch (Exception e)
            {
                FireflyUtils.Log("[Wifi] No Firefly access point found. (" + e.Message + ")");
                return null;
            }
        }

        /// <summary>
        /// Hands the device the credentials for the real network. It saves them,
        /// joins, and drops its own access point — at which point the desktop has to
        /// rejoin the home network before it can reach the device again.
        /// </summary>
        public static bool Provision(string ssid, string password)
        {
            try
            {
                string url = "http://" + SOFTAP_ADDRESS + ":" + SOFTAP_HTTP_PORT + "/provision"
                             + "?ssid=" + Uri.EscapeDataString(ssid)
                             + "&pass=" + Uri.EscapeDataString(password);
                string body = HttpGet(url);
                bool ok = body != null && body.Trim().StartsWith("OK");
                FireflyUtils.Log(ok
                    ? "[Wifi] Provisioned. Rejoin your network, then Scan."
                    : "[Wifi] Provisioning refused by the device.");
                return ok;
            }
            catch (Exception e)
            {
                FireflyUtils.Log("[Wifi] Provisioning failed. (" + e.Message + ")");
                return false;
            }
        }

        private static FireflyDevice ParseReply(string reply, IPAddress from)
        {
            if (!reply.StartsWith(REPLY_PREFIX)) return null;

            // FIREFLY!name|pixelCount
            string[] parts = reply.Substring(REPLY_PREFIX.Length).Trim().Split('|');
            FireflyDevice d = new FireflyDevice();
            d.name = parts.Length > 0 && parts[0].Length > 0 ? parts[0] : "Firefly";
            d.pixelCount = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            d.address = from;
            d.provisioned = true;
            return d;
        }

        /// <summary>
        /// Deliberately HttpWebRequest rather than UnityWebRequest: this runs
        /// synchronously behind a button press, and UnityWebRequest needs a coroutine.
        /// </summary>
        private static string HttpGet(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = 3000;
            req.ReadWriteTimeout = 3000;
            using (WebResponse res = req.GetResponse())
            using (System.IO.StreamReader reader = new System.IO.StreamReader(res.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
