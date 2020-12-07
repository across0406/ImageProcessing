using Base;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using SciChartWrapper.Views;
using SciChartWrapper.Models;

namespace SciChartWrapper.ViewModels
{
    public enum EProfileMode
    {
        Raw,
        FFT,
        FFT_LOG
    }

    public class SciChartProfile2DViewModel : BasePropertyChanged
    {
        #region Members

        #region View, Model
        private SciChartProfile2DView _sciChartProfile2DViewObj;
        private SciChartProfile2DModel _sciChartProfile2DModelObj;
        #endregion

        #region Chart Information
        private EProfileMode _mode;
        private double _srcMin;
        private double _srcMax;
        private bool _isAutoScaled;
        private double _xMin;
        private double _xMax;
        private double _yMin;
        private double _yMax;
        #endregion

        #region Properties
        public SciChartProfile2DView SciChartProfile2DViewObj
        {
            get { return _sciChartProfile2DViewObj; }
            set { _sciChartProfile2DViewObj = value; }
        }

        public SciChartProfile2DModel SciChartProfile2DModelObj
        {
            get { return _sciChartProfile2DModelObj; }
            set { _sciChartProfile2DModelObj = value; }
        }

        public EProfileMode Mode
        {
            get { return _mode; }
            set { _mode = value; OnPropertyChanged("Mode"); }
        }

        public double SrcMin
        {
            get { return _srcMin; }
            set { _srcMin = value; OnPropertyChanged("SrcMin"); }
        }

        public double SrcMax
        {
            get { return _srcMax; }
            set { _srcMax = value; OnPropertyChanged("SrcMax"); }
        }

        public bool IsAutoScaled
        {
            get { return _isAutoScaled; }
            set
            {
                _isAutoScaled = value;

                if(SciChartProfile2DViewObj != null)
                {
                    if (_isAutoScaled)
                    {
                        SciChartProfile2DViewObj.uiXMin.Visibility = Visibility.Hidden;
                        SciChartProfile2DViewObj.uiXMax.Visibility = Visibility.Hidden;
                        SciChartProfile2DViewObj.uiYMin.Visibility = Visibility.Hidden;
                        SciChartProfile2DViewObj.uiYMax.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        SciChartProfile2DViewObj.uiXMin.Visibility = Visibility.Visible;
                        SciChartProfile2DViewObj.uiXMax.Visibility = Visibility.Visible;
                        SciChartProfile2DViewObj.uiYMin.Visibility = Visibility.Visible;
                        SciChartProfile2DViewObj.uiYMax.Visibility = Visibility.Visible;
                    }
                }

                OnPropertyChanged("IsAutoScaled");
            }
        }

        public double XMin
        {
            get { return _xMin; }
            set
            {
                if (value >= _xMax)
                    MessageBox.Show("This value should be less than max of x-axis.");
                else
                {
                    _xMin = value;
                    OnPropertyChanged("XMin");
                }
                    
            } 
        }

        public double XMax
        {
            get { return _xMax; }
            set
            {
                if (value <= _xMin)
                    MessageBox.Show("This value should be less than min of x-axis.");
                else
                { 
                    _xMax = value;
                    OnPropertyChanged("XMax");
                }
            }
        }
        public double YMin
        {
            get { return _yMin; }
            set
            {
                if (value >= _yMax)
                    MessageBox.Show("This value should be less than max of y-axis.");
                else
                {  
                    _yMin = value;
                    OnPropertyChanged("YMin");
                }
            }
        }
        public double YMax
        {
            get { return _yMax; }
            set
            {
                if (value <= _yMin)
                    MessageBox.Show("This value should be greater than min of y-axis.");
                else
                { 
                    _yMax = value;
                    OnPropertyChanged("YMax");
                }
            }
        }
        #endregion

        #endregion

