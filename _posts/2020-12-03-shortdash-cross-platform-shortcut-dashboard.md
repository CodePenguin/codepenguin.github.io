---
layout: post
title: 'ShortDash: Cross-platform Shortcut Dashboard'
date: 2020-12-03 22:04:55.000000000 -05:00
tags:
- blazor
- dotnet
- programming
- shortdash
permalink: "/2020/12/03/shortdash-cross-platform-shortcut-dashboard/"
---
A few months ago, I was sitting in a video conference and noticed that the room was getting hot. I have a fan but it was just out of reach and it would have been rude to walk out of the frame in the middle of the meeting. At that point I wished I had one of those digital power plugs that you can control from your phone. I did eventually get one of those plugs but you had to use their dedicated app to control it. I thought it would be much more convenient to have a central dashboard where I could control it. And then it would be nice to be able to execute other things. This led to some tinkering with [https://ifttt.com/](https://ifttt.com/) and its web request functionality. Soon after, the idea of [ShortDash](https://github.com/CodePenguin/shortdash) began to form.

![ShortDash Dashboard]({{ site.baseurl }}/assets/2020/12/dashboard.png)

ShortDash is a cross-platform shortcut dashboard for your local network. It allows you to create customizable dashboards of shortcuts and actions. You can even turn an old tablet or cell phone into your own personal shortcut dashboard.

<!--more-->
<a name="more" />

A server runs on your local network and allows any device with a web browser to access the dashboards. A cross-platform target application allows actions to be executed on any Windows, Mac or Linux machine. All controlled from a single ShortDash server. Each device can be individually configured to allow full access or just specific dashboards.

![ShortDash Architecture]({{ site.baseurl }}/assets/2020/12/architecture.png)

Shortcuts can be created by customizing existing actions to execute programs, web requests, etc. Multiple actions can be executed with a single shortcut to create advanced workflows. An open plugin architecture allows anyone to quickly create new actions to fit any workflow required.

With ShortDash, I am now able to use an older generation Kindle Fire tablet as a secondary screen that always has my dashboard display. With one tap on a "Fan On" shortcut on my dashboard, a "Web Request" action is executed on the ShortDash server that sends an HTTPS request to IFTTT which then executes the "Turn plug on" action which finally turns on my fan. When I press the shortcut again, a different message is sent to turn the fan off. IFTTT can integrate with countless other products and services.

That is just one example of what ShortDash can do. The open plugin architecture allows anyone to quickly create new plugins to perform any action that is required. But even if you aren't a programmer, a lot can be accomplished with the two main core actions: "Web Request" and "Execute Process". You can quickly create shortcuts that will open your favorite programs or open the websites you read everyday. If you have multiple machines, you can install the Target application and execute actions on Windows, MacOS and Linux, all from the same ShortDash dashboard. The possibilities are endless and we have just barely scratched the surface of the fun and exciting things that you can do with ShortDash.

ShortDash is written in [ASP.NET Core](https://dotnet.microsoft.com/learn/aspnet/what-is-aspnet-core) using the [Blazor Server](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) architecture.

ShortDash is open source and freely available [https://github.com/CodePenguin/shortdash/releases](https://github.com/CodePenguin/shortdash/releases) for Windows, MacOS and Linux.