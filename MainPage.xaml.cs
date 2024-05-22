using System.Timers;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int intermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private DateTime timeWhenFastCanBeBroken;
        private bool fastInProgress;

        public MainPage()
        {
            InitializeComponent();

            this.Loaded += CurrentTime;

            if (!fastInProgress)
            {
                FastTimerBtn.Text = "Start fast";
            } 
            else
            {
                FastTimerBtn.Text = "Click to reset fast";
            }
        }


        private void CurrentTime(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    MainThread.InvokeOnMainThreadAsync(() => { TimeNow.Text = "Current Time: " + DateTime.Now.ToString("T"); });
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }

        private void OnFastButtonClicked(object sender, EventArgs e)
        {
            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(intermittentFastingPeriod);
            FastTimeLbl.Text = "Fast can be broken: " + timeWhenFastCanBeBroken.ToString("T");
            FastTimerBtn.Text = "Click to reset fast";
            Application.Current.MainPage.DisplayAlert("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
        }

    }

}
