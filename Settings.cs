using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntermittentFasting
{
    public class Settings
    {
        public DateTime timeWhenFastCanBeBroken;
        public DateTime timeWhenEatingWindowEnds;
        public static Settings Default { get; set; } = new Settings();
        public bool IsNotificationToggleOn { get; set; }
        private const string notificationToggleKey = "notificationToggleKey";
        private const int DefaultIntermittentFastingPeriod = 57600; //57600 seconds -> 16 hours
        private const int DefaultEatingWindowPeriod = 28800; //28800 seconds -> 8 hours
        private bool defaultNotificationToggleState = true;
        public int DefaultIntermittentFastingPeriodInHours = 16;
        public int DefaultEatingWindowPeriodInHours = 8;
        public const string CustomFastingPeriodKey = "customFastingPeriodKey";
        public const string CustomEatingWindowPeriodKey = "customEatingWindowPeriodKey";
        private const string FastTimeKey = "fastTimeKey";
        private const string BreakFastTimeKey = "breakFastTimeKey";
        public int FastOrEatingWindowNotificationId = 1337;
        public int OneHourLeftNotificationId = 420;
        public int intermittentFastingPeriod = DefaultIntermittentFastingPeriod;
        public int eatingWindowPeriod = DefaultEatingWindowPeriod;
        public int OneHourInSeconds = 3600;
        public bool isFastInProgress = false;
        public bool isEatingWindowInProgress = false;
        public Settings()
        {
            IsNotificationToggleOn = GetNotificationToggleState();
        }

        public void SaveNotificationToggleState()
        {
            Preferences.Default.Set(notificationToggleKey, IsNotificationToggleOn);
        }

        public bool GetNotificationToggleState()
        {
            bool state = Preferences.Default.Get(notificationToggleKey, defaultNotificationToggleState); //True by default if we can't get the key
            return state;
        }

        public void DisplayAlertDialog(string title, string message, string cancel)
        {
            Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public void CancelNotifications()
        {
            CancelNotification(FastOrEatingWindowNotificationId);
            CancelNotification(OneHourLeftNotificationId);
        }

        private void CancelNotification(int id)
        {
            LocalNotificationCenter.Current.Cancel(id);
        }

        public void SaveCustomFastingPeriod(int fastPeriod)
        {
            Preferences.Default.Set(CustomFastingPeriodKey, fastPeriod);
        }

        public int GetCustomFastingPeriod()
        {
            int time = Preferences.Default.Get(CustomFastingPeriodKey, DefaultIntermittentFastingPeriod);
            return time;
        }

        public void SaveCustomEatingWindowPeriod(int eatPeriod)
        {
            Preferences.Default.Set(CustomEatingWindowPeriodKey, eatPeriod);
        }

        public int GetCustomEatingWindowPeriod()
        {
            int time = Preferences.Default.Get(CustomEatingWindowPeriodKey, DefaultEatingWindowPeriod);
            return time;
        }


        public void SaveFastTime(DateTime fastTime)
        {
            Preferences.Default.Set(FastTimeKey, fastTime);
        }

        public DateTime GetFastTime()
        {
            DateTime time = Preferences.Default.Get(FastTimeKey, timeWhenFastCanBeBroken);
            return time;
        }

        public void SaveBreakFastTime(DateTime fastTime)
        {
            Preferences.Default.Set(BreakFastTimeKey, fastTime);
        }

        public DateTime GetBreakFastTime()
        {
            DateTime time = Preferences.Default.Get(BreakFastTimeKey, timeWhenEatingWindowEnds);
            return time;
        }

        public void ResetFast()
        {
            Preferences.Default.Remove(FastTimeKey);

            isFastInProgress = false;

            CancelNotifications();
        }

        public void ResetBreakFastTime()
        {
            Preferences.Default.Remove(BreakFastTimeKey);

            isEatingWindowInProgress = false;

            CancelNotifications();
        }

        public void ResetAllSettings()
        {
            ResetFast();
            ResetBreakFastTime();
            Preferences.Default.Remove(BreakFastTimeKey);
            Preferences.Default.Remove(FastTimeKey);
            Preferences.Default.Remove(CustomFastingPeriodKey);
            Preferences.Default.Remove(CustomEatingWindowPeriodKey);
            Preferences.Default.Remove(notificationToggleKey);

            isEatingWindowInProgress = false;
            isFastInProgress = false;

            CancelNotifications();
        }

    }
}
