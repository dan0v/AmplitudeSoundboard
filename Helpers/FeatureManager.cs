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

#if Windows
            os = OSPlatform.Windows;
#endif
#if MacOS
            os = OSPlatform.OSX;
#endif
#if Linux
            os = OSPlatform.Linux;
#endif

            if (!os.HasValue)
            {
                string osName = RuntimeInformation.OSDescription;
                App.WindowManager.ShowErrorString(osName);
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
                    { OSPlatform.OSX, true },
                    { OSPlatform.Linux, true },
                }
            },
            {
                Feature.CUSTOM_TITLEBAR,
                new()
                {
                    { OSPlatform.Windows, true },
                    { OSPlatform.OSX, true },
                    { OSPlatform.Linux, false },
                }
            }
        };

        public enum Feature
        {
            HOTKEYS,
            CUSTOM_TITLEBAR,
        }
    }
}
