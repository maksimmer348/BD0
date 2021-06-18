using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GodSharp.SerialPort;

namespace BD0.CP
{

    public static class ComPortWorking
    {
        private static GodSerialPort port;
        public static int CfgChannelNum;
        public static void Open(string num, int baud, int parity, int stop, bool dtr)
        {
            if (port == null || port.IsOpen == false)
            {
                port = new GodSerialPort(num, baud, ConvertParity(parity),);
                    port.PortName = num;
                    port.BaudRate = baud;
                    port.Parity = ConvertParity(parity);
                    port.StopBits = ConvertStopBits(stop);
                    port.DtrEnable = dtr;
                    port.TryReadNumber = 1;
                    port.TryReadSpanTime = 0;
                    port.Terminator = null;
                    port.Open();
                    CfgChannelNum = Int32.Parse(num);
            }
        }

        private static Parity ConvertParity(int parity)
        {
            return parity switch
            {
                0 => Parity.None,
                1 => Parity.Odd,
                2 => Parity.Even,
                _=>Parity.None
            };
        }

        public static void Close()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        public static async Task<string> Write(string write, int delay = 200, bool extraDelayOn = false)
        {
            if (port == null) return null;
            const string endOfLine = "\r\n";
            if (!port.Open()) throw new Exception("Порт не открыт или занят");
            port.WriteAsciiString(write + endOfLine);

            return await Read(delay, extraDelayOn);
        }

        public static async Task<string> Read(int delay, bool extraDelayOn)
        {
            await Task.Delay(delay);
            byte[] buffer = port.Read();

            if (buffer == null)
            {
                return String.Empty;
            }
            else if (!buffer.Contains((byte)10) && extraDelayOn)
            {
                await Task.Delay(50);
            }

            string read = Encoding.ASCII.GetString(buffer);

            return DelTrash(read);
        }

        public static StopBits ConvertStopBits(int stopBit)
        {
            return stopBit switch
            {
                1 => StopBits.One,
                2 => StopBits.Two,
                _ => StopBits.One
            };
        }

        public static string DelTrash(string enter)
        {
            char[] trash = new[] { '?', '\n', '\r' };
            return String.Join("", enter.Where((ch) => !trash.Contains(ch)));
        }
    }
}

