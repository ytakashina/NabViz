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
            var enumerable = values as double[] ?? values.ToArray();
            if (!enumerable.Any()) return 0;
            var avg = enumerable.Average();
            var sum = enumerable.Sum(x => Math.Pow(x - avg, 2));
            return Math.Sqrt(sum / (enumerable.Length - 1));
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
        // SlidingThrethold (WindowedGaussian)
        // 閾値法
        class WindowedGaussianDetector
        {
            private readonly int _windowSize;
            private readonly int _stepSize;
            private readonly List<double> _windowData;
            private readonly List<double> _stepBuffer;
            private double _mean;
            private double _standardDeviation;

            public WindowedGaussianDetector()
            {
                _windowSize = 6400;
                _windowData = new List<double>();
                _stepBuffer = new List<double>();
                _stepSize = 100;
                _mean = 0;
                _standardDeviation = 1;
            }

            public double HandleRecord(DataPoint dataPoint)
            {
                var anomaryScore = 0.0;
                var inputValue = dataPoint.YValues[0];
                if (_windowData.Count > 0)
                {
                    anomaryScore = 1 - NormalProbability(inputValue, _mean, _standardDeviation);
                }

                if (_windowData.Count < _windowSize)
                {
                    _windowData.Add(inputValue);
                    UpdateWindow();
                }
                else
                {
                    _stepBuffer.Add(inputValue);
                    if (_stepBuffer.Count == _stepSize)
                    {
                        _windowData.RemoveRange(0, _stepSize);
                        _windowData.AddRange(_stepBuffer);
                        _stepBuffer.Clear();
                        UpdateWindow();
                    }
                }
                return anomaryScore;
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
