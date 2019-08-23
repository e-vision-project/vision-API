using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TensorFlow;

public class TFClassification : IModelPrediction
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

    public TFClassification(string inputName, string outputName, int inputHeight, int inputWidth, float inputMean,
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

    public T FetchOutput<T,U>(U inputTex) where T: class, IList where U : class
    {
        var shape = new TFShape(1, inputWidth, inputHeight, 3);
        var input = graph[inputName][0];
        TFTensor inputTensor = null;

        Flip flip = Flip.NONE;

        if (input.OutputType == TFDataType.Float)
        {
            float[] imgData = Utils.DecodeTexture(inputTex as Texture2D, inputWidth, inputHeight,
                                                  inputMean, inputStd, angle, flip);
            inputTensor = TFTensor.FromBuffer(shape, imgData, 0, imgData.Length);
        }
        else if (input.OutputType == TFDataType.UInt8)
        {
            byte[] imgData = Utils.DecodeTexture(inputTex as Texture2D, inputWidth, inputHeight, angle, flip);
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

        var results = list.OrderByDescending(i => i.Value).Take(numOfResults).ToList();
        return results as T;
    }
}
