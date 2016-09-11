using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ZetaOne
{
    class AnomalyLabels
    {
        private static AnomalyLabels _instance;
        private readonly Dictionary<string, List<Tuple<DateTime, DateTime>>> _windows;

        private AnomalyLabels()
        {
            _windows = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();
            var sr = new StreamReader(@"..\labels\combined_windows.json", Encoding.GetEncoding("utf-8"));
            string json = sr.ReadToEnd();
            sr.Close();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<string>>>>(json);
            foreach (var file in dict)
            {
                var list = new List<Tuple<DateTime, DateTime>>();
                foreach (var obj in file.Value)
                {
                    var format = "yyyy-MM-dd HH:mm:ss.ffffff";
                    var date1 = DateTime.ParseExact(obj[0], format, null);
                    var date2 = DateTime.ParseExact(obj[1], format, null);
                    list.Add(Tuple.Create(date1, date2));
                }
                _windows.Add(file.Key.Split('/').Last(), list);
            }
            _instance = this;
        }

        public List<Tuple<DateTime, DateTime>> this[string key] => _windows[key];

        public static AnomalyLabels Instance => _instance ?? (_instance = new AnomalyLabels());
    }
}
