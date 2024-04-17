using System;
using System.Collections;
using System.Device.Model;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using UnitsNet;

namespace AwsEdukitM5Core2
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
    }

    /// <summary>
    ///  Base class for common functions of the Vaisala HMP1xx sensors.
    /// </summary>
    [Interface("Vaisala HMP1xx temperature and humidity sensor")]
    public class VaisalaHmp1xx : IDisposable
    {
        // TODO: should be an abstract class
        private  SerialPort _sensor;
        private static double _humidity;
        private static double _temperature;
        private static double _probeTemperature;
        // Derived parameters
        private static double _frostPointTemperature;
        private static double _dewPointTemperature;
        private static double _mixingRatio;
        private static double _wetbulbTemperature;

        // The following format was the default one I retrived from my sensor. I am not sure if it is the actual default!
        //private const string defaultTelemtryFormat = "3.1 \"RH=\" RH \" \" U4 3.1 \"Ta=\" Ta \" \" U3 \\r \\n";
        private static bool expectingCommandResponse = false;

        public int ProbeAddress { get; set; } = 0;


        public VaisalaHmp1xx(string port)
        {
            _sensor = new SerialPort(port)
            {
                BaudRate = 4800,
                DataBits = 7,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                NewLine = "\r\n", // this sensor needs to use CRLF for writes, but needs some CR encorragement sometimes.
                ReadBufferSize = 4096 // Input buffer needs to be large enough to handle the biggest response.
            };

        }

        public void Open()
        {
            Close();
            
            _sensor.Open();
            Debug.WriteLine("HMP1xx serial port opened!");

            //Initialize();
            //Thread.Sleep(5_000);

            DebugHelper.DumpHashTable(GetSensorInfo(), 1);
            GetSensorErrors();

            _sensor.DataReceived += Port_DataReceived;
        }

        public void Close()
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }
            _sensor.DataReceived -= Port_DataReceived;
            Thread.Sleep(1000);

        }

        public void MeasurementStop()
        {
            if (_sensor.IsOpen)
            {
                expectingCommandResponse = true;
                _sensor.WriteLine("\r\r\rS"); // It seems the only reliable way to break the connection is to prefix some carrage returns.
            }
        }

        public void MeasurementStart()
        {
            if (_sensor.IsOpen)
            {
                _sensor.WriteLine("R");
                expectingCommandResponse = false;
            }
        }

        /// <summary>
        /// Retrive the error information from the sensor.
        /// </summary>
        public void GetSensorErrors()
        {
            expectingCommandResponse = true;
            Debug.WriteLine("Attempting to get sensor error info!");
            try
            {
                _sensor.WriteLine("\r\r\rS"); // It seems the only reliable way to break the connection is to prefix some carrage returns.

                _sensor.WriteLine("ERRS"); // Get the device info
                Thread.Sleep(1000); // allow enough time for the info to be returned
                while (_sensor.BytesToRead > 0)
                {
                    Debug.WriteLine(_sensor.ReadLine()); // Get the returned device info strings
                }
                Thread.Sleep(2000); // allow enough time after.

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Failed to get sensor info: {ex.Message}");
            }

            _sensor.WriteLine("R"); // Start sending the telemetry again.
            expectingCommandResponse = false;
        }

        /// <summary>
        /// Retrive the sensor information.
        /// </summary>
        public Hashtable GetSensorInfo()
        {
            expectingCommandResponse = true;
            Hashtable infoFields = new(); // TODO : use SensorInfo class
            Debug.WriteLine("Attempting to get sensor info!");
            try
            {
                _sensor.WriteLine("\r\r\rS"); // It seems the only reliable way to break the connection is to prefix some carrage returns.

                _sensor.WriteLine("?"); // Get the device info
                Thread.Sleep(5000); // allow enough time for the info to be returned

                // The first line should contain the sensor model and its firmware version.
                // Should return something like:
                // HMP155 1.24
                var firstline = _sensor.ReadLine();  // Get the returned device info string
                Debug.WriteLine(firstline);
                //if (firstline.Contains(" ")) // FIXME: Contains is faulty
                //{
                    var firstfield = firstline.Split(' ');
                    if (firstfield.Length == 2)
                    {
                        infoFields.Add("Model", firstfield[0]);
                        infoFields.Add("FirmwareVersion", firstfield[1]);
                    }
                //}
                // All other lines are seperated by a colon
                while (_sensor.BytesToRead > 0)
                {
                    // split by colon and trim spaces and tabs.
                    // Should return something like:

                    // Serial number  : Hxxxxxxx
                    // Batch number   : Hxxxxxxx
                    // Module number  : Gxxxxxxx
                    // Sensor number  : Hxxxxxxx
                    // Sensor model   : Humicap 180R
                    // Cal.date       : 20151116
                    // Cal.info       : MI70 2.13
                    // Time           : 14:46:38
                    // Serial mode    :      RUN
                    // Baud P D S     : 4800 E 7 1
                    // Output interval:        2 S
                    // Serial delay   : 10
                    // Address        : 0
                    // Pressure       : 1.013 bar
                    // Filter         :    1.000
                    var line = _sensor.ReadLine();  // Get the returned device info string
                    Debug.WriteLine(line);
                    if (line.Contains(":"))
                    {
                        var field = line.Split(':');
                        if (field.Length == 2)
                        {
                            infoFields.Add(field[0].Trim(), field[1].Trim());
                        }
                    }
                }
                Thread.Sleep(2000); // allow enough time after.
                
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Failed to get sensor info: {ex.Message}");
            }

            _sensor.WriteLine("R"); // Start sending the telemetry again.
            expectingCommandResponse = false;
            return infoFields;
        }

        /// <summary>
        /// Perform initialization command sequence.
        /// </summary>
        private void Initialize()
        {
            // TODO: support poll mode?

            MeasurementStop();
            ////sensor.WriteLine("open 0"); // 0 would be the RS485 address....
            //sensor.WriteLine("intv 15 s");
            //Debug.WriteLine(sensor.ReadLine());
             _sensor.WriteLine("SMODE RUN"); // set the run mode to automatic on next reboot?!
            _sensor.WriteLine("RESET"); // Start sending the telemetry.

            // We should possibily disable the transmit line now (unless we are in poll mode)?!
        }

        /// <summary>
        /// Gets the last relative humidity reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Relative humidity reading.</returns>
        [Telemetry("RelativeHumidity")]
        public RelativeHumidity GetRelativeHumidity()
        {
            return RelativeHumidity.FromPercent(_humidity);
        }

        /// <summary>
        /// Gets the last temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("Temperature")]
        public Temperature GetTemperature()
        {
            return Temperature.FromDegreesCelsius(_temperature);
        }

        /// <summary>
        /// Gets the last probe temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("ProbeTemperature")]
        public Temperature GetProbeTemperature()
        {
            return Temperature.FromDegreesCelsius(_probeTemperature);
        }

        /// <summary>
        /// Gets the last derived frost point temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("FrostPointTemperature")]
        public Temperature GetFrostPointTemperature()
        {
            return Temperature.FromDegreesCelsius(_frostPointTemperature);
        }

        /// <summary>
        /// Gets the last derived dew point temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("DewPointTemperature")]
        public Temperature GetDewPointTemperature()
        {
            return Temperature.FromDegreesCelsius(_dewPointTemperature);
        }


        /// <summary>
        /// Gets the derived mixing ratio reading from the sensor.
        /// </summary>
        /// <remarks>
        /// The mixing ratio of water vapour in air is the weight of water vapour mixed into a given weight of dry air.
        /// </remarks>
        /// <returns>Mass reading.</returns>
        [Telemetry("MixingRatio")]
        public MassConcentration GetMixingRatio()
        {
            return MassConcentration.FromGramsPerLiter(_mixingRatio); //fixme: this is a a strange unit (g/kg)
        }

        /// <summary>
        /// Gets the derived wet bulb temperature reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Temperature reading.</returns>
        [Telemetry("WetBulbTemperature")]
        public Temperature GetWetBulbTemperature()
        {
            // writeline "S"
            // writeline "FORM TW"
            // writeline "SEND"
            // writeline "FORM /" // put it back to the default message format
            // writeline "R"
            return Temperature.FromDegreesCelsius(_wetbulbTemperature);
        }



        private static void DecodeMessage(string message)
        {
            //TODO: better handle message formats as defined with sending the `FORM`.
            //Debug.WriteLine(message);

            if (message.StartsWith("RH=") && message.Contains("%RH Ta=") && message.TrimEnd(' ').EndsWith("'C"))
            {
                // starts with "RH=" and ends with "%RH", strip those chars, then convert to a double?!
                var humidity = message.Substring(3, 6).Trim(' '); // Temporary "workaround?!
                // starts with "Ta=" and ends with "'C", strip those chars, then convert to a double?!
                var probeTemperature = message.Substring(16, 6).Trim(' '); // Temporary "workaround?!
                //Debug.WriteLine($"RH:{humidity}, T:{temperature}");
                try
                {
                    _humidity = double.Parse(humidity);
                    _probeTemperature = double.Parse(probeTemperature);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"unable to convert values for HMP1xx: {ex}");
                }
                
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialDevice = (SerialPort)sender;
            if (!expectingCommandResponse)
            { 
                DecodeMessage(serialDevice.ReadLine().TrimEnd(new char[] { '\r', '\n' }));
            }
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor?.Dispose();
                //_sensor = null; // FIXME: causes readings to always be null!
            }
        }
    }
}
