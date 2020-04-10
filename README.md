# geospatialsimulator
> A C# IoT geolocation simulator

The simulator will travers a route, given orgin/dest as two geopoints.  In this example, the 
lambda will be invoked every 5 seconds with current simulation state of the vehicle.

v.Speed is in meters/s, the times 3.6 converts it into kilometers/h.  The telemetry is then sent on 
to IoT central using the azure DeviceClient library.

```csharp
          Task consumer = new Vehicle(drivingRoute).StartTrip(async (IoTState v) =>
            {
                var telemetryDataPoint = new
                {
                    Location = new { lon = v.Longitude, lat = v.Latitude },
                    Speed = v.Speed * 3.6M
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var msg = new Message(Encoding.UTF8.GetBytes(messageString));
                System.Console.WriteLine(messageString);
                await deviceClient.SendEventAsync(msg);               
            }, 5);
```

