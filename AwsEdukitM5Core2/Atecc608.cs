using System;
using System.Device.I2c;

namespace AwsEdukitM5Core2
{
    /// <summary>
    ///  ATECC608 hardware encryption chip driver
    /// </summary>
    /// <remarks>
    /// http://ww1.microchip.com/downloads/en/DeviceDoc/40001977A.pdf
    /// https://ww1.microchip.com/downloads/en/DeviceDoc/20005928A.pdf
    /// https://github.com/m5stack/Core2-for-AWS-IoT-EduKit/tree/master/Factory-Firmware/components/esp-cryptoauthlib/esp_cryptoauth_utility
    /// https://github.com/MicrochipTech/cryptoauthlib
    /// </remarks>
    public class Atecc608
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


        public Atecc608()
        {
            //var device = new I2cDevice();
            // defaultAddress = 0x35,
            // defaultBusId = 1
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read from a given slot at an offset. Data zone has to be locked for this function to work.
        /// </summary>
        /// <returns></returns>
        public static int Read()
        {
            throw new NotImplementedException();
            //return 0;
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
        public static void Write(short slot, int offset, char[] data, int length)
        {
            throw new NotImplementedException();
        }
    }
}
