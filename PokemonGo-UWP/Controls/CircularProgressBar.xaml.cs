using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PokemonGo_UWP.Controls
{
    public sealed partial class CircularProgressBar
    {
        #region ChangedEventHandlers

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            CircularProgressBar circularProgressBar = sender as CircularProgressBar;
            if (circularProgressBar != null) circularProgressBar.Angle = (circularProgressBar.Percentage * 360) / 100;
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            CircularProgressBar circularProgress = sender as CircularProgressBar;
            if (circularProgress != null)
            {
                if (circularProgress.Value != 0)
                {
                    circularProgress.Percentage = ((double)circularProgress.Value / circularProgress.Maximum) * 100;
                    return;
                }

                circularProgress.Percentage = 0;

            }
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            CircularProgressBar circularProgressBar = sender as CircularProgressBar;
            circularProgressBar?.RenderArc();
        }

        #endregion

        #region Properties

        private static readonly DependencyProperty PercentageProperty = DependencyProperty.Register("Percentage", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(65d, OnPercentageChanged));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(5));

        public static readonly DependencyProperty SegmentColorProperty = DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.LightBlue)));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(25, OnPropertyChanged));

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(120d, OnPropertyChanged));

        public static readonly DependencyProperty ValuePropery = DependencyProperty.Register("Value", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(100, OnValueChanged));

        #endregion

        #region Values

        public int Radius
        {
            get { return (int)GetValue(RadiusProperty) / 2; }
            set { SetValue(RadiusProperty, (value) / 2); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public Brush SegmentColor
        {
            get { return (Brush)GetValue(SegmentColorProperty); }
            set { SetValue(SegmentColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        private double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public int Value
        {
            get { return Convert.ToInt32(GetValue(ValuePropery)); }
            set
            {
                SetValue(ValuePropery, value);
            }
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set
            {
                SetValue(MaximumProperty, value);
            }
        }
        #endregion

        public CircularProgressBar()
        {
            InitializeComponent();

            Angle = (Percentage * 360) / 100;
            RenderArc();
        }

        public void RenderArc()
        {
            Point startPoint = new Point(Radius, 0);
            Point endPoint = ComputeCartesianCoordinate(Angle, Radius);
            endPoint.X += Radius;
            endPoint.Y += Radius;

            PathRoot.Width = Radius * 2 + PathRoot.StrokeThickness;
            PathRoot.Height = Radius * 2 + PathRoot.StrokeThickness;
            PathRoot.RenderTransform = new CompositeTransform
            {
                TranslateX = PathRoot.StrokeThickness / 2,
                TranslateY = PathRoot.StrokeThickness / 2
            };

            bool largeArc = Angle > 180.0;

            Size outerArcSize = new Size(Radius, Radius);

            PathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            ArcSegment.Point = endPoint;
            ArcSegment.Size = outerArcSize;
            ArcSegment.IsLargeArc = largeArc;
        }
        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}
