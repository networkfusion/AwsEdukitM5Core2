// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Latest known working interpreter = `1.8.0.100`
// Perform updates using:
// nanoff --target M5Core2 --update --serialport COM16

using nanoFramework.M5Core2;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Threading;
using Console = nanoFramework.M5Stack.Console;
using Secrets; // Make sure you adjust the template

M5Core2.InitializeScreen();

Debug.WriteLine("Hello from M5Core2!");

// Give 60 seconds to the wifi join to happen
CancellationTokenSource cs = new(60000);
var success = WifiNetworkHelper.ConnectDhcp(WiFi.Ssid, WiFi.Password, requiresDateTime: true, token: cs.Token);
if (!success)
{
    // Something went wrong, you can get details with the ConnectionError property:
    Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
    if (WifiNetworkHelper.HelperException != null)
    {
        Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
    }
}

Debug.WriteLine("Network Setup complete.");
Console.WriteLine("Network Setup complete.");
AddStaticDisplayVariables();

M5Core2.TouchEvent += TouchEventCallback;

Thread.Sleep(Timeout.Infinite);

void ButtonHapticFeedback()
{
    M5Core2.Vibrate = true;
    Thread.Sleep(150);
    M5Core2.Vibrate = false;
}


void TouchEventCallback(object sender, TouchEventArgs e)
{
    const string StrLB = "LEFT BUTTON PRESSED  ";
    const string StrMB = "MIDDLE BUTTON PRESSED  ";
    const string StrRB = "RIGHT BUTTON PRESSED  ";
    const string StrXY1 = "TOUCHED at X= ";
    const string StrXY2 = ",Y= ";
    const string StrID = ",Id= ";
    const string StrDoubleTouch = "Double touch. ";
    const string StrMove = "Moving... ";
    const string StrLiftUp = "Lift up. ";

    Debug.WriteLine($"Touch Panel Event Received Category= {e.EventCategory} Subcategory= {e.TouchEventCategory}");
    Console.CursorLeft = 0;
    Console.CursorTop = 0;
    Console.Clear();

    Debug.WriteLine(StrXY1 + e.X + StrXY2 + e.Y + StrID + e.Id);
    Console.WriteLine(StrXY1 + e.X + StrXY2 + e.Y + StrID + e.Id + "  ");

    if ((e.TouchEventCategory & TouchEventCategory.LeftButton) == TouchEventCategory.LeftButton)
    {
        ButtonHapticFeedback();
        Debug.WriteLine(StrLB);
        Console.WriteLine(StrLB);
    }
    else if ((e.TouchEventCategory & TouchEventCategory.MiddleButton) == TouchEventCategory.MiddleButton)
    {
        ButtonHapticFeedback();
        Debug.WriteLine(StrMB);
        Console.WriteLine(StrMB);
    }
    else if ((e.TouchEventCategory & TouchEventCategory.RightButton) == TouchEventCategory.RightButton)
    {
        ButtonHapticFeedback();
        Debug.WriteLine(StrRB);
        Console.WriteLine(StrRB);
    }

    if ((e.TouchEventCategory & TouchEventCategory.Moving) == TouchEventCategory.Moving)
    {
        Debug.WriteLine(StrMove);
        Console.Write(StrMove);
    }

    if ((e.TouchEventCategory & TouchEventCategory.LiftUp) == TouchEventCategory.LiftUp)
    {
        Debug.WriteLine(StrLiftUp);
        Console.Write(StrLiftUp);
    }

    if ((e.TouchEventCategory & TouchEventCategory.DoubleTouch) == TouchEventCategory.DoubleTouch)
    {
        Debug.WriteLine(StrDoubleTouch);
        Console.Write(StrDoubleTouch);
    }
    AddStaticDisplayVariables();
}

void AddStaticDisplayVariables()
{
    Console.WriteLine("");
    Debug.WriteLine($"IP = {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
    Console.WriteLine($"IP = {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
    Debug.WriteLine($"RTC = {DateTime.UtcNow}");
    Console.WriteLine($"RTC = {DateTime.UtcNow}");
    Debug.WriteLine($"CPU_T = {M5Core2.Power.GetInternalTemperature().DegreesCelsius}°C");
    Console.WriteLine($"CPU_T = {M5Core2.Power.GetInternalTemperature().DegreesCelsius}_C");
    //Debug.WriteLine($"GYRO = {M5Core2.AccelerometerGyroscope.GetGyroscope()}");
    //Console.WriteLine($"GYRO = {M5Core2.AccelerometerGyroscope.GetGyroscope()}");
}
