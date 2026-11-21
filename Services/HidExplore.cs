using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using HidSharp;


namespace GearChargeMonitor.Services
{
    public class HidExplore
    {
        public void FindDongleInterfaces()
        {
            IEnumerable<HidDevice> dongleInterfaces = DeviceList.Local.GetHidDevices(0x25A7, 0xFA07);

            foreach (HidDevice device in dongleInterfaces)
            {
                Debug.WriteLine($"DevicePath: {device.DevicePath}, MaxInputReportLength {device.GetMaxInputReportLength()}, MaxOutputReportLength {device.GetMaxOutputReportLength()}, MaxFeatureReportLength {device.GetMaxFeatureReportLength()}");
            }

        }
    

}

}
