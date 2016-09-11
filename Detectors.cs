using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using static ZetaOne.MathExtensions;

namespace ZetaOne
{
    public static class MathExtensions
    {
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            var array = values as double[] ?? values.ToArray();
            if (!array.Any()) return 0;
            var avg = array.Average();
            var sum = array.Sum(x => Math.Pow(x - avg, 2));
            return Math.Sqrt(sum / array.Length);
        }

        public static double Erf(double x)
        {
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            // Save the sign of x
            var sign = Math.Sign(x);
            x = Math.Abs(x);

            // A&S formula 7.1.26
            var t = 1.0 / (1.0 + p * x);
            var y = 1.0 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        public static double Erfc(double x)
        {
            return 1 - Erf(x);
        }

        public static double NormalProbability(double x, double mean, double standardDeviation)
        {
            if (x < mean)
            {
                var xp = 2 * mean - x;
                return 1.0 - NormalProbability(xp, mean, standardDeviation);
            }
            var z = (x - mean) / standardDeviation;
            return 0.5 * Erfc(z / Math.Sqrt(2));
        }
    }

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
                return 1 - NormalProbability(input, _mean, _standardDeviation);
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
                    _stepBuffer.Add(input);
                    if (_stepBuffer.Count == _stepSize)
                    {
                        _windowData.RemoveRange(0, _stepSize);
                        _windowData.AddRange(_stepBuffer);
                        _stepBuffer.Clear();
                        UpdateWindow();
                    }
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
