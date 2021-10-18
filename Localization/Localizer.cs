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

using AmplitudeSoundboard;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace Amplitude.Localization
{
    class Localizer : INotifyPropertyChanged
    {
        public static readonly Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            { "English", "en" },
            { "Español", "es" },
            { "Nederlands", "nl" },
            { "Russian", "ru" },
        };

        private static Dictionary<string, string> inverseLanguages => Languages.ToDictionary(l => l.Value, l => l.Key);

        public readonly string FALLBACK_LANGUAGE = "English";

        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";
        private ResourceManager resources;

        private Localizer()
        {
            resources = new ResourceManager(typeof(Language));
            Invalidate();
        }

        public void LoadLanguage()
        {
            resources = new ResourceManager(typeof(Language));
            Invalidate();
        }

        public void ChangeLanguage(string language)
        {
            if (string.IsNullOrEmpty(language) || !Languages.ContainsKey(language))
            {
                language = FALLBACK_LANGUAGE;
            }

            CultureInfo.CurrentUICulture = new CultureInfo(Languages[language]);
            LoadLanguage();
        }

        public string TryUseSystemLanguageFallbackEnglish()
        {
            string curLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (inverseLanguages.TryGetValue(curLang, out string fullLang))
            {
                ChangeLanguage(fullLang);
                return fullLang;
            }

            ChangeLanguage(FALLBACK_LANGUAGE);
            return FALLBACK_LANGUAGE;
        }

        public string Language { get; private set; }

        public string this[string key]
        {
            get
            {
                string? ret = resources?.GetString(key)?.Replace(@"\\n", "\n");
                if (string.IsNullOrEmpty(ret))
                {
                    ret = $"Localize:{key}";
                }
                return ret;
            }
        }

        public static Localizer Instance { get; set; } = new Localizer();
        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
