using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TensorFlow.Utils;

public class PerfomClassification : MonoBehaviour
{
    [SerializeField] TextAsset model;
    [SerializeField] TextAsset labels;
    Classifier classifier;

    private IList outputs;

    // Start is called before the first frame update
    void Start()
    {
        RawImage img = GameObject.Find("img").GetComponent<RawImage>();
        classifier = new Classifier(model, labels, output: "MobilenetV1/Predictions/Reshape_1");
        //classifier = new Classifier(model, labels, input: "input_1", output: "Logits/Softmax");
        Texture2D tex = new Texture2D(img.texture.width, img.texture.height);
        tex = img.texture as Texture2D;
        ProcessImage(tex);
    }

    public void ProcessImage(Texture2D tex)
    {
        Debug.Log("Object Detection started");

        outputs = classifier.Classify(tex, angle: 90, threshold: 0.05f);
        foreach (KeyValuePair<string, float> value in outputs)
        {
            Debug.Log(value.Key);
        }
    }

    public void OnGUI()
    {
        if (outputs != null)
        {
            // Object detection
            Utils.DrawOutput(outputs, new Vector2(20, 20), Color.yellow);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
