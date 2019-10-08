using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using System.Text;
using UnityAsync;
using EVISION.Camera.plugin;

[Serializable]
public struct MasoutisItem
{
    public string category_2;
    public string category_3;
    public string category_4;
    public string product_description;
}

[Serializable]
public class MajorityVoting : AsyncBehaviour
{
    public static List<string> desc = new List<string>(115180);
    public static List<string> cat1 = new List<string>(115180);
    public static List<string> cat2 = new List<string>(115180);
    public static List<string> cat3 = new List<string>(115180);
    public static List<string> cat4 = new List<string>(115180);

    private static readonly Dictionary<string, List<int>> dictOfIdx = new Dictionary<string, List<int>>();
    static List<HashSet<string>> descSplitted = new List<HashSet<string>>();

    public MasoutisItem masoutis_item;
    //string[] masoutisFiles = { "masoutis_cat2", "masoutis_cat3", "masoutis_cat4", "masoutis_desc" };
    string[] masoutisFiles = { "masoutis_cat2_cleaned", "masoutis_cat3_cleaned", "masoutis_cat4_cleaned", "masoutis_desc_cleaned" };


    List<string> wordsOCR = new List<string>();
    List<int> sel_k = new List<int>();
    List<int> cnt_found = new List<int>();
    public bool database_ready = false;

    // empty constructor
    public MajorityVoting()
    {

    }

    public IEnumerator PerformMajorityVoting(List<string> wordsOCR)
    {

        // read database to class properties
        LoadDatabaseFiles(masoutisFiles);
        while (!database_ready)
        {
            yield return null;
        }

        // OCR words sanitization
        wordsOCR = KeepElementsWithLen(wordsOCR, 4);
        wordsOCR = RemoveGreekAccentSequential(wordsOCR);
        wordsOCR = RemoveHotWords(wordsOCR);
        // Get desc index with the most votes
        var maxDescIndex = FindMaxVotingIndex(wordsOCR);

        masoutis_item = new MasoutisItem();


        if (maxDescIndex == -1)
        {
            HandlingUnlocatedIndex();
            yield return null;
        }


        try
        {
            masoutis_item.category_2 = cat2[maxDescIndex];
            masoutis_item.category_3 = cat3[maxDescIndex];
            masoutis_item.category_4 = cat4[maxDescIndex];
            masoutis_item.product_description = desc[maxDescIndex];
            if (ApplicationView.MajorityFinalText != null)
            {
                ApplicationView.MajorityFinalText.text = "  Διάδρομος: " + masoutis_item.category_2 + "\n  Ράφι: " + masoutis_item.category_3 + "\n  Ράφι 2: " + masoutis_item.category_4 + "\n  Προϊόν: " + 
                    masoutis_item.product_description;
            }
        }
        catch (System.Exception)
        {
            HandlingUnlocatedIndex();
        }

    }

    private void HandlingUnlocatedIndex()
    {
        Debug.LogError("Problem in category index");
        masoutis_item.category_2 = "μη αναγνωρίσιμο";
        masoutis_item.category_3 = "μη αναγνωρίσιμο";
        masoutis_item.category_4 = "μη αναγνωρίσιμο";
        if (ApplicationView.MajorityFinalText != null)
        {
            ApplicationView.MajorityFinalText.text = "ΠΛΗΡΟΦΟΡΙΕΣ: Διάδρομος: " + masoutis_item.category_2 + "| Ράφι: " + masoutis_item.category_3 + " |Προϊόν: " + masoutis_item.category_4;
        }
    }

