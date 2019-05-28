using System;
using UnityEngine;

namespace FrostweepGames.Plugins.GoogleCloud.Vision.Helpers
{
    public static class Texture2DDrawingExtensions
    {
        public static void DrawLine(this Texture2D texture, Vector2 start, Vector2 end, UnityEngine.Color color)
        {
            Line(texture, (int)start.x, (int)start.y, (int)end.x, (int)end.y, color);
        }

        private static void DrawPixel(this Texture2D texture, int x, int y, int width, int height, UnityEngine.Color color)
        {
            if (x < 0 || x > width || y < 0 || y > height)
                return;
            texture.SetPixel(x, TransformToLeftTop_y(y, height), color);
        }

        private static void Line(Texture2D texture, int x0, int y0, int x1, int y1, UnityEngine.Color color)
        {
            int width = texture.width;
            int height = texture.height;

            bool isSteep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (isSteep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);

            int error = deltax / 2;
            int ystep;
            int y = y0;

            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            for (int x = x0; x < x1; x++)
            {
                if (isSteep)
                    texture.DrawPixel(y, x, width, height, color);
                else
                    texture.DrawPixel(x, y, width, height, color);

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }
        }

        private static void Swap(ref int x, ref int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }

        private static int TransformToLeftTop_y(int y, int height)
        {
            return height - y;
        }

        private static int TransformToLeftTop_y(float y, int height)
        {
            return height - (int)y;
        }
    }
}