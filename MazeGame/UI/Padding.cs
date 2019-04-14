using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.UI
{
    internal struct Padding
    {
        public Padding(int all)
        {
            Left = Top = Right = Bottom = all;
        }

        public Padding(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public int Horizontal => Left + Right;
        public int Vertical => Top + Bottom;

        public static readonly Padding Zero = new Padding(0);
    }
}
