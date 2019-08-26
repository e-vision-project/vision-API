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
        SaveListToTXT("Assets/Resources/masoutis_cat2.txt");

        //var logFile = File.ReadAllLines("Assets/Resources/test.txt");
        //var logList = new List<string>(logFile);
    }

    IEnumerator SaveListToTXT(string path)
    {
        MajorityVoting maj = new MajorityVoting();
        maj.ReadDatabaseFile("masoutis_db");
        while (!maj.database_ready)
        {
            yield return null;
        }

        StreamWriter writer = new StreamWriter(path, true);
        foreach (string s in MajorityVoting.cat2)
        {
            writer.WriteLine(s);
        }

        writer.Close();
        Debug.Log("File written");
    }
}
