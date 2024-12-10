using System;
using System.Collections;
using System.Device.Model;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using UnitsNet;

namespace AwsEdukitM5Core2.VaisalaHmp1xx
{

    /// <summary>
    ///  Base class for common functions of the Vaisala HMP1xx sensors.
    /// </summary>
    [Interface("Vaisala HMP1xx temperature and humidity sensor")]
    public class VaisalaHmp1xx : IDisposable
    {
        // TODO: should be an abstract class
        private readonly SerialPort _sensor;
        private static double _humidity;
        private static readonly double _temperature;
        private static double _probeTemperature;
        // Derived parameters
        private static readonly double _frostPointTemperature;
        private static readonly double _dewPointTemperature;
        private static readonly double _mixingRatio;
        private static readonly double _wetbulbTemperature;

        // The following format was the default one I retrived from my sensor. I am not sure if it is the actual default!
        //private const string defaultTelemtryFormat = "3.1 \"RH=\" RH \" \" U4 3.1 \"Ta=\" Ta \" \" U3 \\r \\n";
        private static bool expectingCommandResponse = false;

        public int ProbeAddress { get; set; } = 0;


        // Measurement commands
        public const string CMD_SENSOR_MEASUREMENT_STOP = "S";
        public const string CMD_SENSOR_MEASUREMENT_RUN = "R";
        public const string CMD_SENSOR_MEASUREMENT_READ_ONCE = "SEND";

        // Formatting commands
        public const string CMD_SENSOR_FORMATTING_FORMAT = "FORM";
        public const string CMD_SENSOR_FORMATTING_UNIT = "UNIT";

        // System commands
        public const string CMD_SENSOR_SYS_INFO = "?";
        public const string CMD_SENSOR_SYS_ERRORS = "ERRS";
        public const string CMD_SENSOR_SYS_REBOOT = "RESET";
        public const string CMD_SENSOR_SYS_VERSION = "VERS";
        public const string CMD_SENSOR_SYS_HELP = "HELP";
        public const string CMD_SENSOR_SYS_FILTERMODE = "FILT";

        // Chemical Purge command
        public const string CMD_SENSOR_CHEMICAL_PURGE = "PUR";


        //Errors (retrived using the 'ERRS' command)
        public const string ERR_T_MEAS = "T MEAS error";
        public const string ERR_T_REF = "T REF error";
        public const string ERR_TA_MEAS = "TS MEAS error";
        public const string ERR_TA_REF = "TA REF error";
        public const string ERR_F_MEAS = "F MEAS error";
        public const string ERR_F_REF1 = "F REF1 error";
        public const string ERR_F_REF3 = "F REF3 error";
        public const string ERR_CHECKSUM_PROGRAM = "Program flash checksum error";
        public const string ERR_CHECKSUM_FLASH = "Parameter flash checksum error";
        public const string ERR_CHECKSUM_INFOA = "INFOA checksum error";
        public const string ERR_CHECKSUM_SCOEFS = "SCOEFS checksum error";


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


            DebugHelper.DumpHashTable(GetDeviceInformation(), 1);
            GetDeviceErrors();

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

        public void AutoMeasurementStop()
        {
            if (_sensor.IsOpen)
            {
                expectingCommandResponse = true;
                // It seems the only reliable way to break the connection is to prefix some carrage returns.
                // Adding newline chars (`\n`) breaks it.
                _sensor.WriteLine("\r\r\r" + CMD_SENSOR_MEASUREMENT_STOP);
            }
        }

        public void AutoMeasurementStart()
        {
            if (_sensor.IsOpen)
            {
                _sensor.WriteLine(CMD_SENSOR_MEASUREMENT_RUN);
                expectingCommandResponse = false;
            }
        }

        /// <summary>
        /// Retrive the error information from the device.
        /// </summary>
        public void GetDeviceErrors()
        {
            Debug.WriteLine("Attempting to get device error info!");
            try
            {
                AutoMeasurementStop();

                _sensor.WriteLine(CMD_SENSOR_SYS_ERRORS); // Get the device info
                Thread.Sleep(3000); // allow enough time for the info to be returned
                while (_sensor.BytesToRead > 0)
                {
                    Debug.WriteLine(_sensor.ReadLine()); // Get the returned device info strings
                }
                Thread.Sleep(500); // allow enough time after.

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Failed to get sensor info: {ex.Message}");
            }

            AutoMeasurementStart();
        }

