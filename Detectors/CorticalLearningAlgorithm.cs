using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ZetaOne.Detectors
{
    class CorticalLearningAlgorithm : Detector
    {
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
