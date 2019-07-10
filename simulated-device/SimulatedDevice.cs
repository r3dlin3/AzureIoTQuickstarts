// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;

namespace simulated_device
{
    class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;
        private static DeviceModel[] s_devices;

        private static Logger _logger;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private readonly static string s_connectionString = "XXX";
        /// <summary>
        /// Number of different devices
        /// </summary>
        private const int NB_DEVICE = 10;
        /// <summary>
        /// Number of values sent by batch
        /// </summary>
        private const int NB_VALUES = 100;

        /// <summary>
        /// Delay between batch of value
        /// </summary>
        private const int DELAY = 5000;

        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            int loopDevice = 0;
            int countMessage = 1;
            while (true)
            {
                var telemetryDataPoints = new List<DeviceModel>(NB_VALUES);
                for (int loopValue = 0; loopValue < NB_VALUES; loopValue++)
                {
                    var telemetryDataPoint = s_devices[loopDevice].NextValue();
                    telemetryDataPoints.Add(telemetryDataPoint);

                    loopDevice++;
                    if (loopDevice >= NB_DEVICE) loopDevice = 0;
                }

                var messageString = JsonConvert.SerializeObject(telemetryDataPoints);
                byte[] inputBytes = Encoding.UTF8.GetBytes(messageString);
                _logger.Info("Sending message. Uncompressed data is {0} bytes", inputBytes.Length);
                Message message;
                using (var outputStream = new MemoryStream())
                {
                    // Cf. http://gigi.nullneuron.net/gigilabs/compressing-strings-using-gzip-in-c/
                    using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                        gZipStream.Write(inputBytes, 0, inputBytes.Length);

                    var outputBytes = outputStream.ToArray();
                    _logger.Info("Sending message. Compressed data is {0} bytes", outputBytes.Length);
                    message = new Message(outputBytes);
                }


                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                //message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                _logger.Info("Message {0} sent", countMessage);
                _logger.Info("Waiting {0}ms", DELAY);

                await Task.Delay(DELAY);
                countMessage++; ;
            }
        }

        private static void initializeDevices()
        {
            s_devices = new DeviceModel[NB_DEVICE];
            for (int i = 0; i < NB_DEVICE; ++i)
            {
                s_devices[i] = new DeviceModel();
            }
        }
        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("IoT Hub Quickstarts #1 - Simulated device. Ctrl-C to exit.");
            try
            {

                // Connect to the IoT hub using the MQTT protocol
                s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
                _logger.Info("Device Client initialized");
                initializeDevices();
                _logger.Info("Devices initialized");
                SendDeviceToCloudMessagesAsync();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                _logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
    }
}
