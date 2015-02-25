using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class INIParser 
{
    private Dictionary<string, Dictionary<string, string>> _tables;
    private StreamReader _iniFile;

    public INIParser(string fileName)
    {
        _tables = new Dictionary<string, Dictionary<string, string>>();
        _iniFile = File.OpenText(fileName);
    }

    public Dictionary<string, Dictionary<string, string>> ParseINI()
    {
        string rawLine = string.Empty;
        string section = string.Empty;

        while((rawLine = _iniFile.ReadLine()) != null)
        {
            string data = rawLine.Replace(" ", "");
            data = data.Replace("\r", "");
            data = data.Replace("\n", "");
            data = data.Replace("\t", "");

            int index = data.IndexOf(';');

            if (index == 0 || data == string.Empty)
                continue;

            if (index != -1)
                data = data.Substring(0, index);
            
            // handle new section
            if(data[0] == '[')
            {
                section = data;
                _tables.Add(section, new Dictionary<string, string>());
                continue;
            }

            var halves = new List<string>(data.Split('='));

            if (halves.Count > 2 || halves.Count <= 1) // error
                break;

            _tables[section].Add(halves[0], halves[1]);
        }

        return _tables;
    }

    public void CloseINI()
    {
        _iniFile.Close();
    }
}
