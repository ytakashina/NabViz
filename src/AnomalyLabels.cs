﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace NabViz
{
    class AnomalyLabels
    {
        private static AnomalyLabels _instance = new AnomalyLabels();
        private readonly Dictionary<string, List<Tuple<DateTime, DateTime>>> _labels;

        private AnomalyLabels()
        {
            _labels = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();
            var path = Path.Combine("..", "labels", "combined_windows.json");
            var sr = new StreamReader(path, Encoding.GetEncoding("utf-8"));
            string json = sr.ReadToEnd();
            sr.Close();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<string>>>>(json);
            foreach (var file in dict)
            {
                var list = new List<Tuple<DateTime, DateTime>>();
                foreach (var window in file.Value)
                {
                    var format = "yyyy-MM-dd HH:mm:ss.ffffff";
                    var date1 = DateTime.ParseExact(window[0], format, null);
                    var date2 = DateTime.ParseExact(window[1], format, null);
                    list.Add(Tuple.Create(date1, date2));
                }
                _labels.Add(Path.Combine(file.Key.Split('/')), list);
            }
            _instance = this;
        }

        public List<Tuple<DateTime, DateTime>> this[string key] => _labels[key];

        public static AnomalyLabels Instance => _instance;
    }
}
