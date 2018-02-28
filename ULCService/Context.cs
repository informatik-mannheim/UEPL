using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectClient
{
    [Serializable]
    public class Context
    {
        [JsonIgnore]
        public static string[] Commands = new string[] {"download", "install", "switch", "unswitch", "remove", "upgrade", "meta"};

        public bool Active = false, Installed = false, Downloaded = false;
        public string Name = string.Empty, ID = string.Empty;
        public int Version = 0;
        
        public Context(bool active = false, bool installed = false, bool downloaded = false, string name = "", string id = "", int version = 0)
        {
            Active = active;
            Installed = installed;
            Downloaded = downloaded;
            Name = name;
            ID = id;
            Version = version;
        }

        public static Context Parse(string parseString)
        {
            return JsonConvert.DeserializeObject<Context>(parseString);
        }

        public static bool TryParse(string parseString, out Context context)
        {
            context = null;

            try
            {
                var obj = JsonConvert.DeserializeObject<Context>(parseString);    

                if (obj == null)
                    return false;
                else
                    context = obj;

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static Context Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                string fileContent = File.ReadAllText(fileName);

                if (TryParse(fileContent, out Context context))
                    return context;
                else
                    throw new JsonSerializationException($"File {fileName} could not be parsed!");
            }
            else
                throw new FileNotFoundException("File does not exist.", fileName);
        }

        public void Save(string fileName)
        {
            string content = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(fileName, content);
        }
    }

    public enum ContextCommand { NONE = -1, Download = 0, Install = 1, Switch = 2, Unswitch = 3, Remove = 4, Upgrade = 5, Meta = 6 };
}
