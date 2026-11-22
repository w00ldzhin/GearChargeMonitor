using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using GearChargeMonitor.Models;
using HidSharp;


namespace GearChargeMonitor.Services
{
    public class HidExplore
    {
        public DeviceBatteryInfo? FindDongleInterfaces()
        {
            IEnumerable<HidDevice> dongleInterfaces = DeviceList.Local.GetHidDevices(0x25A7, 0xFA07);


            HidDevice featureDevice = dongleInterfaces.FirstOrDefault(d =>
    d.DevicePath.IndexOf("col07", StringComparison.OrdinalIgnoreCase) >= 0 &&
    d.GetMaxFeatureReportLength() == 17);

            HidDevice inputDevice = dongleInterfaces.FirstOrDefault(d =>
    d.DevicePath.IndexOf("col05", StringComparison.OrdinalIgnoreCase) >= 0 &&
    d.GetMaxInputReportLength() == 17);


            if (featureDevice == null || inputDevice == null)
            {
                Debug.WriteLine("Не удалось найти оба интерфейса устройства");
                return null;
            }


            try
            {
                using (HidStream inputStream = inputDevice.Open())
                {
                    using (HidStream featureStream = featureDevice.Open())
                    {
                        byte[] feature = { 0x08, 0x04, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x49 };
                        featureStream.SetFeature(feature);
                    }

                    byte[] input = new byte[17];
                    for (int i = 0; i < 5; i++)
                    {
                        inputStream.ReadTimeout = 1000;
                        try
                        {
                            inputStream.Read(input);
                            if (input[0] == 0x09 && input[1] == 0x04)
                            {
                                Debug.WriteLine(BitConverter.ToString(input));
                                return new DeviceBatteryInfo { Name = "Redragon Impact Elite", BatteryPercent = (input[6] * 10), isCharging = (input[7] == 0x01), isAsleep = false };

                            }
                        }
                        catch (TimeoutException e)
                        {
                            return new DeviceBatteryInfo { Name = "Redragon Impact Elite", BatteryPercent = 0, isCharging = false, isAsleep = true };
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine(null);
            }


            return null;
        }
    
       

}

}
