/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
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
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Amplitude.Helpers
{
    public class SharpKeyboardHook : IKeyboardHook
    {
        private static IGlobalHook sharpHook = new TaskPoolGlobalHook();
        private static List<(SoundClip clip, Action<SoundClip, string> callback)> soundClipCallbacks = new();
        private static (Config? config, Action<Config, string> callback) globalStopCallback;

        private static readonly SortedSet<string> keySet = new();
        private readonly object keySetLock = new();

        private static SharpKeyboardHook? _instance;
        public static SharpKeyboardHook Instance => _instance ??= new SharpKeyboardHook();

        private SharpKeyboardHook()
        {
            sharpHook.KeyPressed += HandleKeyPressed;
            sharpHook.KeyReleased += HandleKeyReleased;
            sharpHook.RunAsync();
        }

        private void HandleKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            var currentKey = GetFriendlyKeyName(e.Data.KeyCode);
            lock (keySetLock)
            {
                keySet.Remove(currentKey);
            }
        }

        private void HandleKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            var currentKey = GetFriendlyKeyName(e.Data.KeyCode);

            if (currentKey == "" || currentKey == "LWin" || currentKey == "RWin" || currentKey == "Undefined")
            {
                return;
            }

            if (IsKeySpecial(currentKey))
            {
                lock (keySetLock)
                {
                    keySet.Add(currentKey);
                }
            }
            else
            {
                ProcessHotkey(currentKey);
            }
        }

        private void ProcessHotkey(string currentKey)
        {
            var fullKey = FullKey(currentKey);

            if (currentKey == "Esc")
            {
                currentKey = HotkeysManager.UNBIND_HOTKEY;
            }

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

        private string FullKey(string currentKey)
        {
            lock (keySetLock)
            {
                var keySetCopy = new SortedSet<string>(keySet);
            }

            if (keySet.Count > 0)
            {
                string full = "";
                foreach (string key in keySet)
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

        private static bool IsKeySpecial(string key)
        {
            if (key == "LAlt" || key == "RAlt" || key == "LCtrl" || key == "RCtrl" || key == "LShift" || key == "RShift")
            {
                return true;
            }
            return false;
        }

        private static string GetFriendlyKeyName(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.VcTab: return "Tab";
                case KeyCode.VcEnter: return "Enter";
                case KeyCode.VcCapsLock: return "Caps";
                case KeyCode.VcEscape: return "Esc";
                case KeyCode.VcSpace: return "Space";
                case KeyCode.VcPageUp: return "PgUp";
                case KeyCode.VcPageDown: return "PgDown";
                case KeyCode.VcEnd: return "End";
                case KeyCode.VcHome: return "Home";
                case KeyCode.VcLeft: return "LArrow";
                case KeyCode.VcUp: return "UArrow";
                case KeyCode.VcRight: return "RArrow";
                case KeyCode.VcDown: return "DArrow";
                case KeyCode.VcDelete: return "Del";
                case KeyCode.VcLeftMeta: return "LWin";
                case KeyCode.VcRightMeta: return "RWin";
                case KeyCode.VcPeriod: return ".";
                case KeyCode.VcNumPadDecimal: return "Num.";
                case KeyCode.VcNumPadEnter: return "NumEnter";
                case KeyCode.VcNumPadMultiply: return "Num*";
                case KeyCode.VcNumPadAdd: return "Num+";
                case KeyCode.VcNumPadSubtract: return "Num-";
                case KeyCode.VcNumPadDivide: return "Num/";
                case KeyCode.VcNumLock: return "NumLck";
                case KeyCode.VcLeftShift: return "LShift";
                case KeyCode.VcRightShift: return "RShift";
                case KeyCode.VcLeftControl: return "LCtrl";
                case KeyCode.VcRightControl: return "RCtrl";
                case KeyCode.VcLeftAlt: return "LAlt";
                case KeyCode.VcRightAlt: return "RAlt";
                case KeyCode.VcSemicolon: return ";";
                case KeyCode.VcEquals: return "=";
                case KeyCode.VcComma: return ",";
                case KeyCode.VcMinus: return "-";
                case KeyCode.VcSlash: return "/";
                case KeyCode.VcBackslash: return "\\";
                case KeyCode.VcOpenBracket: return "[";
                case KeyCode.VcCloseBracket: return "]";
                case KeyCode.VcQuote: return "\'";
                case KeyCode.VcBackQuote: return "`";
                case KeyCode.VcNumPad0: return "Num0";
                case KeyCode.VcNumPad1: return "Num1";
                case KeyCode.VcNumPad2: return "Num2";
                case KeyCode.VcNumPad3: return "Num3";
                case KeyCode.VcNumPad4: return "Num4";
                case KeyCode.VcNumPad5: return "Num5";
                case KeyCode.VcNumPad6: return "Num6";
                case KeyCode.VcNumPad7: return "Num7";
                case KeyCode.VcNumPad8: return "Num8";
                case KeyCode.VcNumPad9: return "Num9";
            }

            return keycode.ToString()[2..];
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
            sharpHook.KeyPressed -= HandleKeyPressed;
            sharpHook.KeyReleased -= HandleKeyReleased;
            sharpHook.Dispose();
        }
    }
}