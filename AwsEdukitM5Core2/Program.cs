// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Latest known working interpreter = `1.12.1.33`
// Perform updates using:
// nanoff --target M5Core2 --update --serialport COM7 --masserase --fwversion 1.12.1.33

using nanoFramework.M5Core2;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Threading;
using Console = nanoFramework.M5Stack.Console;
using Secrets; // Make sure you adjust the template
using System.IO.Ports;
using AwsEdukitM5Core2;
using AwsEdukitM5Core2.VaisalaHmp1xx;

const bool HARDWARE_DEBUG_MODE = false;

M5Core2.InitializeScreen();
Menu.CurrentDisplayContext = DisplayContext.Startup;

Thread.Sleep(5_000); // Helps with debug!
Debug.WriteLine("Hello from M5Core2!");
Debug.WriteLine($"Waiting for WiFi...{WiFi.Ssid}");
Console.WriteLine("Waiting for WiFi...");
Console.WriteLine($"  SSID: \"{WiFi.Ssid}\"");
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

Debug.WriteLine("Network Setup complete.");
Console.WriteLine("Network Setup complete.");
Thread.Sleep(5_000); // Helps with debug!
Console.Clear();


if (HARDWARE_DEBUG_MODE)
{
    Menu.CurrentDisplayContext = DisplayContext.DebugUI;
    var ports = SerialPort.GetPortNames();
    Debug.WriteLine("Available SerialPorts:");
    foreach (var port in ports)
    {
        Debug.WriteLine(port);
    }
}

Menu.CurrentDisplayContext = DisplayContext.SystemConfiguration;
// Test the HMP155
// PORT-C (Blue) - UART (COM2)
// pins 13 (RXD1 - GPIO3) / 14 (TXD1 - GPIO1)
var Hmp155 = new VaisalaHmp1xx("COM2");

M5Core2.TouchEvent += TouchEventCallback;

new Thread(() =>
{
    Menu.CurrentDisplayContext = DisplayContext.DeviceInformation;
    Hmp155.Open(); // This can take a while and also retrives the device info + error status!
    
    Menu.CurrentDisplayContext = DisplayContext.DeviceTelemetry;

    AddStaticDisplayVariables_MainDisplay();
    for ( ; ; ) // a forever loop
    {
        Thread.Sleep(15_000); // every 15 seconds
        Console.Clear();
        AddStaticDisplayVariables_MainDisplay(); // update the static display variables.
    }
}).Start();

Thread.Sleep(Timeout.Infinite);



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
        Menu.ButtonHapticFeedback();
        Debug.WriteLine(StrLB);
        Console.WriteLine(StrLB);
        Menu.CurrentDisplayContext = DisplayContext.DeviceTelemetry;

    }
    else if ((e.TouchEventCategory & TouchEventCategory.MiddleButton) == TouchEventCategory.MiddleButton)
    {
        Menu.ButtonHapticFeedback();
        Debug.WriteLine(StrMB);
        Console.WriteLine(StrMB);
        Menu.CurrentDisplayContext = DisplayContext.DeviceInformation;
        Console.WriteLine("Get Info!");
        Hmp155.GetDeviceInformation();
    }
    else if ((e.TouchEventCategory & TouchEventCategory.RightButton) == TouchEventCategory.RightButton)
    {
        Menu.ButtonHapticFeedback();
        Debug.WriteLine(StrRB);
        Console.WriteLine(StrRB);
        Console.WriteLine("Measurement start!");
        Hmp155.AutoMeasurementStart();
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
    AddStaticDisplayVariables_MainDisplay();
}

void AddStaticDisplayVariables_MainDisplay()
{
    Menu.DrawHeader();
    Console.WriteLine("");

    Debug.WriteLine($"RTC = {DateTime.UtcNow}");
    Console.WriteLine($"RTC = {DateTime.UtcNow.ToString("o")}");
    Console.WriteLine("");

    if (Menu.CurrentDisplayContext == DisplayContext.SystemConfiguration || Menu.CurrentDisplayContext == DisplayContext.DeviceTelemetry)
    {
        Debug.WriteLine($"IP = {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
        Console.WriteLine($"IP = {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
        Debug.WriteLine($"CPU_T = {M5Core2.Power.GetInternalTemperature().DegreesCelsius.ToString("f2")}°C");
        Console.WriteLine($"CPU_T = {M5Core2.Power.GetInternalTemperature().DegreesCelsius.ToString("f2")}*C");
        //Debug.WriteLine($"GYRO = {M5Core2.AccelerometerGyroscope.GetGyroscope()}"); // TODO: should use gyro to set display orientation
        //Console.WriteLine($"GYRO = {M5Core2.AccelerometerGyroscope.GetGyroscope()}");
        Console.WriteLine("");
    }

    if (Menu.CurrentDisplayContext == DisplayContext.DeviceTelemetry)
    {
        Console.WriteLine($"HMP-RH = {Hmp155.GetRelativeHumidity().Percent.ToString("f2")}%");
        Console.WriteLine($"HMP-Ta = {Hmp155.GetProbeTemperature().DegreesCelsius.ToString("f4")}*C");
        //Console.WriteLine($"HMP-T = {Hmp110.GetTemperature().DegreesCelsius.ToString("f4")}*C"); // TODO: this sensor does not have a probe
        // TODO: the following parameters must be read, rather than auto sent:
        //Console.WriteLine($"HMP-TW = {Hmp155.GetWetBulbTemperature().DegreesCelsius.ToString("f2")}*C");
        //Console.WriteLine($"HMP-TDF = {Hmp155.GetFrostPointTemperature().DegreesCelsius.ToString("f2")}*C");
        //Console.WriteLine($"HMP-TD = {Hmp155.GetDewPointTemperature().DegreesCelsius.ToString("f2")}*C");
        //Console.WriteLine($"HMP-X = {Hmp155.GetMixingRatio().ToString("f2")}.g/kg");
        Console.WriteLine("");
    }

    Menu.DrawFooter();
}
