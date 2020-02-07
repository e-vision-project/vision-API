using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TensorFlow;
using System.Runtime.InteropServices;
using TensorFlow.Utils;

public class TFSharpClassification : TFSharpModel
{
    private int angle;
    private float thresshold = 0.05f;
    private string[] labels;
    private int numOfResults = 5;


    public TFSharpClassification(string inputName, string outputName, int inputHeight, int inputWidth, float inputMean,
        float inputStd, TextAsset modelFile, TextAsset labelFile, int angle, float thresshold)
    {

        base.inputName = inputName;
        base.outputName = outputName;
        base.inputHeight = inputHeight;
        base.inputWidth = inputWidth;
        base.inputMean = inputMean;
        base.inputStd = inputStd;
        base.modelFile = modelFile;
        base.labelFile = labelFile;

        this.angle = angle;
        this.thresshold = thresshold;
        graph = new TFGraph();
        graph.Import(modelFile.bytes);
        session = new TFSession(graph);

        labels = labelFile.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public override List<float> FetchIntermidiateOutput(Texture2D tex)
    {
        var shape = new TFShape(1, inputWidth, inputHeight, 3);
        var input = graph[inputName][0];
        TFTensor inputTensor = null;

        Flip flip = Flip.NONE;

        if (input.OutputType == TFDataType.Float || input.OutputType == TFDataType.UInt8)
        {
            float[] imgData = Utils.DecodeTexture(tex, inputWidth, inputHeight,
                                                  inputMean, inputStd, angle, flip);
            inputTensor = TFTensor.FromBuffer(shape, imgData, 0, imgData.Length);
        }
        //else if (input.OutputType == TFDataType.UInt8)
        //{
        //    byte[] imgData = Utils.DecodeTexture(tex, inputWidth, inputHeight, angle, flip);
        //    inputTensor = TFTensor.FromBuffer(shape, imgData, 0, imgData.Length);
        //}
        else
        {
            throw new Exception($"Input date type {input.OutputType} is not supported.");
        }

        var runner = session.GetRunner();
        runner.AddInput(input, inputTensor).Fetch(graph[outputName][0]);

        var output = runner.Run()[0];
        var feautures = output.GetValue() as float[,,,];
        var jagged_features = ((float[][][][])output.GetValue(jagged: true));

        var flat3D = jagged_features.SelectMany(a => a).ToArray();
        var flat2D = flat3D.SelectMany(a => a).ToArray();
        var flat1D = flat2D.SelectMany(a => a).ToArray();

        List<float> featureVector = new List<float>(flat1D);

        inputTensor.Dispose();
        output.Dispose();

        return featureVector;

    }

    public override IList FetchOutput(Texture2D tex) 
    {
        Debug.Log("Shekrio");
        var scaled = TextureTools.scaled(tex, 224, 224, FilterMode.Trilinear);
        var rotated = TextureTools.RotateImageMatrix(scaled.GetPixels32(), scaled.width, scaled.height,180);

      
        var input = graph[inputName][0];
        var inputTensor = TransformInput(rotated, 224, 224);
        
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
            if (confidence < 0.01f) continue;

            list.Add(new KeyValuePair<string, float>(labels[i], confidence));
        }

        var results = list.OrderByDescending(i => i.Value).Take(numOfResults).ToList();
        return results;
    }

    public static TFTensor TransformInput(Color32[] pic, int width, int height)
    {
        float[] floatValues = new float[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (color.r - 127.5f) / 127.0f;
            floatValues[i * 3 + 1] = (color.g - 127.5f) / 127.0f;
            floatValues[i * 3 + 2] = (color.b - 127.5f) / 127.0f;
        }

        // save image
        //Texture2D target = new Texture2D(224, 224);
        //target.SetPixels32();
        //target.Apply();
        //var img = target.EncodeToJPG();
        //System.IO.File.WriteAllBytes(Application.dataPath + "/captureImage.jpg", img);
        //Debug.Log("saved at: " + Application.dataPath);

        TFShape shape = new TFShape(1, width, height, 3);

        return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
    }

}
