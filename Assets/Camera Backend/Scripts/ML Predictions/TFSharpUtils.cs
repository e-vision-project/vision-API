using System.IO;
using UnityEngine;
using TensorFlow;

public static class TFSharpUtils
{
    public static TFTensor TransformInput(Color32[] pic, int width, int height, float mean, float std)
    {
        float[] floatValues = new float[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (color.r - mean) / std;
            floatValues[i * 3 + 1] = (color.g - mean) / std;
            floatValues[i * 3 + 2] = (color.b - mean) / std;
        }

        TFShape shape = new TFShape(1, width, height, 3);

        return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
    }

    public static void SaveImage(Color32[] colors, int width, int height)
    {
        // save image
        Texture2D target = new Texture2D(width, height);
        target.SetPixels32(colors);
        target.Apply();
        var img = target.EncodeToJPG();
        File.WriteAllBytes(Application.dataPath + "/captureImage.jpg", img);
        Debug.Log("saved at: " + Application.dataPath);
    }
}