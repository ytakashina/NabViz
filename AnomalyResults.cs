using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NabViz
{
    class AnomalyResults
    {
        private static AnomalyResults _instance;
        private readonly Dictionary<string, List<double>> _results;

        private AnomalyResults()
        {
            _results = new Dictionary<string, List<double>>();
            var rootDir = new DirectoryInfo(Path.Combine("..", "results"));
            foreach (var detectorDir in rootDir.GetDirectories())
            {
                foreach (var dir in detectorDir.GetDirectories())
                {
                    var files = dir.GetFiles("*.csv").Select(file => Path.Combine("..", "results", detectorDir.ToString(), dir.ToString(), file.ToString()));
                    foreach (var file in files)
                    {
                        _results.Add(file, new List<double>());
                        using (var sr = new StreamReader(file))
                        {
                            // timestamp           ,value ,anomaly_score,label ,S(t)_reward_low_FP_rate ,S(t)_reward_low_FN_rate ,S(t)_standard
                            // 2015-09-11 15:34:00 ,64.0  ,0.5          ,1     ,-1.0                    ,-2.0                    ,-1.0
                            var head = sr.ReadLine().Split(',').ToList();
                            var targetColumnIndex = head.IndexOf("anomaly_score");
                            while (!sr.EndOfStream)
                            {
                                var line = sr.ReadLine().Split(',');
                                _results[file].Add(double.Parse(line[targetColumnIndex]));
                            }
                        }
                    }
                }
            }
            _instance = this;
        }

        public List<double> this[string key] => _results[key];

        public static AnomalyResults Instance => _instance ?? (_instance = new AnomalyResults());

    }
}
