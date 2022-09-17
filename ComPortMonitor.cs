using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace PicoProgrammer
{
    public class ComPortMonitor
    {
        private Action<string> dataHandler;
        private Action<string> logger;
        private SerialPort serialPort;

        public ComPortMonitor(Action<string> dataHandler, Action<string> logger)
        {
            this.dataHandler = dataHandler ?? throw new NullReferenceException();
            this.logger = logger ?? throw new NullReferenceException();
        }

        public void Refresh(string comPortName)
        {
            try
            {
                if (serialPort == null ||
                    serialPort.PortName != comPortName ||
                    !serialPort.IsOpen)
                {
                    Close();
                    Open(comPortName);
                }
            }
            catch (Exception e) {
                logger($"SerialPort: {e.Message}");
            }
        }

        private void Open(string comPortName)
        {
            serialPort = new SerialPort(comPortName, 115200/*, Parity.None, 8, StopBits.One*/);
            serialPort.DtrEnable = true; // necessary for PiPico
            serialPort.Open();
            serialPort.DataReceived += delegate { ReadFromPort(); };
            logger($"Opened {comPortName}");
        }

        public void Close()
        {
            if (serialPort != null) {
                if(serialPort.IsOpen) serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }
        }

        public bool TrySendChar(char ch)
        {
            if (serialPort == null || !serialPort.IsOpen) return false;
            try {
                serialPort.Write(ch.ToString());
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        private void ReadFromPort()
        {
            if (serialPort == null) return;

            // read bytes in as quickly as possible
            var buf = new StringBuilder();
            while (serialPort.BytesToRead > 0)
            {
                int ch = serialPort.ReadChar();
                buf.Append((char)ch);
            }

            dataHandler(buf.ToString());
        }
    }
}
