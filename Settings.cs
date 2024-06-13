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
        public static Settings Default { get; set; } = new Settings();
        public bool IsNotificationToggleOn { get; set; }
        private const string notificationToggleKey = "notificationToggleKey";
        public int FastOrEatingWindowNotificationId = 1337;
        public int OneHourLeftNotificationId = 420;
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
            bool state = Preferences.Default.Get(notificationToggleKey, IsNotificationToggleOn);
            return state;
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

    }
}
