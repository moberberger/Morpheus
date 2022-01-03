using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    public class TextGrid
    {
        public IEnumerable<IEnumerable<string>> strings;

        public int Width => strings.Max( row => row.Count() );
        public int Height => strings.Count();
        //public string this[int row, int col] =>
    }
}
