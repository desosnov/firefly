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
        public const string WIN_COM = "COM9";
        public const string MAC_COM = "/dev/cu.usbmodem27946701";
        public const int COM_BAUD = 9600;

        private SerialPort port;

        public bool InitComms()
        {
            string portName = Application.platform == RuntimePlatform.WindowsPlayer
                              || Application.platform == RuntimePlatform.WindowsEditor
                ? WIN_COM
                : MAC_COM;

            try
            {
                port = new SerialPort(portName, COM_BAUD);
                port.WriteTimeout = 1000;
                port.ReadTimeout = 1000;
                port.Open();
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
                FireflyUtils.Log("[Serial] Write failed, closing port. (" + e.Message + ")");
                Close();
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
