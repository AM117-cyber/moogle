public class Similarity
{


    public static Dictionary<Document, float> Similarity_Threshold(string query, Dictionary<string, TermData> Pquery)
    {

        double Norm_of_query = Vector_Norm(Pquery);
        Dictionary<Document, float> Coincidences = new Dictionary<Document, float>();

        foreach (var doc in DocumentProcessor.docs)
        {
            float cos_similarity = (float)Cos_Similarity(doc, Pquery, Norm_of_query);
            if (cos_similarity > 0.017)
                Coincidences[doc] = cos_similarity;
        }
        return Coincidences;
    }

    public static double Vector_Norm(Dictionary<string, TermData> vector)
    {
        double sum = 0;
        foreach (var value in vector.Values)
        {
            sum += Math.Pow(value.TF_IDF, 2);
        }
        return Math.Sqrt(sum);
    }


    public static double Cos_Similarity(Document document, Dictionary<string, TermData> Pquery, double Norm_of_query)
    {
        double vector_mult = 0;
        foreach (var key in Pquery.Keys)
        {
            if (document.Content.ContainsKey(key))
            {

                vector_mult += document.Content[key].TF_IDF * Pquery[key].TF_IDF;

            }
        }
        double cos_similarity = vector_mult / (Vector_Norm(document.Content) * Norm_of_query);//sacar Vector_Norm de docs en un inicio.
        return cos_similarity;
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
           int s = SimilarityBetweenWords(word,docword);
          if (s > similarity)
          {
            similarity = s;
            suggestion = (word,docword);
          } 
     }
    }
    return suggestion;
    }
        

    public static int SimilarityBetweenWords(string queryword, string docword)
    {
      int x = Math.Min(queryword.Length,docword.Length);
      int y = Math.Max(queryword.Length,docword.Length);
      string longestword = queryword;
      string shortestword = docword;
      if (queryword.Length != y)
      {
        longestword = docword;
        shortestword = queryword;
      }
      
      
      int[,] matrix = new int[x+1,y+1];  
      for (int i = 0; i < x; i++)
      {
        matrix[i,0] = i;
      }
      for (int i = 0; i < y; i++)
      {
        matrix[0,i] = i;
      }
        
      for (int i = 1; i < x; i++)
      { 
        for (int j = 1; j < y; j++)
        { int changes = 0;
          int minimun = Math.Min(matrix[i-1,j-1],Math.Min(matrix[i,j-1],matrix[i-1,j]));
           if (longestword[j-1] != shortestword[i])
          {
            changes++;
          }
          matrix[i,j] = minimun + changes;
        }
        
      }
       int similarity =  queryword.Length-matrix[x,y];
       return similarity;
      

    }
}


