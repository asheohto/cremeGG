<div align="center">

  <img src="assets/logo.png" alt="CremeGG Logo" width="180" />

  # CremeGG

  Steelseries's virtual devices annihilator.

  [![Latest Release](https://img.shields.io/github/v/release/ashemarya/CremeGG?style=for-the-badge&logo=tag&color=f39c12)](https://github.com/ashemarya/CremeGG/releases/latest)
  [![Downloads](https://img.shields.io/github/downloads/ashemarya/CremeGG/total?style=for-the-badge&logo=github&color=3498db)](https://github.com/ashemarya/CremeGG/releases)
  [![License](https://img.shields.io/badge/License-MIT-2ecc71?style=for-the-badge)](LICENSE)
  [![Support on Ko-fi](https://img.shields.io/badge/Ko--fi-omoretti-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/omoretti)

</div>

---

## Support

<a href='https://ko-fi.com/omoretti' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi5.png' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

If CremeGG helps you keep your sound settings clean, you can support the project on [Ko-fi](https://ko-fi.com/omoretti).

---

## About

SteelSeries GG installs virtual audio devices for Sonar, including Gaming, Chat, Media, Aux, and Stream. These devices often change your default Windows playback settings, create audio routing bugs, and clutter the sound menu.

CremeGG turns off these virtual devices automatically. It runs for three seconds when Windows starts, disables the devices you selected, and exits. It does not stay open in your system tray or use RAM in the background.

The app is written in C# and compiled with .NET 9 Native AOT. It runs directly as native code without requiring a separate .NET runtime.



---

## Features

- Native AOT binary: Starts instantly and uses zero background memory after exiting.
- Fast device disable: Turns off unwanted Sonar endpoints in parallel.
- Startup check: Verifies that SteelSeries GG did not recreate endpoints while Windows was booting.
- Clean exit: Runs, cleans your audio list, and shuts down completely. No background task or tray icon.
- Transparent notification: Shows a quick slide-in alert so you know the devices were disabled.
- Setup wizard: Lets you pick which virtual devices to hide and which to keep.
- Windows startup: Adds itself to startup with one click during setup.
  
  <img width="320" height="240" alt="0928" src="https://github.com/user-attachments/assets/20de7702-0b91-401d-aa25-c42a4d5ade3e" />

---

## Installation

1. Go to the [Releases](https://github.com/ashemarya/CremeGG/releases) page.
2. Download the latest `CremeGG.zip` and extract it to a folder.
3. Open `CremeGG.exe`.
4. Choose the audio devices you want to disable.
5. Check "Run CremeGG automatically on startup" and click Finish.

CremeGG will now run once every time you log in to Windows.

### Command line options

| Command | Action |
| :--- | :--- |
| `CremeGG.exe` | Cleans audio devices, shows notification, and exits. |
| `CremeGG.exe --setup` | Opens the setup window to change your settings. |
| `CremeGG.exe --now` | Cleans devices immediately without the startup wait. |
| `CremeGG.exe --notify` | Tests the slide-in notification animation. |

---

## Tags

steelseries sonar, remove steelseries sonar, disable steelseries sonar, sonar audio cleaner, steelseries bloatware remover, steelseries gg, virtual audio devices, remove virtual audio devices, windows audio cleaner, sound device manager, soundvolumeview, native aot, dotnet 9, windows 11 audio, gaming audio fix
