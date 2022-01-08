using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    public class TextGrid
    {
        public readonly static TextGridBorders Null = new NullTextGridBorders();
        public readonly static TextGridBorders Spaces = new( "Spaces", @"           " );
        public readonly static TextGridBorders Single = new( "Single", @"┌┬┐├┼┤└┴┘─│" );
        public readonly static TextGridBorders Double = new( "Double", @"╔╦╗╠╬╣╚╩╝═║" );
        public readonly static TextGridBorders Ascii = new( "Ascii", @"+++++++++-|" );
        public readonly static TextGridBorders Ascii2 = new( "Ascii2", @"/v\>+<\+/-|" );

        private string[,] strings;
        private int[] RowHeights;
        private int[] ColumnWidths;
        private StringBuilder outstr = new();
        public readonly int Width;
        public readonly int Height;

        public int RowPadding = 0;
        public int ColumnPadding = 1;
        public string Header = "";
        public TextGridBorders Borders = Single;
        public GridAlignments HeaderAlign = GridAlignments.Center;
        public GridAlignments HorizontalAlign = GridAlignments.Center;
        public GridAlignments VerticalAlign => GridAlignments.Center;

        public bool HasHeader => Header?.Length > 0;
        public TextGrid WithRowPadding( int p ) => this.With( _ => RowPadding = p );
        public TextGrid WithColumnPadding( int p ) => this.With( _ => ColumnPadding = p );
        public TextGrid WithHeader( string h ) => this.With( _ => Header = h );
        public TextGrid WithBorders( TextGridBorders b ) => this.With( _ => Borders = b );
        public TextGrid WithHeaderAlign( GridAlignments a ) => this.With( _ => HeaderAlign = a );
        public TextGrid WithHorizontalAlign( GridAlignments a ) => this.With( _ => HorizontalAlign = a );
        public TextGrid WithVerticalAlign( GridAlignments a ) => throw new NotImplementedException();




        public TextGrid( object[,] objects ) =>
            (Width = objects.GetLength( 1 ), Height = objects.GetLength( 0 ))
                .NowUse( Setup() )
                .Apply( colRow => strings[colRow.Item2, colRow.Item1] = objects[colRow.Item2, colRow.Item1]?.ToString() ?? "" )
                .NowUse( this )
                .SetStartingData();

        public TextGrid( IEnumerable<IEnumerable> objects ) =>
            ((Width, Height) = BoxSize( objects ))
                .Translate( wh => Setup() )
                .Apply( colRow => strings[colRow.Item2, colRow.Item1] = "" )
                .NowUse( objects )
                    .Apply( ( row, rowIndex ) =>
                        row.Apply( ( obj, colIndex ) => 
                            strings[rowIndex, colIndex] = obj?.ToString() ?? "" )
                            .NowUse( this ) )
                    .NowUse( this )
                        .SetStartingData();

        public TextGrid( string header, object[,] objects ) : this( objects ) =>
            Header = header ?? throw new ArgumentNullException( nameof( header ) );

        public TextGrid( string header, IEnumerable<IEnumerable> objects ) : this( objects ) =>
            Header = header ?? throw new ArgumentNullException( nameof( header ) );

        public TextGrid( string header, string newlineAndTabDelimited )
            : this( newlineAndTabDelimited
                        .Split( "\n" )
                        .Select( line => line.Split( "\t" ) ) ) =>
            Header = header ?? throw new ArgumentNullException( nameof( header ) );



        private IEnumerable<(int, int)> Setup() =>
            this
                .With( _ => ColumnWidths = new int[Width] )
                .With( _ => RowHeights = new int[Height] )
                .With( _ => strings = new string[Height, Width] )
            .NowUse( (Width, Height) )
                .Range();

        private void SetStartingData() =>
            (Height, Width).ForEach( ( r, c ) =>
                (strings[r, c] = strings[r, c]
                    .Replace( "\r", "" )
                    .Replace( "\t", " " ))
                .Translate( s => BoxSize( s ) )
                .With( wh => ColumnWidths[c] = Math.Max( ColumnWidths[c], wh.Item1 ) )
                .With( wh => RowHeights[r] = Math.Max( RowHeights[r], wh.Item2 ) ) );



        public static (int, int) BoxSize( IEnumerable<IEnumerable> objects ) =>
            (objects.Max( list => list.Count() ),
            objects.Count());

        public static (int, int) BoxSize( string multiLineString ) =>
            BoxSize( multiLineString.Split( '\n' ) );


        public int OuterTableWidth =>
            Borders.BorderWidth +
            ColumnWidths.Sum( w => w + 2 * ColumnPadding + Borders.BorderWidth );

        public int InnerTableWidth =>
            OuterTableWidth - Borders.BorderWidth * 2;




        public override string ToString() =>
            outstr.Clear()
            .NowUse( HasHeader )
                .If(
                    () => OutputHeader(),
                    () => OutputBar( Borders.TopLeft, Borders.TopCenter, Borders.TopRight )
                ).Height.AsRange().Apply( row =>
                    (row > 0).If(
                        () => OutputBar( Borders.CenterLeft, Borders.CenterCenter, Borders.CenterRight ),
                        () => this
                    ).OutputTextRow( row )
            ).NowUse( this )
                .OutputBar( Borders.BottomLeft, Borders.BottomCenter, Borders.BottomRight )
                .outstr.ToString();


        private TextGrid OutputHeader() =>
            this.OutputBar( Borders.TopLeft, Borders.Horizontal, Borders.TopRight )
                .OutputCell( Header, InnerTableWidth - 2 * ColumnPadding, HeaderAlign )
                .outstr.Append( Borders.Vertical )
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
                .NowUse( this ),

                () => this );


        private TextGrid OutputCell( string text, int width, GridAlignments align ) =>
            outstr.Append( Borders.Vertical )
                .Append( ' ', ColumnPadding )
                .AppendPadded( text, width, align )
                .Append( ' ', ColumnPadding )
            .NowUse( this );



        private TextGrid OutputTextRow( int row ) =>
            Enumerable.Range( 0, RowHeights[row] )
                .Apply( line =>
                    Enumerable.Range( 0, Width )
                        .Apply( col =>
                            OutputCell(
                                LineFromCell( row, col, line ),
                                ColumnWidths[col],
                                HorizontalAlign )
                    ).NowUse( outstr )
                        .Append( Borders.Vertical )
                        .AppendLine()
            ).NowUse( this );

        private string LineFromCell( int row, int col, int line ) =>
            strings[row, col]
            .Split( "\n" )
            .Translate( arr => (arr.Length > line) ? arr[line] : "" );
    }
}
