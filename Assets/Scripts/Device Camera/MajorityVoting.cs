using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using CsvHelper;
using System.Text;

//"Assets/masoutis_dataset_cleaned.csv"
// Assets/test.csv

public class MasoutisDataBase
{
    public string desc;
    public string cat1;
    public string cat2;
}

public class MajorityVoting
{
    List<string> desc = new List<string>();
    List<string> cat1 = new List<string>();
    List<string> cat2 = new List<string>();
    List<string> cat3 = new List<string>();
    List<string> cat4 = new List<string>();
    List<string> brand = new List<string>();

    List<string> wordsOCR = new List<string>();
    List<int> sel_k = new List<int>();
    List<int> cnt_found = new List<int>();


    // empty constructor
    public MajorityVoting()
    {

    }

    public void PerformMajorityVoting()
    {

        TextAsset database = Resources.Load<TextAsset>("masoutis_db");
        using (var streamReader = new StreamReader(new MemoryStream(database.bytes)))
        {
            using (var csv = new CsvReader(streamReader))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    string descField = csv.GetField<string>("Description");
                    string cat1Field = csv.GetField<string>("Unnamed: 1");
                    string cat2Field = csv.GetField<string>("Unnamed: 3");
                    string cat3Field = csv.GetField<string>("Unnamed: 5");
                    string cat4Field = csv.GetField<string>("Unnamed: 7");
                    string brandField = csv.GetField<string>("Brand Name");
                    desc.Add(descField);
                    cat1.Add(cat1Field);
                    cat2.Add(cat2Field);
                    cat3.Add(cat3Field);
                    cat4.Add(cat4Field);
                    brand.Add(brandField);
                }
            }
        }
        Debug.Log(cat2.Count);
        Debug.Log(cat3.Count);
        Debug.Log(cat4.Count);

        wordsOCR.Add("MOZZARELLA");
        wordsOCR.Add("ΔώΔΩΝΗ");
        //wordsOCR.Add("ΨΗΤΟ");
        //wordsOCR.Add("Nά");
        //wordsOCR.Add("ΚΟΤΟΠΟΥΛΟ");
        //wordsOCR.Add("ΦΙΛΕΤΟ");
        //wordsOCR.Add("ΦήΛΕΤΟ");

        // keep only elements with lenght >= 3
        wordsOCR = KeepElementsWithLen(wordsOCR, 3);
        // remove greek accent and make all uppercase
        wordsOCR = RemoveGreekAccent(wordsOCR);
        // Get valid words from db
        wordsOCR = GetValidWordsFromDb(wordsOCR, desc);
        //Get all products from the db that contain the valid words.
        


        //List<string> temp_cat2 = new List<string>(), temp_cat3 = new List<string>();
        //List<string> temp_cat4 = new List<string>(), temp_desc = new List<string>();

        //List<string> mycat2 = new List<string>(), mycat3 = new List<string>();
        //List<string> mycat4 = new List<string>(), mydesc = new List<string>();

        //int cnt = 0;
        //for (int i = 0; i < cnt_found.Count; i++)
        //{
        //    for (int j = 0; j < cnt_found[i]; j++)
        //    {
        //        temp_cat2.Add(cat2[sel_k[cnt]]);
        //        temp_cat3.Add(cat3[sel_k[cnt]]);
        //        temp_cat4.Add(cat4[sel_k[cnt]]);
        //        temp_desc.Add(desc[sel_k[cnt]]);
        //        cnt++;
        //    }
        //    mycat2.AddRange(temp_cat2.Distinct().ToList());
        //    mycat3.AddRange(temp_cat3.Distinct().ToList());
        //    mycat4.AddRange(temp_cat4.Distinct().ToList());
        //    mydesc.AddRange(temp_desc.Distinct().ToList());
        //}

        //mydesc.ForEach(Debug.Log);

    }

    private void ReadDatabaseFile(string name)
    {
        
    } 

    private List<string> GetValidWordsFromDb (List<string> words, List<string> masoutisDesc)
    {
        List<string> validWords = new List<string>();
        int cnt = 0;

        for (int i = 0; i < words.Count; i++)
        {
            for (int k = 0; k < masoutisDesc.Count; k++)
            {
                List<string> new_data = new List<string>();
                string[] splitted = masoutisDesc[k].Split(' ');
                for (int j = 0; j < splitted.Length; j++)
                {
                    new_data.Add(masoutisDesc[k].Split(' ')[j]);
                }
                if (new_data.Contains(words[i]))
                {
                    cnt += 1;
                    sel_k.Add(k);
                    validWords.Add(words[i]);
                }
            }
            if (cnt != 0)
            {
                cnt_found.Add(cnt);
            }
        }

        //keep only distinct elements from the database.
        return validWords.Distinct().ToList();
    }

    private List<string> KeepElementsWithLen (List<string> words, int len)
    {
        List<string> wordsEdited = new List<string>();

        // keep elements with length >= len
        var vnlist = from word in words
                     where word.Length > len
                     select word;

        wordsEdited.AddRange(vnlist);

        return wordsEdited;
    }

    /* 
     * Also apart from the accent returns the list elements to upper case.
     */
    private List<string> RemoveGreekAccent (List<string> words)
    {
        List<string> wordsEdited = new List<string>();

        foreach (var word in words)
        {
            string edited = new StringBuilder(word)
            .Replace('ά', 'α')
            .Replace('ί', 'ι')
            .Replace('ή', 'η')
            .Replace('ύ', 'υ')
            .Replace('ό', 'ο')
            .Replace('ώ', 'ω')
            .Replace('έ', 'ε')
            .ToString().ToUpper();
            wordsEdited.Add(edited);
        }

        return wordsEdited;
    }


}
