﻿using Microsoft.Maui.Graphics.Text;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.Threading.Channels;

namespace IntermittentFasting
{
    public partial class MainPage : ContentPage
    {
        private const int defaultIntermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private const int defaultEatingWindowPeriod = 28800; //28800 seconds -> 8 hours
        private int intermittentFastingPeriod = 56600;
        private int eatingWindowPeriod = 28800;
        private DateTime timeWhenFastCanBeBroken;
        private DateTime timeWhenEatingWindowEnds;
        private const string fastTimeKey = "fastTimeKey";
        private const string breakFastTimeKey = "breakFastTimeKey";
        private const int fastOrEatingWindowNotificationId = 1337;
        private bool isFastInProgress = false;
        private bool isEatingWindowInProgress = false;
        private const string notificationToggleKey = "notificationToggleKey";
        private bool isNotificationsToggleOn = true;
        private const int minHours = 1;
        private const int maxHours = 23;

        public MainPage()
        {
            InitializeComponent();
            TimeNowLbl.Text = TimeNowLbl.Text = "Current Time: " + DateTime.Now.ToString("T");

            this.Loaded += CurrentTime;

            timeWhenFastCanBeBroken = GetFastTime();
            timeWhenEatingWindowEnds = GetBreakFastTime();

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

            timeWhenFastCanBeBroken = DateTime.Now.AddSeconds(defaultIntermittentFastingPeriod);
            SetStartFastTexts();
            DisplayAlertDialog("", "Fast Started! Fast can be broken " + timeWhenFastCanBeBroken.ToString("T"), "Start Fasting!");
            SaveFastTime(timeWhenFastCanBeBroken);
            isFastInProgress = true;

            CreateReminderNotification("Fast over!", "You can now break your fast!", 1337, false);
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

            timeWhenEatingWindowEnds = DateTime.Now.AddSeconds(defaultEatingWindowPeriod);
            //SetStartEatingWindowTexts();
            FastTimeLbl.Text = "Eating window ends: " + timeWhenEatingWindowEnds.ToString("T");
            BreakFastBtn.Text = "Click to end eating window";

            DisplayAlertDialog("", "Fast broken! Eating window ends " + timeWhenEatingWindowEnds.ToString("T"), "Start Eating!");
            SaveBreakFastTime(timeWhenEatingWindowEnds);
            isEatingWindowInProgress = true;

            CreateReminderNotification("Eating window over", "Start fasting!", fastOrEatingWindowNotificationId, false);
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

            CancelNotification(fastOrEatingWindowNotificationId);
        }

        private void CreateReminderNotification(string title, string subtitle, int notificationId, bool isRepeating)
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
                    NotifyTime = DateTime.Now.AddSeconds(defaultIntermittentFastingPeriod),
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

        //TODO: One hour left notification

        private void OnCustomizeFastBtnClicked(object sender, EventArgs e)
        {
            int fastHours = Int32.Parse(FastHoursEntry.Text);
            int eatHours = Int32.Parse(EatHoursEntry.Text);

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

            intermittentFastingPeriod = fastHours;
            eatingWindowPeriod = eatHours;
        }
    }

}
