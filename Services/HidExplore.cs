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
        public static readonly Dictionary<(int Vid, int Pid), MouseProfile> SupportedMice = new()
        {
            {
                (0x25A7, 0xFA07), new MouseProfile
                {
                    Name = "Redragon Impact Elite",
                    RequestPacket = new byte[] { 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x49 },
                    PercentMultiplier = 10,
                    UseOutputReport = false,
                      ExpectedResponseHeader = 0x09
                }
            },
             {
                (0x373B, 0x1085), new MouseProfile
                {
                    Name = "VXE R1 SE+ / Red Square IO",
                    RequestPacket = new byte[] { 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x49 },
                    PercentMultiplier = 1,
                    UseOutputReport = true,
                      ExpectedResponseHeader = 0x08
                }
            }
        };
        public DeviceBatteryInfo? FindDongleInterfaces()
        {
            foreach (var entry in SupportedMice)
            {
                int vid = entry.Key.Vid;
                int pid = entry.Key.Pid;
                MouseProfile profile = entry.Value;

                IEnumerable<HidDevice> dongleInterfaces = DeviceList.Local.GetHidDevices(vid, pid);
                if (!dongleInterfaces.Any())
                {
                    continue;
                }

                if (profile.UseOutputReport)
                {
                    HidDevice? duplexDevice = dongleInterfaces.FirstOrDefault(d =>
                        d.GetMaxOutputReportLength() == 17 &&
                        d.GetMaxInputReportLength() == 17);

                    if (duplexDevice != null)
                    {
                        return PollOutputDevice(duplexDevice, profile);
                    }
                }
                else
                {
                    HidDevice? featureDevice = dongleInterfaces.FirstOrDefault(d =>
                        d.DevicePath.IndexOf("col07", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        d.GetMaxFeatureReportLength() == 17);

                    HidDevice? inputDevice = dongleInterfaces.FirstOrDefault(d =>
                        d.DevicePath.IndexOf("col05", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        d.GetMaxInputReportLength() == 17);

                    if (featureDevice != null && inputDevice != null)
                    {
                        return PollFeatureDevice(featureDevice, inputDevice, profile);
                    }
                }
            }

            return null;
        }

        private DeviceBatteryInfo? PollFeatureDevice(HidDevice featureDevice, HidDevice inputDevice, MouseProfile profile)
        {
            try
            {
                using HidStream inputStream = inputDevice.Open();
                inputStream.ReadTimeout = 1000;

                using (HidStream featureStream = featureDevice.Open())
                {
                    featureStream.SetFeature(profile.RequestPacket);
                }

                byte[] input = new byte[17];
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        inputStream.Read(input);
                        if (input[0] == profile.ExpectedResponseHeader && input[1] == 0x04)
                        {
                            return ParseBatteryResponse(input, profile);
                        }
                    }
                    catch (TimeoutException)
                    {
                        return CreateAsleepInfo(profile.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Survey error {profile.Name}: {ex.Message}");
            }

            return null;
        }

        private DeviceBatteryInfo? PollOutputDevice(HidDevice duplexDevice, MouseProfile profile)
        {
            try
            {
                using HidStream stream = duplexDevice.Open();
                stream.ReadTimeout = 1000;

                stream.Write(profile.RequestPacket);

                byte[] input = new byte[17];
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        stream.Read(input);
                        if (input[0] == profile.ExpectedResponseHeader && input[1] == 0x04)
                        {
                            if (input[2] == 0x01)
                            {
                                return CreateAsleepInfo(profile.Name);
                            }

                            return ParseBatteryResponse(input, profile);
                        }
                    }
                    catch (TimeoutException)
                    {
                        return CreateAsleepInfo(profile.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Survey error {profile.Name}: {ex.Message}");
            }

            return null;
        }

        private static DeviceBatteryInfo ParseBatteryResponse(byte[] input, MouseProfile profile)
        {
            Debug.WriteLine($"[{profile.Name}] {BitConverter.ToString(input)}");
            int percent = Math.Clamp(input[6] * profile.PercentMultiplier, 0, 100);

            return new DeviceBatteryInfo
            {
                Name = profile.Name,
                BatteryPercent = percent,
                isCharging = (input[7] == 0x01),
                isAsleep = false
            };
        }

        private static DeviceBatteryInfo CreateAsleepInfo(string name)
        {
            return new DeviceBatteryInfo
            {
                Name = name,
                BatteryPercent = 0,
                isCharging = false,
                isAsleep = true
            };
        }
    }
}