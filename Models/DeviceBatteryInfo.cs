using System;
using System.Collections.Generic;
using System.Text;

namespace GearChargeMonitor.Models
{
    public class DeviceBatteryInfo
    {
        public string Name { get; set; }
        public int BatteryPercent { get; set; }
        public bool isCharging { get; set; }
        public bool isAsleep {  get; set; }
    }
}
