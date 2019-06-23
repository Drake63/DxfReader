using System;
using System.Runtime.InteropServices;

namespace DxfOpener
{
    class SendMessageHelper
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        public const int WM_COPYDATA = 0x4A;

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public static int SendWindowStringMessage(IntPtr hWnd, string message)
        {
            int result = 0;

            if(hWnd != IntPtr.Zero)
            {
                COPYDATASTRUCT lParam = new COPYDATASTRUCT
                {
                    dwData = IntPtr.Zero,
                    cbData = message.Length * Marshal.SystemDefaultCharSize,
                    lpData = Marshal.StringToHGlobalAnsi(message)
                };
                result = SendMessage(hWnd, WM_COPYDATA, 0, ref lParam);
            }
            return result;
        }
    }
}
