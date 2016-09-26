using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ZetaOne.Detectors
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
            
        }
    }
}
