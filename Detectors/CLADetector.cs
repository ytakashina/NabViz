using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz.Detectors
{
    class ClaDetector : Detector
    {
        private int _columns;
        private readonly int _desiredLocalActivity;
        private readonly double _inhibitionRadius;
        private readonly int _minOverlap;
        private readonly double _permanenceInc;
        private readonly double _permanenceDec;

        public override void Initialize()
        {
            
        }

        public override double AnomalyScore(DataPoint dataPoint)
        {
            return 0;
        }

        public override void Record(DataPoint dataPoint)
        {
            var date = DateTime.FromOADate(dataPoint.XValue);
            var value = dataPoint.YValues[0];


        }
    }
}
