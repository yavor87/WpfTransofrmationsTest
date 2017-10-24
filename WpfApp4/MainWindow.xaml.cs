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

        private int _manipulationCount;

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
                System.Diagnostics.Debug.WriteLine("#{0}: Scaling selection X:{1:N2} Y:{2:N2}", _manipulationCount, scaleX, scaleY);
                transformMatrix.ScaleAtPrepend(scaleX, scaleY, newBounds.X, newBounds.Y);
            }
            if (!AreClose(newBounds.X, currentBounds.X) || !AreClose(newBounds.Y, currentBounds.Y))
            {
                Vector offset = newBounds.Location - currentBounds.Location;
                System.Diagnostics.Debug.WriteLine("#{0}: Moving selection with vector {1}", _manipulationCount, offset);
                transformMatrix.TranslatePrepend(offset.X, offset.Y);
            }

            foreach (var child in GetChildren())
            {
                double childAngle = child.GetRotation();
                Rect finalBounds;
                if (childAngle != 0)
                {
                    // Transform child bounds to be in 0 rotation
                    Rect childBounds = RotateBounds(child.GetBounds(), -childAngle, currentBounds.Center());
                    // Apply translation
                    Rect updatedBounds = Rect.Transform(childBounds, transformMatrix);
                    // Rotate child to the desired angle again
                    finalBounds = RotateBounds(updatedBounds, childAngle, newBounds.Center());
                }
                else
                {
                    finalBounds = Rect.Transform(child.GetBounds(), transformMatrix);
                }
                child.SetBounds(finalBounds);
            }

            Rect newSelectionBounds = ComputeSelectionBounds();
            this.selection.SetBounds(newSelectionBounds);
            _manipulationCount++;
        }

        /// <summary>
        /// Rotates the selection and its contents to the <see cref="newAngle"/>.
        /// </summary>
        /// <param name="newAngle"></param>
        private void SetSelectionAngle(double newAngle)
        {
            Rect currentBounds = this.selection.GetBounds();
            Point pivotPoint = currentBounds.Center();

            System.Diagnostics.Debug.WriteLine("#{0}: Rotating at angle {1:N2}", _manipulationCount, newAngle);

            foreach (var child in GetChildren())
            {
                double delta = newAngle - child.GetRotation();
                if (delta == 0)
                    continue;

                Rect childBounds = child.GetBounds();
                Rect updatedBounds = RotateBounds(childBounds, delta, pivotPoint);
                child.SetBounds(updatedBounds);
                child.SetRotation(newAngle);
            }

            Rect newSelectionBounds = ComputeSelectionBounds();
            this.selection.SetBounds(newSelectionBounds);
            _manipulationCount++;
        }

        private Rect RotateBounds(Rect bounds, double delta, Point pivotPoint)
        {
            var childCenter = bounds.Center();
            var rotatedChildPivot = childCenter.Rotate(pivotPoint, delta);
            if (rotatedChildPivot != childCenter)
            {
                double newX = Math.Round(rotatedChildPivot.X - (bounds.Width / 2), 4);
                double newY = Math.Round(rotatedChildPivot.Y - (bounds.Height / 2), 4);

                Rect updatedBounds = new Rect(new Point(newX, newY), bounds.Size);
                return updatedBounds;
            }
            return bounds;
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
