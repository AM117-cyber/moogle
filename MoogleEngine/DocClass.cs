using System.Text.RegularExpressions;
public class Document
{
   public string Title;
   public string Path { get; }
   public static int Total_of_Docs = Directory.GetFiles(@"..\Content").Length;
   public Dictionary<string, TermData> Content { get; }
   public int WordCount { get; }
   private string pattern = @"\w+";


    public Document(string path, Dictionary<string,TermData> content)
    {
        this.Path = path;
        this.Title = $"''{GettingTitle(path)}''";
        this.Content = content;
        this.WordCount = Regex.Split(File.ReadAllText(path).Trim(), pattern).Length;
    }


    private string GettingTitle(string path)
    {
        string title = string.Empty;
        path = path.Remove(0, 11);
        for (int i = 0; i < path.Length - 4; i++)
        {
            title += path[i];

        }
        return title;
    }
    
}
