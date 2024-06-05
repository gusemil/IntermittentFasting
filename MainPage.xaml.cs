using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int DefaultIntermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private const int DefaultEatingWindowPeriod = 28800; //28800 seconds -> 8 hours
        private const int OneHourInSeconds = 3600;
        private const string FastTimeKey = "fastTimeKey";
        private const string BreakFastTimeKey = "breakFastTimeKey";
        private const int FastOrEatingWindowNotificationId = 1337;
        private const int OneHourLeftNotificationId = 420;
        private const string CustomFastingPeriodKey = "customFastingPeriodKey";
        private const string CustomEatingWindowPeriodKey = "customEatingWindowPeriodKey";
        private const string NotificationToggleKey = "notificationToggleKey";

        private int intermittentFastingPeriod = DefaultIntermittentFastingPeriod;
        private int eatingWindowPeriod = DefaultEatingWindowPeriod;
        private DateTime timeWhenFastCanBeBroken;
        private DateTime timeWhenEatingWindowEnds;
        private bool isFastInProgress = false;
        private bool isEatingWindowInProgress = false;
        private bool isNotificationsToggleOn = true;

        private Timer repeatFastTimer = null;


        public MainPage()
        {
            InitializeComponent();
            TimeNowLbl.Text = TimeNowLbl.Text = "Current Time: " + DateTime.Now.ToString("T");

            timeWhenFastCanBeBroken = GetFastTime();
            timeWhenEatingWindowEnds = GetBreakFastTime();
            intermittentFastingPeriod = GetCustomFastingPeriod();
            eatingWindowPeriod = GetCustomEatingWindowPeriod();

            FastHoursEntry.Text = (intermittentFastingPeriod / OneHourInSeconds).ToString();
            EatHoursEntry.Text = (eatingWindowPeriod / OneHourInSeconds).ToString();

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
            if(!isNotificationsToggleOn)
            {
                CancelNotification(OneHourLeftNotificationId);
                CancelNotification(FastOrEatingWindowNotificationId);
            }
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
            Fast(true, false);
        }

        private void Fast(bool isManual = false, bool isScheduled = false)
        {
            if (!isScheduled) repeatFastTimer?.Dispose();

            if (isEatingWindowInProgress && !isScheduled)
            {
                ResetBreakFastTime();
                DisplayAlertDialog("", "Eating window has been reset!", "Ok");
                return;
            }
            else if (isFastInProgress && !isScheduled)
            {
                ResetFast();
                DisplayAlertDialog("", "Fast has been reset!", "Start Fasting!");
                return;
            }

            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(intermittentFastingPeriod);
            DisplayAlertDialog("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            SaveFastTime(timeWhenFastCanBeBroken);
            isFastInProgress = true;

            if (eatingWindowPeriod > OneHourInSeconds) CreateReminderNotification("Fast can be broken in one hour", "1 hour left!", OneHourLeftNotificationId, intermittentFastingPeriod - OneHourInSeconds, isScheduled);
            CreateReminderNotification("Fast over!", "You can now break your fast!", FastOrEatingWindowNotificationId, intermittentFastingPeriod, isScheduled);

            SetStartFastTexts();
        }

        private void DisplayAlertDialog(string title, string message, string cancel)
        {
            Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        private void OnBreakFastButtonClicked(object sender, EventArgs e)
        {
            BreakFast(true, false);
        }

        private void BreakFast(bool isManual = false, bool isRepeating = false)
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

            DisplayAlertDialog("", "Fast broken! Eating window ends " + timeWhenEatingWindowEnds.ToString("T"), "Start Eating!");
            SaveBreakFastTime(timeWhenEatingWindowEnds);
            isEatingWindowInProgress = true;

            if (intermittentFastingPeriod > OneHourInSeconds) CreateReminderNotification("Eating window is over in one hour", "1 hour left!", OneHourLeftNotificationId, eatingWindowPeriod - OneHourInSeconds, isRepeating);
            CreateReminderNotification("Eating window over", "Start fasting!", FastOrEatingWindowNotificationId, eatingWindowPeriod, isRepeating);

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

            CancelNotification(FastOrEatingWindowNotificationId);
            CancelNotification(OneHourLeftNotificationId);
        }
        
        private void CancelNotification(int id)
        {
            LocalNotificationCenter.Current.Cancel(id);
        }

        private void SaveNotificationToggleState()
        {
            Preferences.Default.Set(NotificationToggleKey, isNotificationsToggleOn);
        }

        private bool GetNotificationToggleState()
        {
            bool state = Preferences.Default.Get(NotificationToggleKey, isNotificationsToggleOn);
            return state;
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

            CancelNotification(OneHourLeftNotificationId);
            CancelNotification(FastOrEatingWindowNotificationId);
        }

        private void CreateReminderNotification(string title, string subtitle, int notificationId, int notificationDelayInSeconds, bool isScheduled)
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

            /*
            if (isRepeating) //not necessary due timer method? Try first without
            {
                request.Schedule.NotifyRepeatInterval = TimeSpan.FromDays(1);
                request.Schedule.RepeatType = NotificationRepeat.Daily;
            }
            */

            LocalNotificationCenter.Current.Show(request);
        }

        private void ExecuteFast(object? state)
        {
            Fast(false,true);
        }

        private void StopFastTimer()
        {
            repeatFastTimer.Dispose();
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

            intermittentFastingPeriod = fastHours * OneHourInSeconds; //To seconds
            eatingWindowPeriod = eatHours * OneHourInSeconds; //To seconds

            SaveCustomFastingPeriod(intermittentFastingPeriod);
            SaveCustomEatingWindowPeriod(eatingWindowPeriod);

            DisplayAlertDialog("", "Set custom fast of " + fastHours + " fasting hours with an eating window of " + eatHours + " hours. Aka a " + fastHours + ":" + eatHours + " fast", "Ok");
        }

        private void SaveCustomFastingPeriod(int fastPeriod)
        {
            Preferences.Default.Set(CustomFastingPeriodKey, fastPeriod);
        }

        private int GetCustomFastingPeriod()
        {
            int time = Preferences.Default.Get(CustomFastingPeriodKey, DefaultIntermittentFastingPeriod);
            return time;
        }

        private void SaveCustomEatingWindowPeriod(int eatPeriod)
        {
            Preferences.Default.Set(CustomEatingWindowPeriodKey, eatPeriod);
        }

        private int GetCustomEatingWindowPeriod()
        {
            int time = Preferences.Default.Get(CustomEatingWindowPeriodKey, DefaultEatingWindowPeriod);
            return time;
        }

        private void OnSetScheduledFasttBtnClicked(object sender, EventArgs e)
        {
            double eatingPeriodStartTime = EatHoursStartTimePicker.Time.TotalSeconds;
            double eatingPeriodEndTime = EatHoursEndTimePicker.Time.TotalSeconds;

            int eatingPeriodTotal = Convert.ToInt32(eatingPeriodEndTime - eatingPeriodStartTime);

            if(eatingPeriodTotal <= 0)
            {
                DisplayAlertDialog("", "Scheduled Eating Period End Time is less than Start Time", "Ok");
                return;
            } 
            else
            {
                DisplayAlertDialog("", "Set Eating period of " + EatHoursStartTimePicker.Time + " to " + EatHoursEndTimePicker.Time, "Ok");
            }

            //Reset All before scheduling (or not?)
            repeatFastTimer?.Dispose();
            ResetFast();
            ResetBreakFastTime();

            //Check whether we are fasting or eating

            //Set fasting period and save it

            //Create fast timer
            //repeatFastTimer = new Timer(ExecuteFast, null, TimeSpan.Zero, TimeSpan.FromSeconds(eatingPeriodTotal));

            //Set eating period and save it

            //Create eating period timer

            //EatHoursEndTimePicker
        }
    }

}
