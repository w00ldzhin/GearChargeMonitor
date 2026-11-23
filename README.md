# GearChargeMonitor

[English](README.md) | [Русский](README.ru.md)

A simple Windows tray app to check the battery level of my wireless mouse without keeping the official software running in the background

## Features
* Shows the battery percentage right on the tray icon (updates every 30 seconds)
* Changes text color depending on the status:
  * **White** - normal mode
  * **Green** - charging
  * **Red** - low battery (`<= 20%`)
  * **Gray** - mouse is asleep (shows the last known percentage) or disconnected (`?`)
* Autostart with Windows via a checkbox in the tray menu

## Supported devices
* **Redragon Impact Elite (M913)** - 2.4G dongle (`VID: 25A7`, `PID: FA07`)

*(I plan to add Bluetooth devices like headphones and gamepads later)*

## How it works
2.4G mice don't send their battery level to Windows by default. I sniffed the USB packets from the official Redragon app using Wireshark + USBPcap to see how it asks the dongle for the charge:
1. Send a 17-byte request (`0x08 0x04 ... 0x49`) to the `Col07` interface
2. Read the 17-byte response (`0x09 0x04 ...`) from `Col05` - byte `6` is the battery level (`0–10`, so I just multiply it by `10`), and byte `7` shows if the cable is plugged in (`0x01`)

## Built with
* C# / .NET / WPF
* HidSharp
* H.NotifyIcon.Wpf
* System.Drawing.Common

## How to run
1. Download the archive from [Releases](https://github.com)
2. Unpack it and run `GearChargeMonitor.exe`
3. Drag the icon from the hidden tray menu (`^`) to the taskbar so you can always see the percentage

If you want to build it yourself, just clone the repo and open `GearChargeMonitor.sln` in Visual Studio
