using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NabViz
{
    class DetectionResults
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> _results;

        private DetectionResults()
        {
            _results = Detectors.List.ToDictionary(s => s, s => new Dictionary<string, Dictionary<DateTime, double>>());
            _instance = this;
        }

        public static void Load(string dataPath)
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

        private static DetectionResults _instance = new DetectionResults();
        public static Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> Dictionary => _instance._results;
    }
}
