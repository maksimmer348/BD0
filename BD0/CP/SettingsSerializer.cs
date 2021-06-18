using System.IO;
using Json.Net;
using File = System.IO.File;

namespace BD0.CP
{
    public static class SettingsSerializer
    {

        public static string Path = "Settings.json";

        public static void Serialize(ComConfig cnfg)
        {
            var json = JsonNet.Serialize(cnfg);
            File.WriteAllText(Path, json);
        }
        
        public static ComConfig Deserialize() => JsonNet.Deserialize<ComConfig>(File.ReadAllText(Path));

        public static void InitSettings()
        {
            if (!File.Exists(Path))
            {
                var json = JsonNet.Serialize(ComConfig.DefaultConfig);
                File.WriteAllText(Path, json);
            }
        }
    }
}