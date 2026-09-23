using System;
using System.Runtime.InteropServices;

public class MouseOperations
{
    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010
    }

    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out MousePoint lpMousePoint);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    #endif

    public static void SetCursorPosition(int x, int y)
    {
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        SetCursorPos(x, y);
        #endif
    }

    public static void SetCursorPosition(MousePoint point)
    {
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        SetCursorPos(point.X, point.Y);
        #endif
    }

    public static MousePoint GetCursorPosition()
    {
        MousePoint currentMousePoint;
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        var gotPoint = GetCursorPos(out currentMousePoint);
        if (!gotPoint)
        {
            currentMousePoint = new MousePoint(0, 0);
        }
        #else
        currentMousePoint = new MousePoint(0, 0);;
        #endif
        

        return currentMousePoint;
    }

    public static void MouseEvent(MouseEventFlags value)
    {
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        MousePoint position = GetCursorPosition();

        mouse_event
            ((int) value,
                position.X,
                position.Y,
                0,
                0)
            ;
        #endif
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MousePoint
    {
        public int X;
        public int Y;

        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}