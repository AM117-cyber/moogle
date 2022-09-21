using System.Linq;
using System.Text.RegularExpressions;

namespace MoogleEngine;


public static class Moogle
{
    private static bool initialized;


    public static SearchResult Query(string query)
    {
        var resultLength = 10;
        Dictionary<string, TermData> Pquery = DocumentProcessor.Process_Query(query);

        Dictionary<Document, float> Coincidences = Similarity.Similarity_Threshold(query, Pquery);

        var Results = Coincidences.OrderByDescending(kvp => kvp.Value).Take(resultLength);

        List<SearchItem> items = new List<SearchItem>();
        foreach (var result in Results)
        {
            items.Add(new SearchItem(result.Key.Title + " " + result.Value, GetSnippet(result.Key, Pquery), result.Value));
        }
        List<string> WordsForSuggestion = Similarity.WordsForSuggestion(Pquery.Keys);
        if (Coincidences.Count <= 3 && WordsForSuggestion.Count != 0)
        {
            var suggest = Similarity.GetSuggestions(WordsForSuggestion);

            return new SearchResult(items,query.Replace(suggest.query,suggest.suggested) );
        }
        else
        {
            return new SearchResult(items, string.Empty);
        }
    }
    public static string GetSnippet(Document document, Dictionary<string, TermData> Pquery)
    {
        string TermForSnippet = null;
        float highestTF_IDF = 0;
        var queryterms = Pquery.Keys;
        Dictionary<string, TermData> content = document.Content;
        foreach (var term in queryterms)
        {
            if (content.ContainsKey(term) && content[term].TF_IDF > highestTF_IDF)
            {
                highestTF_IDF = ((float)content[term].TF_IDF);
                TermForSnippet = term;
            }
        }
        string snippet = string.Empty;
        int index = content[TermForSnippet].Indexes[0];
        string text = File.ReadAllText(document.Path);
        int[] array = FindRange(text, index);
        int i = array[0];
        int j = array[1];

        for (int a = i; i <= j; i++)
        {

            snippet += text[i];

        }

        List<MatchCollection> matches = new List<MatchCollection>(Pquery.Count);

        foreach (var term in Pquery.Keys) {
          matches.Add(Regex.Matches(snippet, term, RegexOptions.IgnoreCase));
        }

        foreach (MatchCollection matchColl in matches) {
          foreach (Match match in matchColl) {
            snippet = snippet.Insert(match.Index, "<span style=\"background-color:yellow\">");
            snippet = snippet.Insert(match.Index + match.Length, "</span>");
          }
        }

        return snippet;

    }
    public static int[] FindRange(string text, int index)
    {
        int[] array = new int[2];
        if (index < 50)
        {
            index += 50;
        }

        int j = index + 100;
        if (text.Length < j)
        {
            j = text.Length - 1;
        }
        int i = index - 50;
        while (text[j] != ' ' && j != text.Length - 1)
        {
            j++;
        }

        while (text[i] != ' ' && i > 0)
        {
            i--;
        }
        array[0] = i;
        array[1] = j;
        return array;

    }
    public static void Initialize()
    {
        if (!initialized)
        {

            DocumentProcessor.Setting_each_TF_IDF(DocumentProcessor.Get_IDF());

        }
        initialized = true;
    }

}


