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

            public char TopLeft => orderedBorderChars[0];
            public char TopCenter => orderedBorderChars[1];
            public char TopRight => orderedBorderChars[2];
            public char CenterLeft => orderedBorderChars[3];
            public char CenterCenter => orderedBorderChars[4];
            public char CenterRight => orderedBorderChars[5];
            public char BottomLeft => orderedBorderChars[6];
            public char BottomCenter => orderedBorderChars[7];
            public char BottomRight => orderedBorderChars[8];
            public char Horizontal => orderedBorderChars[9];
            public char Vertical => orderedBorderChars[10];

            public char this[int rowType, int colType]
                => orderedBorderChars[TypeToIndex( rowType ) * 3 + TypeToIndex( colType )];

            public static int TypeToIndex( int type )
                => (type == 0) ? 0 : (type == -1) ? 2 : 1;
            // public char Left( int rowType ) => orderedBorderChars[rowType * 3]; public char
            // CenterHorizontal( int rowType ) => orderedBorderChars[rowType * 3 + 1]; public
            // char Right( int rowType ) => orderedBorderChars[rowType * 3 + 2];

            // public char Top( int colType ) => orderedBorderChars[colType]; public char
            // CenterVertical( int colType ) => orderedBorderChars[3 + colType]; public char
            // Bottom( int colType ) => orderedBorderChars[6 + colType];
        }

        public static TextGridBorders Spaces => new TextGridBorders( "Spaces", "           " );
        public static TextGridBorders Single => new TextGridBorders( "Single", "┌┬┐├┼┤└┴┘─│" );
        public static TextGridBorders Double => new TextGridBorders( "Double", "╔╦╗╠╬╣╚╩╝═║" );
        public static TextGridBorders Ascii => new TextGridBorders( "Double", "/v\\>+<\\+/-|" );
        public static TextGridBorders AsciiSquare => new TextGridBorders( "Double", "+++++++++-|" );
        public static TextGridBorders Thick => new TextGridBorders( "Thick", "┏┳┓┣╋┫┗┻┛━┃" );

        public enum Alignments { Left, Top, Right, Bottom, Center };
        #endregion

        string[,] strings;


        public int Width { get; private set; }
        public int Height { get; private set; }


        public TextGridBorders Borders { get; set; } = Single;
        public int RowPadding { get; set; } = 0;
        public int ColumnPadding { get; set; } = 1;
        public Alignments HorizontalAlign { get; set; } = Alignments.Center;
        public Alignments VerticalAlign { get; set; } = Alignments.Center;
        public int[] ColumnWidths { get; set; }
        public int[] RowHeights { get; set; }

        public TextGrid( IEnumerable<IEnumerable> objects )
        {
            (Width, Height) = BoxSize( objects );

            strings = new string[Height, Width];
            (Height, Width).ForEach( ( r, c ) => strings[r, c] = "" );

            objects
                .ForEach( ( row, rowIndex )
                    => row.ForEach( ( obj, colIndex )
                        => strings[rowIndex, colIndex] = obj.ToString()
                )
            );

            ColumnWidths = new int[Width];
            RowHeights = new int[Height];
            (Height, Width).ForEach( ( r, c ) =>
            {
                var (w, h) = BoxSize( strings[r, c] );
                ColumnWidths[c] = Math.Max( ColumnWidths[c], w );
                RowHeights[r] = Math.Max( RowHeights[r], h );
            } );
        }

        public static (int, int) BoxSize( IEnumerable<IEnumerable> objects )
            => (objects.Max( list => list.Count() ),
                objects.Count());

        public static (int, int) BoxSize( string multiLineString )
            => BoxSize( multiLineString.Split( '\n' ) );



        public override string ToString()
        {
            StringBuilder sb = new();

            for (int r = 0; r < Height; r++)
            {
                OutputLine( sb, r );
                OutputStrings( sb, r );
            }
            OutputLine( sb, -1 );

            return sb.ToString();
        }

        private void OutputLine( StringBuilder sb, int row, int lineIndex = -1 )
        {
            for (int col = 0; col < Width; col++)
            {
                if (lineIndex == -1)
                {
                    sb.Append( Borders[row, col] );
                    sb.Append( Borders.Horizontal, ColumnWidths[col] + ColumnPadding*2 );
                }
                else
                {
                    sb.Append( Borders.Vertical );
                    sb.Append( ' ', ColumnPadding );

                    var slist = strings[row, col].Split( "\n" );
                    var s = (lineIndex < slist.Length) ? slist[lineIndex] : "";
                    int padding = ColumnWidths[col] - s.Length;

                    sb.Append( s ).Append( ' ', padding + ColumnPadding );
                }
            }
            sb.Append( lineIndex == -1 ? Borders[row, -1] : Borders.Vertical ).AppendLine();
        }


        private void OutputStrings( StringBuilder sb, int rowType )
            => RowHeights[rowType].ForEach( linenum => OutputLine( sb, rowType, linenum ) );


    }
}
