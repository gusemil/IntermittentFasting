namespace IntermittentFasting;

public partial class SettingsPage : ContentPage
{
    private Settings settings = Settings.Default;
    public SettingsPage()
	{
		InitializeComponent();

        ToggleNotificationSwitch.IsToggled = Settings.Default.GetNotificationToggleState();

        settings.intermittentFastingPeriod = settings.GetCustomFastingPeriod();
        settings.eatingWindowPeriod = settings.GetCustomEatingWindowPeriod();

        FastHoursEntry.Text = (settings.intermittentFastingPeriod / settings.OneHourInSeconds).ToString();
        EatHoursEntry.Text = (settings.eatingWindowPeriod / settings.OneHourInSeconds).ToString();
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

    private void OnCustomizeFastBtnClicked(object sender, EventArgs e)
    {
        Int32.TryParse(FastHoursEntry.Text, out int fastHours);
        Int32.TryParse(EatHoursEntry.Text, out int eatHours);

        if (fastHours <= 0)
        {
            settings.DisplayAlertDialog("", "Invalid Fasting hours input", "Ok");
            return;
        }

        if (eatHours <= 0)
        {
            settings.DisplayAlertDialog("", "Invalid Eating period input", "Ok");
            return;
        }

        if ((fastHours + eatHours) != 24)
        {
            settings.DisplayAlertDialog("", "Invalid hours in a day inserted", "Ok");
            return;
        }

        settings.intermittentFastingPeriod = fastHours * settings.OneHourInSeconds; //To seconds
        settings.eatingWindowPeriod = eatHours * settings.OneHourInSeconds; //To seconds

        settings.SaveCustomFastingPeriod(settings.intermittentFastingPeriod);
        settings.SaveCustomEatingWindowPeriod(settings.eatingWindowPeriod);

        settings.DisplayAlertDialog("", "Set custom fast of " + fastHours + " fasting hours with an eating window of " + eatHours + " hours. Aka a " + fastHours + ":" + eatHours + " fast", "Ok");
    }

    private void OnResetAllSettingsClicked(object sender, EventArgs e)
    {
        FastHoursEntry.Text = settings.DefaultIntermittentFastingPeriodInHours.ToString();
        EatHoursEntry.Text = settings.DefaultEatingWindowPeriodInHours.ToString();
        if(!ToggleNotificationSwitch.IsToggled) ToggleNotificationSwitch.IsToggled = true;
        settings.ResetAllSettings();
        settings.DisplayAlertDialog("", "Reset all settings", "Ok");
    }

    private void OnToMainPageButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage.Navigation.PushModalAsync(new MainPage(), true);
    }

}