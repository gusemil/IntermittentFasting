using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int intermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private const int eatingWindowPeriod = 28800; //28800 seconds -> 8 hours
        private DateTime timeWhenFastCanBeBroken;
        private DateTime timeWhenEatingWindowEnds;
        private const string fastTimeKey = "fastTimeKey";
        private const string breakFastTimeKey = "breakFastTimeKey";
        private bool isFastInProgress = false;
        private bool isEatingWindowInProgress = false;
        private LocalNotificationCenter currentNotification;

        public MainPage()
        {
            InitializeComponent();
            TimeNowLbl.Text = DateTime.Now.ToString("T");

            this.Loaded += CurrentTime;

            timeWhenFastCanBeBroken = GetFastTime();
            timeWhenEatingWindowEnds = GetBreakFastTime();

            if (DateTime.Now > timeWhenFastCanBeBroken && isFastInProgress)
            {
                ResetFast();
            } 
            else if(!isEatingWindowInProgress)
            {
                isFastInProgress = true;
                SetStartFastTexts();
            }

            if(DateTime.Now > timeWhenEatingWindowEnds && isEatingWindowInProgress)
            {
                ResetBreakFastTime();
            }
            else if (!isFastInProgress)
            {
                isEatingWindowInProgress = true;
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
            if(isFastInProgress) 
            {
                //TimeNowLbl.Text = "Fasting Time Left: " + ((DateTime.Now - timeWhenFastCanBeBroken) * -1).ToString("HH:mm:ss");
                TimeNowLbl.Text = "Fasting Time Left: " + ((DateTime.Now - timeWhenFastCanBeBroken) * -1).ToString("T");
            }
            else
            {
                TimeNowLbl.Text = "Current Time: " + DateTime.Now.ToString("T");
            }
            if(DateTime.Now > timeWhenFastCanBeBroken)
            {
                ResetFast();
            }
        }

        private void OnFastButtonClicked(object sender, EventArgs e)
        {
            if(isFastInProgress)
            {
                ResetFast();
                Application.Current.MainPage.DisplayAlert("", "Fast has been reset!", "Start Fasting!");
                return;
            }

            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(intermittentFastingPeriod);
            SetStartFastTexts();
            Application.Current.MainPage.DisplayAlert("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            SaveFastTime(timeWhenFastCanBeBroken);
            isFastInProgress = true;

            CreateFastOverNotification(false);
        }

        private void OnBreakFastButtonClicked(object sender, EventArgs e)
        {
            if (isFastInProgress)
            {
                ResetFast();
                Application.Current.MainPage.DisplayAlert("", "Fast has been reset!", "Start Fasting!");
                return;
            }
            else if (isEatingWindowInProgress)
            {
                ResetBreakFastTime();
                Application.Current.MainPage.DisplayAlert("", "Eating window has been reset!", "Ok");
                return;
            }

            timeWhenEatingWindowEnds = DateTime.Now.AddSeconds(eatingWindowPeriod);
            //SetStartEatingWindowTexts();
            FastTimeLbl.Text = "Eating window ends: " + timeWhenEatingWindowEnds.ToString("T");
            BreakFastBtn.Text = "Click to end eating window";

            Application.Current.MainPage.DisplayAlert("", "Fast broken! Eating window ends " + timeWhenEatingWindowEnds.ToString("T"), "Start Eating!");
            SaveBreakFastTime(timeWhenEatingWindowEnds);
            isEatingWindowInProgress = true;

            CreateEatingWindowOverNotification(false);

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
        }

        private void CreateFastOverNotification(bool isRepeating)
        {
            NotificationRequest request = new NotificationRequest()
            {
                NotificationId = 1337,
                Title = "Fast over!",
                Subtitle = "You can now break your fast!",
                //Description = "You can now break your fast!",
                CategoryType = NotificationCategoryType.Reminder,
                Schedule = new NotificationRequestSchedule()
                {
                    NotifyTime = DateTime.Now.AddSeconds(intermittentFastingPeriod),
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

        private void CreateEatingWindowOverNotification(bool isRepeating)
        {
            NotificationRequest request = new NotificationRequest()
            {
                NotificationId = 1337,
                Title = "Eating window over",
                Subtitle = "Start fasting!",
                //Description = "You can now break your fast!",
                CategoryType = NotificationCategoryType.Reminder,
                Schedule = new NotificationRequestSchedule()
                {
                    NotifyTime = DateTime.Now.AddSeconds(eatingWindowPeriod),
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

        private void CreateStartFastNotification(bool isRepeating)
        {
            NotificationRequest request = new NotificationRequest()
            {
                NotificationId = 1337,
                Title = "Fast is starting!",
                Subtitle = "Don't eat until: " + timeWhenFastCanBeBroken,
                CategoryType = NotificationCategoryType.Reminder,
                Schedule = new NotificationRequestSchedule()
                {
                    NotifyTime = DateTime.Now.AddSeconds(intermittentFastingPeriod),
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

    }

}
