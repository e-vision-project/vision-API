using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public static class Utils
{
    public static string PerformMajorityVoting(List<string> OCR_List)
    {
        MajorityVoting maj = new MajorityVoting();
        string product = maj.PerformMajorityVoting(OCR_List);
        return product;
    }

    public static List<string> SplitStringToList(string text)
    {
        List<string> l = text.Split(new Char[] { ' ', '\n' }).ToList();
        return l;
    }

    public static Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }
}
