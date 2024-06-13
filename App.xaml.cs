namespace IntermittentFasting
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}
