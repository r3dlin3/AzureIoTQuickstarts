using System;

namespace simulated_device
{
    public class DeviceModel
    {
        // CONST

        public const double MAX_HUMIDITY = 100;
        public const double MIN_HUMIDITY = 0;

        public const double MAX_TEMPERATURE = 100;
        public const double MIN_TEMPERATURE = -40;
        private static readonly Random rand = new Random();
        // telemetry values

        public double Temperature { get; set; }
        public double Humidity { get; set; }

        public Guid DeviceId { get; set; }

        public DeviceModel()
        {
            Temperature = 20;
            Humidity = 60;
            this.DeviceId = Guid.NewGuid();
        }

        public DeviceModel(double temperature, double humidity, Guid deviceId)
        {
            this.Temperature = temperature;
            this.Humidity = humidity;
            this.DeviceId = deviceId;

        }
        public DeviceModel NextValue()
        {
            Temperature = Temperature + rand.NextDouble() * 15 - 7.5;
            Temperature = Math.Max(MIN_TEMPERATURE, Temperature);
            Temperature = Math.Min(MAX_TEMPERATURE, Temperature);

            Humidity = Humidity + rand.NextDouble() * 20 - 10;
            Humidity = Math.Max(MIN_HUMIDITY, Humidity);
            Humidity = Math.Min(MAX_TEMPERATURE, Humidity);

            return new DeviceModel
            {
                DeviceId = DeviceId,
                Temperature = Temperature,
                Humidity = Humidity
            };
        }
    }
}