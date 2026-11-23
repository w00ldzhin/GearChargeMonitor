using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearChargeMonitor.Services
{
    public class AutoStartService
    {
        private const string RegistryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "GearChargeMonitor";

        public bool IsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false);
            return key?.GetValue(AppName) != null;
        }

        public void Enable()
        {
            string? exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath)) return;

            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true);
            key?.SetValue(AppName, $"\"{exePath}\"");
        }

        public void Disable()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true);
            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }
}
