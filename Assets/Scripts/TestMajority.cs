using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMajority : MonoBehaviour
{
    int count;
    // Start is called before the first frame update
    void Start()
    {
        MajorityVoting maj = new MajorityVoting();

        List<string> wordsOCR = new List<string>();
        //wordsOCR.Add("MOZZARELLA");
        //wordsOCR.Add("ΔώΔΩΝΗ");
        //wordsOCR.Add("Ψιτό");
        //wordsOCR.Add("Nά");
        wordsOCR.Add("Σοκολάτα");
        wordsOCR.Add("LACTA");
        //wordsOCR.Add("Κοτόπουλο");
        //wordsOCR.Add("Χοιρινό");
        //wordsOCR.Add("DUREX");
        //wordsOCR.Add("ΦήΛΕΤΟ");
        MasoutisItem str = maj.PerformMajorityVoting(wordsOCR);
        Debug.Log(Time.realtimeSinceStartup);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
