﻿using System;
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

        public static TextGridBorders Single => new TextGridBorders( "Single", "─│┌┬┐├┼┤└┴┘" );
        public static TextGridBorders Double => new TextGridBorders( "Double", "═║╔╦╗╠╬╣╚╩╝" );
        public static TextGridBorders Thick => new TextGridBorders( "Thick", "━┃┏┳┓┣╋┫┗┻┛" );

        public enum Alignments { Left, Top = Left, Right, Bottom = Right, Center };
        #endregion

        string[,] strings;


        public int Width { get; private set; }
        public int Height { get; private set; }


        public TextGridBorders Borders { get; set; } = Single;
        public int RowPadding { get; set; } = 0;
        public int ColumnPadding { get; set; } = 0;
        public Alignments HorizontalAlign { get; set; } = Alignments.Center;
        public Alignments VerticalAlign { get; set; } = Alignments.Center;


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
        }

        public static (int, int) BoxSize( IEnumerable<IEnumerable> objects )
            => (objects.Max( list => list.Count() ),
                objects.Count());

        public static (int, int) BoxSize( string multiLineString )
            => BoxSize( multiLineString.Split( '\n' ) );



        //<code>
        //public override string ToString()
        //{
        //    (Height, Width).ForEach( ( r, c ) =>
        //}
    }
}
