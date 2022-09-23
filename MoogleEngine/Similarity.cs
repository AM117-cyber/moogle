using System.Text.RegularExpressions;
using System.Text;
public class Similarity

{


    public static Dictionary<Document, float> Similarity_Threshold(string query, Dictionary<string, TermData> Pquery)
    {

        Dictionary<Document, float> Coincidences = new Dictionary<Document, float>();
        //|\^|\*+

        var matchesToAvoid = Regex.Matches(query, @"!\w+");
        var matchesNeeded = Regex.Matches(query, @"\^\w+");
        var matchesImportance = Regex.Matches(query, @"\*+\w+");

        foreach (Match match in matchesImportance)
        {
            var key = match.Value.Replace("*", ""); 
            var count = match.Value.Length - key.Length;
            for (int i = 1; i <= count; i++)
            {
                Pquery[Regex.Replace(key.ToLower().Normalize(NormalizationForm.FormD), @"[^a-zA-Z0-9 ]+", "")].TF++;
            }
        }
        DocumentProcessor.Get_TF_IDF(DocumentProcessor.IDF, Pquery);
        double queryNorm = VectorNorm(Pquery);

        foreach (var doc in DocumentProcessor.docs)
        {
            double norm = VectorNorm(doc.Content) * queryNorm;

            float cos_similarity = -1;

            foreach (Match match in matchesToAvoid)
            {
                string term = Regex.Replace(match.Value.ToLower().Normalize(NormalizationForm.FormD), @"[^a-zA-Z0-9 ]+", "");
                Pquery.Remove(term);
                if (doc.Content.ContainsKey(term))
                {
                    cos_similarity = 0;
                }
            }
            
            foreach (Match match in matchesNeeded)
            {   string matchModified = Regex.Replace(match.Value.ToLower().Normalize(NormalizationForm.FormD), @"[^a-zA-Z0-9 ]+", "");
                if (!doc.Content.ContainsKey(matchModified)) 
                { 
                    cos_similarity = 0;
                }
            }

            if (cos_similarity == 0)
            {
                continue;
            }

            var closeness = ClosenessOperatorCheck(query, doc, norm);


            cos_similarity = (float)(Cos_Similarity(doc, Pquery, norm) + closeness);
            if (cos_similarity > 0.02)
                Coincidences[doc] = cos_similarity;
        }
        return Coincidences;
    }
    public static double VectorNorm(Dictionary<string, TermData> vector)
    {
        double sum = 0;
        foreach (var value in vector.Values)
        {
            sum += Math.Pow(value.TF_IDF, 2);
        }
        return Math.Sqrt(sum);
    }


    public static double Cos_Similarity(Document document, Dictionary<string, TermData> Pquery, double norm)
    {
        double vector_mult = 0;
        foreach (var key in Pquery.Keys)
        {
            if (document.Content.ContainsKey(key))
            {

                vector_mult += document.Content[key].TF_IDF * Pquery[key].TF_IDF;


            }
        }
        double cos_similarity = vector_mult / norm;//sacar Vector_Norm de docs en un inicio.
        return cos_similarity;
    }

    public static double ClosenessOperatorCheck(string query, Document document, double norm)
    {
        
        string pattern1 = @"\w+ *~";
        string pattern2 = @"~ *\w+";

        var matches1 = Regex.Matches(query, pattern1);
        List<string> fixedMatches1 = FixMatches(matches1);
        var matches2 = Regex.Matches(query, pattern2);
        List<string> fixedMatches2 = FixMatches(matches2);

        if (fixedMatches1.Count != fixedMatches2.Count)
        {
            throw new Exception("matches are wrong! Check this!!");
        }
        var distance = 0;
        for (int i = 0; i < fixedMatches1.Count; i++)
        {
            if (document.Content.ContainsKey(fixedMatches1[i]) && document.Content.ContainsKey(fixedMatches2[i]) && (fixedMatches1[i] != fixedMatches2[i]))
            {
                distance += GetShortestDistance(fixedMatches1[i], fixedMatches2[i], document);
            }
        }
        
        return distance == 0 ? distance : 1d / (distance * norm);
    }
    public static List<string> FixMatches(MatchCollection matches)
    {
      List<string> result = new List<string>();
        foreach (Match match in matches)
        {
          
           result.Add(match.Value.Replace("~", "").Replace(" ", ""));

        }
        return result;
    }

    public static int GetShortestDistance(string match1, string match2, Document document)
    {
        List<int> indexesOfMatch1 = document.Content[match1].Indexes;
        List<int> indexesOfMatch2 = document.Content[match2].Indexes;
        int minDistance = int.MaxValue;
        for (int i = 0; i < indexesOfMatch1.Count; i++)
        {
            for (int j = 0; j < indexesOfMatch2.Count; j++)
            {
                int currentDistance = Math.Abs(indexesOfMatch1[i] - indexesOfMatch2[j]);
                if (minDistance > currentDistance)
                {
                    minDistance = currentDistance;
                }
            }
        }
        return minDistance;
    }



    /* public static float[] OrderScores(Dictionary<Document, float> DictOfScores)
     {
       int length = DictOfScores.Count;
       float[] scores = new float[length];
       int i = 0;
       foreach (var key in DictOfScores.Keys)
       { 
         scores[i] = DictOfScores[key];
         i++;
       }
       Array.Sort(scores);
       return scores;
     }*/
    //or return array

    public static List<string> WordsForSuggestion(Dictionary<string, TermData>.KeyCollection Pquery)
    {
        List<string> wordsnotfound = new List<string>();

        foreach (var word in Pquery)
        {
            if (!WordExists(word))
            {
                wordsnotfound.Add(word);
            }
        }
        return wordsnotfound;
    }
    public static bool WordExists(string queryword)
    {
        bool WordExists = false;
        if (DocumentProcessor.IDF.ContainsKey(queryword))
        {
            WordExists = true;

        }
        return WordExists;

    }
    public static (string query, string suggested) GetSuggestions(List<string> WordsForSuggestion)
    {
        (string, string) suggestion = (string.Empty, string.Empty);
        int similarity = 0;
        foreach (var docword in DocumentProcessor.IDF.Keys)
        {
            foreach (var word in WordsForSuggestion)
            {
                int s = SimilarityBetweenWords(word, docword);
                if (s > similarity)
                {
                    similarity = s;
                    suggestion = (word, docword);
                }
            }
        }
        return suggestion;
    }


    public static int SimilarityBetweenWords(string queryword, string docword)
    {
        int x = Math.Min(queryword.Length, docword.Length);
        int y = Math.Max(queryword.Length, docword.Length);
        string longestword = queryword;
        string shortestword = docword;
        if (queryword.Length != y)
        {
            longestword = docword;
            shortestword = queryword;
        }


        int[,] matrix = new int[x + 1, y + 1];
        for (int i = 0; i < x + 1; i++)
        {
            matrix[i, 0] = i;
        }
        for (int i = 0; i < y + 1; i++)
        {
            matrix[0, i] = i;
        }

        for (int i = 1; i < x + 1; i++)
        {
            for (int j = 1; j < y + 1; j++)
            {
                int changes = 0;
                if (longestword[j - 1] != shortestword[i - 1])
                {
                    changes++;
                }
                int minimun = Math.Min(matrix[i - 1, j - 1] + changes, Math.Min(matrix[i, j - 1] + 1, matrix[i - 1, j] + 1));

                matrix[i, j] = minimun + changes;
            }

        }
        int similarity = queryword.Length - matrix[x, y];
        return similarity;


    }
}


