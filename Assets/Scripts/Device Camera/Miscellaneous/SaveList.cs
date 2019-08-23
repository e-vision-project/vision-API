using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveList : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SaveListToTXT("Assets/Resources/masoutis_cat4.txt");

        //var logFile = File.ReadAllLines("Assets/Resources/test.txt");
        //var logList = new List<string>(logFile);
    }

    void SaveListToTXT(string path)
    {
        MajorityVoting maj = new MajorityVoting();
        maj.ReadDatabaseFile("masoutis_db");

        StreamWriter writer = new StreamWriter(path, true);
        foreach (string s in MajorityVoting.cat4)
        {
            writer.WriteLine(s);
        }

        writer.Close();
    }
}
