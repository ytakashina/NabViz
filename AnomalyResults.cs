using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NabViz
{
    class AnomalyResults
    {
        private static AnomalyResults _instance = new AnomalyResults();
        private readonly Dictionary<string, Dictionary<string, List<Tuple<DateTime, double>>>> _results;

        private AnomalyResults()
        {
            _results = new Dictionary<string, Dictionary<string, List<Tuple<DateTime, double>>>>();
            var rootDir = new DirectoryInfo(Path.Combine("..", "results"));
            foreach (var detectorDir in rootDir.GetDirectories())
            {
                _results.Add(detectorDir.ToString(), new Dictionary<string, List<Tuple<DateTime, double>>>());
                foreach (var dataDir in detectorDir.GetDirectories())
                {
                    var files = dataDir.GetFiles("*.csv").Select(f => f.ToString());
                    foreach (var file in files)
                    {
                        _results[detectorDir.ToString()].Add(file, new List<Tuple<DateTime, double>>());
                        using (var sr = new StreamReader(Path.Combine("..", "results", detectorDir.ToString(), dataDir.ToString(), file)))
                        {
                            // timestamp           ,value ,anomaly_score,label ,S(t)_reward_low_FP_rate ,S(t)_reward_low_FN_rate ,S(t)_standard
                            // 2015-09-11 15:34:00 ,64.0  ,0.5          ,1     ,-1.0                    ,-2.0                    ,-1.0
                            var head = sr.ReadLine().Split(',').ToList();
                            var dateColumnIndex = head.IndexOf("timestamp");
                            var targetColumnIndex = head.IndexOf("anomaly_score");
                            while (!sr.EndOfStream)
                            {
                                var line = sr.ReadLine().Split(',');
                                var date = DateTime.ParseExact(line[dateColumnIndex], "yyyy-MM-dd HH:mm:ss", null);
                                var value = double.Parse(line[targetColumnIndex]);
                                _results[detectorDir.ToString()][file].Add(Tuple.Create(date, value));
                            }
                        }
                    }
                }
            }
            _instance = this;
        }

        public static Dictionary<string, Dictionary<string, List<Tuple<DateTime, double>>>> Dictionary => _instance._results;

    }
}
