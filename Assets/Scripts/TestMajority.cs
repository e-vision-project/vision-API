using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMajority : MonoBehaviour
{
    MajorityVoting maj;

    // Start is called before the first frame update
    void Start()
    {
        maj = new MajorityVoting();
        maj.PerformMajorityVoting();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
