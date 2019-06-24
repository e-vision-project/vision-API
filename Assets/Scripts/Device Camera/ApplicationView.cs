using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationView : MonoBehaviour
{
    public GameObject image;
    public Text text;
    public ClientApplication clientApp;

    // Start is called before the first frame update
    void Start()
    {
        image.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(clientApp.annotationProccessBusy)
        {
            image.SetActive(true);
            Debug.Log("Annotating");
        }
        else if (!clientApp.annotationProccessBusy)
        {
            image.SetActive(false);
        }
    }
}
