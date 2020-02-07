using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TensorFlow;

/// <summary>
/// This class implements the IModelPrediction interface
/// and aims to extract features from a pre-trained tensorflow
/// model. To fetch output the input is a texture2D and the output
/// is a list of floats (List<float>).
/// </summary>

public class TFFeatureExtraction : IModelPrediction
{
    private string inputName;
    private string outputName;
    private int inputHeight;
    private int inputWidth;
    private float inputMean;
    private float inputStd;
    private TextAsset modelFile;
    private TextAsset labelFile;
    private int angle;
    private float thresshold = 0.05f;
    private string[] labels;
    private int numOfResults = 5;

    private TFGraph graph;
    private TFSession session;

    public TFFeatureExtraction(string inputName, string outputName, int inputHeight, int inputWidth, float inputMean,
        float inputStd, TextAsset modelFile, TextAsset labelFile, int angle, float thresshold)
    {
        this.inputName = inputName;
        this.outputName = outputName;
        this.inputHeight = inputHeight;
        this.inputWidth = inputWidth;
        this.inputMean = inputMean;
        this.inputStd = inputStd;
        this.modelFile = modelFile;
        this.labelFile = labelFile;
        this.angle = angle;
        this.thresshold = thresshold;

        graph = new TFGraph();
        graph.Import(modelFile.bytes);
        session = new TFSession(graph);
        labels = labelFile.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }


    T IModelPrediction.FetchOutput<T, U>(U inputTex)
    {
        //////// TODO : shape should be passed as argument to Transform Input
        var shape = new TFShape(1, inputWidth, inputHeight, 3);
        ////////

        var scaled = TextureTools.scaled(inputTex as Texture2D, 224, 224, FilterMode.Trilinear);
        var rotated = TextureTools.RotateImageMatrix(scaled.GetPixels32(), scaled.width, scaled.height, 180);

        var input = graph[inputName][0];

        var inputTensor = TFSharpUtils.TransformInput(rotated, 224, 224, inputMean, inputStd);

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

        return featureVector as T;
    }
}
