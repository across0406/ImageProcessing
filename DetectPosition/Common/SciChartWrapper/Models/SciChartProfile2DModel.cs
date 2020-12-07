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
    public class SciChartProfile2DModel : BasePropertyChanged
    {
        #region Members

        #region ViewModel
        private SciChartProfile2DViewModel _sciChartProfile2DViewModelObj;
        #endregion

        #region Variables
        private double _xMin;
        private double _xMax;
        private double _yMin;
        private double _yMax;
        private List<double> _xList;
        private List<double> _yList;

        private int _count;
        private bool _enableZoom;
        private bool _enablePan;

        private IRange _xAxis;
        private IRange _yAxis;

        private Dictionary<double, double> _data;
        private IXyDataSeries<double, double> _series;

        private ObservableCollection<IRenderableSeriesViewModel> _renderableSeries;
        #endregion

        #region Properties
        public SciChartProfile2DViewModel SciChartProfile2DViewModelObj
        {
            get { return _sciChartProfile2DViewModelObj; }
            set { _sciChartProfile2DViewModelObj = value; }
        }

        public Dictionary<double, double> Data
        {
            get { return _data; }
            set
            {
                if (value != null)
                {
                    if(_data != null)
                    {
                        _data.Clear();
                    }
                    
                    _data = value;

                    if (RenderableSeries != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.DataBind, new Action(() =>
                        {
                            if (SciChartProfile2DViewModelObj != null)
                            {
                                if (SciChartProfile2DViewModelObj.IsAutoScaled)
                                {
                                    XAxis.SetMinMax(
                                        _data.Keys.Min(),
                                        _data.Keys.Max());
                                    YAxis.SetMinMax(
                                        _data.Values.Min(),
                                        _data.Values.Max());
                                }
                                else
                                {
                                    XAxis.SetMinMax(SciChartProfile2DViewModelObj.XMin, SciChartProfile2DViewModelObj.XMax);
                                    YAxis.SetMinMax(SciChartProfile2DViewModelObj.YMin, SciChartProfile2DViewModelObj.YMax);
                                }

                                SeriesUpdate();
                            }
                        }));
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

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public bool EnableZoom
        {
            get { return _enableZoom; }
            set
            {
                if (_enableZoom != value)
                {
                    _enableZoom = value;
                    OnPropertyChanged("EnableZoom");

                    if (_enableZoom)
                    {
                        EnablePan = false;
                    }
                }
            }
        }

        public bool EnablePan
        {
            get { return _enablePan; }
            set
            {
                if (_enablePan != value)
                {
                    _enablePan = value;
                    OnPropertyChanged("EnablePan");

                    if (_enablePan)
                    {
                        EnableZoom = false;
                    }
                }
            }
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
                if(_renderableSeries != null)
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
                if(_renderableSeries.Count == 0)
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
        #endregion

        #endregion

        #region Constructor
        public SciChartProfile2DModel()
        {
            _data = new Dictionary<double, double>();

            XList = new List<double>();
            YList = new List<double>();
            Series = new XyDataSeries<double, double>();
            Series.AcceptsUnsortedData = true;

            _xMin = 0.0;
            _yMin = 0.0;
            _xMax = 100.0;
            _yMax = 100.0;

            for (double x = 0.0; x < 100.0; x += 0.1)
            {
                XList.Add(x);
                YList.Add(0.0);
            }

            Series.InsertRange(0, XList, YList);

            if(SciChartProfile2DViewModelObj != null && SciChartProfile2DViewModelObj.IsAutoScaled)
            {
                XAxis = new DoubleRange(
                    Double.Parse(Series.XMin.ToString()),
                    Double.Parse(Series.XMax.ToString()));

                YAxis = new DoubleRange(
                        Double.Parse(Series.YMin.ToString()),
                        Double.Parse(Series.YMax.ToString()));
            }
            else
            {
                XAxis = new DoubleRange(
                    Double.Parse(_xMin.ToString()),
                    Double.Parse(_xMax.ToString()));

                YAxis = new DoubleRange(
                        Double.Parse(_yMin.ToString()),
                        Double.Parse(_yMax.ToString()));
            }

            Count = 0;

            RenderableSeries = new ObservableCollection<IRenderableSeriesViewModel>();
            CurrentSeries = new LineRenderableSeriesViewModel()
            {
                StrokeThickness = 2,
                Stroke = System.Windows.Media.Color.FromRgb(186, 187, 190),
                DataSeries = Series
            };
            //RenderableSeries.Add();

            EnableZoom = true;
        }
        #endregion

        #region Methods

        #region Static Methods

        #endregion

        #region Object Methods
        private void SeriesUpdate()
        {
            if (RenderableSeries != null)
            {
                RenderableSeries.Clear();

                RenderableSeries.Add(new LineRenderableSeriesViewModel()
                {
                    StrokeThickness = 2,
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

        public void XMinChange(double min)
        {
            if (Double.Parse(XAxis.Max.ToString()) >= min)
            {
                XAxis.Min = min;
            }
        }

        public void XMaxChange(double max)
        {
            if (Double.Parse(XAxis.Min.ToString()) < max)
            {
                XAxis.Max = max;

                // Should Data size checking
            }
        }

        public void YMinChange(double min)
        {
            if (Double.Parse(YAxis.Max.ToString()) >= min)
            {
                YAxis.Min = min;
            }
        }

        public void YMaxChange(double max)
        {
            if (Double.Parse(YAxis.Min.ToString()) < max)
            {
                YAxis.Max = max;
            }
        }
        #endregion

        #endregion
    }
}
