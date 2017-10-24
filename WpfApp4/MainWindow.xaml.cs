using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double newSX = Double.Parse(this.tbSX.Text, System.Globalization.CultureInfo.InvariantCulture);
            double newSY = Double.Parse(this.tbSY.Text, System.Globalization.CultureInfo.InvariantCulture);
            double newEX = Double.Parse(this.tbEX.Text, System.Globalization.CultureInfo.InvariantCulture);
            double newEY = Double.Parse(this.tbEY.Text, System.Globalization.CultureInfo.InvariantCulture);

            Rect bounds = new Rect(new Point(newSX, newSY), new Point(newEX, newEY));
            SetSelectionBounds(bounds);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            double newAngle = Double.Parse(this.tbA.Text);
            SetSelectionAngle(newAngle);
        }

        /// <summary>
        /// Scales and/or translates the selection bounds and its contents to the provided <see cref="newBounds"/>.
        /// </summary>
        /// <param name="newBounds"></param>
        private void SetSelectionBounds(Rect newBounds)
        {
            Rect currentBounds = this.selection.GetBounds();
            Matrix transformMatrix = Matrix.Identity;
            if (!AreClose(newBounds.Width, currentBounds.Width) || !AreClose(newBounds.Height, currentBounds.Height))
            {
                double scaleX = newBounds.Width / currentBounds.Width;
                double scaleY = newBounds.Height / currentBounds.Height;
                System.Diagnostics.Debug.WriteLine("Scaling selection X:{0:N2} Y:{1:N2}", scaleX, scaleY);
                transformMatrix.ScaleAtPrepend(scaleX, scaleY, newBounds.X, newBounds.Y);
            }
            if (!AreClose(newBounds.X, currentBounds.X) || !AreClose(newBounds.Y, currentBounds.Y))
            {
                Vector offset = newBounds.Location - currentBounds.Location;
                System.Diagnostics.Debug.WriteLine("Moving selection with vector {0}", offset);
                transformMatrix.TranslatePrepend(offset.X, offset.Y);
            }

            foreach (var child in GetChildren())
            {
                Rect childBounds = child.GetBounds();
                Rect updatedBounds = Rect.Transform(childBounds, transformMatrix);
                child.SetBounds(updatedBounds);
            }

            this.selection.SetBounds(newBounds);
        }

        /// <summary>
        /// Rotates the selection and its contents to the <see cref="newAngle"/>.
        /// </summary>
        /// <param name="newAngle"></param>
        private void SetSelectionAngle(double newAngle)
        {
            Rect currentBounds = this.selection.GetBounds();
            Point pivotPoint = currentBounds.Center();

            System.Diagnostics.Debug.WriteLine("Rotating at angle {0:N2}", newAngle);

            foreach (var child in GetChildren())
            {
                double delta = newAngle - child.GetRotation();
                if (delta == 0)
                    continue;

                Rect childBounds = child.GetBounds();
                var childCenter = childBounds.Center();
                var rotatedChildPivot = childCenter.Rotate(pivotPoint, delta);
                if (rotatedChildPivot != childCenter)
                {
                    double newX = Math.Round(rotatedChildPivot.X - (childBounds.Width / 2), 4);
                    double newY = Math.Round(rotatedChildPivot.Y - (childBounds.Height / 2), 4);

                    Rect updatedBounds = new Rect(new Point(newX, newY), childBounds.Size);
                    child.SetBounds(updatedBounds);
                }
                child.SetRotation(newAngle);
            }

            Rect newSelectionBounds = ComputeSelectionBounds();
            this.selection.SetBounds(newSelectionBounds);
        }

        private static bool AreClose(double op1, double op2, double delta = 0.0000000000001)
        {
            return Math.Abs(op2 - op1) < delta;
        }

        private Rect ComputeSelectionBounds()
        {
            var childrenBounds = GetChildren().Select(c => c.GetActualBounds());
            Rect bounds = childrenBounds.First();
            foreach (var item in childrenBounds.Skip(1))
            {
                bounds.Union(item);
            }
            return bounds;
        }

        private IEnumerable<Rectangle> GetChildren()
        {
            foreach (Rectangle item in this.canvas.Children.OfType<Rectangle>())
            {
                if (item.Tag?.Equals("child") == true)
                    yield return item;
            }
        }
    }
}
