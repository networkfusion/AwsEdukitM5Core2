// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Latest known working interpreter = `1.9.1.52`
// Perform updates using:
// nanoff --target M5Core2 --update --serialport COM5 --masserase

using nanoFramework.M5Core2;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Threading;
using Console = nanoFramework.M5Stack.Console;
using Secrets; // Make sure you adjust the template
using System.IO.Ports;
using nanoFramework.Hardware.Esp32;
using AwsEdukitM5Core2;

var ports = SerialPort.GetPortNames();
Debug.WriteLine("Available SerialPorts:");
foreach (var port in ports)
{
    Debug.WriteLine(port);
}

// Test the HMP155
// PORT-C (Blue) - UART
// pins 13 (RXD1 - GPIO3) / 14 (TXD1 - GPIO1)
Configuration.SetPinFunction(Gpio.IO13, DeviceFunction.COM3_RX);
Configuration.SetPinFunction(Gpio.IO14, DeviceFunction.COM3_TX);
var PortC_UART = "COM3";

var Hmp155 = new VaisalaHmp1xx(PortC_UART);
Hmp155.Open();

M5Core2.InitializeScreen();

Thread.Sleep(5000); // Helps with debug!
Debug.WriteLine("Hello from M5Core2!");
Console.WriteLine("Hello from M5Core2!");
Debug.WriteLine("Waiting for WiFi!...");
Console.WriteLine("Waiting for WiFi!...");
// Give 30 seconds to the wifi connection to happen
CancellationTokenSource cs = new(30_000);
var success = false;
while (!success)
{
    success = WifiNetworkHelper.ConnectDhcp(WiFi.Ssid, WiFi.Password, requiresDateTime: true, token: cs.Token);

    if (!success)
    {
        // Something went wrong, you can get details with the ConnectionError property:
        Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
        Console.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
        if (WifiNetworkHelper.HelperException != null)
        {
            Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
            Console.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
        }
    }
}

Console.Clear();


Debug.WriteLine("Network Setup complete.");
Console.WriteLine("Network Setup complete.");
AddStaticDisplayVariables();

M5Core2.TouchEvent += TouchEventCallback;

for( ; ; ) // a forever loop
{
    //Hmp155.GetSensorInfo();
    Thread.Sleep(10_000); // every 10 seconds
    Console.Clear();
    AddStaticDisplayVariables(); // update the static display variables.
}


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
    Console.WriteLine($"RTC = {DateTime.UtcNow.ToString("o")}");
    Debug.WriteLine($"CPU_T = {M5Core2.Power.GetInternalTemperature().DegreesCelsius}�C");
    Console.WriteLine($"CPU_T = {M5Core2.Power.GetInternalTemperature().DegreesCelsius}*C");
    //Debug.WriteLine($"GYRO = {M5Core2.AccelerometerGyroscope.GetGyroscope()}");
    //Console.WriteLine($"GYRO = {M5Core2.AccelerometerGyroscope.GetGyroscope()}");
    Console.WriteLine($"HMP-T = {Hmp155.GetTemperature().DegreesCelsius}*C");
    Console.WriteLine($"HMP-H = {Hmp155.GetHumidity().Percent}%");
    Console.WriteLine("");
}
