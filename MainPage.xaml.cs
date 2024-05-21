using System.Timers;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int intermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private int fastTimerElapsed = 0; //in seconds
        private System.Timers.Timer fastTimer;

        public MainPage()
        {
            //CreateFastTimer();
            InitializeComponent();

            this.Loaded += CurrentTime;
        }

        private void CreateFastTimer()
        {
            //Create fast timer
            fastTimer = new System.Timers.Timer();
            fastTimer.Interval = 1000; // Set the interval to 1 seconds
            fastTimer.Elapsed += OnTimedEvent;
            fastTimer.AutoReset = false;
            fastTimer.Enabled = true;
            fastTimer.Start();
            Console.WriteLine("TIMER CREATED");
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            fastTimerElapsed++;
            FastTimeLbl.Text = "FAK";
            //FastTimeLbl.Text = (intermittentFastingPeriod - fastTimerElapsed).ToString();
            //Console.WriteLine((intermittentFastingPeriod - fastTimerElapsed).ToString());
        }

        /*
        private ElapsedEventHandler OnTimedEvent()
        {
            fastTimerElapsed += 1;
            FastTimeLbl.Text = (intermittentFastingPeriod - fastTimerElapsed).ToString();
            return new ElapsedEventHandler();
        }
        */

        private void CurrentTime(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    MainThread.InvokeOnMainThreadAsync(() => { TimeNow.Text = DateTime.Now.ToString("T"); });
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            /*
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
            */
        }

        private void OnFastTimerButtonClicked(object sender, EventArgs e)
        {
            //this.Loaded += FastTime;
            FastTimeLbl.Text = "CLICKED!";
            //this.Loaded += FastTime;
            //this.Loaded += FastTimeLeft;
        }

        private void FastTime(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    MainThread.InvokeOnMainThreadAsync(() => { FastTimeLbl.Text = DateTime.Now.AddSeconds(intermittentFastingPeriod).ToString("T"); });
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }

        /*
        private void FastTimeLeft(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000; // Set the interval to 1 seconds
            timer.Elapsed += OnTimedEvent();
            timer.AutoReset = true;
            timer.Enabled = false;

            /*
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    MainThread.InvokeOnMainThreadAsync(() => { FastTimeLbl.Text = (new DateTime().AddSeconds(intermittentFastingPeriod)).ToString("T"); });
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
            */
        //}

    }

}
