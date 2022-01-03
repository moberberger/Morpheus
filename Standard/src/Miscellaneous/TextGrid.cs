using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    public class TextGrid
    {
        #region Border Stuff
        public class TextGridBorders
        {
            string name;
            string orderedBorderChars;

            public TextGridBorders( string name, string orderedBorderChars )
                => (this.name, this.orderedBorderChars) = (name, orderedBorderChars);

            public char Horizontal => orderedBorderChars[0];
            public char Vertical => orderedBorderChars[1];
            public char TopLeft => orderedBorderChars[2];
            public char TopCenter => orderedBorderChars[3];
            public char TopRight => orderedBorderChars[4];
            public char CenterLeft => orderedBorderChars[5];
            public char CenterCenter => orderedBorderChars[6];
            public char CenterRight => orderedBorderChars[7];
            public char BottomLeft => orderedBorderChars[8];
            public char BottomCenter => orderedBorderChars[9];
            public char BottomRight => orderedBorderChars[10];
        }

        public static TextGridBorders Thick => new TextGridBorders( "Thick", "━┃┏┳┓┣╋┫┗┻┛" );
        public static TextGridBorders Simple => new TextGridBorders( "Simple", "─│┌┬┐├┼┤└┴┘" );
        public static TextGridBorders Double => new TextGridBorders( "Double", "═║╔╦╗╠╬╣╚╩╝" );
        #endregion

        string[,] strings;


        public int Width { get; private set; }
        public int Height { get; private set; }


        public TextGridBorders Borders { get; set; } = Simple;
        public int RowPadding { get; set; } = 0;
        public int ColumnPadding { get; set; } = 0;


        public TextGrid( IEnumerable<IEnumerable<object>> objects )
        {
            (Width, Height) = BoxSize( objects );
            strings = new string[Height, Width]; // row-major

            objects
                .Run( ( row, rowIndex )
                    => row.Run( ( obj, colIndex )
                        => strings[rowIndex, colIndex] = obj.ToString()
                )
            );
        }

        public static (int, int) BoxSize( IEnumerable<IEnumerable> objects )
            => (objects.Max( list => list.Count() ),
                objects.Count());

        public static (int, int) BoxSize( string multiLineString )
            => BoxSize( multiLineString.Split( '\n' ) );
    }
}
