namespace IntermittentFasting
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            Routing.RegisterRoute(nameof(Settings), typeof(Settings));
        }
    }
}
