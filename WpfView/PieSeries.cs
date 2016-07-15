﻿//The MIT License(MIT)

//Copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts.Definitions.Points;
using LiveCharts.Definitions.Series;
using LiveCharts.Helpers;
using LiveCharts.SeriesAlgorithms;
using LiveCharts.Wpf.Charts.Base;
using LiveCharts.Wpf.Points;

namespace LiveCharts.Wpf
{
    /// <summary>
    /// The pie series should be added only in a pie chart.
    /// </summary>
    public class PieSeries : Series, IPieSeriesView
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of PieSeries class
        /// </summary>
        public PieSeries()
        {
            Model = new PieAlgorithm(this);
            InitializeDefuaults();
        }

        /// <summary>
        /// Initializes a new instance of PieSeries class with a given mapper.
        /// </summary>
        /// <param name="configuration"></param>
        public PieSeries(object configuration)
        {
            Model = new PieAlgorithm(this);
            Configuration = configuration;
            InitializeDefuaults();
        }

        #endregion

        #region Private Properties

        #endregion

        #region Properties

        public static readonly DependencyProperty PushOutProperty = DependencyProperty.Register(
            "PushOut", typeof (double), typeof (PieSeries), new PropertyMetadata(default(double)));
        /// <summary>
        /// Gets or sets the slice push out, this property highlights the slice
        /// </summary>
        public double PushOut
        {
            get { return (double) GetValue(PushOutProperty); }
            set { SetValue(PushOutProperty, value); }
        }
        #endregion

        #region Overridden Methods

        public override IChartPointView GetPointView(IChartPointView view, ChartPoint point, string label)
        {
            var pbv = (view as PiePointView);

            if (pbv == null)
            {
                pbv = new PiePointView
                {
                    IsNew = true,
                    Slice = new PieSlice()
                };

                BindingOperations.SetBinding(pbv.Slice, Shape.FillProperty,
                    new Binding { Path = new PropertyPath(FillProperty), Source = this });
                BindingOperations.SetBinding(pbv.Slice, Shape.StrokeProperty,
                    new Binding { Path = new PropertyPath(StrokeProperty), Source = this });
                BindingOperations.SetBinding(pbv.Slice, Shape.StrokeThicknessProperty,
                    new Binding {Path = new PropertyPath(StrokeThicknessProperty), Source = this});
                BindingOperations.SetBinding(pbv.Slice, Shape.StrokeDashArrayProperty,
                    new Binding {Path = new PropertyPath(StrokeDashArrayProperty), Source = this});
                BindingOperations.SetBinding(pbv.Slice, PieSlice.PushOutProperty,
                    new Binding {Path = new PropertyPath(PushOutProperty), Source = this});
                BindingOperations.SetBinding(pbv.Slice, Panel.ZIndexProperty,
                    new Binding {Path = new PropertyPath(Panel.ZIndexProperty), Source = this});
                BindingOperations.SetBinding(pbv.Slice, VisibilityProperty,
                    new Binding {Path = new PropertyPath(VisibilityProperty), Source = this});

                Model.Chart.View.AddToDrawMargin(pbv.Slice);
            }
            else
            {
                pbv.IsNew = false;
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.Slice);
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.HoverShape);
                point.SeriesView.Model.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.DataLabel);
            }

            if (Model.Chart.RequiresHoverShape && pbv.HoverShape == null)
            {
                pbv.HoverShape = new PieSlice
                {
                    Fill = Brushes.Transparent,
                    StrokeThickness = 0
                };

                Panel.SetZIndex(pbv.HoverShape, int.MaxValue);
                BindingOperations.SetBinding(pbv.HoverShape, VisibilityProperty,
                    new Binding {Path = new PropertyPath(VisibilityProperty), Source = this});

                var wpfChart = (Chart)Model.Chart.View;
                wpfChart.AttachHoverableEventTo(pbv.HoverShape);

                Model.Chart.View.AddToDrawMargin(pbv.HoverShape);
            }

            if (DataLabels && pbv.DataLabel == null)
            {
                pbv.DataLabel = BindATextBlock(0);
                Panel.SetZIndex(pbv.DataLabel, int.MaxValue - 1);

                Model.Chart.View.AddToDrawMargin(pbv.DataLabel);
            }

            if (pbv.DataLabel != null) pbv.DataLabel.Text = label;

            return pbv;
        }

        public override void Erase()
        {
            Values.Points.ForEach(p =>
            {
                if (p.View != null)
                    p.View.RemoveFromView(Model.Chart);
            });
            Model.Chart.View.RemoveFromView(this);
        }

        #endregion

        #region Private Methods

        private void InitializeDefuaults()
        {
            SetCurrentValue(StrokeThicknessProperty, 2d);
            SetCurrentValue(StrokeProperty, Brushes.White);
            SetCurrentValue(ForegroundProperty, Brushes.White);

            Func<ChartPoint, string> defaultLabel = x => Model.CurrentYAxis.GetFormatter()(x.Y);

            SetCurrentValue(LabelPointProperty, defaultLabel);

            DefaultFillOpacity = 1;
        }

        #endregion
    }
}
