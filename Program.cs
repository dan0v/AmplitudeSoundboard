/*
    AmplitudeSoundboard
    Copyright (C) 2021-2026 dan0v
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

using Amplitude.Helpers;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using ReactiveUI.Avalonia.Splat;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AmplitudeSoundboard
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register Lazy<T> factory for circular dependency support
            services.AddTransient(typeof(Lazy<>), typeof(LazyService<>));

            // Register all helper services as singletons
            // Order matters: services are listed in dependency order (dependencies first)
            services.AddSingleton<JsonIoManager>();
            services.AddSingleton<IKeyboardHook, SharpKeyboardHook>();
            services.AddSingleton<ISoundEngine, MSoundEngine>();
            services.AddSingleton<ThemeManager>();
            services.AddSingleton<OutputProfileManager>();
            services.AddSingleton<HotkeysManager>();
            services.AddSingleton<ConfigManager>();
            services.AddSingleton<WindowManager>();
            services.AddSingleton<SoundClipManager>();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUIWithMicrosoftDependencyResolver(ConfigureServices);
    }

    /// <summary>
    /// Enables Lazy{T} resolution from the DI container for circular dependency support.
    /// </summary>
    internal sealed class LazyService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(IServiceProvider serviceProvider)
        : Lazy<T>(serviceProvider.GetRequiredService<T>) where T : class;
}
