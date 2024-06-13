using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const string FastTimeKey = "fastTimeKey";
        private const string BreakFastTimeKey = "breakFastTimeKey";

        private DateTime timeWhenFastCanBeBroken;
        private DateTime timeWhenEatingWindowEnds;
        private bool isFastInProgress = false;
        private bool isEatingWindowInProgress = false;
        private Settings settings = Settings.Default;


        public MainPage()
        {
            InitializeComponent();
            TimeNowLbl.Text = TimeNowLbl.Text = "Current Time: " + DateTime.Now.ToString("T");

            timeWhenFastCanBeBroken = GetFastTime();
            timeWhenEatingWindowEnds = GetBreakFastTime();


            if (DateTime.Now > timeWhenFastCanBeBroken && isFastInProgress)
            {
                ResetFast();
            } 
            else if(!isEatingWindowInProgress || timeWhenFastCanBeBroken.Second < 0)
            {
                isFastInProgress = true;
                SetStartFastTexts();
            }

            if(DateTime.Now > timeWhenEatingWindowEnds && isEatingWindowInProgress)
            {
                ResetBreakFastTime();
            }
            else if (!isFastInProgress || timeWhenEatingWindowEnds.Second > 0)
            {
                isEatingWindowInProgress = true;
                FastTimeLbl.Text = "Eating window ends: " + timeWhenEatingWindowEnds.ToString("T");
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
            if(isFastInProgress && ((DateTime.Now - timeWhenFastCanBeBroken).TotalSeconds < 0)) 
            {
                TimeNowLbl.Text = "Fasting Time Left: " + ((DateTime.Now - timeWhenFastCanBeBroken) * -1).ToString("T");
            }
            else if (isEatingWindowInProgress && ((DateTime.Now - timeWhenEatingWindowEnds).TotalSeconds < 0))
            {
                TimeNowLbl.Text = "Eating time Left: " + ((DateTime.Now - timeWhenEatingWindowEnds) * -1).ToString("T");
            }

            if(DateTime.Now > timeWhenFastCanBeBroken && !isEatingWindowInProgress)
            {
                ResetFast();
            }
            else if(DateTime.Now > timeWhenEatingWindowEnds && !isFastInProgress)
            {
                ResetBreakFastTime();
            }
        }

        private void OnFastButtonClicked(object sender, EventArgs e)
        {
            if (isEatingWindowInProgress)
            {
                ResetBreakFastTime();
                settings.DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }
            else if(isFastInProgress)
            {
                ResetFast();
                settings.DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }

            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(settings.intermittentFastingPeriod);
            settings.DisplayAlertDialog("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            SaveFastTime(timeWhenFastCanBeBroken);
            isFastInProgress = true;

            if(settings.eatingWindowPeriod > settings.OneHourInSeconds) CreateReminderNotification("Fast can be broken in one hour", "1 hour left!", settings.OneHourLeftNotificationId, settings.intermittentFastingPeriod - settings.OneHourInSeconds, false);
            CreateReminderNotification("Fast over!", "You can now break your fast!", settings.FastOrEatingWindowNotificationId, settings.intermittentFastingPeriod, false);

            SetStartFastTexts();
        }

        private void OnBreakFastButtonClicked(object sender, EventArgs e)
        {
            if (isFastInProgress)
            {
                ResetFast();
                settings.DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }
            else if (isEatingWindowInProgress)
            {
                ResetBreakFastTime();
                settings.DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }

            timeWhenEatingWindowEnds = DateTime.Now.AddSeconds(settings.eatingWindowPeriod);
            //SetStartEatingWindowTexts();

            settings.DisplayAlertDialog("", "Fast broken! Eating window ends " + timeWhenEatingWindowEnds.ToString("T"), "Start Eating!");
            SaveBreakFastTime(timeWhenEatingWindowEnds);
            isEatingWindowInProgress = true;

            if (settings.intermittentFastingPeriod > settings.OneHourInSeconds) CreateReminderNotification("Eating window is over in one hour", "1 hour left!", settings.OneHourLeftNotificationId, settings.eatingWindowPeriod - settings.OneHourInSeconds, false);
            CreateReminderNotification("Eating window over", "Start fasting!", settings.FastOrEatingWindowNotificationId, settings.eatingWindowPeriod, false);

            FastTimeLbl.Text = "Eating window ends: " + timeWhenEatingWindowEnds.ToString("T");
            BreakFastBtn.Text = "Click to end eating window";
        }

        private void SetStartFastTexts()
        {
            FastTimeLbl.Text = "Fast can be broken: " + timeWhenFastCanBeBroken.ToString("T");
            FastTimerBtn.Text = "Click to reset fast";
        }
        
        private void SetResetFastTexts()
        {
            FastTimeLbl.Text = "Click to Start Fasting!";
            FastTimerBtn.Text = "Start fast";
        }

        private void SaveFastTime(DateTime fastTime)
        {
            Preferences.Default.Set(FastTimeKey, fastTime);
        }

        private DateTime GetFastTime()
        {
            DateTime time = Preferences.Default.Get(FastTimeKey, timeWhenFastCanBeBroken);
            return time;
        }

        private void ResetFast() 
        {
            Preferences.Default.Remove(FastTimeKey);

            SetResetFastTexts();
            isFastInProgress = false;

            settings.CancelNotifications();
        }

        private void SaveBreakFastTime(DateTime fastTime)
        {
            Preferences.Default.Set(BreakFastTimeKey, fastTime);
        }

        private DateTime GetBreakFastTime()
        {
            DateTime time = Preferences.Default.Get(BreakFastTimeKey, timeWhenEatingWindowEnds);
            return time;
        }

        private void ResetBreakFastTime()
        {
            Preferences.Default.Remove(BreakFastTimeKey);

            //SetResetFastTexts();
            isEatingWindowInProgress = false;
            FastTimeLbl.Text = "Click to start fast";
            BreakFastBtn.Text = "Click to start eating window";

            settings.CancelNotifications();
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
