using System;
using System.Collections.Generic;
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
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.Events;
using SciChart.Data.Model;
using SciChart.Examples.ExternalDependencies.Common;
using SciChart.Examples.ExternalDependencies.Data;
using SciChart.Charting.ChartModifiers;
using SciChart.Charting.Model.ChartSeries;
using SciChartWrapper.Models;
using SciChart.Core.Utility.Mouse;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Annotations;
using SciChartWrapper.Subprofiling;
using System.Collections.ObjectModel;
using Microsoft.Xaml.Behaviors;

namespace SciChartWrapper.Views
{
    public class TextBoxEnterKeyUpdateBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == Key.Return)
                {
                    if (e.Key == Key.Enter)
                    {
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
        }
    }

    public class CursorModifierEx : CursorModifier
    {
        private bool _isClicked;
        public bool IsClicked
        {
            get { return _isClicked; }
            set
            {
                if (_isClicked != value)
                {
                    _isClicked = value;
                    OnPropertyChanged("IsClicked");
                }
            }
        }

        public CursorModifierEx()
            : base()
        {
            IsClicked = false;
        }

        public override void OnModifierMouseMove(ModifierMouseArgs e)
        {
            if (!IsClicked)
            {
                base.OnModifierMouseMove(e);
            }
        }
    }

    public partial class SciChartProfile2DView : UserControl
    {
        #region Members

        #region Model, ViewModel
        private SciChartWrapper.Models.SciChartProfile2DModel _sciChartProfile2DModelObj;
        private SciChartWrapper.ViewModels.SciChartProfile2DViewModel _sciChartProfile2DViewModelObj;
        #endregion

        #region Other Variables
        private bool _dragging;
        private CursorModifierEx _cursorModifierObj;
        private AnnotationCreationModifier _annotationCreationModifier;
        private Dictionary<string, Type> _dictMeasurement;
        #endregion

        #region Properties
        public string Title
        {
            get { return sciChartSurface.ChartTitle; }
            set { sciChartSurface.ChartTitle = value; }
        }

        public SciChartWrapper.Models.SciChartProfile2DModel SciChartProfile2DModelObj
        {
            get { return _sciChartProfile2DModelObj; }
            set { _sciChartProfile2DModelObj = value; }
        }

        public SciChartWrapper.ViewModels.SciChartProfile2DViewModel SciChartProfile2DViewModelObj
        {
            get { return _sciChartProfile2DViewModelObj; }
            set { _sciChartProfile2DViewModelObj = value; }
        }

        public bool Dragging
        {
            get { return _dragging; }
            set { _dragging = value; }
        }

        public CursorModifierEx CursorModifierObj
        {
            get { return _cursorModifierObj; }
            set { _cursorModifierObj = value; }
        }

        public SciChart.Charting.ChartModifiers.AnnotationCreationModifier AnnotationCreationModifier
        {
            get { return _annotationCreationModifier; }
            set { _annotationCreationModifier = value; }
        }

        public System.Collections.Generic.Dictionary<string, System.Type> DictMeasurement
        {
            get { return _dictMeasurement; }
            set { _dictMeasurement = value; }
        }

        public SciChartSurface SciChartSurfaceObj
        {
            get { return sciChartSurface; }
        }
        #endregion

        #endregion

        #region Constructor
        public SciChartProfile2DView()
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

            SciChartProfile2DModelObj = new SciChartWrapper.Models.SciChartProfile2DModel();
            SciChartProfile2DViewModelObj = new SciChartWrapper.ViewModels.SciChartProfile2DViewModel();
            //SciChartProfile2DViewModelObj.SciChartProfile2DViewObj = this;
            SciChartProfile2DViewModelObj.SciChartProfile2DModelObj = SciChartProfile2DModelObj;
            SciChartProfile2DModelObj.SciChartProfile2DViewModelObj = SciChartProfile2DViewModelObj;
            this.DataContext = SciChartProfile2DViewModelObj;
            SciChartProfile2DViewModelObj.SciChartProfile2DViewObj = this;

            InitializeComponent();

            CursorModifierObj = new CursorModifierEx()
            {
                LineOverlayStyle = this.Resources["CursorLineStyle"] as Style,
                ShowAxisLabels = true,
                ShowTooltip = false,
                SourceMode = SourceMode.AllSeries,
                TooltipContainerStyle = this.Resources["CursorTooltipStyle"] as Style,
                UseInterpolation = true
            };

            AnnotationCreationModifier = new AnnotationCreationModifier();
            var xAxisDragModifier = new XAxisDragModifier();
            var yAxisDragModifier = new YAxisDragModifier();

            (sciChartSurface.ChartModifier as ModifierGroup).ChildModifiers.Add(CursorModifierObj);
            (sciChartSurface.ChartModifier as ModifierGroup).ChildModifiers.Add(AnnotationCreationModifier);
            (sciChartSurface.ChartModifier as ModifierGroup).ChildModifiers.Add(xAxisDragModifier);
            (sciChartSurface.ChartModifier as ModifierGroup).ChildModifiers.Add(yAxisDragModifier);

            sciChartSurface.ZoomExtents();
            Dragging = false;

            DictMeasurement = new Dictionary<string, Type>()
            {
                { "HorizontalLineAnnotation", typeof(HorizontalLineAnnotation)},
                { "VerticalLineAnnotation", typeof(VerticalLineAnnotation)}
            };

            uiXMin.Visibility = Visibility.Hidden;
            uiXMax.Visibility = Visibility.Hidden;
            uiYMin.Visibility = Visibility.Hidden;
            uiYMax.Visibility = Visibility.Hidden;
        }

        public SciChartProfile2DView(string name)
            : this()
        {
            sciChartSurface.ChartTitle = name;
        }
        #endregion

        #region Methods

        #region Events
        private void OnMinXAxesChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text != "")
            {
                SciChartProfile2DViewModelObj.XMinChange(Double.Parse((sender as TextBox).Text));
            }
        }

        private void OnMaxXAxesChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text != "")
            {
                SciChartProfile2DViewModelObj.XMaxChange(Double.Parse((sender as TextBox).Text));
            }
        }

        private void OnMinYAxesChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text != "")
            {
                SciChartProfile2DViewModelObj.YMinChange(Double.Parse((sender as TextBox).Text));
            }
        }

        private void OnMaxYAxesChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text != "")
            {
                SciChartProfile2DViewModelObj.YMaxChange(Double.Parse((sender as TextBox).Text));
            }
        }

        public void EditingEnabled(object sender, RoutedEventArgs e)
        {
            foreach (var annotation in sciChartSurface.Annotations)
            {
                annotation.IsEditable = true;
            }
        }

        public void AddAnnotations(ISubprofilingTool tool)
        {
            if (tool.GetType() == typeof(DiffWidthHeightSubprofilingTool))
            {
                var xList = GetDataKeyList();
                var yList = GetDataValueList();

                DiffWidthHeightSubprofilingTool added = tool as DiffWidthHeightSubprofilingTool;
                added.Horizontals[0].Y1 = yList.Min();
                added.Horizontals[1].Y1 = yList.Max();
                added.Verticals[0].X1 = xList.Min();
                added.Verticals[1].X1 = xList.Max();
                added.Height = Math.Round(Math.Abs(
                        (double)(added.Horizontals[0].Y1) -
                        (double)(added.Horizontals[1].Y1)), 6);
                added.Width = Math.Round(Math.Abs(
                        (double)(added.Verticals[0].X1) -
                        (double)(added.Verticals[1].X1)), 6);
                added.Value1 = String.Format("{0:Height:0.######}", Height.ToString());
                added.Value2 = String.Format("{0:Width:0.######}", Width.ToString());
                sciChartSurface.Annotations.Add(added.Horizontals[0]);
                sciChartSurface.Annotations.Add(added.Horizontals[1]);
                sciChartSurface.Annotations.Add(added.Verticals[0]);
                sciChartSurface.Annotations.Add(added.Verticals[1]);
            }

            else if (tool.GetType() == typeof(HeightMinMaxTool))
            {
                var xList = GetDataKeyList();
                var yList = GetDataValueList();

                HeightMinMaxTool added = tool as HeightMinMaxTool;
                added.Verticals[0].X1 = xList.Min();
                added.Verticals[1].X1 = xList.Max();
                added.Update();
                sciChartSurface.Annotations.Add(added.Verticals[0]);
                sciChartSurface.Annotations.Add(added.Verticals[1]);
            }
        }

        public void RemoveAnnotations(ISubprofilingTool tool)
        {
            if (tool.GetType() == typeof(DiffWidthHeightSubprofilingTool))
            {
                DiffWidthHeightSubprofilingTool added = tool as DiffWidthHeightSubprofilingTool;
                sciChartSurface.Annotations.Remove(added.Horizontals[0]);
                sciChartSurface.Annotations.Remove(added.Horizontals[1]);
                sciChartSurface.Annotations.Remove(added.Verticals[0]);
                sciChartSurface.Annotations.Remove(added.Verticals[1]);
            }

            if (tool.GetType() == typeof(HeightMinMaxTool))
            {
                HeightMinMaxTool added = tool as HeightMinMaxTool;
                sciChartSurface.Annotations.Remove(added.Verticals[0]);
                sciChartSurface.Annotations.Remove(added.Verticals[1]);
            }
        }

        public void EditableChange(object owner)
        {

        }
        #endregion

        #region Static Methods

        #endregion

        #region Object Methods
        public void SetData(byte[] data)
        {
            if (data != null)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(byte[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<byte> data)
        {
            if (data.Count != 0)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(List<byte> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(double[] data)
        {
            if (data != null)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(double[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<double> data)
        {
            if (data.Count != 0)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(List<double> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(float[] data)
        {
            if (data != null)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(float[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<float> data)
        {
            if (data.Count != 0)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(List<float> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(int[] data)
        {
            if (data != null)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(int[] data, int div)
        {
            if (data != null || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(List<int> data)
        {
            if (data.Count != 0)
            {
                SciChartProfile2DViewModelObj.SetData(data);
            }
        }

        public void SetData(List<int> data, int div)
        {
            if (data.Count != 0 || div > 0)
            {
                SciChartProfile2DViewModelObj.SetData(data, div);
            }
        }

        public void SetData(byte[] xData, byte[] yData)
        {
            SciChartProfile2DViewModelObj.SetData(xData, yData);
        }

        public void SetData(List<byte> xData, List<byte> yData)
        {
            SciChartProfile2DViewModelObj.SetData(xData, yData);
        }

        public void SetData(double[] xData, double[] yData)
        {
            SciChartProfile2DViewModelObj.SetData(xData, yData);
        }

        public void SetData(List<double> xData, List<double> yData)
        {
            SciChartProfile2DViewModelObj.SetData(xData, yData);
        }

        public void SetData(float[] xData, float[] yData)
        {
            SciChartProfile2DViewModelObj.SetData(xData, yData);
        }

        public void SetData(List<float> xData, List<float> yData)
        {
            SciChartProfile2DViewModelObj.SetData(xData, yData);
        }

        public Dictionary<double, double> GetDataDict()
        {
            return SciChartProfile2DViewModelObj.GetDataDict();
        }

        public Tuple<List<double>, List<double>> GetDataListTuple()
        {
            return SciChartProfile2DViewModelObj.GetDataListTuple();
        }

        public Tuple<double[], double[]> GetDataArrayTuple()
        {
            return SciChartProfile2DViewModelObj.GetDataArrayTuple();
        }

        public List<double> GetDataKeyList()
        {
            return SciChartProfile2DViewModelObj.GetDataKeyList();
        }

        public List<double> GetDataValueList()
        {
            return SciChartProfile2DViewModelObj.GetDataValueList();
        }

        public double[] GetDataKeyArray()
        {
            return SciChartProfile2DViewModelObj.GetDataKeyArray();
        }

        public double[] GetDataValueArray()
        {
            return SciChartProfile2DViewModelObj.GetDataValueArray();
        }

        public void ClearDataSeries()
        {
            if (SciChartProfile2DViewModelObj == null)
            {
                return;
            }

            using (sciChartSurface.SuspendUpdates())
            {
                SciChartProfile2DViewModelObj.DataClear();
            }
        }

        public ObservableCollection<IRenderableSeriesViewModel> GetRenderableSeriesCollection()
        {
            return SciChartProfile2DViewModelObj.GetRenderableSeriesCollection();
        }

        public SciChart.Core.Framework.IUpdateSuspender GetSuspender()
        {
            return sciChartSurface.SuspendUpdates();
        }
        #endregion

        #endregion
    }
}
