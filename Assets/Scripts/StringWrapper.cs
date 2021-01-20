public class StringWrapper
{
    private string s;
    public StringWrapper(string i){ s = i; }
    public override string ToString(){ return s; }
    public void set(string i){s = i;}
    // public static implicit operator StringWrapper(string a) => new StringWrapper(a);
    public static explicit operator string(StringWrapper b) => b.ToString();
}
