using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlow;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Threading;



public class TestTensorflow : MonoBehaviour
{

    private const int INPUT_SIZE = 224;
    private const int IMAGE_MEAN = 224;
    private const float IMAGE_STD = 1;
    private const string INPUT_TENSOR = "input";
    private const string OUTPUT_TENSOR = "output";



    public TextAsset model;
    public TextAsset labelMap;

    private TFGraph graph;
    private TFSession session;
    private string[] labels;

    public RawImage img;
    //#if UNITY_ANDROID
    //    TensorFlowSharp.Android.NativeBinding.Init();
    //#endif

    // Start is called before the first frame update
    void Start()
    {

        //load labels into string array
        labels = labelMap.ToString().Split('\n');
        //load graph
        graph = new TFGraph();
        graph.Import(model.bytes);
        session = new TFSession(graph);

        Texture2D tex = new Texture2D(224, 224);
        StartCoroutine(setImage("https://cdn.pixabay.com/photo/2018/05/28/22/11/message-in-a-bottle-3437294__340.jpg", tex));
        img.texture = tex;
        ProcessImage(tex);


    }

    IEnumerator setImage(string url,Texture2D tex)
    {

        WWW www = new WWW(url);
        yield return www;

        // calling this function with StartCoroutine solves the problem

        www.LoadImageIntoTexture(tex);
        www.Dispose();
        www = null;
    }

    public void ProcessImage(Texture2D tex)
    {
      
        Debug.Log("Image to Pixels32 ok");
        var tensor = TransformInput(tex.GetPixels32(), INPUT_SIZE, INPUT_SIZE);
        var runner = session.GetRunner();
        runner.AddInput(graph[INPUT_TENSOR][0], tensor).Fetch(graph[OUTPUT_TENSOR][0]);
        var output = runner.Run();
        Debug.Log("output Ok");
        //put results into one dimensional array
        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
        Debug.Log("Probs ok");
        //get max value of probabilities and find its associated label index
        float maxValue = probs.Max();
        int maxIndex = probs.ToList().IndexOf(maxValue);
        //print label with highest probability
        string label = labels[maxIndex];
        Debug.Log(label);
    }

    public static TFTensor TransformInput(Color32[] pic, int width, int height)
    {
        float[] floatValues = new float[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
        }

        TFShape shape = new TFShape(1, width, height, 3);

        return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
    }


}


//TextAsset graphModel = Resources.Load("my_model") as TextAsset;
//var graph = new TFGraph();
//graph.Import(graphModel.bytes);
//var session = new TFSession(graph);
//var runner = session.GetRunner();

//TFTensor input = new float[,] { { 0.0f, 1.0f }, { 1.0f, 1.0f }, { 0.0f, 0.0f }, { 1.0f, 0.0f } };

//runner.AddInput(graph["panos_in_input"][0],input).Fetch(graph["panos_out/Sigmoid"][0]);
//var output = runner.Run();
//float[] probs = ((float[][])output[0].GetValue(jagged: true))[1];
//foreach (var item in probs)
//{
//    print(Mathf.Round(item));
//}