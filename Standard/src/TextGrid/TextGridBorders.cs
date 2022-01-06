using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    public class TextGridBorders
    {
        protected string name;
        string orderedBorderChars;

        public TextGridBorders( string name, string orderedBorderChars )
            => (this.name, this.orderedBorderChars) = (name, orderedBorderChars);

        public virtual string TopLeft => orderedBorderChars[0].ToString();
        public virtual string TopCenter => orderedBorderChars[1].ToString();
        public virtual string TopRight => orderedBorderChars[2].ToString();
        public virtual string CenterLeft => orderedBorderChars[3].ToString();
        public virtual string CenterCenter => orderedBorderChars[4].ToString();
        public virtual string CenterRight => orderedBorderChars[5].ToString();
        public virtual string BottomLeft => orderedBorderChars[6].ToString();
        public virtual string BottomCenter => orderedBorderChars[7].ToString();
        public virtual string BottomRight => orderedBorderChars[8].ToString();
        public virtual string Horizontal => orderedBorderChars[9].ToString();
        public virtual string Vertical => orderedBorderChars[10].ToString();
        public virtual string NewLine => "\n";
        public virtual int BorderWidth => 1;

        public virtual string this[int rowType, int colType]
            => orderedBorderChars[TypeToIndex( rowType ) * 3 + TypeToIndex( colType )].ToString();

        public static int TypeToIndex( int type )
            => (type == 0) ? 0 : (type == -1) ? 2 : 1;
    }


    public class NullTextGridBorders : TextGridBorders
    {
        public NullTextGridBorders() : base( "Null", "" ) { }
        public override string TopLeft => "";
        public override string TopCenter => "";
        public override string TopRight => "";
        public override string CenterLeft => "";
        public override string CenterCenter => "";
        public override string CenterRight => "";
        public override string BottomLeft => "";
        public override string BottomCenter => "";
        public override string BottomRight => "";
        public override string Horizontal => "";
        public override string Vertical => "";
        public override string NewLine => "";
        public override int BorderWidth => 0;
        public override string this[int rowType, int colType] => "";
    }
}
