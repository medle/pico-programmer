using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace PicoProgrammer
{
    public class Options
    {
        public string FirmwareFilePath;
        public string TargetDevicePath;
        public string ComPort;

        public void Save()
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            using (var sw = new StreamWriter(GetOptionsFileName()))
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this);
                }
            }
        }

        public static Options Recall()
        {
            using (var tr = new StreamReader(GetOptionsFileName()))
            {
                using (var reader = new JsonTextReader(tr))
                {
                    if (!reader.Read()) throw new Exception("Can't read the options file");
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    return serializer.Deserialize<Options>(reader);
                }
            }
        }

        public static string GetOptionsFileName()
        {
            string path = GetAssemblyPath(typeof(Options));
            return Path.Combine(
                Path.GetDirectoryName(path), 
                Path.GetFileNameWithoutExtension(path) + ".cfg");
        }
        public static string GetAssemblyPath(Type type)
        {
            string codeBase = System.Reflection.Assembly.GetAssembly(type).CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

    }
}
