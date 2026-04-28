# PC-Remote-NET

PC-Remote-NET is a client-server project for remote PC control from Windows and Android. The repository contains the Windows service that executes commands on the target machine, the Windows desktop client, and the Android mobile client for pairing and control.

## What the project does

- connects a phone or desktop client to a Windows machine
- supports device pairing through QR flow
- sends remote commands to the PC
- includes service-side system actions such as shutdown timer and firewall management
- uses shared request/response models and security helpers for message exchange

## Repository structure

```text
windows/
  PCRemote.Core/        shared .NET models, networking, crypto, logging
  PCRemote.Service/     background Windows service that executes commands
  PCRemote.WPF/         WPF desktop client
  PCRemote.TestClient/  console test client
  installer/            installer scripts
  PCRemote.sln          Visual Studio solution

android/
  app/                  Android client on Jetpack Compose
  gradle/               Gradle wrapper and version catalog
```

## Main components

### Windows

- `PCRemote.Core` stores common contracts and infrastructure: `RemoteRequest`, `RemoteResponse`, networking helpers, logging, rate limiting, nonce handling, and cryptography.
- `PCRemote.Service` is the backend side for the PC. It runs as a worker/service and executes system-level actions through modules like `ShutdownTimer` and `FirewallManager`.
- `PCRemote.WPF` is the desktop interface for Windows. It contains the UI, QR window, tray menu, and IPC communication with the service.
- `PCRemote.TestClient` is a lightweight console project for testing command flow.

### Android

- Android app is built with Kotlin + Jetpack Compose.
- Uses CameraX and ML Kit for QR scanning.
- Contains screens for home, control, instructions, QR scanner, and settings.
- Includes network and crypto layers compatible with the Windows side.

## Tech stack

- .NET 8
- WPF
- Worker Service / Windows Services
- Kotlin
- Jetpack Compose
- CameraX
- ML Kit Barcode Scanning
- Gradle Kotlin DSL

## Running the project

### Windows solution

Open `windows/PCRemote.sln` in Visual Studio or build from terminal:

```powershell
dotnet build .\windows\PCRemote.sln
```

Run the service:

```powershell
dotnet run --project .\windows\PCRemote.Service\PCRemote.Service.csproj
```

Run the WPF client:

```powershell
dotnet run --project .\windows\PCRemote.WPF\PCRemote.WPF.csproj
```

### Android client

Open the `android` folder in Android Studio or build from terminal:

```powershell
cd .\android
.\gradlew.bat assembleDebug
```

## Notes

- Some Windows actions require administrator privileges.
- WPF client is Windows-only.
- Android app requires camera permission for QR pairing.
- The repository is intended for personal remote control scenarios, not enterprise remote administration.

## Status

The project is under active development. Desktop and Android parts are stored together to keep protocol, pairing flow, and feature development synchronized.
