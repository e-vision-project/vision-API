using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoScene : MonoBehaviour
{
    public List<string> scenes;

    public void GotoGCSpeechRecognition()
    {
      SceneManager.LoadScene(scenes[0]);
    }

    public void GotoGCVision()
    {
        SceneManager.LoadScene(scenes[1]);
    }

    public void GotoGCNaturalLanguage()
    {
        SceneManager.LoadScene(scenes[2]);
    }

    public void GotoGCTranslation()
    {
        SceneManager.LoadScene(scenes[3]);
    }

    public void GotoGCTTS()
    {
        SceneManager.LoadScene(scenes[4]);
    }

    public void GotoGCVI()
    {
        SceneManager.LoadScene(scenes[5]);
    }
}
