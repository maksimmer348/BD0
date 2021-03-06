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
        public static void Open(string num, int parity,int baud, int stop , bool dtr)
        {
            if (port == null || port.IsOpen == false)
            {

                port = new GodSerialPort("COM" + num, baud, parity, dataBits:8, stop)
                {
                    DtrEnable = dtr,
                    TryReadNumber = 1,
                    TryReadSpanTime = 0,
                    Terminator = null
                };

                port.Open();
                CfgChannelNum = Int32.Parse(num);
            }
        }

        public static void Close()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        public static async Task<string> Write(string write, int delay = 50, bool extraDelayOn = false)
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
        private static Parity ConvertParity(int parity)
        {
            return parity switch
            {
                0 => Parity.None,
                1 => Parity.Odd,
                2 => Parity.Even,
                _ => Parity.None
            };
        }

        public static string DelTrash(string enter)
        {
            char[] trash = new[] { '?', '\n', '\r' };
            return String.Join("", enter.Where((ch) => !trash.Contains(ch)));
        }
    }
}

