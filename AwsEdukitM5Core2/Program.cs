// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Latest known working interpreter = `1.8.0.344`
// Perform updates using:
// nanoff --target M5Core2 --update --serialport COM10

using nanoFramework.M5Core2;
using nanoFramework.M5Stack;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using Console = nanoFramework.M5Stack.Console;

using Secrets; // Make sure you adjust the template

using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Provisioning.Client;

const string AzureIotDpsAddress = "global.azure-devices-provisioning.net";

M5Core2.InitializeScreen();

Thread.Sleep(5000); // Helps with debug! Remove if not required!
Debug.WriteLine("Hello from M5Core2!");
Console.WriteLine("Hello from M5Core2!");
Debug.WriteLine("Waiting for WiFi!...");
Console.WriteLine("Waiting for WiFi!...");
// Give 30 seconds to the wifi join to happen (will retry).
CancellationTokenSource cs = new(30000);
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

M5Core2.TouchEvent += TouchEventCallback;

Debug.WriteLine("Connecting to Azure IoT Central.");
Console.WriteLine("Connecting to Azure IoT Central.");


X509Certificate azureCA = new X509Certificate(AzureIot.CaRootCertificates);
X509Certificate2 deviceCert = new X509Certificate2(AzureIot.DeviceCertificate, AzureIot.DeviceCertificatePrivateKey, "");
var provisioning = ProvisioningDeviceClient.Create(AzureIotDpsAddress, AzureIot.IdScope, AzureIot.RegistrationID, deviceCert, azureCA);

var myDevice = provisioning.Register(null, new CancellationTokenSource(30000).Token);

if (myDevice.Status != ProvisioningRegistrationStatusType.Assigned)
{
    Debug.WriteLine($"Registration is not assigned: {myDevice.Status}, error message: {myDevice.ErrorMessage} [code {myDevice.ErrorCode}]");
    Console.WriteLine($"Registration is not assigned: {myDevice.Status}, error message: {myDevice.ErrorMessage} [code {myDevice.ErrorCode}]");
}
else
{
    Debug.WriteLine($"Device successfully assigned:");
    Debug.WriteLine($"  Assigned Hub: {myDevice.AssignedHub}");
    Debug.WriteLine($"  Created time: {myDevice.CreatedDateTimeUtc}");
    Debug.WriteLine($"  Device ID: {myDevice.DeviceId}");
    Debug.WriteLine($"  ETAG: {myDevice.Etag}");
    Debug.WriteLine($"  Generation ID: {myDevice.GenerationId}");
    Debug.WriteLine($"  Last update: {myDevice.LastUpdatedDateTimeUtc}");
    Debug.WriteLine($"  Status: {myDevice.Status}");
    Debug.WriteLine($"  Sub Status: {myDevice.Substatus}");

    var device = new DeviceClient(myDevice.AssignedHub, myDevice.DeviceId, deviceCert, nanoFramework.M2Mqtt.Messages.MqttQoSLevel.AtMostOnce, azureCA);

    var res = device.Open();
    if (!res)
    {
        Debug.WriteLine($"Failed to connect the device with IoT Central");
        return;
    }

    var twin = device.GetTwin(new CancellationTokenSource(15000).Token);

    if (twin != null)
    {
        Debug.WriteLine($"Received desired device twin properties.");
        Debug.WriteLine($"  {twin.Properties.Desired.ToJson()}");
    }
    else
    {
        Debug.WriteLine($"Failed to receive the desired device twin properties.");
    }
}

Console.Clear();



AddStaticDisplayVariables();


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
