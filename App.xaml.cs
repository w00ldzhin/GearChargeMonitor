using GearChargeMonitor.Models;
using GearChargeMonitor.Services;
using H.NotifyIcon;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GearChargeMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        HidExplore hidExplore = new HidExplore();
        private TaskbarIcon? _trayIcon;
        private readonly HidExplore _hidExplore = new();
        private readonly IconGeneratorService _iconGenerator = new();
        private readonly AutoStartService _autoStartService = new();
        private DispatcherTimer? _timer;
        private int? _lastKnownPercent;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            SetTrayIcon("..", Color.White);
            _trayIcon.ForceCreate();

            if (_trayIcon.ContextMenu?.Items.OfType<MenuItem>()
              .FirstOrDefault(m => m.Name == "AutoStartMenuItem") is MenuItem autoStartItem)
            {
                autoStartItem.IsChecked = _autoStartService.IsEnabled();
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _timer.Tick += async (s, args) => await UpdateBatteryStatusAsync();
            _timer.Start();

            await UpdateBatteryStatusAsync();
        }

        private void SetTrayIcon(string text, Color color)
        {
            if (_trayIcon == null) return;

            Icon? oldIcon = _trayIcon.Icon;
            _trayIcon.Icon = _iconGenerator.CreateTextIcon(text, color);
            oldIcon?.Dispose();
        }

        private async Task UpdateBatteryStatusAsync()
        {
            if (_trayIcon == null) return;

            DeviceBatteryInfo? info = await Task.Run(() => _hidExplore.FindDongleInterfaces());

            if (info == null)
            {
                SetTrayIcon("?", Color.Gray);
                _trayIcon.ToolTipText = "Cant find dongle";
                return;
            }

            if (info.isAsleep)
            {
                string iconText = _lastKnownPercent.HasValue ? _lastKnownPercent.Value.ToString() : "?";
                string tipText = _lastKnownPercent.HasValue ? $"{_lastKnownPercent}%" : "unknown";

                SetTrayIcon(iconText, Color.Gray);
                _trayIcon.ToolTipText = $"{info.Name}: {tipText} (sleeping)";
                return;
            }

            _lastKnownPercent = info.BatteryPercent;

            Color textColor = Color.White;
            if (info.isCharging)
            {
                textColor = Color.LightGreen;
            }
            else if (info.BatteryPercent <= 20)
            {
                textColor = Color.OrangeRed;
            }

            SetTrayIcon(info.BatteryPercent.ToString(), textColor);
            _trayIcon.ToolTipText = info.isCharging
                ? $"{info.Name}: {info.BatteryPercent}%  (charging)"
                : $"{info.Name}: {info.BatteryPercent}%";
        }

        private async void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await UpdateBatteryStatusAsync();
        }

        private void AutoStartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.IsChecked)
                {
                    _autoStartService.Enable();
                }
                else
                {
                    _autoStartService.Disable();
                }
            }
        }
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            _trayIcon?.Icon?.Dispose();
            _trayIcon?.Dispose();
            Shutdown();
        }
    }
}