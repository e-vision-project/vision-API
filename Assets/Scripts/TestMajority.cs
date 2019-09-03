using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TestMajority : MonoBehaviour
{
    int count;
    public List<string> words;
    // Start is called before the first frame update
    void Start()
    {
        //wordsOCR.Add("MOZZARELLA");
        //wordsOCR.Add("ΔώΔΩΝΗ");
        //wordsOCR.Add("Ψhτό");
        //wordsOCR.Add("Nά");
        //wordsOCR.Add("Σοκολάτα");
        //wordsOCR.Add("LACTA");
        //wordsOCR.Add("Κοτόπουλο");
        //wordsOCR.Add("Χοιρινό");
        //wordsOCR.Add("DUREX");
        //wordsOCR.Add("ΦήΛΕΤΟ");
    }

    public void OnButtonClick()
    {
        StartCoroutine(TestMajorityVoring());
    }

    public IEnumerator TestMajorityVoring()
    {
        MajorityVoting maj = new MajorityVoting();
        yield return maj.PerformMajorityVoting(words);
    }
}
