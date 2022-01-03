using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    public class TextGrid
    {
        public IEnumerable<IEnumerable<object>> objects;
        public string[,] strings;

        public TextGrid( IEnumerable<IEnumerable<object>> _objects )
        {
            objects = _objects;

            int w = objects.Max( row => row.Count() );
            int h = objects.Count();
            strings = new string[h, w]; // row-major

            _objects
                .Run( (row, rowIndex) 
                => row.Run( (obj, colIndex) 
                    => strings[rowIndex, colIndex] = obj.ToString() 
                )
            );
        }

        // public string this[int row, int col] =>
    }
}
