using System;
using System.Device.Model;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using UnitsNet;

namespace AwsEdukitM5Core2
{

    /// <summary>
    ///  Base class for common functions of the Vaisala HMP1xx sensors.
    /// </summary>
    [Interface("Vaisala HMP1xx temperature and humidity sensor")]
    public class VaisalaHmp1xx : IDisposable
    {
        // TODO: should be an abstract class
        private readonly SerialPort _sensor;
        private double _temperature;
        private double _humidity;

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
                //Mode = SerialMode.RS485
            };
            _sensor.NewLine = "\r\n"; // this sensor needs to use CRLF for writes.
        }

        public void Open()
        {
            Close();
            Thread.Sleep(100);
            _sensor.Open();
            Debug.WriteLine("HMP1xx serial port opened!");

            //Initialize();
            //Thread.Sleep(5_000);


            _sensor.DataReceived += Port_DataReceived;
        }

        public void Close()
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }
            _sensor.DataReceived -= Port_DataReceived;

        }

        /// <summary>
        /// Perform initialization command sequence.
        /// </summary>
        public void GetSensorInfo()
        {
            Debug.WriteLine("Attempting to get sensor info!");
            //for (int i = 0; i < 10; i++)
            //{
            _sensor.WriteLine("s");
            Thread.Sleep(1000); // allow enough time for the command
            //}

            Thread.Sleep(1000);
            _sensor.WriteLine("?"); // Get the device info
            Thread.Sleep(100); // allow enough time for the info to be returned
            while (_sensor.BytesToRead > 0)
            {
                // TODO : create a class
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
            _sensor.WriteLine("r"); // Start sending the telemetry again.
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
        /// Received every 1 second.
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
        /// Received every 1 second.
        /// </remarks>
        /// <returns>Relative humidity reading.</returns>
        [Telemetry("Humidity")]
        public RelativeHumidity GetHumidity()
        {
            return RelativeHumidity.FromPercent(_humidity);
        }

        private void DecodeMessage(string message)
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
                catch (Exception)
                {
                    Debug.WriteLine("unable to convert values for HMP1xx");
                }
                
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialDevice = (SerialPort)sender;
            DecodeMessage(serialDevice.ReadLine().TrimEnd(new char[] { '\r', '\n' }));
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor?.Dispose();
                //_sensor = null;
            }
        }
    }
}
