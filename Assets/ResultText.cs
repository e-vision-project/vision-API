using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVISION.Camera.plugin;
using System;
using UnityEngine.UI;

public class ResultText : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<Text>();
        EventCamManager.onProcessEnded += SetText;
    }

    private void SetText()
    {
        text.text = MasoutisManager.text_result;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
