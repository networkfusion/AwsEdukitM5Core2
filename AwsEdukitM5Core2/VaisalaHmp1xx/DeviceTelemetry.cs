using System;

namespace AwsEdukitM5Core2.VaisalaHmp1xx
{

    internal class DeviceTelemetryStatus
    {
        internal enum Status
        {
            NotHeating,
            WarmedProbeActive,
            PurgeHeatingInProgress,
            PurgeCoolingInProgress,
            AdditionalHeatingActive
        }

        internal static char NO_HEATING_ACTIVE = 'N';
        internal static char WARMED_PROBE_ACTIVE = 'h';
        internal static char PURGE_HEATING_ACTIVE = 'H';
        internal static char PURGE_COOLING_ACTIVE = 'S';
        internal static char EXTRA_SENSOR_HEATING_ACTIVE = 'X';
    }

    internal class DeviceTelemetryError
    {
        internal bool TemperatureError { get; private set; }
        internal bool TemperatureProbeError { get; private set; }
        internal bool HumidityError { get; private set; }
        internal bool DeviceError { get; private set; }

        internal DeviceTelemetryError()
        {
            TemperatureError = false;
            TemperatureProbeError = false;
            HumidityError = false;
            DeviceError = false;
        }

        internal DeviceTelemetryError(string errors)
        {
            if (errors.Length == 4)
            {
                var errorChars = errors.ToCharArray();
                if (errorChars[0] == '1')
                {
                    TemperatureError = true;
                }
                if (errorChars[1] == '1')
                {
                    TemperatureProbeError = true;
                }
                if (errorChars[2] == '1')
                {
                    HumidityError = true;
                }
                if (errorChars[3] == '1')
                {
                    DeviceError = true;
                }
            }
        }
    }

    internal class DeviceTelemetry
    {
        internal double Humidity { get; private set; } = double.NaN;
        internal double Temperature { get; private set; } = double.NaN;
        internal double ProbeTemperature { get; private set; } = double.NaN;
        internal double FrostPointTemperature { get; private set; } = double.NaN;
        internal double DewPointTemperature { get; private set; } = double.NaN;
        internal double MixingRatio { get; private set; } = double.NaN;
        internal double WetbulbTemperature { get; private set; } = double.NaN;

        internal string _errors = "0000"; // actually, this is 4 chars representing [T, Ta, RH, MEM]
        internal char _status;

        internal ushort ChecksumValue { get; private set; } = 0;
        internal bool ChecksumValid { get; private set; } = false;

        internal string ProbeSerialNumber { get; private set; } = string.Empty;
        internal DateTime Timestamp { get; private set; } = DateTime.MinValue;



        //internal void PopulateUsingLaske7(double humidity, double probeTemperature, double dewpoint, double frostpoint, double wetbulb, double mixratio, short stat, short error )
        //{

        //}
    }
}
