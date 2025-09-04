using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Strategy
{
    public class TIniReader
    {
        Dictionary<string, List<List<string>>> scr = new Dictionary<string, List<List<string>>>();
        public TIniReader(string file, char delimiter = ',')
        {
            var section = new List<List<string>>();
            scr[""] = section;
            if (!File.Exists(file)) return;
            var txt = File.ReadAllText(file, Encoding.GetEncoding(1250));
            var lines = txt.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                   .Where(t => !string.IsNullOrWhiteSpace(t))
                                   .Select(t => t.Trim());
            foreach (var line in lines)
            {
                if (line.StartsWith(";"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = new List<List<string>>();
                    scr[line.Substring(1, line.LastIndexOf("]") - 1)] = section;
                    continue;
                }
                var command = new List<string>();
                string param;
                var args = line.Split('(');
                if (args.Length > 1)
                {
                    command.Add(args[0]);
                    var idx = args[1].IndexOf(")");
                    param = args[1].Substring(0, idx);
                }
                else
                    param = args[0];
                command.AddRange(param.Split(delimiter));
                section.Add(command);
            }
        }

        public Dictionary<string, int> LoadVars()
        {
            var variables = new Dictionary<string, int>();
            var varSection = scr["VAR"];
            foreach (var variableLine in varSection)
            {
                var variable = variableLine[0].Split('=');
                variables.Add(variable[0], int.Parse(variable[1]));
            }
            return variables;
        }

        public List<List<string>> this[string section]
        {
            get { return scr[section]; }
        }

        //public string GetValue(string key)
        //{
        //    return GetValue(key, "", "");
        //}

        //public string GetValue(string key, string section)
        //{
        //    return GetValue(key, section, "");
        //}

        //public string GetValue(string key, string section, string @default)
        //{
        //    if (!scr.ContainsKey(section))
        //        return @default;

        //    if (!scr[section].ContainsKey(key))
        //        return @default;

        //    return scr[section][key];
        //}

        //public string[] GetKeys(string section)
        //{
        //    if (!scr.ContainsKey(section))
        //        return new string[0];

        //    return scr[section].Keys.ToArray();
        //}

        //public string[] GetSections()
        //{
        //    return scr.Keys.Where(t => t != "").ToArray();
        //}
    }
}
