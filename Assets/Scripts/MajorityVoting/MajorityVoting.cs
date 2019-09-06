﻿using System.Collections;
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
using System.Threading.Tasks;
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
    static List<HashSet<string>> descSplitted = new List<HashSet<string>>();

    public MasoutisItem masoutis_item;
    string[] masoutisFiles = { "masoutis_cat2", "masoutis_cat3", "masoutis_cat4", "masoutis_desc" };


    List<string> wordsOCR = new List<string>();
    List<int> sel_k = new List<int>();
    List<int> cnt_found = new List<int>();
    public bool database_ready = false;

    // empty constructor
    public MajorityVoting()
    {
        //masoutis_item = new MasoutisItem();
    }

    public IEnumerator PerformMajorityVoting(List<string> wordsOCR)
    {
        Debug.Log("OCR: " + Time.realtimeSinceStartup);
        ApplicationView.wordsText.text = string.Join(", ", wordsOCR.ToArray());

        // read database to class properties
        LoadDatabaseFiles(masoutisFiles);
        while (!database_ready)
        {
            yield return null;
        }

        ApplicationView.MajorityValidText.text = string.Join(", ", wordsOCR.ToArray());

        // keep only elements with lenght >= 3
        wordsOCR = KeepElementsWithLen(wordsOCR, 3);
        Debug.Log("get len words: " + Time.realtimeSinceStartup);
        // remove greek accent and make all uppercase
        wordsOCR = RemoveGreekAccentSequential(wordsOCR);
        Debug.Log("Remove tones words: " + Time.realtimeSinceStartup);
        //Get valid words from db
        wordsOCR = GetValidWordsFromDb(wordsOCR, descSplitted);
        Debug.Log("get valid words: " + Time.realtimeSinceStartup);
        //wordsOCR.ForEach(Debug.Log);

        Debug.Log("load db: " + Time.realtimeSinceStartup);


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
                //cropped_desc.Add(desc[sel_k[j]]);
            }
        }
        Debug.Log("cropped categories: " + Time.realtimeSinceStartup);

        // keep only distinct elemets in each category
        List<string> cropped_cat2_unq = cropped_cat2.Distinct().ToList();
        List<string> cropped_cat3_unq = cropped_cat3.Distinct().ToList();
        List<string> cropped_cat4_unq = cropped_cat4.Distinct().ToList();
        //List<string> cropped_desc_unq = cropped_desc.Distinct().ToList();
        Debug.Log("distinct cropped categories: " + Time.realtimeSinceStartup);

        // get number of occurancies of each element in every category
        List<int> count_cat2 = GetCategoryCount(cropped_cat2, cropped_cat2_unq);
        List<int> count_cat3 = GetCategoryCount(cropped_cat3, cropped_cat3_unq);
        List<int> count_cat4 = GetCategoryCount(cropped_cat4, cropped_cat4_unq);
        //List<int> count_desc = GetCategoryCount(cropped_desc, cropped_desc_unq);
        Debug.Log("Category count " + Time.realtimeSinceStartup);

        masoutis_item = new MasoutisItem();

        try
        {
            masoutis_item.category_2 = cropped_cat2_unq[count_cat2.IndexOf(count_cat2.Max())];
            masoutis_item.category_3 = cropped_cat3_unq[count_cat3.IndexOf(count_cat3.Max())];
            masoutis_item.category_4 = cropped_cat4_unq[count_cat4.IndexOf(count_cat4.Max())];
            Debug.Log(masoutis_item.category_2);
            Debug.Log(masoutis_item.category_3);
            Debug.Log(masoutis_item.category_4);
            ApplicationView.MajorityFinalText.text = " Διάδρομος: " + masoutis_item.category_2 + "| Ράφι: " + masoutis_item.category_3 + " |Ράφι2: " + masoutis_item.category_4;


        }
        catch (System.Exception)
        {
            Debug.LogError("Problem in category index");
            masoutis_item.category_2 = "μη αναγνωρίσιμο";
            masoutis_item.category_3 = "μη αναγνωρίσιμο";
            masoutis_item.category_4 = "μη αναγνωρίσιμο";
            ApplicationView.MajorityFinalText.text = "Διάδρομος: " + masoutis_item.category_2 + "| Ράφι: " + masoutis_item.category_3 + " |Ράφι2: " + masoutis_item.category_4;
        }

    }


    public async void LoadDatabaseFiles(string[] files)
    {
        if (desc.Count == 0 && cat2.Count == 0 & cat3.Count == 0
            && cat4.Count == 0 && cat1.Count == 0)
        {
            //Debug.Log("reading database: " + Time.realtimeSinceStartup);
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
                            case "masoutis_cat2":
                                cat2.Add(inp_ln);
                                break;
                            case "masoutis_cat3":
                                cat3.Add(inp_ln);
                                break;
                            case "masoutis_cat4":
                                cat4.Add(inp_ln);
                                break;
                            case "masoutis_desc":
                                desc.Add(inp_ln);
                                break;
                        }
                    }
                    await new WaitForUpdate();
                }
            }
            //Debug.Log("files read: " + Time.realtimeSinceStartup);
            for (int i = 0; i < desc.Count; i++)
            {
                string[] splitted = desc[i].Split(' ');
                HashSet<string> temp_hash = new HashSet<string>(splitted);
                descSplitted.Add(temp_hash);
            }
        }
        database_ready = true;
    }

    public async void ReadDatabaseFile(string name)
    {
        if (desc.Count == 0 && cat2.Count == 0 & cat3.Count == 0
            && cat4.Count == 0 && cat1.Count == 0)
        {
            TextAsset database = Resources.Load<TextAsset>(name);
            using (var streamReader = new StreamReader(new MemoryStream(database.bytes)))
            {
                using (var csv = new CsvReader(streamReader))
                {
                    await new WaitForBackgroundThread();
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
                        cat1.Add(cat1Field);
                        cat2.Add(cat2Field);
                        cat3.Add(cat3Field);
                        cat4.Add(cat4Field);
                    }

                    await new WaitForUpdate();
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

    private void LoadDataToDict(List<string> desc)
    {
        //add every splitted element to list
        List<string> descSplitted = new List<string>();
        for (int i = 0; i < desc.Count; i++)
        {
            string[] splitted = desc[i].Split(' ');
            desc.AddRange(splitted);
        }

        Dictionary<string, int[]> dict = new Dictionary<string, int[]>();
        for (int i = 0; i < descSplitted.Count; i++)
        {
            // get all indexes of the element (desc[i])
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
                    cnt += 1;
                    sel_k.Add(i);
                    validWords.Add(word);
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

    private List<string> KeepElementsWithLen(List<string> words, int len)
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


}
