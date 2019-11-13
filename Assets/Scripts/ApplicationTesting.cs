using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EVISION.Camera.plugin.ApplicationView;
using EVISION.Camera.plugin;

public class ApplicationTesting : MonoBehaviour
{
    public MasoutisClient client;
    public bool beginTesting = false;
    public int dataset;
    public string path;

    // Start is called before the first frame update
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        if (!client.DB_LoadProccessBusy && beginTesting == false)
        {
            beginTesting = true;
            StartCoroutine(Test());
        }
    }

    public IEnumerator Test()
    {
        string item = "";
        string results = "";
        for (int i = 8; i < dataset; i++)
        {
            item = path + $"/p ({i})";
            Debug.Log(item);
            if (!client.annotationProccessBusy)
            {
               yield return StartCoroutine(client.MockTesting(item, 2));
               results += item + "| " + MajorityFinalText.text + "| " + MajorityValidText.text + "|" + "\n" + distanceString + "\n";
            }
        }

        SaveTXT(results);

    }
}
