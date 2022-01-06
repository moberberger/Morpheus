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
        public static TextGridBorders Spaces => new( "Spaces", @"           " );
        public static TextGridBorders Single => new( "Single", @"┌┬┐├┼┤└┴┘─│" );
        public static TextGridBorders Double => new( "Double", @"╔╦╗╠╬╣╚╩╝═║" );
        public static TextGridBorders Ascii => new( "Ascii", @"+++++++++-|" );
        public static TextGridBorders Ascii2 => new( "Ascii2", @"/v\>+<\+/-|" );

        #endregion

        string[,] strings;
        private StringBuilder outstr = new();

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TextGrid WithBorders( TextGridBorders b ) { Borders = b; return this; }
        public TextGrid WithHeader( string h ) { Header = h; return this; }
        public TextGrid WithRowPadding( int p ) { RowPadding = p; return this; }
        public TextGrid WithColumnPadding( int p ) { ColumnPadding = p; return this; }
        public TextGrid WithHorizontalAlign( GridAlignments a ) { HorizontalAlign = a; return this; }
        public TextGrid WithVerticalAlign( GridAlignments a ) { throw new NotImplementedException(); }
        public TextGrid WithHeaderAlign( GridAlignments a ) { HeaderAlign = a; return this; }

        public TextGridBorders Borders { get; set; } = Single;
        public string Header { get; set; } = "";
        public bool HasHeader => Header?.Length > 0;

        public int RowPadding { get; set; } = 0;
        public int ColumnPadding { get; set; } = 1;

        public int[] RowHeights { get; private set; }
        public int[] ColumnWidths { get; private set; }

        public GridAlignments HorizontalAlign { get; set; } = GridAlignments.Center;
        public GridAlignments VerticalAlign => GridAlignments.Center;
        public GridAlignments HeaderAlign { get; set; } = GridAlignments.Center;







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

        public TextGrid( string header, object[,] objects )
            : this( objects )
            => Header = header ?? throw new ArgumentNullException( nameof( header ) );

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


        public int OuterTableWidth => Borders.BorderWidth +
                                        ColumnWidths.Sum( w => w + 2 * ColumnPadding + Borders.BorderWidth );

        protected int InnerTableWidth => OuterTableWidth - Borders.BorderWidth * 2;










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

        private TextGrid OutputHeader() =>
                OutputBar( Borders.TopLeft, Borders.Horizontal, Borders.TopRight )
                .OutputCell( Header, InnerTableWidth - 2 * ColumnPadding, HeaderAlign )
                .outstr
                .Append( Borders.Vertical )
                .AppendLine()
                .NowUse( this )
                .OutputBar( Borders.CenterLeft, Borders.TopCenter, Borders.CenterRight );


        private TextGrid OutputBar( string left, string center, string right ) =>
            (left + center + right != "").If(
                () => outstr
                    .Append( left )
                    .NowUse( Enumerable.Range( 0, Width ) )
                    .Apply( col =>
                        outstr.AppendIf( col > 0, center, "" )
                              .Append( Borders.Horizontal, ColumnWidths[col] + ColumnPadding * 2 ) )
                    .NowUse( outstr )
                    .Append( right )
                    .Append( Borders.NewLine )
                    .NowUse( this )

                , () =>
                    this );


        private TextGrid OutputCell( string text, int width, GridAlignments align ) =>
            outstr.Append( Borders.Vertical )
                .Append( ' ', ColumnPadding )
                .AppendPadded( text, width, align )
                .Append( ' ', ColumnPadding )
                .NowUse( this );



        private TextGrid OutputTextRow( int row )
        {
            for (int line = 0; line < RowHeights[row]; line++)
            {
                for (int col = 0; col < Width; col++)
                {
                    var slist = strings[row, col].Split( "\n" );
                    var s = (line < slist.Length) ? slist[line] : "";

                    int width = ColumnWidths[col];
                    OutputCell( s, width, HorizontalAlign );
                }
                outstr.Append( Borders.Vertical ).AppendLine();
            }
            return this;
        }
    }

    public static class TextGridStringBuilderExtension
    {
        public static T NowUse<T, U>( this U current, T newObj )
        {
            if (current is IEnumerable && !(
                    current is string ||
                    current is StringBuilder ||
                    current is Array))

                foreach (var _ in current as IEnumerable) ;

            return newObj;
        }
        public static T If<T>( this bool condition, Func<T> trueAction, Func<T> falseAction ) =>
            condition
                ? trueAction()
                : falseAction();
    }
}
