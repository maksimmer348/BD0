using System.Collections.Generic;
using System.Threading;

namespace BD0.BD
{
    public static class Commands
    {
       
        public static Dictionary<string, string> CommandsLib = new Dictionary<string, string>();

        static Commands()
        {
            CommandsLib.Add("Set voltage", ":chan1:volt");
            CommandsLib.Add("Return voltage", ":chan1:meas:volt ?");
            CommandsLib.Add("Return set voltage", ":chan1:volt ?");

            CommandsLib.Add("Set current", ":chan1: curr");
            CommandsLib.Add("Return current", ":chan1:meas:curr ?");
            CommandsLib.Add("Return set current", ":chan1:curr ?");

            CommandsLib.Add("Output", ":outp:stat");
            CommandsLib.Add("Get Output", ":outp:stat?");


        }

        public static string GetCommand(string key, string param = null)
        {
            return $"{CommandsLib[key]} {param}".Replace(",", ".");
        }

        
    }
}