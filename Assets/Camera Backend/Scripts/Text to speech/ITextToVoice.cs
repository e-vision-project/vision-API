using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;

public interface ITextToVoice
{
    IEnumerator PerformSpeechFromText(string text);
    void StopSpeech();
}
