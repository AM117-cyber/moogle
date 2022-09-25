using System.Text;
using System.Text.RegularExpressions;
public static class DocumentProcessor
{
    //the two dots get the path in which the folder to the project is located, you only specify the content folder.
    private static Dictionary<string, double>? IDFs = null;
    public static IEnumerable<Document> docs = ProcessFolder(Directory.GetFiles(@"..\Content")).ToList();
    

    public static IEnumerable<Document> ProcessFolder(string[] path)
    {
        foreach (var indivpath in path)
        {
            string text = File.ReadAllText(indivpath);
            string pattern = @"\w+";
            var Docs = new Document(indivpath, ProcessText(text, pattern));
            yield return Docs;
        }
    }

    public static Dictionary<string, TermData> ProcessText(string text, string pattern)
    {

        var matches = Regex.Matches(text, pattern);//Match devuelve una coleccion.
        Dictionary<string, TermData> terms_of_doc = new Dictionary<string, TermData>();
        foreach (Match word in matches)
        {
            
            string word_modified = Regex.Replace(word.Value.ToLower().Normalize(NormalizationForm.FormD), @"[^a-zA-Z0-9 ]+", "");
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


    public static Dictionary<string, double> GetIDF()
    {
        if (IDFs is null)
        {
            int N = Document.Total_of_Docs;
            IDFs = new Dictionary<string, double>();
            foreach (var document in docs)
            {
                foreach (var term in document.Content.Keys)
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
    public static Dictionary<string, double> IDF = GetIDF();
    public static void SettingEachTFIDF(Dictionary<string, double> IDFs)
    {
        foreach (var doc in docs)
        {
            int wordCount = doc.WordCount;
            foreach (var term in doc.Content.Keys)
            {
                doc.Content[term].TF_IDF = ((doc.Content[term].TF) / wordCount) * IDFs[term];
            }
        }
    }

    public static Dictionary<string, TermData> ProcessQuery(string query)
    {
        string pattern = @"\w+";
        Dictionary<string, TermData> Pquery = ProcessText(query, pattern);
        //GetTFIDF(IDF,Pquery);
        return Pquery;
    }
    public static void GetTFIDF(Dictionary<string, double> IDFs, Dictionary<string, TermData> Pquery)
    {
        foreach (var key in Pquery.Keys)
        {
            if (IDFs.ContainsKey(key))
            {
                Pquery[key].TF_IDF = (Pquery[key].TF / Pquery.Count) * IDFs[key];
            }
            else
            {
                Pquery[key].TF_IDF = 0;
            }
        }
    }
}
