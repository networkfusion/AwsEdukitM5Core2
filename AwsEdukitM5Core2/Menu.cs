//using System;
//using nanoFramework.Presentation.Controls;
//using System.Drawing;
using Console = nanoFramework.M5Stack.Console;
using nanoFramework.M5Stack;
//using nanoFramework.UI;
using System.Threading;

namespace AwsEdukitM5Core2
{
    public enum DisplayContext
    {
        Startup,
        DeviceTelemetry,
        DeviceInformation,
        SystemConfiguration,
        DebugUI,
    }

    public static class Menu
    {
        public static DisplayContext CurrentDisplayContext { get; set; } = DisplayContext.Startup;

        public static string HeaderText = "VAISALA HMP155 | M5Stack Core2";
        public static string FooterText = "Telemetry | Sensor Info | Networking";

        //public static void MenuInitialize()
        //{

        //}

        //public static void SelectMenuContext(MenuContext context)
        //{
        //    CurrentMenuContext = context;
        //}

        //public static void TestDraw(string contextText)
        //{
        //    //const int screenWidth = 320;
        //    //const int screenHeight = 240;

        //    //const int textPosX = 10; //margin
        //    //const int textPosY = 0;

        //    //Font displayFont; // = Resource.GetFont(Resource.FontResources.segoeuiregular12);
        //    //Bitmap charBitmap = new Bitmap(DisplayFont.MaxWidth + 1, DisplayFont.Height);

        //    string text;
        //    if (CurrentDisplayContext == DisplayContext.Startup)
        //    {
        //        text = $"{contextText}\n";
        //    }
        //    else
        //    {
        //        Console.Clear();
        //        text = $"{HeaderText}\n\n{contextText}/n/n{FooterText}\n";
        //    }

        //    //DisplayControl.Clear();
        //    //DisplayControl.Write(text, textPosX, textPosY, screenWidth, screenHeight, displayFont, Color.White, Color.Black);

        //    Console.Write(text);
        //}

        public static void ButtonHapticFeedback()
        {
            M5Core2.Vibrate = true;
            Thread.Sleep(150);
            M5Core2.Vibrate = false;
        }
    }
}
