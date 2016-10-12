using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NabViz
{
    class Detection
    {
        private static readonly Detection _instance = new Detection();
        private readonly List<string> _detectors;
        private readonly Dictionary<string, string> _profiles;
        private readonly Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> _results;
        private readonly Dictionary<string, double> _thresholds;

        private Detection()
        {
            _detectors = new List<string>();
            _profiles = new Dictionary<string, string>
            {
                {"STD", "standard"},
                {"LFP", "reward_low_FP_rate"},
                {"LFN", "reward_low_FN_rate"}
            };
            _results = new Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>>();
            _thresholds = new Dictionary<string, double>();

            Initialize();
        }

        private void Initialize()
        {
            var rootDir = Path.Combine("..", "results");
            if (!Directory.Exists(rootDir)) return;
            var detectors = new DirectoryInfo(rootDir).GetDirectories().Select(d => d.ToString()).ToList();
            if (!detectors.Any()) return;

            foreach (var detector in detectors)
            {
                _detectors.Add(detector);
                _results.Add(detector, new Dictionary<string, Dictionary<DateTime, double>>());

                var path = Path.Combine("..", "results", detector, detector + "_standard_scores.csv");
                if (!File.Exists(path)) continue;
                using (var sr = new StreamReader(path))
                {
                    var head = sr.ReadLine().Split(',').ToList();
                    var thresholdColumnIndex = head.IndexOf("Threshold");
                    var threshold = double.Parse(sr.ReadLine().Split(',')[thresholdColumnIndex]);
                    _thresholds.Add(detector, threshold);
                }
            }
        }

        public static List<string> Detectors => _instance._detectors;
        public static Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> Results => _instance._results;
        public static Dictionary<string, double> Thresholds => _instance._thresholds;

        public static void LoadResults(string dataPath)
        {
            if (!_instance._results.Keys.Any()) throw new Exception("detector not found");
            // return if results had been already loaded
            if (_instance._results.First().Value.ContainsKey(dataPath)) return;

            foreach (var detectorName in _instance._results.Keys)
            {
                _instance._results[detectorName].Add(dataPath, new Dictionary<DateTime, double>());
                var path = Path.Combine("..", "results", detectorName, dataPath.Insert(dataPath.LastIndexOf(Path.DirectorySeparatorChar) + 1, detectorName + "_"));
                if (!File.Exists(path)) throw new FileNotFoundException(path + " does not exist.");
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
    }
}
