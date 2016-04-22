using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Host
{
    public class Settings
    {
        private IDictionary<string, object> _settings;

        internal Settings(Application application)
        {
            MergeSettings(application);
        }
        private Settings(IDictionary<string, object> settings)
        {
            _settings = settings;
        }

        public object Count => _settings.Count;

        private void MergeSettings(Application application)
        {
            _settings = application.Settings != null ? 
                new ConcurrentDictionary<string, object>(application.Settings) : 
                new ConcurrentDictionary<string, object>();

            if (application.MessageReceivers != null)
            {
                foreach (var messageReceiver in application.MessageReceivers)
                    _settings[messageReceiver.Type] = new Settings(messageReceiver.Settings);
            }
            if (application.NotificationReceivers != null)
            {
                foreach (var notificationReceiver in application.NotificationReceivers)
                    _settings[notificationReceiver.Type] = new Settings(notificationReceiver.Settings);
            }
        }

        public object this[string key]
        {
            get
            {
                object value;
                return _settings.TryGetValue(key, out value) ? value : null;
            }
        }
    }
}