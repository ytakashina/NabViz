using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NabViz
{
    class Detection
    {
        private readonly List<string> _detectors;
        private readonly Dictionary<string, string> _profiles;
        private readonly Dictionary<string, double> _thresholds;
        private readonly Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> _results;

        private Detection()
        {
            var dir = new DirectoryInfo(Path.Combine("..", "results"));
            _detectors = dir.GetDirectories().Select(d => d.ToString()).ToList();

            _profiles = new Dictionary<string, string>
            {
                {"STD", "standard"},
                {"LFP", "reward_low_FP_rate"},
                {"LFN", "reward_low_FN_rate"}
            };

            _thresholds = new Dictionary<string, double>();
            foreach (var detector in _detectors)
            {
                var path = Path.Combine("..", "results", detector, detector + "_standard_scores.csv");
                using (var sr = new StreamReader(path))
                {
                    var head = sr.ReadLine().Split(',').ToList();
                    var thresholdColumnIndex = head.IndexOf("Threshold");
                    var threshold = double.Parse(sr.ReadLine().Split(',')[thresholdColumnIndex]);
                    _thresholds.Add(detector, threshold);
                }
            }

            _results = _detectors.ToDictionary(s => s, s => new Dictionary<string, Dictionary<DateTime, double>>());

            _instance = this;
        }

        public static void LoadResults(string dataPath)
        {
            if (_instance._results.First().Value.ContainsKey(dataPath)) return;
            foreach (var detectorName in _instance._results.Keys)
            {
                _instance._results[detectorName].Add(dataPath, new Dictionary<DateTime, double>());
                var path = Path.Combine("..", "results", detectorName, dataPath.Insert(dataPath.LastIndexOf(Path.DirectorySeparatorChar) + 1, detectorName + "_"));
                using (var sr = new StreamReader(path))
                {
                    var head = sr.ReadLine().Split(',').ToList();
                    var dateColumnIndex = head.IndexOf("timestamp");
                    var scoreColumnIndex = head.IndexOf("anomaly_score");
                    if (dateColumnIndex == -1) throw new FormatException("\"timestamp\" column does not exist.");
                    if (scoreColumnIndex == -1) throw new FormatException("\"anomaly_score\" column does not exist.");
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine().Split(',');
                        var date = DateTime.ParseExact(line[dateColumnIndex], "yyyy-MM-dd HH:mm:ss", null);
                        var value = double.Parse(line[scoreColumnIndex]);
                        _instance._results[detectorName][dataPath][date] = value;
                        //_instance._results[detectorName][dataPath].Add(date, value);
                    }
                }
            }
        }

        private static Detection _instance = new Detection();
        public static List<string> Detectors => _instance._detectors;
        public static Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> Results => _instance._results;
        public static Dictionary<string, double> Thresholds => _instance._thresholds;
    }
}
