using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace espresso
{
    public class EspressoContext : ApplicationContext
    {
        private const int IntervalSeconds = 30;

        private readonly Random _random = new Random();

        private readonly NotifyIcon _notifyIcon;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public EspressoContext()
        {
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

        async Task ConsumeEspresso()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Keyboard.Send(Keyboard.ScanCodeShort.F15);
                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds + _random.Next(-10, 10)), _cancellationTokenSource.Token);
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
