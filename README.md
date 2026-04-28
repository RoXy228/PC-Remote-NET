# PC-Remote-NET

## RU

### Что это

PC-Remote-NET это проект для удаленного управления компьютером с телефона.  
Проще говоря: ставишь программу на ПК, ставишь приложение на Android, связываешь их между собой и после этого можешь управлять нужными функциями компьютера без прямого доступа к нему.

### Для кого это

Проект сделан для обычного пользователя, которому нужно:

- быстро подключиться к своему ПК
- отправить команду с телефона
- выполнить базовые действия без сложной настройки
- использовать отдельное приложение, а не перегруженные корпоративные решения

### Как это работает

Внутри проект состоит из двух основных частей:

- Windows-программа на компьютере
- Android-приложение на телефоне

Сценарий работы простой:

1. На компьютере устанавливается программа, которая принимает команды.
2. На телефоне открывается мобильное приложение.
3. Устройства связываются между собой через QR-код.
4. После привязки телефон получает доступ к управлению компьютером.
5. Когда ты нажимаешь действие в приложении, команда отправляется на ПК и выполняется там.

То есть телефон сам ничего не делает с компьютером напрямую.  
Он только отправляет команду, а уже установленная программа на Windows принимает ее и выполняет нужное действие.

### Что умеет проект

- связывать телефон и компьютер через QR-код
- отправлять команды на ПК
- работать через отдельный фоновый сервис на Windows
- использовать защищенный обмен данными между устройствами
- давать простой способ управления без ручной настройки команд

### Как устроено с точки зрения пользователя

Для пользователя все выглядит так:

- на ПК ставится Windows-версия
- на телефон ставится Android-версия
- один раз выполняется привязка
- дальше телефон используется как пульт управления для компьютера

### Установщики

Для использования проекта нужны готовые установочные файлы:

- Android: [PC Remote NET.apk](./downloads/PC%20Remote%20NET.apk)
- Windows: [PCRemoteSetup.exe](./downloads/PCRemoteSetup.exe)

### Архитектура проекта

Без лишней технической детализации проект делится на три части:

- `Windows Client` — интерфейс на Windows
- `Windows Service` — фоновая часть, которая реально выполняет команды на компьютере
- `Android Client` — мобильное приложение для управления

Смысл этой схемы простой:  
интерфейс показывает состояние и принимает действия от пользователя, а отдельный сервис на ПК отвечает за выполнение команд более стабильно и безопасно.

### Главное о проекте

Это не просто один `.exe` и не просто мобильное приложение.  
Это связка из настольной части, фонового сервиса и Android-клиента, которые работают вместе как единая система удаленного управления компьютером.

---

## EN

### What It Is

PC-Remote-NET is a project for controlling a computer remotely from a phone.  
In simple terms: you install the PC app, install the Android app, connect them together, and then use your phone to control selected computer functions.

### Who It Is For

This project is made for regular users who want to:

- connect to their PC quickly
- send commands from a phone
- perform basic remote actions without complicated setup
- use a focused personal tool instead of heavy enterprise software

### How It Works

The project has two main user-facing parts:

- a Windows application on the computer
- an Android application on the phone

The flow is simple:

1. You install the Windows app on the PC.
2. You open the Android app on the phone.
3. The devices are paired using a QR code.
4. After pairing, the phone is linked to the computer.
5. When you press an action in the app, the command is sent to the PC and executed there.

So the phone does not directly control the computer by itself.  
It sends a request, and the installed Windows side receives that request and performs the action on the machine.

### What the Project Does

- pairs a phone and a computer through a QR-based flow
- sends remote commands to the PC
- uses a dedicated background Windows service
- exchanges data between devices in a protected way
- provides a simpler control experience without manual command setup

### User View

From a normal user perspective, the experience is:

- install the Windows version on the PC
- install the Android version on the phone
- pair the devices once
- use the phone as a remote control for the computer

### Installers

The project is intended to be used through ready-made installers:

- Android: [PC Remote NET.apk](./downloads/PC%20Remote%20NET.apk)
- Windows: [PCRemoteSetup.exe](./downloads/PCRemoteSetup.exe)

### Project Architecture

At a high level, the project is split into three parts:

- `Windows Client` — the desktop interface
- `Windows Service` — the background component that actually executes commands on the PC
- `Android Client` — the mobile control app

The reason for this structure is simple:  
the visible app handles interaction, while the background service on Windows is responsible for stable command execution.

### Key Idea

This is not just a standalone `.exe` and not just a mobile app.  
It is a complete remote-control system where the desktop side, background service, and Android client work together.
