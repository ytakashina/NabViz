using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz.Detectors
{
    abstract class Detector
    {
        public abstract void Initialize();
        public abstract double AnomalyScore(DataPoint dataPoint);
        public abstract void Record(DataPoint dataPoint);
        public string Name => ToString().Split('+').Last();
    }
}
