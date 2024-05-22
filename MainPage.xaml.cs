﻿using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int intermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private DateTime timeWhenFastCanBeBroken;
        private const string fastTimeKey = "timeKey";
        private bool isFastinProgress = false;
        private LocalNotificationCenter currentNotification;

        public MainPage()
        {
            InitializeComponent();

            this.Loaded += CurrentTime;

            timeWhenFastCanBeBroken = GetFastTime();

            if (DateTime.Now > timeWhenFastCanBeBroken)
            {
                ResetFast();
            } 
            else
            {
                isFastinProgress = true;
                SetStartFastTexts();
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
            TimeNow.Text = "Current Time: " + DateTime.Now.ToString("T");
            if(DateTime.Now > timeWhenFastCanBeBroken)
            {
                ResetFast();
            }
        }

        private void OnFastButtonClicked(object sender, EventArgs e)
        {
            if(isFastinProgress)
            {
                ResetFast();
                Application.Current.MainPage.DisplayAlert("", "Fast has been reset!", "Start Fasting!");
                return;
            }

            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(intermittentFastingPeriod);
            SetStartFastTexts();
            Application.Current.MainPage.DisplayAlert("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            SaveFastTime(timeWhenFastCanBeBroken);
            isFastinProgress = true;

            CreateFastOverNotification(false);
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
            isFastinProgress = false;
        }

        private void CreateFastOverNotification(bool isRepeating)
        {
            NotificationRequest request = new NotificationRequest()
            {
                NotificationId = 1337,
                Title = "Fast over!",
                Subtitle = "You can now break your fast!",
                //Description = "You can now break you!",
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

        private void CreateStartFastNotification(bool isRepeating)
        {
            NotificationRequest request = new NotificationRequest()
            {
                NotificationId = 1338,
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
