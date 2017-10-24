using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp4
{
    public static class HelperMethods
    {
        public static Rect GetBounds(this Rectangle rect)
        {
            return new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height);
        }

        public static Rect GetActualBounds(this Rectangle rect)
        {
            Rect bounds = rect.GetBounds();
            Point pivot = bounds.Center();
            Matrix transform = Matrix.Identity;
            transform.RotateAt(rect.GetRotation(), pivot.X, pivot.Y);
            return Rect.Transform(bounds, transform);
        }

        public static void SetBounds(this Rectangle rect, Rect bounds)
        {
            Canvas.SetLeft(rect, bounds.X);
            Canvas.SetTop(rect, bounds.Y);
            rect.Width = bounds.Width;
            rect.Height = bounds.Height;
        }

        public static double GetRotation(this Rectangle rect)
        {
            return (rect.RenderTransform as RotateTransform)?.Angle ?? 0;
        }

        public static void SetRotation(this Rectangle rect, double angle)
        {
            RotateTransform transform = rect.RenderTransform as RotateTransform;
            if (transform != null)
            {
                transform.Angle = angle;
            }
            else
            {
                transform = new RotateTransform(angle);
                rect.RenderTransform = transform;
                rect.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public static Point Center(this Rect rect)
        {
            return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
        }

        public static Point Rotate(this Point point, Point pivot, double angle)
        {
            Matrix transformMatrix = Matrix.Identity;
            transformMatrix.RotateAt(angle, pivot.X, pivot.Y);
            return transformMatrix.Transform(point);
        }
    }
}
