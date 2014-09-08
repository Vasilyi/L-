using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PerfectWard
{
    public delegate void RawInputEventDelegate(RawInputEventArgs e);


    public class Keyboard : IMessageFilter
    {
        #region Public / Methods
        const int WM_INPUT = 0x00ff;
        public event RawInputEventDelegate RawInputEvent;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_INPUT && m.WParam != (IntPtr)0)
                if (RawInputEvent != null)
                {
                    ProcessRawInput(m.LParam);
                    return true;
                }
            return false;
        }

        private List<RAWINPUTDEVICE> devices = new List<RAWINPUTDEVICE>();

        public bool Initialize(IntPtr handle)
        {
            devices.Add(new RAWINPUTDEVICE(1, 6, 0x100, handle));
            var d = devices.ToArray();
            bool result = RegisterRawInputDevices(d, devices.Count, Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
            return result;
        }

        private void ProcessRawInput(IntPtr wParam)
        {
            RAWINPUT rawInput = new RAWINPUT();
            int length = Marshal.SizeOf(typeof(RAWINPUT));

            int result = GetRawInputData(wParam, RawInputCommand.Input, out rawInput, ref length,
                                         Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            if (result != 1)
            {
                if (rawInput.Header.Type == RawInputType.Keyboard)
                {
                    if ((rawInput.Keyboard.Flags == 0))
                    {
                        RawInputEvent(new RawInputEventArgs(rawInput.Keyboard.VirtualKey, false));
                    }
                    if ((rawInput.Keyboard.Flags & 1) != 0)
                    {
                        RawInputEvent(new RawInputEventArgs(rawInput.Keyboard.VirtualKey, true));
                    }
                }
            }
        }

        #endregion

        #region Private / Methods

        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern int GetRawInputData(IntPtr hRawInput,
                                                  RawInputCommand uiCommand, out RAWINPUT pData,
                                                  ref int pcbSize, int cbSizeHeader);

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(int uCode, int uMapType);

        [DllImport("user32.dll")]
        static extern bool RegisterRawInputDevices(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] 
            RAWINPUTDEVICE[] pRawInputDevices,
            int uiNumDevices, int cbSize);

        [DllImport("user32.dll")]
        private static extern int ToAscii(int uVirtKey, int uScanCode,
                                          byte[] lpKeyState, ref int lpChar, int uFlags);

        #endregion
    }

    public class RawInputEventArgs : EventArgs
    {
        #region Public / Attributes

        public ushort virtualKey;
        public bool up;

        #endregion

        #region Public / Constructors

        public RawInputEventArgs(ushort virtualKey, bool up)
        {
            this.up = up;
            this.virtualKey = virtualKey;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RAWINPUTDEVICE
    {
        public ushort UsagePage;
        public ushort Usage;
        public Int32 Flags;
        public IntPtr WindowHandle;
        public RAWINPUTDEVICE(ushort usagePage, ushort usage,
            Int32 flags, IntPtr windowHandle)
        {
            UsagePage = usagePage;
            Usage = usage;
            Flags = flags;
            WindowHandle = windowHandle;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RAWINPUT
    {
        [FieldOffset(0)]
        public RAWINPUTHEADER Header;
        [FieldOffset(16)]
        public RAWKEYBOARD Keyboard;
        //mouse and HID parts omitted
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
        public RawInputType Type;
        public int Size;
        public IntPtr Device;
        public IntPtr wParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWKEYBOARD
    {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;
        public ushort VirtualKey;
        public int Message;
        public int ExtraInformation;
    }

    public enum RawInputType
    {
        Mouse = 0,
        Keyboard = 1,
        HID = 2
    }

    enum RawInputCommand
    {
        Input = 0x10000003,
        Header = 0x10000005
    }
}
