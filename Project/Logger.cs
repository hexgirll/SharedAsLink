using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Travelling;

public class Logger<T>{
    private List<T> log = new List<T>();
    public void Add(T log)
    {
        this.log.Add(log);
    }
    public void Flush(string filePath){
        string s = string.Join(Environment.NewLine, this.log) + Environment.NewLine;
        File.AppendAllText( filePath,  s);
        this.log.Clear();
    }
    
}