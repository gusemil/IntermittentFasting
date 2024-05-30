using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int defaultIntermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private const int defaultEatingWindowPeriod = 28800; //28800 seconds -> 8 hours
        private const int oneHourInSeconds = 3600;
        private int intermittentFastingPeriod = defaultIntermittentFastingPeriod;
        private int eatingWindowPeriod = defaultEatingWindowPeriod;
        private DateTime timeWhenFastCanBeBroken;
        private DateTime timeWhenEatingWindowEnds;
        private const string fastTimeKey = "fastTimeKey";
        private const string breakFastTimeKey = "breakFastTimeKey";
        private const int fastOrEatingWindowNotificationId = 1337;
        private const int oneHourLeftNotificationId = 420;
        private bool isFastInProgress = false;
        private bool isEatingWindowInProgress = false;
        private const string notificationToggleKey = "notificationToggleKey";
        private bool isNotificationsToggleOn = true;
        private const string customFastingPeriodKey = "customFastingPeriodKey";
        private const string customEatingWindowPeriodKey = "customEatingWindowPeriodKey";

        public MainPage()
        {
            InitializeComponent();
            TimeNowLbl.Text = TimeNowLbl.Text = "Current Time: " + DateTime.Now.ToString("T");

            timeWhenFastCanBeBroken = GetFastTime();
            timeWhenEatingWindowEnds = GetBreakFastTime();
            intermittentFastingPeriod = GetCustomFastingPeriod();
            eatingWindowPeriod = GetCustomEatingWindowPeriod();

            FastHoursEntry.Text = (intermittentFastingPeriod / oneHourInSeconds).ToString();
            EatHoursEntry.Text = (eatingWindowPeriod / oneHourInSeconds).ToString();

            isNotificationsToggleOn = GetNotificationToggleState();

            ToggleNotificationSwitch.IsToggled = isNotificationsToggleOn;

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

        private void OnNotificationsToggled(object sender, EventArgs e)
        {
            isNotificationsToggleOn = ToggleNotificationSwitch.IsToggled;
            SaveNotificationToggleState();
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
                DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }
            else if(isFastInProgress)
            {
                ResetFast();
                DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }

            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(intermittentFastingPeriod);
            SetStartFastTexts();
            DisplayAlertDialog("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            SaveFastTime(timeWhenFastCanBeBroken);
            isFastInProgress = true;

            if(eatingWindowPeriod > oneHourInSeconds) CreateReminderNotification("Fast can be broken in one hour", "1 hour left!", oneHourLeftNotificationId, intermittentFastingPeriod - oneHourInSeconds, false);
            CreateReminderNotification("Fast over!", "You can now break your fast!", fastOrEatingWindowNotificationId, intermittentFastingPeriod, false);
        }

        private void DisplayAlertDialog(string title, string message, string cancel)
        {
            Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        private void OnBreakFastButtonClicked(object sender, EventArgs e)
        {
            if (isFastInProgress)
            {
                ResetFast();
                DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }
            else if (isEatingWindowInProgress)
            {
                ResetBreakFastTime();
                DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }

            timeWhenEatingWindowEnds = DateTime.Now.AddSeconds(eatingWindowPeriod);
            //SetStartEatingWindowTexts();
            FastTimeLbl.Text = "Eating window ends: " + timeWhenEatingWindowEnds.ToString("T");
            BreakFastBtn.Text = "Click to end eating window";

            DisplayAlertDialog("", "Fast broken! Eating window ends " + timeWhenEatingWindowEnds.ToString("T"), "Start Eating!");
            SaveBreakFastTime(timeWhenEatingWindowEnds);
            isEatingWindowInProgress = true;

            if (intermittentFastingPeriod > oneHourInSeconds) CreateReminderNotification("Eating window is over in one hour", "1 hour left!", oneHourLeftNotificationId, eatingWindowPeriod - oneHourInSeconds, false);
            CreateReminderNotification("Eating window over", "Start fasting!", fastOrEatingWindowNotificationId, eatingWindowPeriod, false);
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
            Preferences.Default.Set(fastTimeKey, fastTime);
        }

        private DateTime GetFastTime()
        {
            DateTime time = Preferences.Default.Get(fastTimeKey, timeWhenFastCanBeBroken);
            return time;
        }

        private void ResetFast() 
        {
            Preferences.Default.Remove(fastTimeKey);

            SetResetFastTexts();
            isFastInProgress = false;

            CancelNotification(fastOrEatingWindowNotificationId);
            CancelNotification(oneHourLeftNotificationId);
        }
        
        private void CancelNotification(int id)
        {
            LocalNotificationCenter.Current.Cancel(id);
        }

        private void SaveNotificationToggleState()
        {
            Preferences.Default.Set(notificationToggleKey, isNotificationsToggleOn);
        }

        private bool GetNotificationToggleState()
        {
            bool state = Preferences.Default.Get(notificationToggleKey, isNotificationsToggleOn);
            return state;
        }

        private void SaveBreakFastTime(DateTime fastTime)
        {
            Preferences.Default.Set(breakFastTimeKey, fastTime);
        }

        private DateTime GetBreakFastTime()
        {
            DateTime time = Preferences.Default.Get(breakFastTimeKey, timeWhenEatingWindowEnds);
            return time;
        }

        private void ResetBreakFastTime()
        {
            Preferences.Default.Remove(breakFastTimeKey);

            //SetResetFastTexts();
            isEatingWindowInProgress = false;
            FastTimeLbl.Text = "Click to break fast";
            BreakFastBtn.Text = "Click to end fast";

            CancelNotification(oneHourLeftNotificationId);
            CancelNotification(fastOrEatingWindowNotificationId);
        }

        private void CreateReminderNotification(string title, string subtitle, int notificationId, int notificationDelayInSeconds, bool isRepeating)
        {
            if (!isNotificationsToggleOn) return;

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

        private void OnCustomizeFastBtnClicked(object sender, EventArgs e)
        {
            Int32.TryParse(FastHoursEntry.Text, out int fastHours);
            Int32.TryParse(EatHoursEntry.Text, out int eatHours);

            if (fastHours <= 0)
            {
                DisplayAlertDialog("","Invalid Fasting hours input","Ok");
                return;
            }

            if(eatHours <= 0)
            {
                DisplayAlertDialog("", "Invalid Eating period input", "Ok");
                return;
            }

            if ((fastHours + eatHours) != 24)
            {
                DisplayAlertDialog("", "Invalid hours in a day inserted", "Ok");
                return;
            }

            intermittentFastingPeriod = fastHours * oneHourInSeconds; //To seconds
            eatingWindowPeriod = eatHours * oneHourInSeconds; //To seconds

            SaveCustomFastingPeriod(intermittentFastingPeriod);
            SaveCustomEatingWindowPeriod(eatingWindowPeriod);

            DisplayAlertDialog("", "Set custom fast of " + fastHours + " fasting hours with an eating window of " + eatHours + " hours. Aka a " + fastHours + ":" + eatHours + " fast", "Ok");
        }

        private void SaveCustomFastingPeriod(int fastPeriod)
        {
            Preferences.Default.Set(customFastingPeriodKey, fastPeriod);
        }

        private int GetCustomFastingPeriod()
        {
            int time = Preferences.Default.Get(customFastingPeriodKey, defaultIntermittentFastingPeriod);
            return time;
        }

        private void SaveCustomEatingWindowPeriod(int eatPeriod)
        {
            Preferences.Default.Set(customEatingWindowPeriodKey, eatPeriod);
        }

        private int GetCustomEatingWindowPeriod()
        {
            int time = Preferences.Default.Get(customEatingWindowPeriodKey, defaultEatingWindowPeriod);
            return time;
        }
    }

}
