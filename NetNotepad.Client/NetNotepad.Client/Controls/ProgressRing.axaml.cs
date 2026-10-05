using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NetNotepad.Client.Controls
{
    public partial class ProgressRing : UserControl
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<ProgressRing, double>(nameof(Value));

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<ProgressRing, double>(nameof(Minimum), 0);

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<ProgressRing, double>(nameof(Maximum), 100);

        public static readonly StyledProperty<bool> IsIndeterminateProperty =
            AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate));

        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<ProgressRing, double>(nameof(Thickness), 4);

        public static readonly StyledProperty<IBrush?> TrackBrushProperty =
            AvaloniaProperty.Register<ProgressRing, IBrush?>(nameof(TrackBrush), new SolidColorBrush(Color.Parse("#20808080")));

        public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public bool IsIndeterminate { get => GetValue(IsIndeterminateProperty); set => SetValue(IsIndeterminateProperty, value); }
        public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
        public IBrush? TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }

        public ProgressRing()
        {
            InitializeComponent();
            Update();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty ||
                change.Property == MinimumProperty ||
                change.Property == MaximumProperty ||
                change.Property == IsIndeterminateProperty ||
                change.Property == ThicknessProperty ||
                change.Property == TrackBrushProperty ||
                change.Property == ForegroundProperty)
            {
                Update();
            }
        }

        private void Update()
        {
            if (ValueArc is null || Track is null) { return; }

            Track.Stroke = TrackBrush;
            Track.StrokeThickness = Thickness;

            ValueArc.Stroke = Foreground;
            ValueArc.StrokeThickness = Thickness;
            ValueArc.Classes.Set("spin", IsIndeterminate);

            if (IsIndeterminate)
            {
                ValueArc.SweepAngle = 100;
                return;
            }

            var range = Maximum - Minimum;
            var fraction = range <= 0 ? 0 : (Value - Minimum) / range;
            ValueArc.SweepAngle = Math.Clamp(fraction, 0, 1) * 359.99;
        }
    }
}