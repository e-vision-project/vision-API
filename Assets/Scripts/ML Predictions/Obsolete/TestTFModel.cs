using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EVISION.Camera.plugin;

public class TestTFModel : MonoBehaviour
{

    public TextAsset model;
    public TextAsset labelsFile;
    public string image_file;
    public int angle;
    private IModelPrediction classifier_2;
    // Start is called before the first frame update
    void Start()
    {

        TFSharpClassification classifier = new TFSharpClassification("input_1", "Logits/Softmax", 224, 224, 127.5f, 127.5f, model, labelsFile, angle, 0.05f);

        var tex = Resources.Load<Texture2D>("Textures/Masoutis/" + image_file);
        //var tex = Resources.Load<Texture2D>("Textures/" + image_file);

        //Texture2D tex = new Texture2D(img.texture.width, img.texture.height);
        //tex = img.texture as Texture2D;

        RawImage img2 = GameObject.Find("img2").GetComponent<RawImage>();
        img2.texture = tex;;

        //var oute = classifier.FetchOutput(tex);
        //foreach (KeyValuePair<string, float> value in oute)
        //{
        //    Debug.Log("class :" + value.Key + ": " + value.Value);
        //}



        //print("===============================");

        //IModelPrediction classifier_3 = new TFClassification("input_1", "Logits/Softmax", 224, 224, 127.5f, 127.5f, model, labelsFile, angle, 0.01f);

        //var output = classifier_3.FetchOutput<IList, Texture2D>(tex);
        //foreach (KeyValuePair<string, float> value in output)
        //{
        //    Debug.Log("class :" + value.Key + ": " + value.Value);
        //}

        IModelPrediction classifier_4 = new TFFeatureExtraction("input_1", "block_15_project/convolution", 224, 224, 127.5f, 127.5f, model, labelsFile, angle, 0.01f);
        var output = classifier_4.FetchOutput<List<float>, Texture2D>(tex);

        double[] output_array = GenericUtils.ConvertToDouble(output.ToArray());

        SVMClassification obj = new SVMClassification();
        obj.SetModelParameters("SVM_Weights", "mu", "sigma");
        var norm_fv = obj.NormalizeElements(output_array, obj.muData, obj.sigmaData);
        List<double> norm_fv_list = new List<double>(norm_fv);
        IModelPrediction svm = obj;
        var probs = svm.FetchOutput<List<float>, List<double>>(norm_fv_list);
        foreach (var item in probs)
        {
            Debug.Log(item);
        }
    }
}
