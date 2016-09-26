using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using static ZetaOne.MathExtensions;

namespace ZetaOne.Detectors
{
    // SlidingThrethold (WindowedGaussian)
    // 閾値法
    class WindowedGaussian : Detector
    {
        // List じゃなくて配列と begin/end を使って最適化してもよいかもしれないけど
        // 別にそこまでやる必要あるか微妙。
        private readonly List<double> _windowData;
        private readonly int _windowSize;
        private double _mean;
        private double _standardDeviation;

        public WindowedGaussian()
        {
            _windowSize = 6400;
            _windowData = new List<double>();
            _mean = 0;
            _standardDeviation = 1;
        }

        public override void Initialize()
        {
            _windowData.Clear();
            _mean = 0;
            _standardDeviation = 1;
        }

        public override double AnomalyScore(DataPoint dataPoint)
        {
            var input = dataPoint.YValues[0];
            if (_windowData.Count == 0) return 0;
            return 1 - QFunction(input, _mean, _standardDeviation);
        }

        public override void Record(DataPoint dataPoint)
        {
            var input = dataPoint.YValues[0];
            if (_windowData.Count < _windowSize)
            {
                _windowData.Add(input);
                UpdateWindow();
            }
            else
            {
                _windowData.RemoveAt(0);
                _windowData.Add(input);
                UpdateWindow();
            }
        }

        private void UpdateWindow()
        {
            _mean = _windowData.Average();
            _standardDeviation = _windowData.StandardDeviation();
            if (Math.Abs(_standardDeviation) < 0.000001)
            {
                _standardDeviation = 0.000001;
            }
        }
    }
}