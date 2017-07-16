using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Editors.RangeControl;

namespace BluePrints.Common.ViewModel
{
    public class GanttCanvas : Canvas
    {
        public static readonly DependencyProperty EditRangeProperty =
            DependencyProperty.Register("EditRange", typeof(EditRange),
            typeof(GanttCanvas), new UIPropertyMetadata(null, OnRangeChanged));

        public EditRange EditRange
        {
            get { return (EditRange)GetValue(EditRangeProperty); }
            set { SetValue(EditRangeProperty, value); }
        }

        public GanttCanvas()
        {
            ClipToBounds = true;
            SizeChanged += OnSizeChanged;
        }

        private static void OnRangeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var canvas = (GanttCanvas)dependencyObject;

            var data = (P6_Activity)canvas.DataContext;
            if (data == null) return;

            var width = canvas.ActualWidth;

            var range = (EditRange)eventArgs.NewValue;

            var end = (DateTime)range.End;
            end = end.AddHours(24 - end.Hour);

            var start = (DateTime)range.Start;
            start = start.AddHours(-start.Hour);

            var totHours = (end - start).TotalHours - 24d;
            var scale = width / totHours;

            //data.Left = (data.Start - start).TotalHours * scale;

            //var hours = (data.End - data.Start).TotalHours;
            //data.Width = hours * scale;
            //data.DayWidth = 24 * scale;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var data = (P6_Activity)DataContext;
            if (data == null) return;

            var width = e.NewSize.Width;

            var end = (DateTime)EditRange.End;
            end = end.AddHours(24 - end.Hour);

            var start = (DateTime)EditRange.Start;
            start = start.AddHours(-start.Hour);

            var totHours = (end - start).TotalHours - 24d;
            var scale = width / totHours;

            //var hours = (data.End - data.Start).TotalHours;
            //data.Width = hours * scale;
            //data.DayWidth = 24 * scale;

            //data.Left = (data.Start - start).TotalHours * scale;

            IHaveCanvasWidth canvasWidthViewModel = ((P6_Activity)DataContext).ParentViewModel as IHaveCanvasWidth;
            if (canvasWidthViewModel != null)
                canvasWidthViewModel.CanvasWidth = width;
        }
    }
}
