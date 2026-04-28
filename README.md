# PC-Remote-NET

## RU

### Что это

PC-Remote-NET это приложение для удаленного управления компьютером с телефона на Android.

Идея простая:

- на компьютер ставится Windows-программа
- на телефон ставится Android-приложение
- устройства связываются между собой
- после этого телефон работает как пульт для ПК

Проект рассчитан не на системных администраторов, а на обычного пользователя, которому нужно включить компьютер, подключиться к нему и отправить нужную команду без сложных корпоративных решений.

### Как это работает в обычных словах

На компьютере работает установленная программа. Она ждёт команды от твоего телефона.

Когда ты нажимаешь кнопку в мобильном приложении:

1. телефон отправляет запрос
2. компьютер получает его через локальную сеть или через интернет
3. установленная Windows-часть выполняет нужное действие

Если компьютер выключен, приложение может сначала попытаться включить его через Wake-on-LAN, а уже потом работать с ним дальше.

### Два режима работы

#### 1. Локальная сеть

Это самый простой вариант.

Он подходит, когда:

- телефон и компьютер подключены к одному роутеру
- оба устройства находятся в одной домашней сети

В этом режиме приложение работает через локальный IP-адрес компьютера, например `192.168.0.100`.

Плюс этого режима в том, что он проще и стабильнее.  
Ничего не нужно открывать наружу в интернет.

#### 2. Через интернет

Этот режим нужен, если ты хочешь управлять своим ПК не из дома, а из любой точки.

В этом случае приложение подключается к твоему роутеру через внешний адрес, а роутер уже перенаправляет запрос на нужный компьютер внутри дома.

Для этого обычно нужны:

- белый внешний IP-адрес
- проброс порта на роутере
- правильная настройка Wake-on-LAN

### Что такое проброс порта

Если объяснить совсем просто, проброс порта говорит роутеру:

> "Когда команда приходит из интернета на определённый порт, отправь её вот этому компьютеру внутри сети."

Без этого роутер не понимает, какому именно устройству внутри дома нужно передать внешний запрос.

Для PC-Remote-NET это важно, если ты хочешь:

- включать ПК через интернет
- отправлять команды не из локальной сети, а извне

### Что такое белый внешний IP

Белый внешний IP это настоящий публичный адрес твоего интернета, который виден снаружи.

Если он есть, твой роутер можно найти из интернета напрямую.  
Именно это и нужно для нормального внешнего доступа к домашнему ПК.

Примерно это выглядит так:

- у тебя дома есть роутер
- провайдер выдаёт ему внешний IP
- приложение подключается к этому адресу
- роутер пересылает команду на твой ПК

### Что такое CGNAT

CGNAT это ситуация, когда провайдер не даёт тебе настоящий внешний IP, а сажает сразу много клиентов за один общий адрес.

Для пользователя это означает следующее:

- интернет дома работает
- сайты открываются
- но подключиться к своему дому снаружи напрямую нельзя

Именно поэтому при CGNAT часто не работают:

- проброс портов
- прямой доступ к домашнему ПК из интернета
- Wake-on-LAN через внешний адрес

Если коротко:  
при CGNAT роутер у тебя дома может быть настроен правильно, но извне до него всё равно невозможно достучаться напрямую.

### Что такое DDNS

DDNS это способ привязать постоянное доменное имя к твоему внешнему IP-адресу.

Это нужно, если провайдер меняет внешний IP время от времени.

Тогда вместо того чтобы каждый раз искать новый адрес, ты используешь один и тот же домен, например:

- `myhome.ddns.net`

Смысл очень простой:

- провайдер поменял IP
- DDNS обновил привязку
- приложение продолжает использовать то же доменное имя

DDNS полезен только тогда, когда у тебя вообще есть белый внешний IP.  
Если у тебя CGNAT, один только DDNS проблему не решит.

### Wake-on-LAN простыми словами

Wake-on-LAN позволяет отправить специальный сетевой сигнал, чтобы компьютер включился.

Но для этого недостаточно просто поставить приложение.  
Должны совпасть сразу несколько условий:

- функция Wake-on-LAN включена в BIOS/UEFI
- сетевая карта в Windows разрешает пробуждение
- компьютер подключён по Ethernet, а не по Wi-Fi
- роутер настроен корректно

Если одно из этих условий не выполнено, включение по сети может не работать даже при правильном приложении.

### Что умеет проект

- связывать телефон и компьютер
- работать внутри одной сети
- работать через интернет при правильной сетевой настройке
- отправлять команды на ПК
- использовать фоновую Windows-службу для выполнения действий
- использовать Wake-on-LAN для включения компьютера

### Как проект устроен без лишней технички

Внутри есть три части:

- `Windows Client` — видимая программа на компьютере
- `Windows Service` — фоновая часть, которая реально выполняет команды
- `Android Client` — мобильное приложение

