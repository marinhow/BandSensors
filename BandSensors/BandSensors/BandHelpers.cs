using Microsoft.Band;
using Microsoft.Band.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace BandSensors
{
    class BandHelpers
    {

        IBandClient bandClient;
        public async void ConnectBand()
        {
            var bandManager = BandClientManager.Instance;
            // query the service for paired devices
            var pairedBands = await bandManager.GetBandsAsync();
            // connect to the first device
            var bandInfo = pairedBands.FirstOrDefault();

            bandClient = await bandManager.ConnectAsync(bandInfo);
        }
        //public async Tuple<string,string> GetBandInfo()
        //{

        //    try
        //    {
        //        return 
        //            new Tuple<string, string>(
        //                await bandClient.GetFirmwareVersionAsync(),
        //                await bandClient.GetHardwareVersionAsync());
        //    }
        //    catch (BandException ex)
        //    {
        //        throw;
        //    }

        //}
        public async void CreateNewTile()
        {

        }
        public async void deleteExistingTile(Guid guid)
        {
            var bandManager = BandClientManager.Instance;
            // query the service for paired devices
            var pairedBands = await bandManager.GetBandsAsync();
            // connect to the first device
            var bandInfo = pairedBands.FirstOrDefault();

            using (bandClient = await bandManager.ConnectAsync(bandInfo))
            {
                var tiles = await bandClient.TileManager.GetTilesAsync();
                await bandClient.TileManager.RemoveTileAsync(guid);
            }
        }
        public async void SetBandTile()
        {
            //just vibration because of reasons - no message
            //await bandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.NotificationAlarm);
            //    bandClient.TileManager.TileButtonPressed += TileManager_TileButtonPressed;
            //await bandClient.TileManager.StartReadingsAsync();

            var smallIconFile = await StorageFile.GetFileFromApplicationUriAsync(
                                    new Uri("ms-appx:///Assets/logo_small.png"));


            using (var smallStream = await smallIconFile.OpenReadAsync())
            {
                var largeBitmap = new WriteableBitmap(48, 48);
                largeBitmap.SetSource(smallStream);
                var largeIcon = largeBitmap.ToBandIcon();

                var guid = Guid.NewGuid();//{be8a038f-2532-4be7-ac5e-aacf1771c788}

                var added = await bandClient.TileManager.AddTileAsync(
                  new BandTile(guid)
                  {
                      Name = "MarvelCrimeCenterUnit",
                      TileIcon = largeIcon,
                      SmallIcon = largeIcon
                  }
                );
                if (added)
                {
                    // NB: This call will return back to us *before* t
                    // user has acknowledged the dialog on their device -
                    // we don't get to know their answer here.
                    //await bandClient.NotificationManager.ShowDialogAsync(guid, "Test", "Hello Mario");

                    //PageData pageContent = new PageData(guid,
                    //    0, // index of our (only) layout 
                    //    new TextButtonData((short)1, "CALL SUPERHEROES!"));

                    //await bandClient.TileManager.SetPagesAsync(guid, pageContent);

                }

            }

            bandClient.TileManager.TileOpened += TileManager_TileOpened;

            await bandClient.TileManager.StartReadingsAsync();

        }
        public async void GetBandNotifications()
        {
            var bandManager = BandClientManager.Instance;
            // query the service for paired devices
            var pairedBands = await bandManager.GetBandsAsync();
            // connect to the first device
            var bandInfo = pairedBands.FirstOrDefault();
            bandClient = await bandManager.ConnectAsync(bandInfo);
            bandClient.TileManager.TileOpened += TileManager_TileOpened;

            await bandClient.TileManager.StartReadingsAsync();
        }
        public async void TileManager_TileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
        {
            await bandClient.NotificationManager.ShowDialogAsync(e.TileEvent.TileId, "Help is on the way!", "It shouldn't take long...");
            heartRateMonitor();
            //skinTemperatureMonitor();
            //GyroscopeMonitor();
            //PedometerMonitor();
            //GsrMonitor();
            //UVMonitor();

            // start the Heartrate sensor
            try
            {
                await bandClient.SensorManager.HeartRate.StartReadingsAsync();
            }
            catch (BandException ex)
            {
                // handle a Band connection exception
                throw ex;
            }
            //try
            //{
            //    await bandClient.SensorManager.Gyroscope.StartReadingsAsync();
            //}
            //catch (BandException ex)
            //{
            //    // handle a Band connection exception
            //    throw ex;
            //}
            //try
            //{
            //    await bandClient.SensorManager.Pedometer.StartReadingsAsync();
            //}
            //catch (BandException ex)
            //{
            //    // handle a Band connection exception
            //    throw ex;
            //}
            //try
            //{
            //    await bandClient.SensorManager.Gsr.StartReadingsAsync();
            //}
            //catch (BandException ex)
            //{
            //    // handle a Band connection exception
            //    throw ex;
            //}
            //try
            //{
            //    await bandClient.SensorManager.UV.StartReadingsAsync();
            //}
            //catch (BandException ex)
            //{
            //    // handle a Band connection exception
            //    throw ex;
            //}

            //try
            //{
            //    await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
            //}
            //catch (BandException ex)
            //{
            //    // handle a Band connection exception
            //    throw ex;
            //}





        }
        public async void heartRateMonitor()
        {
            //// get a list of available reporting intervals
            IEnumerable<TimeSpan> supportedHeartBeatReportingIntervals =
            bandClient.SensorManager.HeartRate.SupportedReportingIntervals;

            //// check current user heart rate consent
            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() !=
            UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await
                bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }
            // set the reporting interval
            bandClient.SensorManager.HeartRate.ReportingInterval = supportedHeartBeatReportingIntervals.First();
            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;

        }
        public async void skinTemperatureMonitor()
        {
            // get a list of available reporting intervals
            IEnumerable<TimeSpan> supportedSkinTemperatureIntervals =
            bandClient.SensorManager.SkinTemperature.SupportedReportingIntervals;


            // check current user heart rate consent
            if (bandClient.SensorManager.SkinTemperature.GetCurrentUserConsent() !=
            UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await
                bandClient.SensorManager.SkinTemperature.RequestUserConsentAsync();
            }
            // set the reporting interval
            bandClient.SensorManager.SkinTemperature.ReportingInterval = supportedSkinTemperatureIntervals.First();
            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;

        }
        public async void GyroscopeMonitor()
        {
            // get a list of available reporting intervals
            IEnumerable<TimeSpan> supportedGyroscopeIntervals =
            bandClient.SensorManager.Gyroscope.SupportedReportingIntervals;


            // check current user heart rate consent
            if (bandClient.SensorManager.Gyroscope.GetCurrentUserConsent() !=
            UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await
                bandClient.SensorManager.Gyroscope.RequestUserConsentAsync();
            }
            // set the reporting interval
            bandClient.SensorManager.Gyroscope.ReportingInterval = supportedGyroscopeIntervals.First();
            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
        }
        public void Gyroscope_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandGyroscopeReading> e)
        {
            SendLiveData("Gyroscope", e.SensorReading.AccelerationX);
        }
        public async void PedometerMonitor()
        {
            // get a list of available reporting intervals
            IEnumerable<TimeSpan> supportedPedometerIntervals =
            bandClient.SensorManager.Pedometer.SupportedReportingIntervals;


            // check current user heart rate consent
            if (bandClient.SensorManager.Pedometer.GetCurrentUserConsent() !=
            UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await
                bandClient.SensorManager.Pedometer.RequestUserConsentAsync();
            }
            // set the reporting interval
            bandClient.SensorManager.Pedometer.ReportingInterval = supportedPedometerIntervals.First();
            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.Pedometer.ReadingChanged += Pedometer_ReadingChanged;
        }
        public void Pedometer_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandPedometerReading> e)
        {
            SendLiveData("Pedometer", e.SensorReading.StepsToday);
        }
        public async void GsrMonitor()
        {
            // get a list of available reporting intervals
            IEnumerable<TimeSpan> supportedGsrIntervals =
            bandClient.SensorManager.Gsr.SupportedReportingIntervals;


            // check current user heart rate consent
            if (bandClient.SensorManager.Gsr.GetCurrentUserConsent() !=
            UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await
                bandClient.SensorManager.Gsr.RequestUserConsentAsync();
            }
            // set the reporting interval
            bandClient.SensorManager.Gsr.ReportingInterval = supportedGsrIntervals.First();
            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.Gsr.ReadingChanged += Gsr_ReadingChanged;

        }
        public void Gsr_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandGsrReading> e)
        {
            SendLiveData("Gsr", e.SensorReading.Resistance);
        }
        public async void UVMonitor()
        {
            // get a list of available reporting intervals
            IEnumerable<TimeSpan> supportedUVIntervals =
            bandClient.SensorManager.UV.SupportedReportingIntervals;


            // check current user heart rate consent
            if (bandClient.SensorManager.UV.GetCurrentUserConsent() !=
            UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await
                bandClient.SensorManager.UV.RequestUserConsentAsync();
            }
            // set the reporting interval
            bandClient.SensorManager.UV.ReportingInterval = supportedUVIntervals.First();
            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.UV.ReadingChanged += UV_ReadingChanged;
        }
        public void UV_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandUVReading> e)
        {
            SendLiveData("UV", e.SensorReading.ExposureToday);
        }
        public void SkinTemperature_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandSkinTemperatureReading> e)
        {
            SendLiveData("SkinTemperature", e.SensorReading.Temperature);
        }
        public void HeartRate_ReadingChanged(object sender, Microsoft.Band.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Sensors.IBandHeartRateReading> e)
        {
            SendLiveData("HeartRate", e.SensorReading.HeartRate);
        }
        public void SendLiveData(string type, double value)
        {
         
        }
    }
}
