/*
    AmplitudeSoundboard
    Copyright (C) 2021 dan0v
    https://git.dan0v.com/AmplitudeSoundboard

    This file is part of AmplitudeSoundboard.

    AmplitudeSoundboard is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AmplitudeSoundboard is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AmplitudeSoundboard.  If not, see <https://www.gnu.org/licenses/>.
*/

using Amplitude.Models;
using AmplitudeSoundboard;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Amplitude.Helpers
{
    public class WinKeyboardHook
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

        private static WinKeyboardHook _instance;
        public static WinKeyboardHook Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WinKeyboardHook();
                }
                return _instance;
            }
        }

        private WinKeyboardHook()
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