        /// <summary>
        /// Retrive the device information.
        /// </summary>
        public Hashtable GetDeviceInformation()
        {
            AutoMeasurementStop();
            //Thread.Sleep(3000); // allow enough time for the measurements to be stopped
            //while (_sensor.BytesToRead > 0) // FIXME: we just want to ensure the read buffer is empty.
            //{
            //    var dbg = _sensor.ReadLine();
            //    Debug.WriteLine($"Debug read: {dbg}");
            //}

            Hashtable infoFields = new(); // TODO : use DeviceInformation class
            Debug.WriteLine("Attempting to get sensor info!");
            try
            {

                _sensor.WriteLine(CMD_SENSOR_SYS_INFO); // Get the device info
                Thread.Sleep(3000); // allow enough time for the info to be returned

                // The first line should contain the sensor model and its firmware version.
                // Should return something like:
                // HMP155 1.24
                var firstline = _sensor.ReadLine();  // Get the returned device info string
                Debug.WriteLine(firstline);
                if (firstline.Contains(" "))
                    {
                        var firstfield = firstline.Split(' ');
                    if (firstfield.Length == 2)
                    {
                        infoFields.Add("Model", firstfield[0]);
                        infoFields.Add("FirmwareVersion", firstfield[1]);
                    }
                }
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
                            string fieldKey = field[0].Trim();
                            if (fieldKey.Contains(" ")) //hack to remove space. Ideally it would captialize the second field.
                            {
                                var split_field = fieldKey.Split(' ');
                                fieldKey = $"{split_field[0]}{split_field[1]}";
                                if (split_field.Length > 2) // specifically handle Baud P D S
                                {
                                    fieldKey += fieldKey = $"{split_field[2]}{split_field[3]}";
                                }
                            }
                            if (fieldKey.Contains(".")) //hack to remove '.' and improve text for 'cal.' entries. Ideally it would captialize the second field.
                            {
                                var split_field = fieldKey.Split('.');
                                fieldKey = $"{split_field[0]}ibration{split_field[1]}";
                            }
                            infoFields.Add(fieldKey, field[1].Trim());
                        }
                    }
                }
                Thread.Sleep(100); // allow enough time after (sdelay is default 10ms).

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Failed to get sensor info: {ex.Message}");
            }

            AutoMeasurementStart();
            return infoFields;
        }

        /// <summary>
        /// Perform initialization command sequence.
        /// </summary>
        private void Initialize()
        {
            // TODO: support poll mode?

            AutoMeasurementStop();
            ////sensor.WriteLine("open 0"); // 0 would be the RS485 address....
            //sensor.WriteLine("intv 15 s");
            //Debug.WriteLine(sensor.ReadLine());
            _sensor.WriteLine("SMODE RUN"); // set the run mode to automatic on next reboot?!
            _sensor.WriteLine(CMD_SENSOR_SYS_REBOOT); // Start sending the telemetry.

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
            //// writeline "S"
            //MeasurementStop();
            //if (_sensor.BytesToRead > 0) // FIXME: we just want to ensure the read buffer is empty.
            //{
            //    var dbg = _sensor.ReadLine();
            //    Debug.WriteLine($"WB Debug read: {dbg}");
            //}

            //// writeline "FORM TW"
            //_sensor.WriteLine(CMD_SENSOR_FORMATTING_FORMAT + " TW");
            //Thread.Sleep(250);
            //if (_sensor.BytesToRead > 0) // FIXME: we just want to ensure the read buffer is empty.
            //{
            //    var dbg = _sensor.ReadLine();
            //    Debug.WriteLine($"WB Debug read: {dbg}");
            //}

            //// writeline "SEND"
            //_sensor.WriteLine(CMD_SENSOR_MEASUREMENT_READ_ONCE);

            //try
            //{
            //    var val = _sensor.ReadLine();
            //    Debug.WriteLine($"WB Debug val read: {val}");
            //    _wetbulbTemperature = double.Parse(val);
            //}
            //catch (Exception)
            //{

            //    Debug.WriteLine("error with wetbulb conversion.");
            //}

            //// writeline "FORM /" // put it back to the default message format
            //_sensor.WriteLine(CMD_SENSOR_FORMATTING_FORMAT + " /");
            //// writeline "R"
            AutoMeasurementStart();
            return Temperature.FromDegreesCelsius(_wetbulbTemperature);
        }



        private static void DecodeAutoMessage(string message)
        {
            //TODO: better handle message formats as defined with sending the `FORM`. For the HMP110, it will likely use 'T' rather than 'Ta'
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
                DecodeAutoMessage(serialDevice.ReadLine().TrimEnd(new char[] { '\r', '\n' }));
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
