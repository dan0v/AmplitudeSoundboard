/*
    AmplitudeSoundboard
    Copyright (C) 2021-2023 dan0v
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
    public class WinKeyboardHook : IKeyboardHook
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

        private static List<(SoundClip clip, Action<SoundClip, string> callback)> soundClipCallbacks = new List<(SoundClip, Action<SoundClip, string>)>();
        private static (Config config, Action<Config, string> callback) globalStopCallback;

        // Kind of janky, but works for now
        public const long KEYPRESSTIMEOUT = 150;
        private static string currentKey = "";

        readonly LowLevelKeyboardProcDelegate hook;
        const int WH_KEYBOARD_LL = 13;
        private readonly IntPtr winHook;

        private static SortedSet<string> specialKey = new SortedSet<string>();

        private static WinKeyboardHook? _instance;
        public static WinKeyboardHook Instance => _instance ??= new WinKeyboardHook();

        private WinKeyboardHook()
        {
            hook = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
            IntPtr handle = GetModuleHandle(IntPtr.Zero);
            winHook = SetWindowsHookEx(WH_KEYBOARD_LL, hook, handle, 0);
        }

        private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (nCode >= 0)
            {
                switch (wParam)
                {
                    // KeyUp and SysKeyUp
                    case 257:
                    case 261:
                        {
                            currentKey = GetFriendlyKeyName(lParam.vkCode);
                            if (KeySpecial(currentKey))
                            {
                                specialKey.Remove(currentKey);
                                break;
                            }
                            break;
                        }
                    // KeyDown and SysKeyDown
                    case 256:
                    case 260:
                        {
                            currentKey = GetFriendlyKeyName(lParam.vkCode);

                            if (currentKey == "" || currentKey == "LWin" || currentKey == "RWin")
                            {
                                break;
                            }

                            if (currentKey == "Esc")
                            {
                                currentKey = HotkeysManager.UNBIND_HOTKEY;
                            }

                            if (KeySpecial(currentKey))
                            {
                                specialKey.Add(currentKey);
                                break;
                            }

                            if (!KeySpecial(currentKey))
                            {
                                if (globalStopCallback.config != null)
                                {
                                    globalStopCallback.callback(globalStopCallback.config, fullKey);
                                    globalStopCallback.config = null;
                                }
                                else if (soundClipCallbacks.Count > 0)
                                {
                                    foreach (var callback in soundClipCallbacks)
                                    {
                                        callback.callback(callback.clip, fullKey);
                                    }
                                    soundClipCallbacks.Clear();
                                }
                                else if (currentKey != HotkeysManager.UNBIND_HOTKEY && App.HotkeysManager.Hotkeys.TryGetValue(fullKey, out List<string> values))
                                {
                                    // Go through and call all the Play methods on these id's
                                    foreach (string item in values)
                                    {
                                        if (item == HotkeysManager.MASTER_STOP_SOUND_HOTKEY)
                                        {
                                            HotkeysManager.StopAllSound();
                                        }
                                        else
                                        {
                                            var clip = App.SoundClipManager.GetClip(item);
                                            clip?.PlayAudio();
                                        }

                                    }
                                }
                            }

                            break;
                        }
                }
            }

            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        private static string fullKey
        {
            get
            {
                if (specialKey.Count > 0)
                {
                    string full = "";
                    foreach (string key in specialKey)
                    {
                        full += key + "|";
                    }
                    return full + currentKey;
                }
                else
                {
                    return currentKey;
                }
            }
        }

        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string GetFriendlyKeyName(int keycode)
        {
            //Debug.WriteLine(keycode);
            if (keycode >= 48 && keycode <= 57)
            {
                return (keycode - 48) + "";
            }
            if (keycode >= 65 && keycode <= 90)
            {
                return alphabet[keycode - 65] + "";
            }
            if (keycode >= 96 && keycode <= 105)
            {
                return "Num" + (keycode - 96);
            }
            if (keycode >= 112 && keycode <= 123)
            {
                return "F" + (keycode - 111);
            }
            switch (keycode)
            {
                case 9: return "Tab";
                case 13: return "Enter";
                case 20: return "Caps";
                case 27: return "Esc";
                case 32: return "Space";
                case 33: return "PgUp";
                case 34: return "PgDown";
                case 35: return "End";
                case 36: return "Home";
                case 37: return "LArrow";
                case 38: return "UArrow";
                case 39: return "RArrow";
                case 40: return "DArrow";
                case 46: return "Del";
                case 91: return "LWin";
                case 92: return "RWin";
                case 106: return "Num*";
                case 107: return "Num+";
                case 109: return "Num-";
                case 110: return ".";
                case 111: return "Num/";
                case 144: return "NumLck";
                case 160: return "LShift";
                case 161: return "RShift";
                case 162: return "LCtrl";
                case 163: return "RCtrl";
                case 164: return "LAlt";
                case 165: return "RAlt";
                case 186: return ";";
                case 187: return "=";
                case 188: return ",";
                case 189: return "-";
                case 191: return "/";
                case 219: return "[";
                case 220: return "\\";
                case 221: return "]";
                case 222: return "\'";
            }
            return "";

        }

        public void SetSoundClipHotkey(SoundClip clip, Action<SoundClip, string> callback)
        {
            soundClipCallbacks.Add((clip, callback));
        }

        public void SetGlobalStopHotkey(Config config, Action<Config, string> callback)
        {
            globalStopCallback = (config, callback);
        }

        public void Dispose()
        {
            _instance = null;
            UnhookWindowsHookEx(winHook);
        }

        private static bool KeySpecial(string key)
        {
            if (key == "LAlt" || key == "RAlt" || key == "LCtrl" || key == "RCtrl" || key == "LShift" || key == "RShift")
            {
                return true;
            }
            return false;
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