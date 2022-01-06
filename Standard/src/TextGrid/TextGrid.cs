using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    public class TextGrid
    {
        #region Border Stuff

        public static TextGridBorders Null => new NullTextGridBorders();
        public static TextGridBorders Spaces => new TextGridBorders( "Spaces", "           " );
        public static TextGridBorders Single => new TextGridBorders( "Single", "┌┬┐├┼┤└┴┘─│" );
        public static TextGridBorders Double => new TextGridBorders( "Double", "╔╦╗╠╬╣╚╩╝═║" );
        public static TextGridBorders Ascii => new TextGridBorders( "Double", "/v\\>+<\\+/-|" );
        public static TextGridBorders AsciiSquare => new TextGridBorders( "Double", "+++++++++-|" );

        #endregion

        string[,] strings;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TextGrid WithHeader( string h ) { Header = h; return this; }
        public TextGrid WithRowPadding( int p ) { RowPadding = p; return this; }
        public TextGrid WithColumnPadding( int p ) { ColumnPadding = p; return this; }
        public TextGrid WithHorizontalAlign( GridAlignments a ) { HorizontalAlign = a; return this; }
        // public TextGrid WithVerticalAlign( Alignments a ) { VerticalAlign = a; return this; }
        public TextGrid WithBorders( TextGridBorders b ) { Borders = b; return this; }

        public string Header { get; set; } = "";
        public int RowPadding { get; set; } = 0;
        public int ColumnPadding { get; set; } = 1;
        public GridAlignments HorizontalAlign { get; set; } = GridAlignments.Center;
        // public Alignments VerticalAlign { get; set; } = Alignments.Center;
        public TextGridBorders Borders { get; set; } = Single;
        public int[] ColumnWidths { get; private set; }
        public int[] RowHeights { get; private set; }




        public bool HasHeader => Header?.Length > 0;



        public TextGrid( object[,] objects )
        {
            Height = objects.GetLength( 0 );
            Width = objects.GetLength( 1 );

            strings = new string[Height, Width];
            (Height, Width).ForEach( ( r, c ) => strings[r, c] = objects[r, c].ToString() );

            SetStartingData();
        }

        public TextGrid( IEnumerable<IEnumerable> objects )
        {
            (Width, Height) = BoxSize( objects );

            strings = new string[Height, Width];
            (Height, Width).ForEach( ( r, c ) => strings[r, c] = "" );

            objects
                .ForEach( ( row, rowIndex )
                    => row.ForEach( ( obj, colIndex )
                        => strings[rowIndex, colIndex] = obj.ToString() ) );

            SetStartingData();
        }

        public TextGrid( string newlineAndTabDelimited )
            : this( newlineAndTabDelimited
                .Split( "\n" )
                .Select( line => line.Split( "\t" ) ) )
        { }

        public TextGrid( string header, IEnumerable<IEnumerable> objects )
            : this( objects )
            => Header = header ?? throw new ArgumentNullException( nameof( header ) );


        public TextGrid( string header, string newlineAndTabDelimited )
            : this( newlineAndTabDelimited )
            => Header = header ?? throw new ArgumentNullException( nameof( header ) );



        private void SetStartingData()
        {
            ColumnWidths = new int[Width];
            RowHeights = new int[Height];
            (Height, Width).ForEach( ( r, c ) =>
            {
                strings[r, c] = strings[r, c]
                    .Replace( "\r", "" )
                    .Replace( "\t", " " );

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








        private StringBuilder outstr = new StringBuilder();
        int CharWidth => ColumnWidths.Sum( w => w + 2 * ColumnPadding + Borders.BorderWidth ) - Borders.BorderWidth;
        void OutputCell( string text, int width )
        => outstr.Append( Borders.Vertical )
                 .Append( ' ', ColumnPadding )
                 .AppendPadded( text, width, HorizontalAlign )
                 .Append( ' ', ColumnPadding );



        public override string ToString()
        {
            outstr.Clear();

            if (HasHeader)
                OutputHeader();
            else
                OutputBar( Borders.TopLeft, Borders.TopCenter, Borders.TopRight );

            for (int row = 0; row < Height; row++)
            {
                if (row > 0)
                    OutputBar( Borders.CenterLeft, Borders.CenterCenter, Borders.CenterRight );
                OutputTextRow( row );
            }

            OutputBar( Borders.BottomLeft, Borders.BottomCenter, Borders.BottomRight );

            return outstr.ToString();
        }

        void OutputHeader()
        {
            outstr.Append( Borders.TopLeft )
                  .Append( Borders.Horizontal, CharWidth )
                  .Append( Borders.TopRight )
                  .Append( Borders.NewLine );

            OutputCell( Header, CharWidth - 2 * ColumnPadding );
            outstr.Append( Borders.Vertical ).AppendLine();

            OutputBar( Borders.CenterLeft, Borders.TopCenter, Borders.CenterRight );
        }

        void OutputBar( string left, string center, string right )
        {
            outstr.Append( left );
            Width.ForEach( col =>
                outstr.AppendIf( col > 0, center, "" )
                      .Append( Borders.Horizontal, ColumnWidths[col] + ColumnPadding * 2 ) );
            outstr.Append( right );
            outstr.Append( Borders.NewLine );
        }

        private void OutputTextRow( int row )
        {
            for (int line = 0; line < RowHeights[row]; line++)
            {
                for (int col = 0; col < Width; col++)
                {
                    var slist = strings[row, col].Split( "\n" );
                    var s = (line < slist.Length) ? slist[line] : "";

                    int width = ColumnWidths[col];
                    OutputCell( s, width );
                }
                outstr.Append( Borders.Vertical ).AppendLine();
            }
        }
    }
}
