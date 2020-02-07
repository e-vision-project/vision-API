using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

[Serializable]
public class SVMLibTest : MonoBehaviour
{
    void Start()
    {
        SVMClassification obj = new SVMClassification();

        IModelPrediction svm = obj;
    }
}


//private void Start()
//{
//    TextAsset jsonFile = Resources.Load<TextAsset>("modelSVM");
//    string json = jsonFile.text;
//    ModelWrapper svmModel = JsonUtility.FromJson<ModelWrapper>("{\"users\":" + json + "}");
//}


//TextAsset jsonFile = Resources.Load<TextAsset>("modelSVM");
//        using (var streamReader = new StreamReader(new MemoryStream(jsonFile.bytes)))
//        {
//            string json = streamReader.ReadToEnd();
//JSONObject obj = new JSONObject(json);
//JSONObject j = (JSONObject)obj.list[0];
//JSONObject arr = j["weights"];
//double[] weights = new double[7840];
//            for (int i = 0; i<arr.Count; i++)
//            {
//                weights[i] = arr[i].n;
//                Debug.Log((decimal) arr[i].n);
//            }
//        }


//TextAsset file = Resources.Load<TextAsset>("Model_SVM");
//string text = file.text;
//var lines = text.Split("\n"[0]);
//var lineData = (lines[0].Trim()).Split(","[0]);
//decimal[] weights = new decimal[lineData.Length - 1];
//        for (int i = 1; i<lineData.Length; i++)
//        {
//            weights[i - 1] = decimal.Parse(lineData[i]);
//        }