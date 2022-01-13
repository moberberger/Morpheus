using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendIf( this StringBuilder sb, bool condition, string ifTrue, string ifFalse = "" )
            => sb.Append( condition ? ifTrue : ifFalse );

        public static StringBuilder AppendLines( this StringBuilder sb, IEnumerable objects )
        {
            foreach (var obj in objects)
                sb.AppendLine( obj.ToString() );
            return sb;
        }

        public static StringBuilder AppendTab( this StringBuilder sb, string text = null ) => sb.Append( $"{text}\t" );

        public static StringBuilder Append( this StringBuilder sb, string s, int count )
        {
            while (count-- > 0) sb.Append( s );
            return sb;
        }


        public static StringBuilder AppendPadded( this StringBuilder sb, string text, int width, GridAlignments alignment = GridAlignments.Left )
        {
            text = text.Trim();

            if (text.Length >= width)
            {
                text = text[..width];
                sb.Append( text );
            }
            else
            {
                int padding = width - text.Length;

                // Add the left padding
                if (alignment == GridAlignments.Center)
                    sb.Append( ' ', padding / 2 );
                else if (alignment == GridAlignments.Right)
                    sb.Append( ' ', padding );

                // Add the string
                sb.Append( text );

                // Add the right padding
                if (alignment == GridAlignments.Center)
                    sb.Append( ' ', padding - padding / 2 );
                else if (alignment == GridAlignments.Left)
                    sb.Append( ' ', padding );
            }

            return sb;
        }

    }
}
