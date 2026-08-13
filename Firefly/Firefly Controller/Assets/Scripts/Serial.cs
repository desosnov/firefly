using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Port of Serial.h / .cpp plus the two platform back-ends (Serial-PC.cpp on
    /// Windows, arduino-serial-lib.cpp on Mac). System.IO.Ports.SerialPort covers
    /// both, so the #ifdef split collapses to a few platform-conditional settings.
    ///
    /// Beyond the port: the device name is no longer fixed at compile time. Ports
    /// that connect successfully are remembered in a text file beside the
    /// executable, and the most recent one is reopened on the next run. This is an
    /// addition, not a port — the C++ had the name in a #define. See Port Notes.
    ///
    /// Requires Player Settings -> Api Compatibility Level = .NET Framework.
    /// </summary>
    public class Serial : ATransport
    {
        // The C++ #defines, now only fallbacks for a first run with no history.
        public const string WIN_COM = "COM4";
        public const string MAC_COM = "/dev/cu.usbmodem27946701";

        // Serial.cpp's COM_BAUD of 9600 was only ever passed to the Mac path;
        // Serial-PC.cpp hardcoded BaudRate = 1000000 in its DCB. Both are moot on a
        // Teensy, whose USB CDC ignores the baud setting, but the values are kept.
        public const int COM_BAUD = 9600;
        public const int WIN_BAUD = 1000000;

        // Serial-PC.cpp slept this long after connecting, for the board to reset.
        public const int ARDUINO_WAIT_TIME = 2000;

        public const string PORTS_FILE = "firefly_ports.txt";
        public const int MAX_REMEMBERED_PORTS = 8;

        private SerialPort port;
        private List<string> recentPorts = new List<string>();

        /// <summary>Ports that have connected before, most recent first.</summary>
        public IList<string> RecentPorts { get { return recentPorts; } }

        /// <summary>The port currently open, or the last one attempted.</summary>
        public string CurrentPort { get; private set; }

        public static bool IsWindows
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsPlayer
                    || Application.platform == RuntimePlatform.WindowsEditor;
            }
        }

        public static string DefaultPortName { get { return IsWindows ? WIN_COM : MAC_COM; } }

        /// <summary>
        /// Beside the executable in a build; beside the Assets folder in the Editor.
        /// </summary>
        private static string PortsFilePath
        {
            get { return Path.Combine(Directory.GetParent(Application.dataPath).FullName, PORTS_FILE); }
        }

        public bool InitComms()
        {
            LoadRecentPorts();

            string first = recentPorts.Count > 0 ? recentPorts[0] : DefaultPortName;
            CurrentPort = first;
            TryOpen(first);

            return true;
        }

        /// <summary>
        /// Closes any open port and attempts the named one. Returns whether it
        /// opened. Only successful ports are remembered.
        /// </summary>
        public bool TryOpen(string portName)
        {
            if (string.IsNullOrEmpty(portName)) return false;

            portName = portName.Trim();
            CurrentPort = portName;
            Close();

            try
            {
                port = new SerialPort(portName, IsWindows ? WIN_BAUD : COM_BAUD);

                // The C++ used a blocking WriteFile / write(). A finite timeout here
                // would throw whenever the device fell behind, which it will — the
                // desktop renders far faster than the Teensy can consume frames.
                port.WriteTimeout = SerialPort.InfiniteTimeout;
                port.ReadTimeout = 1000;

                // Serial-PC.cpp set DTR_CONTROL_ENABLE in its DCB. The Mac path left
                // its DTR ioctls commented out, but asserting it is harmless there.
                port.DtrEnable = true;

                port.Open();

                if (IsWindows)
                {
                    // Matches PurgeComm(PURGE_RXCLEAR | PURGE_TXCLEAR) followed by
                    // Sleep(ARDUINO_WAIT_TIME) in Serial-PC.cpp. Blocks for two
                    // seconds, so the window freezes briefly on connect.
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    System.Threading.Thread.Sleep(ARDUINO_WAIT_TIME);
                }

                RememberPort(portName);
                FireflyUtils.Log("[Serial] Opened " + portName);
                return true;
            }
            catch (Exception e)
            {
                // The C++ simply carried on with a dead handle and Available() went
                // false; same outcome here, and the app runs preview-only.
                port = null;
                FireflyUtils.Log("[Serial] Could not open " + portName + " — running without LED output. (" + e.Message + ")");
                return false;
            }
        }

        public override bool Available()
        {
            return port != null && port.IsOpen;
        }

        public override string Describe()
        {
            return Available() ? "Serial: " + CurrentPort : "Serial";
        }

        public override int Write(byte[] buffer, int size)
        {
            if (!Available()) return 0;

            try
            {
                port.Write(buffer, 0, size);
                return size;
            }
            catch (Exception e)
            {
                // Log and carry on rather than closing. The C++ kept writing to its
                // handle regardless; closing here would blank the sculpture for the
                // rest of the run on a single hiccup.
                FireflyUtils.Log("[Serial] Write failed. (" + e.Message + ")");
                return 0;
            }
        }

        public int Read(byte[] buffer, int size)
        {
            if (!Available()) return 0;

            try
            {
                return port.Read(buffer, 0, size);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public override void Close()
        {
            if (port != null)
            {
                try { if (port.IsOpen) port.Close(); }
                catch (Exception) { }
                port = null;
            }
        }

        // ── Remembered ports ────────────────────────────────────

        private void RememberPort(string portName)
        {
            recentPorts.Remove(portName);
            recentPorts.Insert(0, portName);
            while (recentPorts.Count > MAX_REMEMBERED_PORTS)
            {
                recentPorts.RemoveAt(recentPorts.Count - 1);
            }
            SaveRecentPorts();
        }

        private void LoadRecentPorts()
        {
            recentPorts.Clear();
            try
            {
                if (!File.Exists(PortsFilePath)) return;

                foreach (string line in File.ReadAllLines(PortsFilePath))
                {
                    string p = line.Trim();
                    if (p.Length > 0 && !recentPorts.Contains(p)) recentPorts.Add(p);
                }
            }
            catch (Exception e)
            {
                FireflyUtils.Log("[Serial] Could not read " + PORTS_FILE + ". (" + e.Message + ")");
            }
        }

        private void SaveRecentPorts()
        {
            try
            {
                File.WriteAllLines(PortsFilePath, new List<string>(recentPorts).ToArray());
            }
            catch (Exception e)
            {
                FireflyUtils.Log("[Serial] Could not write " + PORTS_FILE + ". (" + e.Message + ")");
            }
        }
    }
}
