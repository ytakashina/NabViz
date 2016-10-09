using System.Collections.Generic;
using System.IO;

namespace NabViz
{
    class Detectors
    {
        private readonly List<string> _detectors;

        private Detectors()
        {
            _detectors = new List<string>();
            var dir = new DirectoryInfo(Path.Combine("..", "results"));
            foreach (var detectorDir in dir.GetDirectories())
            {
                _detectors.Add(detectorDir.ToString());
            }
        }

        private static readonly Detectors _instance = new Detectors();
        public static List<string> List => _instance._detectors;
    }
}