    private void FindMaxIndex_OBSOLETE(List<string> wordsOCR)
    {
        wordsOCR = GetValidWordsFromDb(wordsOCR, descSplitted);
        //wordsOCR.ForEach(Debug.Log);
        //ApplicationView.MajorityValidText.text = string.Join(", ", wordsOCR.ToArray());



        //Get all products from the db that contain the valid words.
        List<string> cropped_cat2 = new List<string>(), cropped_cat3 = new List<string>();
        List<string> cropped_cat4 = new List<string>(), cropped_desc = new List<string>();

        // iterate for all unique valid words
        for (int i = 0; i < cnt_found.Count; i++)
        {
            // occurences of the valid word
            for (int j = 0; j < cnt_found[i]; j++)
            {
                // sel_k gives the index of the valid word.
                cropped_cat2.Add(cat2[sel_k[j]]);
                cropped_cat3.Add(cat3[sel_k[j]]);
                cropped_cat4.Add(cat4[sel_k[j]]);
                //cropped_desc.Add(desc[sel_k[j]]);
            }
        }

        // keep only distinct elemets in each category
        List<string> cropped_cat2_unq = cropped_cat2.Distinct().ToList();
        List<string> cropped_cat3_unq = cropped_cat3.Distinct().ToList();
        List<string> cropped_cat4_unq = cropped_cat4.Distinct().ToList();
        List<string> cropped_desc_unq = cropped_desc.Distinct().ToList();


        // get number of occurancies of each element in every category
        List<int> count_cat2 = GetCategoryCount(cropped_cat2);
        List<int> count_cat3 = GetCategoryCount(cropped_cat3);
        List<int> count_cat4 = GetCategoryCount(cropped_cat4);
        //Debug.Log("category count end :" + Time.realtimeSinceStartup);

        masoutis_item = new MasoutisItem();

        try
        {
            masoutis_item.category_2 = cropped_cat2_unq[count_cat2.IndexOf(count_cat2.Max())];
            masoutis_item.category_3 = cropped_cat3_unq[count_cat3.IndexOf(count_cat3.Max())];
            masoutis_item.category_4 = cropped_cat4_unq[count_cat4.IndexOf(count_cat4.Max())];
            //ApplicationView.MajorityFinalText.text = " Διάδρομος: " + masoutis_item.category_2 + "\n Ράφι: " + masoutis_item.category_3 + "\n Ράφι2: " + masoutis_item.category_4;


        }
        catch (System.Exception)
        {
            Debug.LogError("Problem in category index");
            masoutis_item.category_2 = "μη αναγνωρίσιμο";
            masoutis_item.category_3 = "μη αναγνωρίσιμο";
            masoutis_item.category_4 = "μη αναγνωρίσιμο";
            //ApplicationView.MajorityFinalText.text = "Διάδρομος: " + masoutis_item.category_2 + "| Ράφι: " + masoutis_item.category_3 + " |Ράφι2: " + masoutis_item.category_4;
        }
    }

    private static int FindMaxVotingIndex(List<string> wordsOCR)
    {

        //description index to count of index
        var counter = new Dictionary<int, int>();
        var validWords = new List<string>(3);

        foreach (var foundTerm in wordsOCR.Where(s => dictOfIdx.ContainsKey(s)))
        {
            foreach (var idx in dictOfIdx[foundTerm])
            {
                if (!counter.ContainsKey(idx))
                {
                    counter[idx] = 1;
                }
                else
                {
                    counter[idx]++;
                }
            }
            validWords.Add(foundTerm);
        }

        var x = validWords.Distinct().ToList();
        //x.ForEach(Debug.Log);

        if (ApplicationView.MajorityValidText != null)
        {
            ApplicationView.MajorityValidText.text = string.Join(", ", validWords.Distinct().ToList().ToArray());
        }

        try
        {
            var mx = counter.Values.Max();
            var result = counter.FirstOrDefault(kvp => kvp.Value == mx).Key;
            return result;
        }
        catch (Exception)
        {
            Debug.LogError("Problem in locating max category");
            return -1;
        }
        
    }

