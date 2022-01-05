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
        }

        public static TextGridBorders Spaces => new TextGridBorders( "Spaces", "           " );
        public static TextGridBorders Single => new TextGridBorders( "Single", "┌┬┐├┼┤└┴┘─│" );
        public static TextGridBorders Double => new TextGridBorders( "Double", "╔╦╗╠╬╣╚╩╝═║" );
        public static TextGridBorders Ascii => new TextGridBorders( "Double", "/v\\>+<\\+/-|" );
        public static TextGridBorders AsciiSquare => new TextGridBorders( "Double", "+++++++++-|" );
        // public static TextGridBorders Thick => new TextGridBorders( "Thick", "┏┳┓┣╋┫┗┻┛━┃" );

        public enum Alignments { Left, Top, Right, Bottom, Center };
        #endregion

        string[,] strings;


        public int Width { get; private set; }
        public int Height { get; private set; }


        public TextGrid WithHeader( string h ) { Header = h; return this; }
        public TextGrid WithRowPadding( int p ) { RowPadding = p; return this; }
        public TextGrid WithColumnPadding( int p ) { ColumnPadding = p; return this; }
        public TextGrid WithHorizontalAlign( Alignments a ) { HorizontalAlign = a; return this; }
        public TextGrid WithVerticalAlign( Alignments a ) { VerticalAlign = a; return this; }
        public TextGrid WithBorders( TextGridBorders b ) { Borders = b; return this; }

        public string Header { get; set; } = "";
        public int RowPadding { get; set; } = 0;
        public int ColumnPadding { get; set; } = 1;
        public Alignments HorizontalAlign { get; set; } = Alignments.Center;
        public Alignments VerticalAlign { get; set; } = Alignments.Center;
        public TextGridBorders Borders { get; set; } = Single;
        public int[] ColumnWidths { get; private set; }
        public int[] RowHeights { get; private set; }

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

        public TextGrid( string header, IEnumerable<IEnumerable> objects )
            : this( objects )
            => Header = header ?? throw new ArgumentNullException( nameof( header ) );


        public static (int, int) BoxSize( IEnumerable<IEnumerable> objects )
            => (objects.Max( list => list.Count() ),
                objects.Count());

        public static (int, int) BoxSize( string multiLineString )
            => BoxSize( multiLineString.Split( '\n' ) );



        public override string ToString()
        {
            StringBuilder sb = new();

            bool hasBorders = Borders != null;
            int borderCount = hasBorders ? 1 : 0;

            if (Header?.Length > 0)
            {
                var text = Header;
                int width = ColumnWidths.Sum( w => w + 2 * ColumnPadding + borderCount ) - borderCount;

                if (hasBorders)
                    sb.Append( Borders[0, 0] )
                      .Append( Borders.Horizontal, width )
                      .Append( Borders[0, -1] )
                      .AppendLine()
                      .Append( Borders.Vertical );

                int padding = width - text.Length;
                if (padding < 0) // too long
                {
                    padding = 0;
                    text = text[..width];
                }
                sb.Append( ' ', padding / 2 );
                sb.Append( text );
                sb.Append( ' ', padding - padding / 2 );

                if (hasBorders)
                    sb.Append( Borders.Vertical );

                sb.AppendLine();
            }

            for (int r = 0; r < Height; r++)
            {
                OutputLine( sb, r );
                OutputStrings( sb, r );
            }
            OutputLine( sb, -1 );

            return sb.ToString();
        }



        private void OutputStrings( StringBuilder sb, int rowType )
            => RowHeights[rowType].ForEach( linenum => OutputLine( sb, rowType, linenum ) );


        private void OutputLine( StringBuilder sb, int row, int lineIndex = -1 )
        {
            var doBorders = Borders != null;
            bool isTextLine = lineIndex != -1;
            bool isBorderLine = !isTextLine;
            bool hasHeader = Header?.Length > 0;

            for (int col = 0; col < Width; col++)
            {
                if (isTextLine)
                {
                    // Borders
                    if (doBorders) sb.Append( Borders.Vertical );

                    // Column Padding
                    sb.Append( ' ', ColumnPadding );

                    // Get the string
                    int width = ColumnWidths[col];
                    var slist = strings[row, col].Split( "\n" );
                    var s = (lineIndex < slist.Length) ? slist[lineIndex] : "";
                    if (s.Length > width)
                        s = s[..width];
                    int padding = width - s.Length;

                    // Add the left padding
                    if (HorizontalAlign == Alignments.Center)
                        sb.Append( ' ', padding / 2 );
                    else if (HorizontalAlign == Alignments.Right)
                        sb.Append( ' ', padding );

                    // Add the string
                    sb.Append( s );

                    // Add the right padding
                    if (HorizontalAlign == Alignments.Center)
                        sb.Append( ' ', padding - padding / 2 );
                    else if (HorizontalAlign == Alignments.Left)
                        sb.Append( ' ', padding );

                    // Column Padding
                    sb.Append( ' ', ColumnPadding );
                }
                else if (doBorders)
                {
                    int r = row;
                    if (r == 0 && col == 0 && hasHeader) r = 1;

                    sb.Append( Borders[r, col] );
                    sb.Append( Borders.Horizontal, ColumnWidths[col] + ColumnPadding * 2 );
                }
            }

            if (doBorders)
                sb.Append( isBorderLine
                    ? Borders[row == 0 && hasHeader ? 1 : row, -1]
                    : Borders.Vertical );

            if (doBorders || isTextLine)
                sb.AppendLine();
        }


    }
}
