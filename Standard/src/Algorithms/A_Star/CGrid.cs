using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    public class CGrid
    {
        public int Width
        {
            get { return m_width; }
        }
        private int m_width;

        public int Height
        {
            get { return m_height; }
        }
        private int m_height;

        protected CCell[,] m_cells;
        public virtual CCell this[int x, int y]
        {
            get { return m_cells[x, y]; }
        }


        public CGrid( int p_width, int p_height )
        {
            if (p_width < 1)
                throw new ArgumentException( "Width not allowed to be less than 1" );
            if (p_height < 1)
                throw new ArgumentException( "Height not allowed to be less than 1" );

            m_width = p_width;
            m_height = p_height;
            m_cells = new CCell[m_width, m_height];

            BuildGrid();
        }

        protected void BuildGrid()
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    CCell c = new CCell( this, x, y );
                    m_cells[x, y] = c;
                }
            }
        }
    }
}
