using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class LogManager
{
    public static string OCRWordsText;
    public static string MajorityValidText;
    public static string MajorityFinalText;
    public static string majorityFinal;
    public static string TimeText;


    public static void SaveResultLogs(string text)
    {
        string path;
        #if UNITY_EDITOR_WIN
        path = Application.dataPath + "/evision_result_logs.txt";
        #endif

        #if UNITY_ANDROID || UNITY_IOS
        path = Application.persistentDataPath + "/evision_result_logs.txt";
        #endif

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "\n");
        }

        File.AppendAllText(path, text);
        Debug.Log("saved as :" + path);
    }

    public static string GetResponseTime(string cameraTime, string cloudTime, string classTime, string majTime, string sum)
    {
        string text = "#Response Times: " + "<Receive image>: " + cameraTime + "|| <Cloud vision>: " + cloudTime + "|| <Classification>: " + classTime +
                      "|| <Majority>: " + majTime + "|| <sum>: " + sum;
        return text;
    }

    public static string GetResponseTime(string cameraTime, string cloudTime, string classTime, string sum)
    {
        string text = "#Response Times: " + "<Receive image>: " + cameraTime + "|| <Cloud vision>: " + cloudTime + "|| <Classification>: " + classTime
                       + "|| <sum>: " + sum;
        return text;
    }

    public static string GetResultLogs(string title ,string results)
    {
        string text = string.Format("#{0}: ", title) + results;
        return text;
    }
}
