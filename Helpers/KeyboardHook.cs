using Amplitude.Models;
using AmplitudeSoundboard;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Amplitude.Helpers
{
    public class KeyboardHook
    {
        #region WinAPI Definitions
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(IntPtr path);
        #endregion

        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        static LowLevelKeyboardProcDelegate hook;
        const int WH_KEYBOARD_LL = 13;
        private IntPtr winHook;

        private static KeyboardHook _instance;
        public static KeyboardHook Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KeyboardHook();
                }
                return _instance;
            }
        }

        private KeyboardHook()
        {
            hook = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
            IntPtr handle = GetModuleHandle(IntPtr.Zero);
            winHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, handle, 0);
        }

        private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (nCode >= 0)
                switch (wParam)
                {
                    case 256:
                    case 260:
                        string key = lParam.vkCode + "|" + lParam.flags; // Subject to change based on json implementation

                        // Call the hotkeys manager

                        if (App.HotkeysManager.Hotkeys.TryGetValue(key, out List<string> values))
                        {
                            // Go through and call all the Play methods on these id's
                            foreach (string item in values)
                            {
                                // DICTIONARYofSOUNDCLIPS.Get(item).PlayAudio();
                                // Sound clip manager instance
                            }
                        }

                        break;
                }
            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        public void Dispose()
        {
            _instance = null;
            UnhookWindowsHookEx(winHook);
        }

        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            int scanCode;
            public int flags;
            int time;
            int dwExtraInfo;
        }
    }
}