    public async void LoadDatabaseFiles(string[] files)
    {
        if (desc.Count == 0 && cat2.Count == 0 & cat3.Count == 0
            && cat4.Count == 0 && cat1.Count == 0)
        {
            foreach (var file in files)
            {
                TextAsset datafile = Resources.Load<TextAsset>(file);
                string datafile_name = datafile.name;
                using (var streamReader = new StreamReader(new MemoryStream(datafile.bytes)))
                {
                    await new WaitForBackgroundThread();
                    while (!streamReader.EndOfStream)
                    {
                        string inp_ln = streamReader.ReadLine();
                        switch (datafile_name)
                        {
                            case "masoutis_cat2_cleaned":
                                cat2.Add(inp_ln);
                                break;
                            case "masoutis_cat3_cleaned":
                                cat3.Add(inp_ln);
                                break;
                            case "masoutis_cat4_cleaned":
                                cat4.Add(inp_ln);
                                break;
                            case "masoutis_desc_cleaned":
                                desc.Add(inp_ln);
                                break;
                        }
                    }
                    await new WaitForUpdate();
                }
            }

            // split desc elements and add to dictinonary (splitted word to unsplitted parent index).
            for (int i = 0; i < desc.Count; i++)
            {
                string[] splitted = desc[i].Split(' ');
                HashSet<string> temp_hash = new HashSet<string>(splitted);
                descSplitted.Add(temp_hash);

                List<int> ls;

                foreach (var it in temp_hash)
                {
                    if (!dictOfIdx.TryGetValue(it, out ls))
                    {
                        ls = new List<int>();
                        dictOfIdx[it] = ls;
                    }
                    ls.Add(i);
                }
            }
        }
        database_ready = true;
    }

    public void ReadDatabaseFile(string name)
    {
        if (desc.Count == 0 && cat2.Count == 0 & cat3.Count == 0
            && cat4.Count == 0 && cat1.Count == 0)
        {
            Debug.Log("reading");
            TextAsset database = Resources.Load<TextAsset>(name);
            using (var streamReader = new StreamReader(new MemoryStream(database.bytes)))
            {
                using (var csv = new CsvReader(streamReader))
                {
                    //await new WaitForBackgroundThread();
                    // ENTER THE NEW THREAD
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        string descField = csv.GetField<string>("Description");
                        string cat1Field = csv.GetField<string>("Unnamed: 1");
                        string cat2Field = csv.GetField<string>("Unnamed: 3");
                        string cat3Field = csv.GetField<string>("Unnamed: 5");
                        string cat4Field = csv.GetField<string>("Unnamed: 7");
                        desc.Add(descField);
                        //cat1.Add(cat1Field);
                        cat2.Add(cat2Field);
                        cat3.Add(cat3Field);
                        cat4.Add(cat4Field);
                    }

                    //await new WaitForUpdate();
                    // RETURN TO MAIN UNITY THREAD
                }
            }
            for (int i = 0; i < desc.Count; i++)
            {
                string[] splitted = desc[i].Split(' ');
                HashSet<string> temp_hash = new HashSet<string>(splitted);
                descSplitted.Add(temp_hash);
            }
            database_ready = true;
        }
        else
        {
            database_ready = true;
            return;
        }
    }

    private Dictionary<string, int[]> LoadDataToDict(List<string> desc)
    {
        //add every splitted element to list
        List<string> descSplit = new List<string>();
        for (int i = 0; i < desc.Count; i++)
        {
            string[] splitted = desc[i].Split(' ');
            descSplit.AddRange(splitted);
        }

        // create dictionary from descriptions
        var output = desc.Select((x, i) => new { Value = x, Index = i })
                  .GroupBy(x => x.Value)
                  .ToDictionary(x => x.Key, x => x.Select(y => y.Index)
                                                  .ToArray());

        return output;
    }


    /// <summary>
    /// Group categories based on number of occurences and return the count of each group.
    /// </summary>
    private List<int> GetCategoryCount(List<string> cat)
    {
        return cat.GroupBy(x => x).Select(g => g.Count()).ToList();
    }

    private List<int> GetCategoryCountParallel(List<string> cat, List<string> cat_unq)
    {
        List<int> cat_count = new List<int>();

        string[] cat_arr = cat.ToArray();
        string[] cat_unq_arr = cat_unq.ToArray();

        for (int i = 0; i < cat_unq.Count; i++)
        {
            cat_count.Add(cat_arr.Where(x => x.Equals(cat_unq_arr[i])).Count());
        }

        return cat_count;
    }

    private List<string> GetValidWordsFromDbSequential(List<string> words, List<string> masoutisDesc)
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

    private List<string> GetValidWordsFromDb(List<string> words, List<HashSet<string>> masoutisDesc)
    {
        List<string> validWords = new List<string>();
        int cnt = 0;

        foreach (var word in words)
        {
            for (int i = 0; i < masoutisDesc.Count; i++)
            {
                if (masoutisDesc[i].Contains(word))
                {
                    cnt += 1; // Count of that desc id.
                    sel_k.Add(i); // add index of the desc in the db. Index corresponds also to the categories.
                    validWords.Add(word);
                }
            }
            if (cnt != 0)
            {
                cnt_found.Add(cnt); // cnt_found keeps the count of each word found in the db.
            }
        }

        //keep only distinct elements from the database.
        return validWords.Distinct().ToList();
    }

    private List<string> GetValidWordsFromDbParallel(List<string> words, List<HashSet<string>> masoutisDesc)
    {
        List<string> validWords = new List<string>();
        int cnt = 0;
        Parallel.ForEach(words, word =>
        {
            for (int i = 0; i < masoutisDesc.Count; i++)
            {
                if (masoutisDesc[i].Contains(word))
                {
                    cnt += 1; // Count of that desc id.
                    sel_k.Add(i); // add index of the desc in the db. Index corresponds also to the categories.
                    validWords.Add(word);
                }
            }
            if (cnt != 0)
            {
                cnt_found.Add(cnt); // cnt_found keeps the count of each word found in the db.
            }
        });

        //keep only distinct elements from the database.
        return validWords.Distinct().ToList();
    }

    private List<string> KeepElementsWithLen(List<string> words, int len)
    {
        // keep elements with length >= len
        var croppedList = (from word in words
                           where word.Length >= len
                           select word).ToList();
        return croppedList;
    }

    /* 
     * Also apart from the accent returns the list elements to upper case.
     */
    private List<string> RemoveGreekAccentSequential(List<string> words)
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

    // TODO Optimize with a hashset and txt to read words.
    private List<string> RemoveHotWords(List<string> wordsOCR)
    {
        var wordsSanitized = new List<string>();
        foreach (var word in wordsOCR)
        {
            if (word != "GRAND" && word != "ΔΩΡΟ" && word != "SUPER" && word != "ΠΑΙΧΝΙΔΙ" && word != "KIDS" && word != "HELLAS")
            {
                wordsSanitized.Add(word);
            }
        }

        return wordsSanitized;
    }

}




