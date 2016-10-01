using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabViz.Encoders
{
    class ScalarEncoder : Encoder
    {
        private int _w;
        private double _minInput;
        private double _maxInput;
        private double _range;
        private double _rangeInternal;
        private bool _periodic;
        private bool _clipInput;
        private int _n;
        private double _radius;
        private double _resolution;

        public ScalarEncoder(int w, double minInput, double maxInput, int n=0,
                             bool periodic = false, bool clipInput=false)
        {
            _w = w;
            _minInput = minInput;
            _maxInput = maxInput;
            _rangeInternal = maxInput - minInput;
            _periodic = periodic;
            _clipInput = clipInput;
            _n = n;

            if (_periodic)
            {
                _resolution = _rangeInternal/(_n - _w);
                _range = _rangeInternal;
            }
            else
            {
                _resolution = _rangeInternal / _n;
                _range = _rangeInternal + _resolution;
            }
            _radius = _w * _resolution;

        }

        protected override bool[] Encode(object input)
        {
            return new[] { true };
        }
    }
}
