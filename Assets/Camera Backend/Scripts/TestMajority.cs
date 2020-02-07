using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TestMajority : MonoBehaviour
{
    int count;
    public List<string> words;
    public bool product;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnButtonClick()
    {
        StartCoroutine(TestMajorityVoring());
    }

    public IEnumerator TestMajorityVoring()
    {
        MajorityVoting maj = new MajorityVoting();
        if (!product)
        {
            yield return maj.PerformMajorityVoting(words);
            Debug.Log(maj.masoutis_item.category_2 + "," + maj.masoutis_item.category_3 + ", " + maj.masoutis_item.category_4);
        }
        else
        {
            Debug.Log(MajorityVoting.GetProductDesciption(words));
        }

    }
}