        #region Constructor
        public SciChartProfile2DViewModel()
        {
            IsAutoScaled = true;
            _xMin = 0;
            _yMin = 0;
            _xMax = 100;
            _yMax = 100;
        }
        #endregion Constructor

        #region Methods
        public Dictionary<double, double> GetDataDict()
        {
            Dictionary<double, double> dict = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                dict = SciChartProfile2DModelObj.Data;
            }

            return dict;
        }

        public Tuple<List<double>, List<double>> GetDataListTuple()
        {
            Tuple<List<double>, List<double>> tuple = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                tuple = new Tuple<List<double>, List<double>>(GetDataKeyList(), GetDataValueList());
            }

            return tuple;
        }

        public Tuple<double[], double[]> GetDataArrayTuple()
        {
            Tuple<double[], double[]> tuple = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                tuple = new Tuple<double[], double[]>(GetDataKeyArray(), GetDataValueArray());
            }

            return tuple;
        }

        public List<double> GetDataKeyList()
        {
            List<double> list = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                list = SciChartProfile2DModelObj.Data.Keys.ToList();
            }

            return list;
        }

        public List<double> GetDataValueList()
        {
            List<double> list = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                list = SciChartProfile2DModelObj.Data.Values.ToList();
            }

            return list;
        }

        public double[] GetDataKeyArray()
        {
            double[] array = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                array = SciChartProfile2DModelObj.Data.Keys.ToArray();
            }

            return array;
        }

        public double[] GetDataValueArray()
        {
            double[] array = null;

            if (SciChartProfile2DModelObj.Data != null)
            {
                array = SciChartProfile2DModelObj.Data.Values.ToArray();
            }

            return array;
        }

        public void SetData(byte[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(byte[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)i / div, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(int[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(int[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<int> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<int> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)i / div, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)i / div, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)i / div, data[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(byte[] xData, byte[] yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(byte[] xData, byte[] yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> xData, List<byte> yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> xData, List<byte> yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] xData, float[] yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] xData, float[] yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> xData, List<float> yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> xData, List<float> yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] xData, double[] yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] xData, double[] yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> xData, List<double> yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> xData, List<double> yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartProfile2DModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(Dictionary<double, double> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartProfile2DModelObj != null)
                    {
                        SciChartProfile2DModelObj.Data = data;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public ObservableCollection<IRenderableSeriesViewModel> GetRenderableSeriesCollection()
        {
            ObservableCollection<IRenderableSeriesViewModel> renderableSeriesCollection = null;

            if (SciChartProfile2DModelObj != null)
            {
                renderableSeriesCollection = SciChartProfile2DModelObj.RenderableSeries;
            }

            return renderableSeriesCollection;
        }

        public void DataClear()
        {
            SciChartProfile2DModelObj.Series.Clear();
            SciChartProfile2DModelObj.Data.Clear();
        }

        public void XMinChange(double min)
        {
            if (SciChartProfile2DModelObj != null)
            {
                XMin = min;
                SciChartProfile2DModelObj.XMinChange(min);
            }
        }

        public void XMaxChange(double max)
        {
            if (SciChartProfile2DModelObj != null)
            {
                XMax = max;
                SciChartProfile2DModelObj.XMaxChange(max);
            }
        }

        public void YMinChange(double min)
        {
            if (SciChartProfile2DModelObj != null)
            {
                YMin = min;
                SciChartProfile2DModelObj.YMinChange(min);
            }
        }

        public void YMaxChange(double max)
        {
            if (SciChartProfile2DModelObj != null)
            {
                YMax = max;
                SciChartProfile2DModelObj.YMaxChange(max);
            }
        }

        public SciChart.Core.Framework.IUpdateSuspender GetSuspender()
        {
            SciChart.Core.Framework.IUpdateSuspender returnSuspender = null;

            if (SciChartProfile2DViewObj != null)
            {
                returnSuspender = SciChartProfile2DViewObj.GetSuspender();
            }

            return returnSuspender;
        }

        #endregion
    }
}
