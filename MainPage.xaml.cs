using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private Settings settings;


        public MainPage()
        {
            settings = Settings.Default;
            InitializeComponent();
            TimeNowLbl.Text = TimeNowLbl.Text = "Current Time: " + DateTime.Now.ToString("T");

            settings.timeWhenFastCanBeBroken = settings.GetFastTime();
            settings.timeWhenEatingWindowEnds = settings.GetBreakFastTime();


            if (DateTime.Now > settings.timeWhenFastCanBeBroken && settings.isFastInProgress)
            {
                settings.ResetFast();
            } 
            else if(!settings.isEatingWindowInProgress || settings.timeWhenFastCanBeBroken.Second < 0)
            {
                settings.isFastInProgress = true;
                SetStartFastTexts();
            }

            if(DateTime.Now > settings.timeWhenEatingWindowEnds && settings.isEatingWindowInProgress)
            {
                settings.ResetBreakFastTime();
            }
            else if (!settings.isFastInProgress || settings.timeWhenEatingWindowEnds.Second > 0)
            {
                settings.isEatingWindowInProgress = true;
                FastTimeLbl.Text = "Eating window ends: " + settings.timeWhenEatingWindowEnds.ToString("T");
                BreakFastBtn.Text = "Click to end eating window";
            }

            this.Loaded += CurrentTime;
        }

        private void CurrentTime(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    MainThread.InvokeOnMainThreadAsync(() => { CheckTime(); });
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
        }

        private void CheckTime()
        {
            if(settings.isFastInProgress && ((DateTime.Now - settings.timeWhenFastCanBeBroken).TotalSeconds < 0)) 
            {
                TimeNowLbl.Text = "Fasting Time Left: " + ((DateTime.Now - settings.timeWhenFastCanBeBroken) * -1).ToString("T");
            }
            else if (settings.isEatingWindowInProgress && ((DateTime.Now - settings.timeWhenEatingWindowEnds).TotalSeconds < 0))
            {
                TimeNowLbl.Text = "Eating time Left: " + ((DateTime.Now - settings.timeWhenEatingWindowEnds) * -1).ToString("T");
            }

            if(DateTime.Now > settings.timeWhenFastCanBeBroken && !settings.isEatingWindowInProgress)
            {
                settings.ResetFast();
            }
            else if(DateTime.Now > settings.timeWhenEatingWindowEnds && !settings.isFastInProgress)
            {
                settings.ResetBreakFastTime();
            }
        }

        private void OnFastButtonClicked(object sender, EventArgs e)
        {
            if (settings.isEatingWindowInProgress)
            {
                settings.ResetBreakFastTime();
                settings.DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }
            else if(settings.isFastInProgress)
            {
                settings.ResetFast();
                settings.DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }

            settings.timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(settings.intermittentFastingPeriod);
            settings.DisplayAlertDialog("", "Fast Started! Fast can be broken " + settings.timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            settings.SaveFastTime(settings.timeWhenFastCanBeBroken);
            settings.isFastInProgress = true;

            if(settings.eatingWindowPeriod > settings.OneHourInSeconds) CreateReminderNotification("Fast can be broken in one hour", "1 hour left!", settings.OneHourLeftNotificationId, settings.intermittentFastingPeriod - settings.OneHourInSeconds, false);
            CreateReminderNotification("Fast over!", "You can now break your fast!", settings.FastOrEatingWindowNotificationId, settings.intermittentFastingPeriod, false);

            SetStartFastTexts();
        }

        private void OnBreakFastButtonClicked(object sender, EventArgs e)
        {
            if (settings.isFastInProgress)
            {
                SetResetFastTexts();
                settings.ResetFast();
                settings.DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }
            else if (settings.isEatingWindowInProgress)
            {
                FastTimeLbl.Text = "Click to start fast";
                BreakFastBtn.Text = "Click to start eating window";

                settings.ResetBreakFastTime();
                settings.DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }

            settings.timeWhenEatingWindowEnds = DateTime.Now.AddSeconds(settings.eatingWindowPeriod);
            //SetStartEatingWindowTexts();

            settings.DisplayAlertDialog("", "Fast broken! Eating window ends " + settings.timeWhenEatingWindowEnds.ToString("T"), "Start Eating!");
            settings.SaveBreakFastTime(settings.timeWhenEatingWindowEnds);
            settings.isEatingWindowInProgress = true;

            if (settings.intermittentFastingPeriod > settings.OneHourInSeconds) CreateReminderNotification("Eating window is over in one hour", "1 hour left!", settings.OneHourLeftNotificationId, settings.eatingWindowPeriod - settings.OneHourInSeconds, false);
            CreateReminderNotification("Eating window over", "Start fasting!", settings.FastOrEatingWindowNotificationId, settings.eatingWindowPeriod, false);

            FastTimeLbl.Text = "Eating window ends: " + settings.timeWhenEatingWindowEnds.ToString("T");
            BreakFastBtn.Text = "Click to end eating window";
        }

        private void SetStartFastTexts()
        {
            FastTimeLbl.Text = "Fast can be broken: " + settings.timeWhenFastCanBeBroken.ToString("T");
            FastTimerBtn.Text = "Click to reset fast";
        }
        
        private void SetResetFastTexts()
        {
            FastTimeLbl.Text = "Click to Start Fasting!";
            FastTimerBtn.Text = "Start fast";
        }

        private void CreateReminderNotification(string title, string subtitle, int notificationId, int notificationDelayInSeconds, bool isRepeating)
        {
            if (!settings.IsNotificationToggleOn) return;

            NotificationRequest request = new NotificationRequest()
            {
                NotificationId = notificationId,
                Title = title,
                Subtitle = subtitle,
                //Description = "You can now break your fast!",
                CategoryType = NotificationCategoryType.Reminder,
                Schedule = new NotificationRequestSchedule()
                {
                    NotifyTime = DateTime.Now.AddSeconds(notificationDelayInSeconds),
                },
                Android = new AndroidOptions
                {
                    LaunchAppWhenTapped = true
                }
            };

            if (isRepeating)
            {
                request.Schedule.NotifyRepeatInterval = TimeSpan.FromDays(1);
                request.Schedule.RepeatType = NotificationRepeat.Daily;
            }

            LocalNotificationCenter.Current.Show(request);
        }

        private void OnSettingsPageButtonClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new SettingsPage(), true);
        }
    }

}
