public class TermData
{
    public double TF = 1;
    public List<int> Indexes { get; }
    private string name { get; }//revisar si hace falta.
    public double TF_IDF {get; set;}

    public TermData(string term, int Index)
    {
        name = term;
       Indexes = new List<int>{Index};

    } 
    public void AddIndex(int index)
    {
        Indexes.Add(index);
    }

    public void IncreaseTF()
    {
        TF++;
    }


}