﻿/*
    AmplitudeSoundboard
    Copyright (C) 2021-2022 dan0v
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

using AmplitudeSoundboard;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Amplitude.Helpers
{
    public class FeatureManager
    {
        public static bool IsFeatureEnabled(Feature feature)
        {
            OSPlatform platform = GetPlatform();

            bool enabled = false;
            if (Features.TryGetValue(feature, out var platforms))
            {
                if (platforms.TryGetValue(platform, out bool enabled_))
                {
                    enabled = enabled_;
                }
            }
            return enabled;
        }

        private static OSPlatform GetPlatform()
        {
            OSPlatform? os = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = OSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = OSPlatform.OSX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = OSPlatform.Linux;
            }
            if (!os.HasValue)
            {
                string osName = RuntimeInformation.OSDescription;
                App.WindowManager.ErrorList.AddErrorString(osName);
                return OSPlatform.Create(osName);
            }

            return os.Value;
        }

        public static readonly Dictionary<Feature, Dictionary<OSPlatform, bool>> Features = new()
        {
            {
                Feature.HOTKEYS,
                new()
                {
                    { OSPlatform.Windows, true },
                    { OSPlatform.OSX, false },
                    { OSPlatform.Linux, false },
                }
            }
        };

        public enum Feature
        {
            HOTKEYS
        }
    }
}
