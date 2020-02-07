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
using TensorFlow.Utils;


public class TestTensorflow : MonoBehaviour
{

    private const int INPUT_SIZE = 224;
    private const int IMAGE_MEAN = 127;
    private const float IMAGE_STD = 127;
    private const string INPUT_TENSOR = "input";
    private const string OUTPUT_TENSOR = "output";

    public TextAsset model;
    public TextAsset labelsFile;

    private TFGraph graph;
    private TFSession session;
    private string[] labels;

    private string inputName = "input_1";
    //private string outputName = "fc1000/Softmax";
    private string outputName = "Logits/Softmax";
    private int inputHeight= 224;
    private int inputWidth = 224;
    private float inputMean = 127.5f;
    private float inputStd = 127.5f;

    //#if UNITY_ANDROID
    //    TensorFlowSharp.Android.NativeBinding.Init();
    //#endif

    // Start is called before the first frame update
    void Start()
    {
        RawImage img = GameObject.Find("img").GetComponent<RawImage>();
        Texture2D tex = new Texture2D(img.texture.width, img.texture.height);
        tex = img.texture as Texture2D;


        //load graph
        graph = new TFGraph();
        graph.Import(model.bytes);
        session = new TFSession(graph);
        labels = labelsFile.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        LoadTFModel(tex);
    }

    public void LoadTFModel(Texture2D tex)
    {
        var shape = new TFShape(1, inputWidth, inputHeight, 3);
        var input = graph[inputName][0];
        TFTensor inputTensor = null;

        int angle = 90;
        Flip flip = Flip.NONE; 

        if (input.OutputType == TFDataType.Float)
        {
            float[] imgData = Utils.DecodeTexture(tex, inputWidth, inputHeight,
                                                  inputMean, inputStd, angle, flip);
            inputTensor = TFTensor.FromBuffer(shape, imgData, 0, imgData.Length);
        }
        else if (input.OutputType == TFDataType.UInt8)
        {
            byte[] imgData = Utils.DecodeTexture(tex, inputWidth, inputHeight, angle, flip);
            inputTensor = TFTensor.FromBuffer(shape, imgData, 0, imgData.Length);
        }
        else
        {
            throw new Exception($"Input date type {input.OutputType} is not supported.");
        }

        var runner = session.GetRunner();
        runner.AddInput(input, inputTensor).Fetch(graph[outputName][0]);

        var output = runner.Run()[0];
        var outputs = output.GetValue() as float[,];

        inputTensor.Dispose();
        output.Dispose();

        var list = new List<KeyValuePair<string, float>>();

        for (int i = 0; i < labels.Length; i++)
        {
            var confidence = outputs[0, i];
            if (confidence < 0.05f) continue;

            list.Add(new KeyValuePair<string, float>(labels[i], confidence));
        }

        var results = list.OrderByDescending(i => i.Value).Take(3).ToList();
        foreach (KeyValuePair<string, float> value in results)
        {
            Debug.Log("my model: " + value.Key);
        }
    }

    public void ProcessImage(Texture2D tex)
    {

        graph = new TFGraph();
        graph.Import(model.bytes);
        session = new TFSession(graph);
        Debug.Log("Model loaded");

        var scaled = TextureTools.scaled(tex, 224, 224, FilterMode.Bilinear);
        var img = scaled.GetPixels32();
        var tensor = TransformInput(img, INPUT_SIZE, INPUT_SIZE);
        var runner = session.GetRunner();
        runner.AddInput(graph[inputName][0], tensor).Fetch(graph[outputName][0]);
        var output = runner.Run();
        //put results into one dimensional array
        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
        //get max value of probabilities and find its associated label index
        float maxValue = probs.Max();
        int maxIndex = probs.ToList().IndexOf(maxValue);
        print(maxIndex);
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



//TFTensor input = new float[,] { { 0.0f, 1.0f }, { 1.0f, 1.0f }, { 0.0f, 0.0f }, { 1.0f, 0.0f } };
//var runner = session.GetRunner();
////runner.AddInput(graph["panos_in_input"][0],input).Fetch(graph["panos_out/Sigmoid"][0]);
//runner.AddInput(graph["panos_in_input"][0],input).Fetch(graph["panos_out/Sigmoid"][0]);
//var output = runner.Run();

//float[] probs = ((float[][])output[0].GetValue(jagged: true))[1];
//foreach (var item in probs)
//{
//    print(item);
//}