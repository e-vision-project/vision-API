using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.VideoIntelligence.Helpers
{
    public static class InternalTools
    {
        public static void FixVerticalLayoutGroupFitting(Object value)
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

        public static bool IsValidVideoFile(string path)
        {
            if (path.EndsWith(".mp4") || path.EndsWith(".mkv") || path.EndsWith(".jpeg"))
                return true;

            return false;
        }
    }
}