using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace espresso
{
    public class Window
    {
        private static SessionSwitchReason LastSessionSwitchReason;

        static Window()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            LastSessionSwitchReason = e.Reason;
        }

        public static bool IsSessionActive()
        {
            return LastSessionSwitchReason switch
            {
                SessionSwitchReason.SessionLock => false,
                SessionSwitchReason.SessionLogoff => false,
                SessionSwitchReason.RemoteDisconnect => false,
                SessionSwitchReason.ConsoleDisconnect => false,
                SessionSwitchReason.RemoteConnect => true,
                SessionSwitchReason.SessionLogon => true,
                SessionSwitchReason.SessionUnlock => true,
                SessionSwitchReason.SessionRemoteControl => true,
                _ => true,
            };

        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        /// <summary>
        ///     Retrieves a handle to the foreground window (the window with which the user is currently working). The system
        ///     assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633505%28v=vs.85%29.aspx for more information.</para>
        /// </summary>
        /// <returns>
        ///     C++ ( Type: Type: HWND )<br /> The return value is a handle to the foreground window. The foreground window
        ///     can be NULL in certain circumstances, such as when a window is losing activation.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static Process GetForegroundProcess()
        {
            var foregroundHwnd = GetForegroundWindow();
            GetWindowThreadProcessId(foregroundHwnd, out var processId);

            var process = Process.GetProcessById((int)processId);
            return process;
        }

        public static string GetForegroundProcessName()
        {
            return GetForegroundProcess().ProcessName;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);


        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        public static TimeSpan GetTimeSinceLastInput()
        {
            var lastInputInfo = new LASTINPUTINFO
            {
                cbSize = (uint)LASTINPUTINFO.SizeOf
            };
            GetLastInputInfo(ref lastInputInfo);

            int elapsedMillis = Environment.TickCount - (int)lastInputInfo.dwTime;

            return elapsedMillis > 0 ? TimeSpan.FromMilliseconds(elapsedMillis) : TimeSpan.Zero;
        }
    }
}
