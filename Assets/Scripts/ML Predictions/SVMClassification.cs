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
    double[][] lineData;
    private int classNum = 4;
    SVM_Model[] svms = new SVM_Model[4];
    

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

            float[] feautures = (fVector as List<float>).ToArray();

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

    public void SetModelParameters(string filename)
    {
        TextAsset file = Resources.Load<TextAsset>(filename);
        using (var streamReader = new StreamReader(new MemoryStream(file.bytes)))
        {
            string text = streamReader.ReadToEnd();
            var lines = text.Split("\n"[0]);
            lineData = new double[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                var data_str = (lines[i].Trim()).Split(","[0]);
                var data = Array.ConvertAll(data_str, double.Parse);
                lineData[i] = new double[data.Length];
                lineData[i] = data;
            }
        }
        SVM_Model smvClass1 = new SVM_Model(lineData[0], 0.59267967767095653, -5.13194210631268, 0.92507592350160261, 0);
        SVM_Model smvClass2 = new SVM_Model(lineData[1], 0.49120650582242237, -4.3241794097144188, 0.24364938814918963, 0);
        SVM_Model smvClass3 = new SVM_Model(lineData[2], -0.29671019075683935, -3.8673131634685123, -0.22813251638919005, 1);
        SVM_Model smvClass4 = new SVM_Model(lineData[3], 0.64504533684299536, -4.1485740812047123, 1.0796708491407188, 0);
        svms[0] = smvClass1;
        svms[1] = smvClass2;
        svms[2] = smvClass3;
        svms[3] = smvClass4;
    }
}