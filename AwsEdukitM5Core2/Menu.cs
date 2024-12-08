using nanoFramework.M5Stack;
using System.Threading;
using System.Drawing;
using Console = nanoFramework.M5Stack.Console;

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

        public static string HeaderText = "   VAISALA HMP155 | {THE ASSET ID}  ";
        public static string FooterText = " [Telemetry][SensorInfo][SystemInfo]";


        public static void DrawHeader()
        {
            Console.BackgroundColor = Color.Blue;
            Console.ForegroundColor = Color.White;
            Console.WriteLine(HeaderText);
            Console.BackgroundColor = Color.Black;
            Console.ForegroundColor = Color.White;
        }

        public static void DrawFooter()
        {
            Console.BackgroundColor = Color.LightGreen;
            Console.ForegroundColor = Color.Black;
            Console.WriteLine(FooterText);
            Console.BackgroundColor = Color.Black;
            Console.ForegroundColor = Color.White;
        }

        public static void ButtonHapticFeedback()
        {
            M5Core2.Vibrate = true;
            Thread.Sleep(150);
            M5Core2.Vibrate = false;
        }
    }
}
