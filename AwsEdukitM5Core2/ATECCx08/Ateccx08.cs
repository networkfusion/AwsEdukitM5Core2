// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Net.Sockets;
using System.Threading;

namespace AwsEdukitM5Core2.ATECC608
{
    /// <summary>
    ///  ATECC608 hardware encryption chip driver
    /// </summary>
    public class Ateccx08
    {

        public enum Status
        {
            ATCA_SUCCESS,
            ATCA_RX_NO_RESPONSE,
            ATCA_WAKE_SUCCESS,
            PSA_SUCCESS,
            ATCA_BAD_PARAM,
            ATCA_INVALID_ID,
            ATCA_ASSERT_FAILURE,
            ATCA_SMALL_BUFFER,
            ATCA_RX_CRC_ERROR,
            ATCA_RX_FAIL,
            ATCA_STATUS_CRC,
            ATCA_RESYNC_WITH_WAKEUP,
            ATCA_PARITY_ERROR,
            ATCA_TX_TIMEOUT,
            ATCA_RX_TIMEOUT,
            ATCA_TOO_MANY_COMM_RETRIES,
            ATCA_COMM_FAIL,
            ATCA_TIMEOUT,
            ATCA_TX_FAIL,
            ATCA_NO_DEVICES,
            ATCA_UNIMPLEMENTED,
            ATCA_ALLOC_FAILURE,
            ATCA_BAD_OPCODE,
            ATCA_CONFIG_ZONE_LOCKED,
            ATCA_DATA_ZONE_LOCKED,
            ATCA_NOT_LOCKED,
            ATCA_WAKE_FAILED,
            ATCA_STATUS_UNKNOWN,
            ATCA_STATUS_ECC,
            ATCA_STATUS_SELFTEST_ERROR,
            ATCA_CHECKMAC_VERIFY_FAILED,
            ATCA_PARSE_ERROR,
            ATCA_FUNC_FAIL,
            ATCA_GEN_FAIL,
            ATCA_EXECUTION_ERROR,
            ATCA_HEALTH_TEST_ERROR,
            ATCA_INVALID_SIZE,
            PSA_ERROR_HARDWARE_FAILURE

        }

        /// <summary>
        /// The I2C device used for communication.
        /// </summary>
        private readonly I2cDevice _i2CDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ateccx08"/> class.
        /// </summary>
        public Ateccx08()
        {

            var defaultAddress = 0x35;
            var defaultBusId = 1;

            _i2CDevice = new I2cDevice(new I2cConnectionSettings(defaultBusId, defaultAddress));

            Initialize();
        }

        private void Initialize()
        {

        }

        public string GetSerialNumber()
        {
            //int ret;
            //uint8_t serial[ATCA_SERIAL_NUM_SIZE];

            //ret = atcab_read_serial_number(serial);
            //if (ret != ATCA_SUCCESS)
            //{
            //    ESP_LOGI(TAG, "*FAILED* atcab_read_serial_number returned %02x", ret);
            //    handleErr();
            //}

            //for (size_t i = 0; i < ATCA_SERIAL_NUM_SIZE; i++)
            //    sprintf(sn + i * 2, "%02X", serial[i]);
            //return ret;

            var snBytes = new byte[12];
            var serialNumber = string.Empty;

            //# 4-byte reads only
            //serial_num = self.read(0, 0x00, 4)
            Read();
            //time.sleep(0.001)
            Thread.Sleep(1);
            //serial_num += self.read(0, 0x02, 4)
            Read();
            //time.sleep(0.001)
            Thread.Sleep(1);
            //# Append Rev
            //serial_num += self.read(0, 0x03, 4)[:1]
            Read();
            //time.sleep(0.001)
            Thread.Sleep(1);
            //# neaten up the serial for printing
            //serial_num = str(hexlify(bytes(serial_num)), "utf-8")

            //serial_num = serial_num.upper()
            serialNumber = serialNumber.ToUpper();
            //return serial_num
            return serialNumber;
            

        }

        /// <summary>
        /// Read from a given slot at an offset. Data zone has to be locked for this function to work.
        /// </summary>
        /// <returns></returns>
        private static int Read()
        {
            //_i2CDevice.Read();
            return 0;
        }


        /// <summary>
        /// Write to a given slot at an offset. If the data zone is locked, offset and 
        /// length must be multiples of a word(4 bytes). If the data zone is unlocked, 
        /// only 32-byte writes are allowed, and the offset and length must be multiples of 32.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        private static void Write(short slot, int offset, char[] data, int length)
        {
            //_i2CDevice.Write();
        }


        // SendCommand


        // Sign

        
    }
}
