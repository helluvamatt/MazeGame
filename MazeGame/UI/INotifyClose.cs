using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.UI
{
    internal interface INotifyClose
    {
        event EventHandler Close;
    }
}
