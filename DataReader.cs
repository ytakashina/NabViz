using System.Windows.Forms.DataVisualization.Charting;

namespace ZetaOne
{
    class DataReader
    {
        private readonly Series _series;
        private int _current;

        public DataReader(Series series)
        {
            _series = series;
        }

        public DataPoint Current => _series.Points[_current];
        public DataPoint Next => _series.Points[++_current];
        public DataPoint Prev => _series.Points[--_current];
        public DataPoint First => _series.Points[0];
        public DataPoint Last => _series.Points[_series.Points.Count - 1];
        public bool StartOfStream => _current == 0;
        public bool EndOfStream => _current == _series.Points.Count - 1;
    }
}
