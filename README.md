# Amplitude Soundboard
![GitHub Logo](Branding/Banner.png)
[![Release .NET Cross Platform App](https://github.com/dan0v/AmplitudeSoundboard/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/dan0v/AmplitudeSoundboard/actions/workflows/dotnet-desktop.yml)
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-13-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

# [Website](https://amplitude-soundboard.dan0v.com/index.html)

## Features
- Cross-platform availability: Windows, Linux, and MacOS
- Play audio files from many formats
  |  |  |
  | - | - |
  | AAC | AIFF |
  | ALAC | FLAC |
  | M4A | MP3 |
  | MP4 | OGG |
  | OPUS | WAV |

- Customize a grid view of your sound clips to your liking
- Trigger sound clips with custom hotkeys or a button press
- Set output device and volume at a per-clip level
- Toggle between light and dark theme
- Automatically update to the newest version if desired
- Localized versions available ([work in progress](https://github.com/dan0v/AmplitudeSoundboard/issues/7))
  |  |  |  |
  | - | - | - |
  | English | Español | Italiano |
  | Magyar | Nederlands | Polski |
  | Pусский |  |  |

## Installation
### Windows *(x64)*
1. Download and unzip [latest Windows build](https://git.dan0v.com/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_win_x86_64.zip) from the [Releases page](https://git.dan0v.com/AmplitudeSoundboard/releases/).
2. Run executable (`amplitude_soundboard.exe`).
3. If you would like to play sound through an input device like a microphone, set up a program like [Virtual Audio Cable](https://vac.muzychenko.net/en/download.htm) and set the newly created virtual cable as your clip output device.

### MacOS *(x64)*
1. Download and unzip [latest MacOS build](https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_macOS_x86_64.tar.gz) from the [Releases page](https://git.dan0v.com/AmplitudeSoundboard/releases/).
2. Run executable (`Amplitude Soundboard.app`).
 - If a security warning blocks the app from running, open `System Preferences -> Privacy & Security -> General` and click `Open Anyway`. This is due to not signing the app with an Apple Developer ID.
 - In order to use hotkeys, Amplitude Soundboard must be given accessibility permission to control your computer. This can be done under `System Preferences -> Privacy & Security -> Accessibility`. A popup should take you directly to this option at application startup. Amplitude Soundboard will need to be restarted after granting permissions for hotkeys to start working.
3. If you would like to play sound through an input device like a microphone, set up a program like [BlackHole](https://github.com/ExistentialAudio/BlackHole) and set the newly created virtual cable as your clip output device.

### Linux *(x64)*
1. Download and unzip [latest Linux build](https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard-x86_64.AppImage) from the [Releases page](https://git.dan0v.com/AmplitudeSoundboard/releases/).
2. Run executable (`Amplitude_Soundboard-x86_64.AppImage`).
 - Hotkeys will currently only work under x11, not Wayland, as supported by [SharpHook](https://github.com/TolikPylypchuk/SharpHook).
3. If you would like to play sound through an input device like a microphone, [set up a sink in PulseAudio](https://www.onetransistor.eu/2017/10/virtual-audio-cable-in-linux-ubuntu.html) and set the monitor output of it to your desired input device.

## Updates
### Windows
A dialog automatically notifies users of available updates at application startup. To automatically update, click `Update`, or, to manually update, just download the [latest Windows build](https://git.dan0v.com/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_win_x86_64.zip) from the [Releases page](https://git.dan0v.com/AmplitudeSoundboard/releases/) and replace your `amplitude_soundboard.exe` with the new version.

### MacOS
A dialog automatically notifies users of available updates at application startup. To manually update, just download the [latest MacOS build](https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard_macOS_x86_64.tar.gz) from the [Releases page](https://git.dan0v.com/AmplitudeSoundboard/releases/) and replace your `Amplitude Soundboard.app` with the new version.

### Linux
A dialog automatically notifies users of available updates at application startup. To manually update, just download the [latest Linux build](https://github.com/dan0v/AmplitudeSoundboard/releases/latest/download/Amplitude_Soundboard-x86_64.AppImage) from the [Releases page](https://git.dan0v.com/AmplitudeSoundboard/releases/) and replace your `Amplitude_Soundboard-x86_64.AppImage` with the new version.

## Screenshots
Main grid|Soundclip list
-|-
![Main grid](https://github.com/dan0v/AmplitudeSoundboard/raw/master/docs/assets/img/MainGrid.png) | ![Soundclip List](https://github.com/dan0v/AmplitudeSoundboard/raw/master/docs/assets/img/SoundclipList.png)

Editing a soundclip|Global settings
-|-
![Edit Soundclip](https://github.com/dan0v/AmplitudeSoundboard/raw/master/docs/assets/img/EditSoundClip.png)|![Global Settings](https://github.com/dan0v/AmplitudeSoundboard/raw/master/docs/assets/img/Settings.png)

## Contributors

Many thanks to all these people! ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/dan0v"><img src="https://avatars.githubusercontent.com/u/7658521?v=4?s=100" width="100px;" alt="dan0v"/><br /><sub><b>dan0v</b></sub></a><br /><a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=dan0v" title="Code">💻</a> <a href="https://github.com/dan0v/AmplitudeSoundboard/issues?q=author%3Adan0v" title="Bug reports">🐛</a> <a href="#design-dan0v" title="Design">🎨</a> <a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=dan0v" title="Documentation">📖</a> <a href="#maintenance-dan0v" title="Maintenance">🚧</a> <a href="#platform-dan0v" title="Packaging/porting to new platform">📦</a> <a href="https://github.com/dan0v/AmplitudeSoundboard/pulls?q=is%3Apr+reviewed-by%3Adan0v" title="Reviewed Pull Requests">👀</a> <a href="#userTesting-dan0v" title="User Testing">📓</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Taylor-Cozy"><img src="https://avatars.githubusercontent.com/u/19821121?v=4?s=100" width="100px;" alt="Taylor Hetherington"/><br /><sub><b>Taylor Hetherington</b></sub></a><br /><a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=Taylor-Cozy" title="Code">💻</a> <a href="#userTesting-Taylor-Cozy" title="User Testing">📓</a> <a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=Taylor-Cozy" title="Documentation">📖</a> <a href="#design-Taylor-Cozy" title="Design">🎨</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/John-Cozy"><img src="https://avatars.githubusercontent.com/u/36801893?v=4?s=100" width="100px;" alt="John"/><br /><sub><b>John</b></sub></a><br /><a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=John-Cozy" title="Code">💻</a> <a href="#userTesting-John-Cozy" title="User Testing">📓</a> <a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=John-Cozy" title="Documentation">📖</a> <a href="#design-John-Cozy" title="Design">🎨</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/scottajevans"><img src="https://avatars.githubusercontent.com/u/39980206?v=4?s=100" width="100px;" alt="scottajevans"/><br /><sub><b>scottajevans</b></sub></a><br /><a href="#translation-scottajevans" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/SonjaVredeveld"><img src="https://avatars.githubusercontent.com/u/10913197?v=4?s=100" width="100px;" alt="SVredeveld"/><br /><sub><b>SVredeveld</b></sub></a><br /><a href="#translation-SonjaVredeveld" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/GF1977"><img src="https://avatars.githubusercontent.com/u/13718260?v=4?s=100" width="100px;" alt="Ilia Opiakin"/><br /><sub><b>Ilia Opiakin</b></sub></a><br /><a href="#translation-GF1977" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/AntoSkate"><img src="https://avatars.githubusercontent.com/u/36473846?v=4?s=100" width="100px;" alt="Antonio Brugnolo"/><br /><sub><b>Antonio Brugnolo</b></sub></a><br /><a href="#translation-AntoSkate" title="Translation">🌍</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://ktos.info"><img src="https://avatars.githubusercontent.com/u/1633261?v=4?s=100" width="100px;" alt="Marcin Badurowicz"/><br /><sub><b>Marcin Badurowicz</b></sub></a><br /><a href="#translation-ktos" title="Translation">🌍</a> <a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=ktos" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Swell61"><img src="https://avatars.githubusercontent.com/u/32226560?v=4?s=100" width="100px;" alt="Samuel"/><br /><sub><b>Samuel</b></sub></a><br /><a href="#userTesting-swell61" title="User Testing">📓</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/teacup775"><img src="https://avatars.githubusercontent.com/u/2474259?v=4?s=100" width="100px;" alt="teacup775"/><br /><sub><b>teacup775</b></sub></a><br /><a href="https://github.com/dan0v/AmplitudeSoundboard/issues?q=author%3Ateacup775" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://chapien.net/"><img src="https://avatars.githubusercontent.com/u/2068976?v=4?s=100" width="100px;" alt="Chapien"/><br /><sub><b>Chapien</b></sub></a><br /><a href="https://github.com/dan0v/AmplitudeSoundboard/issues?q=author%3AChapien" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/deadnamedimmer"><img src="https://avatars.githubusercontent.com/u/43499958?v=4?s=100" width="100px;" alt="Dimmer"/><br /><sub><b>Dimmer</b></sub></a><br /><a href="https://github.com/dan0v/AmplitudeSoundboard/commits?author=deadnamedimmer" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://jew.pizza/"><img src="https://avatars.githubusercontent.com/u/873944?v=4?s=100" width="100px;" alt="David Cooper"/><br /><sub><b>David Cooper</b></sub></a><br /><a href="#platform-dtcooper" title="Packaging/porting to new platform">📦</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
