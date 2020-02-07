using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.Vision.Helpers
{
    public static class InternalTools
    {
        public static void FixVerticalLayoutGroupFitting(UnityEngine.Object value)
        {
            VerticalLayoutGroup group = null;

            if (value is VerticalLayoutGroup)
                group = value as VerticalLayoutGroup;
            else if (value is GameObject)
                group = (value as GameObject).GetComponent<VerticalLayoutGroup>();
            else if (value is Transform)
                group = (value as Transform).GetComponent<VerticalLayoutGroup>();


            if (group == null)
                return;

            group.enabled = false;
            Canvas.ForceUpdateCanvases();
            group.SetLayoutVertical();
            group.CalculateLayoutInputVertical();
            group.enabled = true;
        }

        public static bool IsValidImageFile(string path)
        {
            if (path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".jpeg"))
                return true;

            return false;
        }


        public static void ProcessImage(Vertex[] vertices, ref Texture2D texture, UnityEngine.Color color)
        {
            int pointer1 = 0,
                pointer2 = 0;

            Vector2 start = Vector2.zero,
                    end = Vector2.zero;

            for (int i = 0; i < vertices.Length; i++)
            {
                pointer1 = i;
                pointer2 = i + 1 >= vertices.Length ? 0 : i + 1;

                start.x = (int)vertices[pointer1].x;
                start.y = (int)vertices[pointer1].y;

                end.x = (int)vertices[pointer2].x;
                end.y = (int)vertices[pointer2].y;

                texture.DrawLine(start, end, color);
            }

            texture.Apply();
        }

        public static void ProcessImage(NormalizedVertex[] normalizedVertices, ref Texture2D texture, UnityEngine.Color color)
        {
            Vertex[] vertices = new Vertex[normalizedVertices.Length];

            for (int i = 0; i < normalizedVertices.Length; i++)
            {
                vertices[i] = new Vertex()
                {
                    x = normalizedVertices[i].x * texture.width,
                    y = normalizedVertices[i].y * texture.height
                };
            }

            ProcessImage(vertices, ref texture, color);
        }
    }
}