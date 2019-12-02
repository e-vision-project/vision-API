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
    public static string[] masoutisFiles = { "masoutis_cat2_cleaned", "masoutis_cat3_cleaned", "masoutis_cat4_cleaned", "masoutis_desc_cleaned" };
    //public static string[] masoutisFiles = { "masoutis_cat2", "masoutis_cat3", "masoutis_cat4", "masoutis_desc" };

    public static bool database_ready = false;

    // empty constructor
    public MajorityVoting()
    {

    }

    public IEnumerator PerformMajorityVoting(List<string> wordsOCR)
    {
        // read database to class properties
        //LoadDatabaseFiles(masoutisFiles);
        while (!database_ready)
        {
            yield return null;
        }

        // OCR words sanitization
        wordsOCR = OCRWordsSanitization(wordsOCR, 4);
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
                ApplicationView.MajorityFinalText.text = "  Διάδρομος: " + masoutis_item.category_2 + 
                    "\n  Ράφι: " + masoutis_item.category_3 + "\n  Κατηγορία Ραφιού: " + masoutis_item.category_4;
            }
        }
        catch (System.Exception)
        {
            HandlingUnlocatedIndex();
        }

    }

    private static List<string> OCRWordsSanitization(List<string> wordsOCR, int maxLen)
    {
        wordsOCR = KeepElementsWithLen(wordsOCR, maxLen);
        wordsOCR = RemoveGreekAccentSequential(wordsOCR);
        wordsOCR = RemoveHotWords(wordsOCR);
        return wordsOCR;
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

    public static string GetProductDesciption(List<string> wordsOCR)
    {
        var _validWords = new List<string>(3);
        var _sanitizedWords = OCRWordsSanitization(wordsOCR, 4);

        _validWords = GetDistinctValidWords(_validWords, _sanitizedWords);

        //description index to count of index
        var counter = new Dictionary<int, int>();

        foreach (string foundTerm in _validWords.Where(s => dictOfIdx.ContainsKey(s)))
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
        }

        try
        {
            // keep max (thresshold) indexes based on their count.
            var maxVals = counter.OrderByDescending(k => k.Value).Take(100).ToList();
            
            // sort valid words alphabetically.
            _validWords = SortAlphabetically(_validWords);
            // create string for product description based on SORTED valid words.
            string _OCRDesc = GenericUtils.ListToString(_validWords);

            if (ApplicationView.MajorityValidText != null)
            {
                ApplicationView.MajorityValidText.text = string.Join(", ", _validWords.Distinct().ToList().ToArray());
            }

            int score = 0;
            // key : index, value : score
            Dictionary<int, int> dictOfDescriptions = new Dictionary<int, int>();
            for (int i = 0; i < maxVals.Count; i++)
            {
                //sort string alphabetically.
                var db_desc = SortStringAlphabetically(desc[maxVals[i].Key]);

                score = LevenshteinDistance.Compute(db_desc, _OCRDesc);
                dictOfDescriptions.Add(maxVals[i].Key, score);
            }

            // FOR TESTING ONLY
            ApplicationView.distanceString = "";
            var ordered = dictOfDescriptions.OrderBy(x => x.Value).ToList();
            var ordered_distinct = ordered.Distinct().ToList();
            int b = 0;
            foreach (var item in ordered_distinct)
            {
                ApplicationView.distanceString += desc[item.Key] + ", " + "(MJ count): " + maxVals[b].Value.ToString() + ", " + "(DIST count): " + item.Value.ToString() + "\n";
                b++;
            }
            // END TESTING SECTION

            var min = dictOfDescriptions.Values.Min();
            var keyMin = dictOfDescriptions.FirstOrDefault(kvp => kvp.Value == min).Key;

            if (ApplicationView.MajorityFinalText != null)
            {
                ApplicationView.MajorityFinalText.text = "Προϊόν: " + desc[keyMin];
            }

            return desc[keyMin];

        }
        catch (Exception)
        {
            Debug.LogError("Problem in locating max category");
            return "Δεν αναγνωρίστηκε προϊόν";
        }
    }

    private static List<string> GetDistinctValidWords(List<string> _validWords, List<string> _sanitizedWords)
    {
        foreach (string foundTerm in _sanitizedWords.Where(s => dictOfIdx.ContainsKey(s)))
        {
            _validWords.Add(foundTerm);
        }

        _validWords = _validWords.Distinct().ToList();
        return _validWords;
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

    public static async void LoadDatabaseFiles(string[] files)
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


    
    public static List<string> KeepElementsWithLen(List<string> words, int len)
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
    private static List<string> RemoveGreekAccentSequential(List<string> words)
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
    private static List<string> RemoveHotWords(List<string> wordsOCR)
    {
        var wordsSanitized = new List<string>();
        foreach (var word in wordsOCR)
        {
            if ( word != "ΔΩΡΟ" && word != "SUPER" && word != "ΠΑΙΧΝΙΔΙ" && word != "KIDS" && word != "HELLAS" && word != " " && word != "ΠΡΟΣΦΟΡΑ" && word != "ΔΩΡA")
            {
                wordsSanitized.Add(word);
            }
        }

        return wordsSanitized;
    }

    private static List<string> SortAlphabetically(List<string> sentence) 
    {
        var sorted = (from word in sentence
                     orderby word
                     select word).ToList();

        return sorted;
    }

    private static string SortStringAlphabetically(string desc)
    {
        var listOfDesc = GenericUtils.SplitStringToList(desc);

        var sorted = (from word in listOfDesc
                      orderby word
                      select word).ToList();

        return GenericUtils.ListToString(sorted);
    }

}