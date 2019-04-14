using System;

namespace MazeGame.UI
{
    [Flags]
    internal enum MouseButton
    {
        None = 0,
        Left = 0x1,
        Right = 0x2,
        Middle = 0x4,
        XButton1 = 0x8,
        XButton2 = 0x10,
    }

    internal enum Alignment { Near, Middle, Far }

    internal enum FrameType { None, LargeScroll, MediumScroll, SmallScroll }

    internal enum DockMode { Center, LowerThird, Fill }
}
