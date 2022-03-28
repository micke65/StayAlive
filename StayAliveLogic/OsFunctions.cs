using System;
using System.Runtime.InteropServices;

namespace StayAliveLogic
{
    public interface IOsFunctions
    {
        void DoLockWorkStation();
        void SetPresentationMode();
        void UnSetPresentationMode();
        int LastInputSeconds();
    }

    public class OsFunctions : IOsFunctions
    {
        private DateTime _bootTime;
        public OsFunctions()
        {
            _bootTime = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount);
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref Lastinputinfo plii);

        [StructLayout(LayoutKind.Sequential)]
        struct Lastinputinfo
        {
            public uint cbSize;
            public int dwTime;
        }

        [DllImport("user32.dll")]
        private static extern void LockWorkStation();

        [Flags]
        private enum ExecutionState : uint
        {
            EsContinuous = 0x80000000,
            EsDisplayRequired = 0x00000002
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        public void DoLockWorkStation()
        {
            LockWorkStation();
        }

        public void SetPresentationMode()
        {
            SetThreadExecutionState(ExecutionState.EsDisplayRequired | ExecutionState.EsContinuous);
        }

        public void UnSetPresentationMode()
        {
            SetThreadExecutionState(ExecutionState.EsContinuous);
        }

        public int LastInputSeconds()
        {
            Lastinputinfo lii = new Lastinputinfo();
            lii.cbSize = (uint)Marshal.SizeOf(typeof(Lastinputinfo));
            GetLastInputInfo(ref lii);

            DateTime lastInputTime = _bootTime.AddMilliseconds(lii.dwTime);
            return (int)DateTime.UtcNow.Subtract(lastInputTime).TotalSeconds;
        }
    }
}