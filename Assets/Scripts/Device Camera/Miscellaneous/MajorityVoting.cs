using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
    static List<string> desc = new List<string>(115180);
    static List<string> cat1 = new List<string>(115180);
    static List<string> cat2 = new List<string>(115180);
    static List<string> cat3 = new List<string>(115180);
    static List<string> cat4 = new List<string>(115180);
    static List<string> brand = new List<string>(115180);

    List<string> wordsOCR = new List<string>();
    List<int> sel_k = new List<int>();
    List<int> cnt_found = new List<int>();

    // empty constructor
    public MajorityVoting()
    {

    }

    public string PerformMajorityVoting(List<string> wordsOCR)
    {
   
        // read database to class properties
        ReadDatabaseFile("masoutis_db");
        Debug.Log("read database: " + Time.realtimeSinceStartup);
        // keep only elements with lenght >= 3
        wordsOCR = KeepElementsWithLen(wordsOCR, 3);
        // remove greek accent and make all uppercase
        wordsOCR = RemoveGreekAccentParallel(wordsOCR);
        desc.Sort();
        Debug.Log("sort: " + Time.realtimeSinceStartup);
        //desc.ForEach(Debug.Log);

        // Get valid words from db
        //wordsOCR = GetValidWordsFromDbParallel(wordsOCR, desc);
        //Debug.Log("get valid words: " + Time.realtimeSinceStartup);
        //wordsOCR.ForEach(Debug.Log);

        return null;
        //Get all products from the db that contain the valid words.
        List<string> cropped_cat2 = new List<string>(), cropped_cat3 = new List<string>();
        List<string> cropped_cat4 = new List<string>(), cropped_desc = new List<string>();

        for (int i = 0; i < cnt_found.Count; i++)
        {
            for (int j = 0; j < cnt_found[i]; j++)
            {
                cropped_cat2.Add(cat2[sel_k[j]]);
                cropped_cat3.Add(cat3[sel_k[j]]);
                cropped_cat4.Add(cat4[sel_k[j]]);
                cropped_desc.Add(desc[sel_k[j]]);
            }
        }
        // keep only distinct elemets in each category
        List<string> cropped_cat2_unq = cropped_cat2.Distinct().ToList();
        List<string> cropped_cat3_unq = cropped_cat3.Distinct().ToList();
        List<string> cropped_cat4_unq = cropped_cat4.Distinct().ToList();
        List<string> cropped_desc_unq = cropped_desc.Distinct().ToList();
        Debug.Log(Time.realtimeSinceStartup);
        // get number of occurancies of each element in every category
        List<int> count_cat2 = GetCategoryCount(cropped_cat2, cropped_cat2_unq);
        List<int> count_cat3 = GetCategoryCount(cropped_cat3, cropped_cat3_unq);
        List<int> count_cat4 = GetCategoryCount(cropped_cat4, cropped_cat4_unq);
        List<int> count_desc = GetCategoryCount(cropped_desc, cropped_desc_unq);
        Debug.Log("End: " + Time.realtimeSinceStartup);
        try
        {
            string category_2 = cropped_cat2_unq[count_cat2.IndexOf(count_cat2.Max())];
            return category_2;
        }
        catch (System.Exception)
        {
            Debug.LogError("Problem in category 2 index");
            return "προϊόν μη αναγνωρίσιμο";
        }

        //string category_3 = cropped_cat3_unq [count_cat3.IndexOf(count_cat3.Max())];
        //string category_4 = cropped_cat4_unq [count_cat3.IndexOf(count_cat4.Max())];
        //string product = cropped_desc_unq [count_desc.IndexOf(count_desc.Max())];

    }

    private void ReadDatabaseFile (string name)
    {
        if(desc.Count == 0 && cat2.Count == 0 & cat3.Count == 0
            && cat4.Count == 0 && cat1.Count == 0)
        {
            TextAsset database = Resources.Load<TextAsset>(name);
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
        }
        else
        {
            Debug.Log("DB already loaded");
        }
    } 

    private List<int> GetCategoryCount(List<string> cat, List<string> cat_unq)
    {
        List<int> cat_count = new List<int>();
        for (int i = 0; i < cat_unq.Count; i++)
        {
            cat_count.Add(cat.Where(x => x.Equals(cat_unq[i])).Count());
        }

        return cat_count;
    }

    private List<string> GetValidWordsFromDbSequential (List<string> words, List<string> masoutisDesc)
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

    private List<string> GetValidWordsFromDbParallel (List<string> words, HashSet<string> masoutisDesc)
    {
        List<string> validWords = new List<string>();

        foreach (var word in words)
        {
            
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
    private List<string> RemoveGreekAccentSequential (List<string> words)
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

    /* 
     * Also apart from the accent returns the list elements to upper case.
     */
    private List<string> RemoveGreekAccentParallel(List<string> words)
    {
        List<string> wordsEdited = new List<string>();

        Parallel.ForEach(words, word =>
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
        });

        return wordsEdited;
    }


}
