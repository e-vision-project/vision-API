using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestTFModel : MonoBehaviour
{

    public TextAsset model;
    public TextAsset labelsFile;
    public RawImage img;
    public int angle;
    private IModelPrediction classifier_2;
    // Start is called before the first frame update
    void Start()
    {

        RawImage img = GameObject.Find("img").GetComponent<RawImage>();
        TFSharpClassification classifier = new TFSharpClassification("input_1", "block_15_project/convolution", 224, 224, 127.0f, 127.0f, model, labelsFile, angle, 0.05f);

        classifier_2 = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.0f, 127.0f, model, labelsFile, angle, 0.05f);

        Texture2D tex = new Texture2D(img.texture.width, img.texture.height);
        tex = img.texture as Texture2D;

        RawImage img2 = GameObject.Find("img2").GetComponent<RawImage>();
        img2.texture = tex;

        var output = classifier.FetchIntermidiateOutput(tex);
        print(output[0]);

        var output_2 = classifier_2.FetchOutput<IList, Texture2D>(tex);
        print(output_2[0]);

        //var output = classifier.FetchOutput<IList,Texture2D>(tex);
        //foreach (KeyValuePair<string, float> value in output)
        //{
        //    Debug.Log("class :" + value.Key + ": " + value.Value);
        //}

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
