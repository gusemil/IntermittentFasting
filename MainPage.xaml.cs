namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            this.Loaded += CurrentTime;
        }

        private void CurrentTime(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    MainThread.InvokeOnMainThreadAsync(() => { TimeNow.Text = DateTime.Now.ToString("T"); });
                },null, TimeSpan.FromSeconds(1),TimeSpan.FromSeconds(1));
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
    }

}
