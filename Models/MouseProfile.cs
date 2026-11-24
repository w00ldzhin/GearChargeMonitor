using System;
using System.Collections.Generic;
using System.Text;

namespace GearChargeMonitor.Models
{
    public class MouseProfile
    {
        public string Name { get; set; } = string.Empty;
        public byte[] RequestPacket { get; set; } = Array.Empty<byte>();
        public int PercentMultiplier { get; set; } = 1;
        public bool UseOutputReport {  get; set; }
        public byte ExpectedResponseHeader { get; set; } = 0x09;

    }
}