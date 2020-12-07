using Base;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using SciChartWrapper.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace SciChartWrapper.Models
{
    public class SciChartColumnChartModel : BasePropertyChanged
    {
        #region ViewModel

        private SciChartColumnChartViewModel _sciChartColumnChartViewModelObj;

        #endregion

        #region Variables

        private Dictionary<double, double> _data;
        private IXyDataSeries<double, double> _series;
        private List<double> _xList;
        private List<double> _yList;
        private IRange _xAxis;
        private IRange _yAxis;
        private ObservableCollection<IRenderableSeriesViewModel> _renderableSeries;

        #endregion Variables

        #region Properties

        public SciChartColumnChartViewModel SciChartColumnChartViewModelObj
        {
            get { return _sciChartColumnChartViewModelObj; }
            set { _sciChartColumnChartViewModelObj = value; }
        }

        public Dictionary<double, double> Data
        {
            get { return _data; }
            set
            {
                if (value != null)
                {
                    if (_data != null)
                    {
                        _data.Clear();
                    }

                    _data = value;

                    if (RenderableSeries != null)
                    {
                        Application.Current.Dispatcher.InvokeAsync( new Action(() =>
                        {
                            if (SciChartColumnChartViewModelObj != null)
                            {
                                XAxis.SetMinMax(
                                    _data.Keys.Min(),
                                    _data.Keys.Max());
                                YAxis.SetMinMax(
                                    _data.Values.Min(),
                                    _data.Values.Max());
                                SeriesUpdate();
                            }
                        }), DispatcherPriority.ApplicationIdle );
                    }
                }
            }
        }

        public System.Collections.Generic.List<double> XList
        {
            get { return _xList; }
            set { _xList = value; }
        }

        public System.Collections.Generic.List<double> YList
        {
            get { return _yList; }
            set { _yList = value; }
        }

        public IRange XAxis
        {
            get { return _xAxis; }
            set
            {
                if (_xAxis != value)
                {
                    _xAxis = value;
                    OnPropertyChanged("XAxis");
                }
            }
        }

        public IRange YAxis
        {
            get { return _yAxis; }
            set
            {
                if (_yAxis != value)
                {
                    _yAxis = value;
                    OnPropertyChanged("YAxis");
                }
            }
        }

        public IXyDataSeries<double, double> Series
        {
            get { return _series; }
            set { _series = value; OnPropertyChanged("Series"); }
        }

        public ObservableCollection<IRenderableSeriesViewModel> RenderableSeries
        {
            get { return _renderableSeries; }
            set
            {
                _renderableSeries = value;
                OnPropertyChanged("RenderableSeries");
            }
        }

        public IRenderableSeriesViewModel CurrentSeries
        {
            get
            {
                if (_renderableSeries != null)
                {
                    return _renderableSeries[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_renderableSeries.Count == 0)
                {
                    _renderableSeries.Add(value);
                }
                else
                {
                    _renderableSeries[0] = value;
                }

                OnPropertyChanged("CurrentSeries");
            }
        }

        #endregion Properties

        #region Methods

        public SciChartColumnChartModel()
        {
            _data = new Dictionary<double, double>();

            XList = new List<double>();
            YList = new List<double>();
            Series = new XyDataSeries<double, double>();
            Series.AcceptsUnsortedData = true;

            for (double x = 0.0; x < 100.0; x += 0.1)
            {
                XList.Add(x);
                YList.Add(0.0);
            }

            Series.InsertRange(0, XList, YList);

            XAxis = new DoubleRange(
                    Double.Parse(Series.XMin.ToString()),
                    Double.Parse(Series.XMax.ToString()));

            YAxis = new DoubleRange(
                    Double.Parse(Series.YMin.ToString()),
                    Double.Parse(Series.YMax.ToString()));

            RenderableSeries = new ObservableCollection<IRenderableSeriesViewModel>();
            CurrentSeries = new ColumnRenderableSeriesViewModel()
            {
                DataPointWidth = 1,
                Stroke = System.Windows.Media.Color.FromRgb(186, 187, 190),
                DataSeries = Series
            };
        }

        private void SeriesUpdate()
        {
            if (RenderableSeries != null)
            {
                RenderableSeries.Clear();

                RenderableSeries.Add(new ColumnRenderableSeriesViewModel()
                {
                    DataPointWidth = 1,
                    Stroke = System.Windows.Media.Color.FromRgb(186, 187, 190),
                    DataSeries = Series
                });
            }

            if (Data != null && Series != null)
            {
                XList.Clear();
                YList.Clear();

                XList = Data.Keys.ToList();
                YList = Data.Values.ToList();

                try
                {
                    Series.Clear();
                    Series.InsertRange(0, XList, YList);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        #endregion Methods
    }
}
