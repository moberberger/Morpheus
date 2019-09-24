using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    public class CCell
    {
        public CGrid Grid
        {
            get { return m_grid; }
        }
        private CGrid m_grid;

        public int X
        {
            get { return m_x; }
        }
        private int m_x;

        public int Y
        {
            get { return m_y; }
        }
        private int m_y;

        private bool m_isBlocked;
        public bool IsBlocked
        {
            get { return m_isBlocked || m_grid == null; }
            set { m_isBlocked = value; }
        }
        public bool IsOpen
        {
            get { return !m_isBlocked; }
            set { m_isBlocked = !value; }
        }


        public CCell( CGrid p_grid, int p_x, int p_y )
        {
            m_grid = p_grid;
            m_x = p_x;
            m_y = p_y;
        }


        public double EuclideanDistance( CCell p_goalCell )
        {
            int dx = X - p_goalCell.X;
            int dy = Y - p_goalCell.Y;

            return Math.Sqrt( dx * dx + dy * dy );
        }

        public double ManhattanDistance( CCell p_goalCell )
        {
            int dx = Math.Abs( X - p_goalCell.X );
            int dy = Math.Abs( Y - p_goalCell.Y );

            return dx + dy;
        }

        public double GridDistance( CCell p_goalCell )
        {
            int dx = Math.Abs( X - p_goalCell.X );
            int dy = Math.Abs( Y - p_goalCell.Y );

            return Math.Max( dx, dy );
        }

        public override string ToString()
        {
            return "cell[" + X + "," + Y + "]";
        }
    }
}
