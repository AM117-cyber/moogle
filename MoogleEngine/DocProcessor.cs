using System.Text;
using System.Text.RegularExpressions;
public static class DocumentProcessor
{
    //the two dots get the path in which the folder to the project is located, you only specify the content folder.
    private static Dictionary<string, double>? IDFs = null;
    public static IEnumerable<Document> docs = ProcessFolder(Directory.GetFiles(@"..\Content")).ToList();
    //hasta aqui funciona pero el orden no es el mismo que el de la carpeta,aunque eso no debe importar.

    public static IEnumerable<Document> ProcessFolder(string[] path)
    {
        foreach (var indivpath in path)
        {
            string text = File.ReadAllText(indivpath);
            var Docs = new Document(indivpath, ProcessText(text));
            yield return Docs;
        }
    }

    public static Dictionary<string, TermData> ProcessText(string text)
    {
        var matches = Regex.Matches(text, @"\w+");//Match devuelve una coleccion.Averiguar si Trim() es necesario.
        Dictionary<string, TermData> terms_of_doc = new Dictionary<string, TermData>();
        foreach (Match word in matches)
        {
            string word_modified = Regex.Replace(word.Value.ToLower().Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
            if (terms_of_doc.ContainsKey(word_modified))
            {
                terms_of_doc[word_modified].AddIndex(word.Index);
                terms_of_doc[word_modified].IncreaseTF();
            }
            else
            {
                terms_of_doc[word_modified] = new TermData(word_modified, word.Index);
            }
        }
        return terms_of_doc;
    }


    public static Dictionary<string, double> Get_IDF()
    {
        if (IDFs is null) 
        {
            int N = Document.Total_of_Docs;
            IDFs = new Dictionary<string, double>();
            foreach (var item in docs)
            {
                foreach (var term in item.Content.Keys)
                {
                    if (IDFs.ContainsKey(term))
                    {
                        IDFs[term]++;
                    }
                    else
                    {
                        IDFs[term] = 1;
                    }
                }
            }
            foreach (var term in IDFs.Keys)
            {
                var idf = N / IDFs[term];
                IDFs[term] = Math.Log10(idf);
            }
        }
        
        return IDFs;
    }
public static Dictionary<string, double> IDF = Get_IDF();
    public static void Setting_each_TF_IDF(Dictionary<string, double> IDFs)
    {
        foreach (var doc in docs)
        { double highestTF = 0;
          foreach (var key in doc.Content.Keys)
          {
            if (doc.Content[key].TF > highestTF)
            {
                highestTF = doc.Content[key].TF;
            }
          }
            int wordCount = doc.WordCount;
            foreach (var term in doc.Content.Keys)
            {
                doc.Content[term].TF_IDF =((doc.Content[term].TF)/wordCount) * IDFs[term];
            }
        }
    }

    public static Dictionary<string, TermData> Process_Query(string query)
    {
        Dictionary<string, TermData> Pquery = ProcessText(query);
        Get_TF_IDF(IDF,Pquery);
        return Pquery;
    }
    public static void Get_TF_IDF(Dictionary<string, double> IDFs,Dictionary<string, TermData> Pquery)
    {   double highestTF = 0;
        foreach (var key in Pquery.Keys)
        {
            if (Pquery[key].TF > highestTF)
            {
                highestTF = Pquery[key].TF;
            }
        }
        foreach (var key in Pquery.Keys)
        {
            if (IDFs.ContainsKey(key)) 
            {
              Pquery[key].TF_IDF = (Pquery[key].TF/Pquery.Count) * IDFs[key];
            }
            else 
            {
              Pquery[key].TF_IDF = 0;
            }
        }
    }
}