Пользователю не нужно вникать в код.  
Важно понимать только одно: мобильное приложение отправляет запрос, а Windows-часть принимает его и делает нужное действие на компьютере.

### Загрузка

Для обычного пользователя правильнее скачивать готовые сборки через GitHub Releases, а не из исходников репозитория.

Там должны лежать:

- Android APK
- Windows installer

Пока релизная схема не настроена, в репозитории уже есть готовые файлы:

- Android: [PC Remote NET.apk](./downloads/PC%20Remote%20NET.apk)
- Windows: [PCRemoteSetup.exe](./downloads/PCRemoteSetup.exe)

---

## EN

### What It Is

PC-Remote-NET is an application for controlling a computer remotely from an Android phone.

The idea is simple:

- install the Windows app on the computer
- install the Android app on the phone
- link the devices together
- use the phone as a remote control for the PC

This project is aimed at normal users, not system administrators. It is meant for someone who wants to wake up a computer, connect to it, and send commands without using heavy enterprise tools.

### How It Works in Simple Terms

The installed Windows side runs on the computer and waits for commands from the phone.

When you press a button in the mobile app:

1. the phone sends a request
2. the computer receives it through the local network or through the internet
3. the Windows side performs the requested action

If the computer is powered off, the app can first try to wake it up through Wake-on-LAN and then continue working with it.

### Two Connection Modes

#### 1. Local Network

This is the easiest mode.

It is used when:

- the phone and the computer are connected to the same router
- both devices are inside the same home network

In this mode, the app connects through the computer's local IP address, such as `192.168.0.100`.

This is the simplest and most reliable setup because nothing has to be exposed to the internet.

#### 2. Through the Internet

This mode is needed if you want to control your PC while you are away from home.

In that case, the app connects to your router through an external address, and the router forwards the request to the correct computer inside your home network.

This usually requires:

- a public external IP address
- port forwarding on the router
- proper Wake-on-LAN setup

### What Port Forwarding Means

In simple words, port forwarding tells your router:

> "When something arrives from the internet on this port, send it to this specific computer inside the network."

Without that, the router does not know which device inside your home should receive the incoming request.

For PC-Remote-NET, this matters if you want to:

- wake up the PC over the internet
- send commands from outside your local network

### What a Public External IP Is

A public external IP is a real internet-facing address assigned to your connection.

If you have one, your router can be reached directly from the internet.  
That is what makes direct remote access possible.

The rough idea is:

- your router gets an external IP from the provider
- the app connects to that address
- the router passes the request to the PC inside your home

### What CGNAT Is

CGNAT means your provider does not give you a real public IP and instead places many customers behind one shared external address.

For the user, that usually means:

- the home internet works normally
- websites open as usual
- but direct incoming access from the internet is not available

Because of that, CGNAT often breaks:

- port forwarding
- direct access to a home PC from outside
- Wake-on-LAN through an external address

In short:  
with CGNAT, your router may be configured correctly, but outside devices still cannot reach it directly.

### What DDNS Is

DDNS is a way to bind a permanent domain name to your changing external IP address.

This is useful when your provider changes your public IP from time to time.

Instead of checking the new address every time, you use one domain name, for example:

- `myhome.ddns.net`

The idea is simple:

- your provider changes the IP
- DDNS updates the domain mapping
- the app keeps using the same host name

DDNS is useful only if you actually have a public external IP.  
If you are behind CGNAT, DDNS alone will not solve the problem.

### Wake-on-LAN in Simple Terms

Wake-on-LAN lets a device send a special network signal that turns the computer on.

But installing the app alone is not enough.  
Several things must be set correctly:

- Wake-on-LAN must be enabled in BIOS/UEFI
- the network adapter in Windows must allow wake events
- the computer should use Ethernet instead of Wi-Fi
- the router should be configured correctly

If one of these conditions is missing, waking the PC over the network may fail even if the app itself is fine.

### What the Project Does

- links a phone and a computer
- works inside one local network
- works through the internet when the network is configured correctly
- sends commands to the PC
- uses a background Windows service to perform actions
- supports Wake-on-LAN for turning the computer on

### High-Level Structure

There are three main parts inside the project:

- `Windows Client` — the visible app on the computer
- `Windows Service` — the background part that actually performs commands
- `Android Client` — the mobile application

The user does not need to understand the source code.  
The key idea is simple: the mobile app sends the request, and the Windows side receives it and performs the action on the computer.

### Downloads

For normal users, the correct way to get the application is through GitHub Releases rather than through raw repository files.

That is where the project should provide:

- Android APK
- Windows installer

Until the release flow is fully set up, the repository already contains the current files:

- Android: [PC Remote NET.apk](./downloads/PC%20Remote%20NET.apk)
- Windows: [PCRemoteSetup.exe](./downloads/PCRemoteSetup.exe)
