using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;


public class AsyncTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GetReturnValue().WrapErrors();
        MajorityVoting maj = new MajorityVoting();
        maj.ReadDatabaseFile("masoutis_db");
    }

    async Task GetReturnValue()
    {
        Debug.Log("read database: " + Time.realtimeSinceStartup); MajorityVoting maj = new MajorityVoting();
        // call with await the IENUMA
        Debug.Log("Database is read");
    }

    async void MultiThread()
    {
        // Unity thread
        int count = 0;

        await new WaitForBackgroundThread();

        while(count < 10000)
        {
            count++;
        }
        await new WaitForUpdate();
        Debug.Log(count);
        // Unity thread again
    }


}
