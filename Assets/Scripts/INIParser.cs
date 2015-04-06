using UnityEngine;
using System.Linq;
using System;
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
            string data = rawLine;
            int index = data.IndexOf(';');

            if (index == 0 || data == string.Empty)
                continue;

            if (index != -1)
                data = data.Substring(0, index);
            
            // handle new section
            int sectStart = -1;
            int sectEnd = -1;
            bool nextLine = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '[')
                    sectStart = i;

                if(data[i] == ']')
                    sectEnd = i;

                if (sectStart != -1 && sectEnd != -1)
                {
                    section = data.Substring(sectStart, sectEnd - sectStart + 1);
                    _tables.Add(section, new Dictionary<string, string>());
                    nextLine = true;
                    break;
                }
            }

            if (nextLine)
                continue;

            var halves = new List<string>(data.Split('='));

            if (halves.Count > 2 || halves.Count <= 1) // error
                break;

            _tables[section].Add(halves[0].Trim(), halves[1].Trim());
        }

        return _tables;
    }

    public void CloseINI()
    {
        _iniFile.Close();
    }
}
