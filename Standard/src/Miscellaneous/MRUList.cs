using System.Collections;

namespace Morpheus;




public class MRUList : IEnumerable<string>
{
    private string MRUFileName => Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.Personal,
            Environment.SpecialFolderOption.Create ),
        "MRU.txt" );

    public bool MRUFileExists => File.Exists( MRUFileName );

    public IList<string> GetMRUList()
    {
        List<string> list = new();
        if (MRUFileExists)
        {
            using var fp = File.OpenText( MRUFileName );
            string line;

            while ((line = fp.ReadLine()) != null)
                if (File.Exists( line ))
                    list.Add( line );
        }

        return list;
    }

    public void AddFileName( string fileName )
    {
        List<string> mru = new() { fileName };
        mru.AddRange( 
            GetMRUList()
                .Where( fname => fname != fileName ) );

        using var ofp = File.CreateText( MRUFileName );
        foreach (var fname in mru)
            ofp.WriteLine( fname );
    }



    public IEnumerator<string> GetEnumerator() => GetMRUList().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
