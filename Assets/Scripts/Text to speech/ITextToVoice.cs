using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;

public interface ITextToVoice
{
    void PerformSpeechFromText(string text);
    void StopSpeech();
}
