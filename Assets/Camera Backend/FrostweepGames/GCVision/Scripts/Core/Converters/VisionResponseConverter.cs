using UnityEngine;


namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public static class VisionResponseConverter
    {
        public static Color32 ConvertToUnityColor(Color color)
        {
            Color32 unityColor = new Color32((byte)color.red,
                                             (byte)color.green,
                                             (byte)color.blue,
                                             (byte)(255 - color.alpha));
            return unityColor;
        }

        public static Vector3 ConvertToUnityVector3(Position position)
        {
            Vector3 vector3 = new Vector3((float)position.x, (float)position.y, (float)position.z);

            return vector3;
        }

        public static Vector2 ConvertToUnityVector2(Vertex vertex)
        {
            Vector2 vector2 = new Vector2((float)vertex.x, (float)vertex.y);

            return vector2;
        }

    }
}