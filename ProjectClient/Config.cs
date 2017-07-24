using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectClient
{
    public class Config
    {
        private const string CONFIG_FILE_NAME = "config.ini";
        private string fileName = CONFIG_FILE_NAME;
        private Dictionary<string, string> configuration = new Dictionary<string, string>();

        public event Action<string, string> OnConfigChanged;

        public Config(string fileName = CONFIG_FILE_NAME)
        {
            this.fileName = fileName;
        }

        public void ReadConfig()
        {
            if (File.Exists(fileName))
            {
                foreach (var line in File.ReadAllLines(fileName))
                {
                    var current = line;
                    int comment = current.IndexOf('#');

                    if (comment != -1)
                        current = current.Remove(comment);

                    if (current.IndexOf("=") != -1)
                    {
                        var splitted = current.Split('=');

                        if (splitted.Length != 2)
                            continue;

                        var key = splitted[0].Trim();
                        var value = splitted[1].Trim();

                        if (configuration.ContainsKey(key))
                            configuration[key] = value;
                        else
                            configuration.Add(key, value);
                    }
                }
            }
        }

        public void WriteConfig()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var kvp in configuration)
            {
                builder.AppendLine($"{kvp.Key} = {kvp.Value}");
            }

            File.WriteAllText(fileName, builder.ToString());
        }

        public string this[string index]
        {
            get
            {
                if (configuration.ContainsKey(index))
                    return configuration[index];
                else
                    return string.Empty;
            }
            set
            {
                bool contains = configuration.ContainsKey(index);
                bool sameValue = configuration[index] == value;

                if (!contains)
                    configuration.Add(index, value);
                else if(!sameValue)
                    configuration[index] = value;

                if(!contains || !sameValue)
                    OnConfigChanged?.Invoke(index, value);
            }
        }
    }
}
