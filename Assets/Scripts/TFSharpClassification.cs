using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TensorFlow;

public class TFSharpClassification : TFSharpModel
{
    private int angle = 90;
    private float thresshold = 0.05f;
    private string[] labels;


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

        graph = new TFGraph();
        graph.Import(modelFile.bytes);
        session = new TFSession(graph);

        labels = labelFile.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public override IList FetchOutput(Texture2D tex) 
    {
        var shape = new TFShape(1, inputWidth, inputHeight, 3);
        var input = graph[inputName][0];
        TFTensor inputTensor = null;

        angle = 90;
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

        var results = list.OrderByDescending(i => i.Value).Take(1).ToList();
        return results;
    }

}
