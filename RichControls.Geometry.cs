using System.Drawing;

namespace RichControls
{
    struct RectangleD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Height;
        public double W => Width;
        public double H => Height;
        public PointF Location => new PointF((float)X, (float)Y);
        public SizeF Size => new SizeF((float)Width, (float)Height);

        public static explicit operator Rectangle(RectangleD r)
        {
            return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

        }

        public static explicit operator RectangleF(RectangleD r)
        {
            return new RectangleF((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        }

    }



}
