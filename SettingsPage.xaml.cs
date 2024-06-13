namespace IntermittentFasting;

public partial class SettingsPage : ContentPage
{
    private Settings settings = Settings.Default;
    public SettingsPage()
	{
		InitializeComponent();

        ToggleNotificationSwitch.IsToggled = Settings.Default.GetNotificationToggleState();
    }

    private void OnNotificationsToggled(object sender, EventArgs e)
    {
        settings.IsNotificationToggleOn = ToggleNotificationSwitch.IsToggled;
        settings.SaveNotificationToggleState();
        if (!settings.IsNotificationToggleOn)
        {
            settings.CancelNotifications();
        }
    }

    private void OnToMainPageButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage.Navigation.PushModalAsync(new MainPage(), true);
    }

}