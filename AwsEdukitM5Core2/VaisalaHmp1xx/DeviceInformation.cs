using System;
using System.Collections;

namespace AwsEdukitM5Core2.VaisalaHmp1xx
{
    public class DeviceInformation
    {
        // Should return something like:
        // HMP155 1.24
        public string ModelType { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        // Serial number  : Hxxxxxxx
        public string SerialNumber { get; set; } = string.Empty;
        // Batch number   : Hxxxxxxx
        public string BatchNumber { get; set; } = string.Empty;
        // Module number  : Gxxxxxxx
        public string ModuleNumber { get; set; } = string.Empty;
        // Sensor number  : Hxxxxxxx
        public string SensorNumber { get; set; } = string.Empty;
        // Sensor model   : Humicap 180R
        public string SensorModel { get; set; } = string.Empty;
        // Cal.date       : 20151116
        public string CalibrationDate { get; set; } = string.Empty;
        // Cal.info       : MI70 2.13
        public string CalibrationInfo { get; set; } = string.Empty;
        // Time           : 14:46:38
        public string CalibrationTime { get; set; } = string.Empty;
        // Serial mode    :      RUN
        public string SerialMode { get; set; } = string.Empty;
        // Baud P D S     : 4800 E 7 1
        public string PortConfiguration { get; set; } = string.Empty;
        // Output interval:        2 S
        public string OutputInverval { get; set; } = string.Empty;
        // Serial delay   : 10
        public string SerialDelay { get; set; } = string.Empty;
        // Address        : 0
        public string DeviceAddress { get; set; } = string.Empty;
        // Pressure       : 1.013 bar
        public string PressureInBar { get; set; } = string.Empty;
        // Filter         :    1.000
        public string Filter { get; set; } = string.Empty;


        //public void TryPopulate(Hashtable deviceInfo)
        //{
        //    foreach (var key in deviceInfo.Keys)
        //    {
        //        if (key)
        //    }

        //}
    }
}
