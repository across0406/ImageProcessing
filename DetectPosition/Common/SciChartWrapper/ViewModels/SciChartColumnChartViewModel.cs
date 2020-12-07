using Base;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SciChartWrapper.Models;
using SciChartWrapper.Views;

namespace SciChartWrapper.ViewModels
{
    public class SciChartColumnChartViewModel : BasePropertyChanged
    {
        #region View, Model
        private SciChartColumnChartView _sciChartColumnChartViewObj;
        private SciChartColumnChartModel _sciChartColumnChartModelObj;
        #endregion

        #region Properties

        public SciChartColumnChartView SciChartColumnChartViewObj
        {
            get { return _sciChartColumnChartViewObj; }
            set { _sciChartColumnChartViewObj = value; }
        }

        public SciChartColumnChartModel SciChartColumnChartModelObj
        {
            get { return _sciChartColumnChartModelObj; }
            set { _sciChartColumnChartModelObj = value; }
        }

        #endregion Properties

        #region Methods
        public Dictionary<double, double> GetDataDict()
        {
            Dictionary<double, double> dict = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                dict = SciChartColumnChartModelObj.Data;
            }

            return dict;
        }

        public Tuple<List<double>, List<double>> GetDataListTuple()
        {
            Tuple<List<double>, List<double>> tuple = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                tuple = new Tuple<List<double>, List<double>>(GetDataKeyList(), GetDataValueList());
            }

            return tuple;
        }

        public Tuple<double[], double[]> GetDataArrayTuple()
        {
            Tuple<double[], double[]> tuple = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                tuple = new Tuple<double[], double[]>(GetDataKeyArray(), GetDataValueArray());
            }

            return tuple;
        }

        public List<double> GetDataKeyList()
        {
            List<double> list = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                list = SciChartColumnChartModelObj.Data.Keys.ToList();
            }

            return list;
        }

        public List<double> GetDataValueList()
        {
            List<double> list = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                list = SciChartColumnChartModelObj.Data.Values.ToList();
            }

            return list;
        }

        public double[] GetDataKeyArray()
        {
            double[] array = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                array = SciChartColumnChartModelObj.Data.Keys.ToArray();
            }

            return array;
        }

        public double[] GetDataValueArray()
        {
            double[] array = null;

            if (SciChartColumnChartModelObj.Data != null)
            {
                array = SciChartColumnChartModelObj.Data.Values.ToArray();
            }

            return array;
        }

        public void SetData(byte[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(byte[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)i / div, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(int[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(int[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<int> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<int> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Length; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i++)
                        {
                            dict.Add((double)i, data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> data, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < data.Count; i *= div)
                        {
                            dict.Add((double)(i / div), data[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(Dictionary<double, double> data)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        SciChartColumnChartModelObj.Data = data;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(byte[] xData, byte[] yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(byte[] xData, byte[] yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> xData, List<byte> yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<byte> xData, List<byte> yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] xData, float[] yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(float[] xData, float[] yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> xData, List<float> yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i++)
                        {
                            dict.Add((double)xData[i], (double)yData[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<float> xData, List<float> yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i *= div)
                        {
                            dict.Add((double)xData[i / div], (double)yData[i / div]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] xData, double[] yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i++)
                        {
                            dict.Add(xData[i], yData[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(double[] xData, double[] yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Length; i *= div)
                        {
                            dict.Add(xData[i / div], yData[i / div]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> xData, List<double> yData)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i++)
                        {
                            dict.Add(xData[i], yData[i]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public void SetData(List<double> xData, List<double> yData, int div)
        {
            Application.Current.Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    if (SciChartColumnChartModelObj != null)
                    {
                        Dictionary<double, double> dict = new Dictionary<double, double>();

                        for (int i = 0; i < xData.Count; i *= div)
                        {
                            dict.Add(xData[i / div], yData[i / div]);
                        }

                        SciChartColumnChartModelObj.Data = dict;
                    }
                }), DispatcherPriority.ApplicationIdle
            );
        }

        public ObservableCollection<IRenderableSeriesViewModel> GetRenderableSeriesCollection()
        {
            ObservableCollection<IRenderableSeriesViewModel> renderableSeriesCollection = null;

            if (SciChartColumnChartModelObj != null)
            {
                renderableSeriesCollection = SciChartColumnChartModelObj.RenderableSeries;
            }

            return renderableSeriesCollection;
        }

        public void DataClear()
        {
            SciChartColumnChartModelObj.Series.Clear();
            SciChartColumnChartModelObj.Data.Clear();
        }

        public SciChart.Core.Framework.IUpdateSuspender GetSuspender()
        {
            SciChart.Core.Framework.IUpdateSuspender returnSuspender = null;

            if (SciChartColumnChartViewObj != null)
            {
                returnSuspender = SciChartColumnChartViewObj.GetSuspender();
            }

            return returnSuspender;
        }

        #endregion Methods
    }
}
