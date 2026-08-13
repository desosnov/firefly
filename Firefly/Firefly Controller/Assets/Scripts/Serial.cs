using System;
using System.IO.Ports;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Port of Serial.h / .cpp plus the two platform back-ends (Serial-PC.cpp on
    /// Windows, arduino-serial-lib.c on Mac). System.IO.Ports.SerialPort covers both,
    /// so the #ifdef split collapses to a port-name choice. See Port Notes.
    ///
    /// Requires Player Settings -> Api Compatibility Level = .NET Framework.
    /// </summary>
    public class Serial
    {
        // Was COM9 in the C++; the Teensy enumerates on COM4 on Denis's current PC.
        public const string WIN_COM = "COM4";
        public const string MAC_COM = "/dev/cu.usbmodem27946701";

        // Serial.cpp's COM_BAUD of 9600 was only ever passed to the Mac path;
        // Serial-PC.cpp hardcoded BaudRate = 1000000 in its DCB. Both are moot on a
        // Teensy, whose USB CDC ignores the baud setting, but the values are kept.
        public const int COM_BAUD = 9600;
        public const int WIN_BAUD = 1000000;

        // Serial-PC.cpp slept this long after connecting, for the board to reset.
        public const int ARDUINO_WAIT_TIME = 2000;

        private SerialPort port;

        public bool InitComms()
        {
            bool windows = Application.platform == RuntimePlatform.WindowsPlayer
                           || Application.platform == RuntimePlatform.WindowsEditor;
            string portName = windows ? WIN_COM : MAC_COM;

            try
            {
                port = new SerialPort(portName, windows ? WIN_BAUD : COM_BAUD);

                // The C++ used a blocking WriteFile / write(). A finite timeout here
                // would throw whenever the device fell behind, which it will — the
                // desktop renders far faster than the Teensy can consume frames.
                port.WriteTimeout = SerialPort.InfiniteTimeout;
                port.ReadTimeout = 1000;

                // Serial-PC.cpp set DTR_CONTROL_ENABLE in its DCB. The Mac path left
                // its DTR ioctls commented out, but asserting it is harmless there.
                port.DtrEnable = true;

                port.Open();

                if (windows)
                {
                    // Matches PurgeComm(PURGE_RXCLEAR | PURGE_TXCLEAR) followed by
                    // Sleep(ARDUINO_WAIT_TIME) in Serial-PC.cpp.
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    System.Threading.Thread.Sleep(ARDUINO_WAIT_TIME);
                }

                FireflyUtils.Log("[Serial] Opened " + portName);
            }
            catch (Exception e)
            {
                // The C++ simply carried on with a dead handle and Available() went
                // false; same outcome here, and the app runs preview-only.
                port = null;
                FireflyUtils.Log("[Serial] Could not open " + portName + " — running without LED output. (" + e.Message + ")");
            }

            return true;
        }

        public bool Available()
        {
            return port != null && port.IsOpen;
        }

        public int Write(byte[] buffer, int size)
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

        public void Close()
        {
            if (port != null)
            {
                try { if (port.IsOpen) port.Close(); }
                catch (Exception) { }
                port = null;
            }
        }
    }
}
