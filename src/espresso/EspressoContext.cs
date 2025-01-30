using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace espresso
{
    public class EspressoContext : ApplicationContext
    {
#if DEBUG
        private const int IntervalSeconds = 5;
#else
        private const int IntervalSeconds = 30;
#endif

        private readonly Random _random = new Random();
        private readonly HashSet<string> _excludedProcessNames;
        private readonly NotifyIcon _notifyIcon;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public EspressoContext()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var configProcessNames = configuration.GetSection("decaffinatedProcessNames")
                .Get<string[]>()
                .Select(s => s.ToLowerInvariant());
            _excludedProcessNames = new HashSet<string>(configProcessNames);

            _notifyIcon = new NotifyIcon()
            {
                Icon = Resources.EspressoIcon,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Exit", Exit), 
                }),
                Text = @"Consuming Espresso",
                Visible = true,
            };
            Task.Run(ConsumeEspresso);
        }

        [Conditional("DEBUG")]
        private void DebugLogConsole(string message)
        {
            Debug.WriteLine(message);
        }

        async Task ConsumeEspresso()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var focussedProcessName = Window.GetForegroundProcessName();

                var idleTime = Window.GetTimeSinceLastInput();
                if (idleTime > TimeSpan.FromSeconds(IntervalSeconds))
                {
                    if (_excludedProcessNames.Contains(focussedProcessName.ToLowerInvariant()))
                    {
                        const Keyboard.ScanCodeShort scanCode = Keyboard.ScanCodeShort.ZOOM;
                        DebugLogConsole($"Excluded Process {focussedProcessName} idle {idleTime}: Sending {scanCode}");
                        Keyboard.Send(scanCode);
                    }
                    else
                    {
                        const Keyboard.ScanCodeShort scanCode = Keyboard.ScanCodeShort.F15;
                        DebugLogConsole($"Allowed Process {focussedProcessName} idle {idleTime}: Sending {scanCode}");
                        Keyboard.Send(scanCode);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds + _random.Next(-1 * IntervalSeconds, IntervalSeconds)), _cancellationTokenSource.Token);
            }
        }

        void Exit(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
