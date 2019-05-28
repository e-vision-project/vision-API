using System.IO;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public static class ImageConvert
    {
        public static string Convert(Texture2D texture)
        {
            return System.Convert.ToBase64String(texture.EncodeToPNG());
        }
        public static Texture2D GetTextureFromPath(string path)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);

            texture.LoadImage(File.ReadAllBytes(path));
            texture.Apply();

            return texture;
        }
    }
}