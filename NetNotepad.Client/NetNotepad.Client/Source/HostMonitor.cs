using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using R3;

namespace NetNotepad.Client.Source
{
    public static class HostMonitor
    {
        private static Dictionary<string, (ReactiveProperty<bool>, int, TimeSpan)> _hostAvailable = [];
        private static TimeSpan _tick = TimeSpan.FromSeconds(1);

        static HostMonitor()
        {
            new Thread(() =>
            {
                while (true)
                {
                    TimeSpan currentUpdate = DateTime.Now.TimeOfDay - TimeSpan.FromSeconds(Settings.UpdateTimer.Value);
                    for (int i = 0; i < _hostAvailable.Count; i++)
                    {
                        KeyValuePair<string, (ReactiveProperty<bool> rp, int count, TimeSpan time)> value = _hostAvailable.ElementAt(i);
                        if (value.Value.time >= currentUpdate)
                        {
                            value.Value.rp.Value = APIWorker.ServerAvailable(value.Key).Result;
                            _hostAvailable[value.Key] = new(value.Value.rp, value.Value.count, DateTime.Now.TimeOfDay);
                        }
                    }
                    Thread.Sleep(_tick);
                }
            })
            { IsBackground = true }.Start();
        }

        public static async Task<ReadOnlyReactiveProperty<bool>> Watch(string host)
        {
            if (!_hostAvailable.TryGetValue(host, out (ReactiveProperty<bool> rp, int count, TimeSpan time) value))
            {
                value.rp = new(await APIWorker.ServerAvailable(host));
                value.count = 0;
                value.time = DateTime.Now.TimeOfDay;
            }
            value.count++;
            _hostAvailable[host] = value;
            return value.rp;
        }

        public static void UnWatch(string host)
        {
            if (!_hostAvailable.TryGetValue(host, out (ReactiveProperty<bool> rp, int count, TimeSpan _) value)) { return; }
            value.count--;
            _hostAvailable[host] = value;
            if (value.count <= 0) { _hostAvailable.Remove(host); }
        }
    }
}