using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SciChartWrapper.Views
{
    /// <summary>
    /// SciChartColumnChartView.xaml에 대한 상호 작용 논리
    /// 
    /// <!--  Declare RenderableSeries  -->
    ///     <s:SciChartSurface.RenderableSeries>
    ///         <s:FastColumnRenderableSeries x:Name="columnSeries"
    ///                                       DataPointWidth="1"
    ///                                       Stroke="#A99A8A">
    ///             <s:FastColumnRenderableSeries.Fill>
    ///                 <LinearGradientBrush StartPoint = "0,0" EndPoint="0,1">
    ///                     <GradientStop Offset = "0" Color="LightSteelBlue" />
    ///                     <GradientStop Offset = "1.0" Color="SteelBlue" />
    ///                 </LinearGradientBrush>
    ///             </s:FastColumnRenderableSeries.Fill>
    ///         </s:FastColumnRenderableSeries>
    ///     </s:SciChartSurface.RenderableSeries>
    /// 
    /// </summary>
    public partial class SciChartColumnChartView : UserControl
    {
        #region Model, ViewModel
        private SciChartWrapper.Models.SciChartColumnChartModel _sciChartColumnChartModelObj;
        private SciChartWrapper.ViewModels.SciChartColumnChartViewModel _sciChartColumnChartViewModelObj;
        #endregion Model, ViewModel

        #region Properties

        public string Title
        {
            get { return sciChart.ChartTitle; }
            set { sciChart.ChartTitle = value; }
        }

        public SciChartWrapper.Models.SciChartColumnChartModel SciChartColumnChartModelObj
        {
            get { return _sciChartColumnChartModelObj; }
            set { _sciChartColumnChartModelObj = value; }
        }

        public SciChartWrapper.ViewModels.SciChartColumnChartViewModel SciChartColumnChartViewModelObj
        {
            get { return _sciChartColumnChartViewModelObj; }
            set { _sciChartColumnChartViewModelObj = value; }
        }

        public SciChartSurface SciChartSurfaceObj
        {
            get { return sciChart; }
        }

        #endregion Properties

        public SciChartColumnChartView()
        {
            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey(
            @"<LicenseContract>
                <Customer>HUMEN INC</Customer>
                <OrderId>ABT180824-6591-16103</OrderId>
                <LicenseCount>1</LicenseCount>
                <IsTrialLicense>false</IsTrialLicense>
                <SupportExpires>08/24/2019 00:00:00</SupportExpires>
                <ProductCode>SC-WPF-SDK-ENTERPRISE-SRC</ProductCode>
                <KeyCode>lwAAAAEAAACfOArucTvUAXUAQ3VzdG9tZXI9SFVNRU4gSU5DO09yZGVySWQ9QUJUMTgwODI0LTY1OTEtMTYxMDM7U3Vic2NyaXB0aW9uVmFsaWRUbz0yNC1BdWctMjAxOTtQcm9kdWN0Q29kZT1TQy1XUEYtU0RLLUVOVEVSUFJJU0UtU1JDAEhyOOgf4VGIFWsSYM3pcvwC/wDfDVvKNfvmtI3mSY77keeN/QvfDCWcLV/kuY8O</KeyCode>
            </LicenseContract>");

            SciChartColumnChartModelObj = new SciChartWrapper.Models.SciChartColumnChartModel();
            SciChartColumnChartViewModelObj = new SciChartWrapper.ViewModels.SciChartColumnChartViewModel();
            SciChartColumnChartViewModelObj.SciChartColumnChartModelObj = SciChartColumnChartModelObj;
            SciChartColumnChartModelObj.SciChartColumnChartViewModelObj = SciChartColumnChartViewModelObj;
            this.DataContext = SciChartColumnChartViewModelObj;
            SciChartColumnChartViewModelObj.SciChartColumnChartViewObj = this;

            InitializeComponent();

            sciChart.ZoomExtents();
        }

        public SciChartColumnChartView(string name)
            : this()
        {
            Title = name;
        }

        public void SetData(byte[] data)
        {
            if (data != null)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(byte[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<byte> data)
        {
            if (data.Count != 0)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(List<byte> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(double[] data)
        {
            if (data != null)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(double[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<double> data)
        {
            if (data.Count != 0)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(List<double> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(float[] data)
        {
            if (data != null)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(float[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<float> data)
        {
            if (data.Count != 0)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(List<float> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(int[] data)
        {
            if (data != null)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(int[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<int> data)
        {
            if (data.Count != 0)
            {
                SciChartColumnChartViewModelObj.SetData(data);
            }
        }

        public void SetData(List<int> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartColumnChartViewModelObj.SetData(data, div);
            }
        }

        public void SetData(byte[] xData, byte[] yData)
        {
            SciChartColumnChartViewModelObj.SetData(xData, yData);
        }

        public void SetData(List<byte> xData, List<byte> yData)
        {
            SciChartColumnChartViewModelObj.SetData(xData, yData);
        }

        public void SetData(double[] xData, double[] yData)
        {
            SciChartColumnChartViewModelObj.SetData(xData, yData);
        }

        public void SetData(List<double> xData, List<double> yData)
        {
            SciChartColumnChartViewModelObj.SetData(xData, yData);
        }

        public void SetData(float[] xData, float[] yData)
        {
            SciChartColumnChartViewModelObj.SetData(xData, yData);
        }

        public void SetData(List<float> xData, List<float> yData)
        {
            SciChartColumnChartViewModelObj.SetData(xData, yData);
        }

        public Dictionary<double, double> GetDataDict()
        {
            return SciChartColumnChartViewModelObj.GetDataDict();
        }

        public Tuple<List<double>, List<double>> GetDataListTuple()
        {
            return SciChartColumnChartViewModelObj.GetDataListTuple();
        }

        public Tuple<double[], double[]> GetDataArrayTuple()
        {
            return SciChartColumnChartViewModelObj.GetDataArrayTuple();
        }

        public List<double> GetDataKeyList()
        {
            return SciChartColumnChartViewModelObj.GetDataKeyList();
        }

        public List<double> GetDataValueList()
        {
            return SciChartColumnChartViewModelObj.GetDataValueList();
        }

        public double[] GetDataKeyArray()
        {
            return SciChartColumnChartViewModelObj.GetDataKeyArray();
        }

        public double[] GetDataValueArray()
        {
            return SciChartColumnChartViewModelObj.GetDataValueArray();
        }

        public void ClearDataSeries()
        {
            if (SciChartColumnChartViewModelObj == null)
            {
                return;
            }

            using (sciChart.SuspendUpdates())
            {
                SciChartColumnChartViewModelObj.DataClear();
            }
        }

        public ObservableCollection<IRenderableSeriesViewModel> GetRenderableSeriesCollection()
        {
            return SciChartColumnChartViewModelObj.GetRenderableSeriesCollection();
        }

        public SciChart.Core.Framework.IUpdateSuspender GetSuspender()
        {
            return sciChart.SuspendUpdates();
        }
    }
}
