using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NabViz
{
    class DetectionThresholds
    {
        private readonly Dictionary<string, string> _profiles;
        private readonly Dictionary<string, double> _thresholds;

        private DetectionThresholds()
        {
            _profiles = new Dictionary<string, string>
            {
                {"STD", "standard"},
                {"LFP", "reward_low_FP_rate"},
                {"LFN", "reward_low_FN_rate"}
            };

            _thresholds = new Dictionary<string, double>();
            foreach (var detector in Detectors.List)
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
            _instance = this;
        }

        private static DetectionThresholds _instance;
        public static Dictionary<string, double> Dictionary => (_instance ?? (_instance = new DetectionThresholds()))._thresholds;
    }
}
