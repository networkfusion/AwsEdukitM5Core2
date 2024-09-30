# ATECC608A hardware encryption chip driver

The ATECC608A includes an EEPROM array which can be used for storage of up to 16 keys, certificates,
miscellaneous read/write, read-only or secret data, consumption logging, and security configurations.
Access to the various sections of memory can be restricted in a variety of ways and then the
configuration can be locked to prevent changes.

Access to the device is made through a standard I2C Interface at speeds of up to 1 Mb/s. The interface is
compatible with standard Serial EEPROM I2C interface specifications. The device also supports a Single-
Wire Interface (SWI), which can reduce the number of GPIOs required on the system processor, and/or
reduce the number of pins on connectors. If the Single-Wire Interface is enabled, the remaining pin is
available for use as a GPIO, an authenticated output or tamper input.

Each ATECC608A ships with a guaranteed unique 72-bit serial number. Using the cryptographic
protocols supported by the device, a host system or remote server can verify a signature of the serial
number to prove that the serial number is authentic and not a copy. Serial numbers are often stored in a
standard Serial EEPROM; however, these can be easily copied with no way for the host to know if the
serial number is authentic or if it is a clone.

The ATECC608A features a wide array of defense mechanisms specifically designed to prevent physical
attacks on the device itself, or logical attacks on the data transmitted between the device and the system.
Hardware restrictions on the ways in which keys are used or generated provide further defense against
certain styles of attack.

## Documentation
* http://ww1.microchip.com/downloads/en/DeviceDoc/40001977A.pdf
* https://ww1.microchip.com/downloads/en/DeviceDoc/20005928A.pdf
* https://github.com/m5stack/Core2-for-AWS-IoT-EduKit/tree/master/Factory-Firmware/components/esp-cryptoauthlib/esp_cryptoauth_utility
* https://github.com/MicrochipTech/cryptoauthlib
* https://github.com/ccrisan/pyatecc/blob/main/pyatecc/atecc.py

## Usage

### Hardware Required
- ATECC608x

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Code

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

## Binding Notes
