using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using static ZetaOne.MathExtensions;

namespace ZetaOne
{
    class Detectors
    {
        public abstract class Detector
        {
            public abstract double AnomalyScore(DataPoint dataPoint);
            public abstract void Record(DataPoint dataPoint);
            public string Name => ToString().Split('+').Last();
        }

        // SlidingThrethold (WindowedGaussian)
        // 閾値法
        public class WindowedGaussian : Detector
        {
            private readonly int _windowSize;
            private readonly int _stepSize;
            private readonly List<double> _windowData;
            private readonly List<double> _stepBuffer;
            private double _mean;
            private double _standardDeviation;

            private static WindowedGaussian _instance;
            public static WindowedGaussian Instance => _instance ?? (_instance = new WindowedGaussian());

            private WindowedGaussian()
            {
                _windowSize = 6400;
                _windowData = new List<double>();
                _stepBuffer = new List<double>();
                _stepSize = 100;
                _mean = 0;
                _standardDeviation = 1;
                _instance = this;
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
                    //_stepBuffer.Add(input);
                    //if (_stepBuffer.Count == _stepSize)
                    //{
                    //    _windowData.RemoveRange(0, _stepSize);
                    //    _windowData.AddRange(_stepBuffer);
                    //    _stepBuffer.Clear();
                    //    UpdateWindow();
                    //}
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
}