////Get valid words from db
//dictOfIndexes = LoadDataToDict(desc); 
//var croppedDict = GetValidWords(wordsOCR.Distinct().ToList());
//Dictionary<List<string>, int[]> dict = new Dictionary<List<string>, int[]>();
//foreach (var key in dictOfIndexes.Keys)
//{
//    List<string> descSplit = new List<string>();
//    var x = key.ToString();
//    string[] splitted = x.Split(' ');
//    descSplit.AddRange(splitted);
//    dict.Add(descSplit, dictOfIndexes[key.ToString()]);
//}


//var dict = desc.Select((s, i) => new { s, i }).ToDictionary(x => x.i, x => x.s);


//var dict = desc.Select((x, i) => new { Value = x, Index = i })
//          .GroupBy(x => x.Value)
//          .ToDictionary(x => x.Key, x => x.Select(y => y.Index)
//                                          .ToArray());

//Dictionary<string, List<int>> dict_2 = new Dictionary<string, List<int>>();

//foreach (var key in dict.Keys)
//{
//    string[] splitted = key.Split(' ');

//    foreach (string s in splitted)
//    {
//        if (dict_2.ContainsKey(s))
//        {
//            dict_2[s].AddRange(dict[key]);
//        }
//        else
//        {
//            dict_2.Add(s, dict[key].ToList());
//        }
//    }
//}