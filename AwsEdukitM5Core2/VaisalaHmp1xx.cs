using System;
using System.Device.Model;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnitsNet;

namespace AwsEdukitM5Core2
{

    public class SensorInfo
    {
        // Should return something like:
        // HMP155 1.24
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
        // Baud P D S     : 4800 E 7 1
        // Output interval:        2 S
        // Serial delay   : 10
        // Address        : 0
        // Pressure       : 1.013 bar
        // Filter         :    1.000
    }

    /// <summary>
    ///  Base class for common functions of the Vaisala HMP1xx sensors.
    /// </summary>
    [Interface("Vaisala HMP1xx temperature and humidity sensor")]
    public class VaisalaHmp1xx : IDisposable
    {
        // TODO: should be an abstract class
        private  SerialPort _sensor;
        private static double _temperature;
        private static double _humidity;
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
                NewLine = "\r\n" // this sensor needs to use CRLF for writes.
            };
        }

        public void Open()
        {
            Close();
            
            _sensor.Open();
            Debug.WriteLine("HMP1xx serial port opened!");

            //Initialize();
            //Thread.Sleep(5_000);

            //GetSensorInfo();

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
                _sensor.WriteLine("\r\n\r\n\r\ns\r\n");
            }
        }

        public void MeasurementStart()
        {
            if (_sensor.IsOpen)
            {
                _sensor.WriteLine("\r\n\r\n\r\nr\r\n");
                expectingCommandResponse = false;
            }
        }

        /// <summary>
        /// Perform initialization command sequence.
        /// </summary>
        public void GetSensorInfo()
        {
            expectingCommandResponse = true;
            Debug.WriteLine("Attempting to get sensor info!");
            try
            {
                //for (int i = 0; i < 10; i++)
                //{
                _sensor.WriteLine("\r\n\r\n\r\ns\r\n");
                //Thread.Sleep(2000); // allow enough time for the command
                                    //}

                _sensor.WriteLine("?\r\n"); // Get the device info
                Thread.Sleep(100); // allow enough time for the info to be returned
                while (_sensor.BytesToRead > 0)
                {
                    // TODO : use SensorInfo class
                    // split by colon and trim spaces and tabs.
                    // Should return something like:
                    // HMP155 1.24
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
                    Debug.WriteLine(_sensor.ReadLine()); // Get the returned device info strings
                }
                Thread.Sleep(5000); // allow enough time after.
                
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Failed to get sensor info: {ex.Message}");
            }

            _sensor.WriteLine("r\r\n"); // Start sending the telemetry again.
            expectingCommandResponse = false;
        }

        /// <summary>
        /// Perform initialization command sequence.
        /// </summary>
        private void Initialize()
        {
            // TODO: support poll mode?

            //_sensor.WriteLine("s"); //stop the output. (we might need to send this more than once to make sure!)
            ////sensor.WriteLine("open 0"); // 0 would be the RS485 address....
            //sensor.WriteLine("intv 15 s");
            //Debug.WriteLine(sensor.ReadLine());
            //sensor.Port.WriteLine("smode run"); // set the run mode to automatic on next reboot?!
            Thread.Sleep(100);
            _sensor.WriteLine("r"); // Start sending the telemetry.

            // We should possibily disable the transmit line now (unless we are in poll mode)?!
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
        /// Gets the last relative humidity reading from the sensor.
        /// </summary>
        /// <remarks>
        /// Received every 1-2 seconds.
        /// </remarks>
        /// <returns>Relative humidity reading.</returns>
        [Telemetry("Humidity")]
        public RelativeHumidity GetHumidity()
        {
            return RelativeHumidity.FromPercent(_humidity);
        }

        private static void DecodeMessage(string message)
        {
            //TODO: handle message. (default format).
            // "RH= 33.0 %RH T= 22.1 'C"
            // Seemingly non default MO use as an 'a' to 'T=' i.e. "RH= 33.0 %RH Ta= 22.1 'C"
            Debug.WriteLine(message);

            if (message.StartsWith("RH=") && message.Contains("%RH Ta=") && message.TrimEnd(' ').EndsWith("'C"))
            {
                // starts with "RH=" and ends with "%RH", strip those chars, then convert to a double?!
                var humidity = message.Substring(3, 6).Trim(' '); // Temporary "workaround?!
                // starts with "Ta=" and ends with "'C", strip those chars, then convert to a double?!
                var temperature = message.Substring(16, 6).Trim(' '); // Temporary "workaround?!
                //Debug.WriteLine($"RH:{humidity}, T:{temperature}");
                try
                {
                    _humidity = double.Parse(humidity);
                    _temperature = double.Parse(temperature);
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
