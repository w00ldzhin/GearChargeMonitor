using GearChargeMonitor.Models;
using GearChargeMonitor.Services;
using H.NotifyIcon;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GearChargeMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private TaskbarIcon? _trayIcon;
        private readonly HidExplore _hidExplore = new();
        private DispatcherTimer? _timer;
        private int? _lastKnownPercent;

        private readonly BitmapImage _normalIcon = new(new Uri("pack://application:,,,/Assets/mouse.ico"));
        private readonly BitmapImage _chargingIcon = new(new Uri("pack://application:,,,/Assets/mouse_charging.ico"));

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            _trayIcon.ForceCreate();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _timer.Tick += async (s, args) => await UpdateBatteryStatusAsync();
            _timer.Start();

            await UpdateBatteryStatusAsync();
        }

        private async Task UpdateBatteryStatusAsync()
        {
            if (_trayIcon == null) return;

            DeviceBatteryInfo? info = await Task.Run(() => _hidExplore.FindDongleInterfaces());

            if (info == null)
            {
                _trayIcon.IconSource = _normalIcon;
                _trayIcon.ToolTipText = "Redragon Impact Elite: донгл не найден";
                return;
            }

            if (info.isAsleep)
            {
                _trayIcon.IconSource = _normalIcon;
                string lastPercentText = _lastKnownPercent.HasValue ? $"{_lastKnownPercent}%" : "неизвестно";
                _trayIcon.ToolTipText = $"{info.Name}: {lastPercentText} (спит)";
                return;
            }

            _lastKnownPercent = info.BatteryPercent;

            if (info.isCharging)
            {
                _trayIcon.IconSource = _chargingIcon;
                _trayIcon.ToolTipText = $"{info.Name}: {info.BatteryPercent}% ⚡ (заряжается)";
            }
            else
            {
                _trayIcon.IconSource = _normalIcon;
                _trayIcon.ToolTipText = $"{info.Name}: {info.BatteryPercent}%";
            }
        }

        private async void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await UpdateBatteryStatusAsync();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            _trayIcon?.Dispose();
            Shutdown();
        }
    }
}