using System.IO;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence
{
    public static class VideoConvert 
    {
        public static string ConvertFromPersistentDataPath(string path)
        {
            return Convert(Path.Combine(Application.persistentDataPath, path));
        }

        public static string ConvertFromStreamingAssets(string path)
        {
            return Convert(Path.Combine(Application.streamingAssetsPath, path));
        }

        public static string ConvertFromDataPath(string path)
        {
            return Convert(Path.Combine(Application.dataPath, path));
        }

        public static string Convert(string path)
        {
            if (File.Exists(path))
            {
                return System.Convert.ToBase64String(File.ReadAllBytes(path));
            }

            return string.Empty;
        }

        public static string Convert(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }
    }
}