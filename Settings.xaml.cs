namespace IntermittentFasting;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
	}

    private void OnToMainPageButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage.Navigation.PushModalAsync(new MainPage(), true);
    }
}