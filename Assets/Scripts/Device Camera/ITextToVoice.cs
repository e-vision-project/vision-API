using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITextToVoice
{
    void PerformSpeechFromText(string text);
    void StopSpeech();
}
