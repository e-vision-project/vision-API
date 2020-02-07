using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveListToTXT : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SaveList("Assets/Resources/products_db_jan_2020.csv"));
        //var logFile = File.ReadAllLines("Assets/Resources/test.txt");
        //var logList = new List<string>(logFile);
    }

    IEnumerator SaveList(string path)
    {
        MajorityVoting maj = new MajorityVoting();
        maj.ReadDatabaseFile("masoutis_db_cleaned_2019");
        while (!MajorityVoting.database_ready)
        {
            yield return null;
        }

        StreamWriter writer = new StreamWriter(path, true);
        foreach (string s in MajorityVoting.desc)
        {
            writer.WriteLine(s);
        }

        writer.Close();
        Debug.Log("File written");
    }
}
