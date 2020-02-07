    using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public struct SVM_Model
{
    public double[] weights;
    public double bias;
    public double probA;
    public double probB;
    public double label;

    public SVM_Model(double[] weights, double bias, double probA, double probB, double label)
    {
        this.weights = weights;
        this.bias = bias;
        this.probA = probA;
        this.probB = probB;
        this.label = label;
    }
}

[Serializable]
public class SVMClassification : IModelPrediction
{
    private double[][] weightData;
    public double[] muData;
    public double[] sigmaData;
    private int classNum = 3;
    SVM_Model[] svms = new SVM_Model[3];
    

    T IModelPrediction.FetchOutput<T, U>(U fVector)
    {
        List<float> class_prob = new List<float>();

        for (int i = 0; i < svms.Length; i++)
        {
            if(svms[i].label == -1)
            {
                for (int j = 0; j < svms[i].weights.Length; j++)
                {
                    svms[i].weights[j] *= -1;
                }
                svms[i].bias *= -1;
            }

            double[] feautures = (fVector as List<double>).ToArray();

            var y = (feautures.Zip(svms[i].weights, (x1, x2) => x1 * x2).Sum() + svms[i].bias) * svms[i].probA + svms[i].probB;

            float propability = 0;
            if (y >= 0)
            {
                propability = Mathf.Exp((float)-y) / (1 + Mathf.Exp((float)-y));
            }
            else
            {
                propability = 1 / (1 + Mathf.Exp((float)y));
            }


            if(svms[i].label == 0)
            {
                propability = 1 - propability;
            }
            // add class propability to list
            class_prob.Add(propability);
        }
        return class_prob as T;
    }

    public void SetModelParameters(string filenameWeights, string filenameMU, string filenameSigma)
    {
        weightData = ReadWeightsFile(filenameWeights);
        muData = ReadTXTFile(filenameMU);
        sigmaData = ReadTXTFile(filenameSigma);

        SVM_Model smvClass1 = new SVM_Model(weightData[0], 0.375351793893002, -5.238364113657447, 0.3182509645140308, 0);
        SVM_Model smvClass2 = new SVM_Model(weightData[1], 0.17534581098550536, -5.158183725130971, 0.18130600962167662, 0);
        SVM_Model smvClass3 = new SVM_Model(weightData[2], -0.6832487663989988, -6.237363912024131, -0.3252349452083724, 1);
        //SVM_Model smvClass4 = new SVM_Model(lineData[3], 0.64504533684299536, -4.1485740812047123, 1.0796708491407188, 0);
        svms[0] = smvClass1;
        svms[1] = smvClass2;
        svms[2] = smvClass3;
        //svms[3] = smvClass4;

    }

    private double[][] ReadWeightsFile(string filenameWeights)
    {
        TextAsset file = Resources.Load<TextAsset>(filenameWeights);
        using (var streamReader = new StreamReader(new MemoryStream(file.bytes)))
        {
            string text = streamReader.ReadToEnd();
            var lines = text.Split("\n"[0]);
            var fileData = new double[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                var data_str = (lines[i].Trim()).Split(","[0]);
                var data = Array.ConvertAll(data_str, double.Parse);
                fileData[i] = new double[data.Length];
                fileData[i] = data;
            }
            return fileData;
        }
    }

    private double[] ReadTXTFile(string filename)
    {
        TextAsset file = Resources.Load<TextAsset>(filename);
        using (var streamReader = new StreamReader(new MemoryStream(file.bytes)))
        {
            string text = streamReader.ReadToEnd();
            string[] splittedText = text.Split(',');
            //var data = new double[splittedText.Length];
            var data = Array.ConvertAll(splittedText, double.Parse);
            return data;
        }
    }

    public double[] NormalizeElements(double[] fv, double[] MU, double[] Sigma)
    {
        double[] norm_array = new double[fv.Length];

        for (int i = 0; i < fv.Length; i++)
        {
            norm_array[i] = fv[i] - MU[i];
            norm_array[i] = norm_array[i] / Sigma[i];
        }
        return norm_array;
    }
}