using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectClient
{
    public class Context
    {
        [JsonIgnore]
        public static string[] Commands = new string[] {"download", "install", "switch", "unswitch", "remove"};

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

        public void Save(string fileName)
        {
            string content = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(fileName, content);
        }
    }

    public enum ContextCommand { NONE = -1, Download = 0, Install = 1, Switch = 2, Unswitch = 3, Remove = 4 };
}
