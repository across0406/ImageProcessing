using Base;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting.Visuals.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SciChartWrapper.Views;

namespace SciChartWrapper.Subprofiling
{
    public class HeightMinMaxTool : BasePropertyChanged, ISubprofilingTool
    {
        #region Members
        #region Variables
        private SciChartProfile2DView _chartView;
        private object _owner;

        private int _no;
        private string _measurementType;
        private string _value1;
        private string _value2;
        private string _value3;
        private string _value4;

        private double _height;
        private double _width;
        private double _min;
        private double _max;

        private bool _isEditable;
        private Color _toolColor;

        private VerticalLineAnnotation[] _verticals;
        #endregion

        #region Properties
        public SciChartProfile2DView ChartView
        {
            get { return _chartView; }
            set { _chartView = value; }
        }

        public object Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public int No
        {
            get { return _no; }
            set { _no = value; OnPropertyChanged("No"); }
        }

        public string DrawingType
        {
            get { return _measurementType; }
            set { _measurementType = value; OnPropertyChanged("DrawingType"); }
        }

        public string Value1
        {
            get { return _value1; }
            set { _value1 = value; OnPropertyChanged("Value1"); }
        }

        public string Value2
        {
            get { return _value2; }
            set { _value2 = value; OnPropertyChanged("Value2"); }
        }

        public string Value3
        {
            get { return _value3; }
            set { _value3 = value; OnPropertyChanged("Value3"); }
        }

        public string Value4
        {
            get { return _value4; }
            set { _value4 = value; OnPropertyChanged("Value4"); }
        }

        public VerticalLineAnnotation[] Verticals
        {
            get { return _verticals; }
            set { _verticals = value; OnPropertyChanged("Verticals"); }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged("Height"); }
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged("Width"); }
        }

        public double Min
        {
            get { return _min; }
            set { _min = value; OnPropertyChanged("Min"); }
        }

        public double Max
        {
            get { return _max; }
            set { _max = value; OnPropertyChanged("Max"); }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;

                if (_verticals != null)
                {
                    _verticals[0].IsEditable = value;
                    _verticals[1].IsEditable = value;
                }

                OnPropertyChanged("IsEditable");
            }
        }

        public System.Windows.Media.Color ToolColor
        {
            get { return _toolColor; }
            set
            {
                _toolColor = value;

                if (_verticals != null)
                {
                    _verticals[0].Stroke = new SolidColorBrush(value);
                    _verticals[1].Stroke = new SolidColorBrush(value);
                }

                OnPropertyChanged("ToolColor");
            }
        }
        #endregion
        #endregion

        #region Constructor
        public HeightMinMaxTool()
        {
            ToolColor = Colors.Orange;
            IsEditable = true;
            Init();
        }

        public HeightMinMaxTool( SciChartWrapper.Views.SciChartProfile2DView view)
        {
            ChartView = view;
            ToolColor = Colors.Orange;
            IsEditable = true;
            Init();
        }

        public HeightMinMaxTool(Color color)
        {
            ToolColor = color;
            IsEditable = true;
            Init();
        }

        public HeightMinMaxTool(Color color, bool isEditable)
        {
            ToolColor = color;
            IsEditable = isEditable;
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            _verticals = new VerticalLineAnnotation[2]
            {
                new VerticalLineAnnotation()
                {
                    Stroke = new SolidColorBrush(_toolColor),
                    StrokeThickness = 2,
                    X1 = 0.0,
                    IsEditable = _isEditable
                },

                new VerticalLineAnnotation()
                {
                    Stroke = new SolidColorBrush(_toolColor),
                    StrokeThickness = 2,
                    X1 = 0.5,
                    IsEditable = _isEditable
                }
            };

            _verticals[0].DragDelta += VerticalChanged;
            _verticals[1].DragDelta += VerticalChanged;

            Update();
        }

        public void Update()
        {
            var xList = ChartView.GetDataKeyList();
            var yList = ChartView.GetDataValueList();

            if ((double)_verticals[0].X1 < xList.Min())
            {
                _verticals[0].X1 = xList.Min();
            }

            else if ((double)_verticals[0].X1 >= xList.Max())
            {
                _verticals[0].X1 = xList.Max();
            }

            if ((double)_verticals[1].X1 < xList.Min())
            {
                _verticals[1].X1 = xList.Min();
            }

            else if ((double)_verticals[1].X1 >= xList.Max())
            {
                _verticals[1].X1 = xList.Max();
            }

            Width = Math.Round(Math.Abs(
                        (double)(_verticals[0].X1) -
                        (double)(_verticals[1].X1)), 6);
            Value2 = String.Format("Width:{0:0.######}", Width);

            var dataSeries = ChartView.GetRenderableSeriesCollection()[0].DataSeries;

            var startVertical = (double)(_verticals[0].X1) > (double)(_verticals[1].X1) ? (double)(_verticals[1].X1) : (double)(_verticals[0].X1);
            var endVertical = (double)(_verticals[0].X1) > (double)(_verticals[1].X1) ? (double)(_verticals[0].X1) : (double)(_verticals[1].X1);

            var startIndex = dataSeries.FindIndex(startVertical, SciChart.Charting.Common.Extensions.SearchMode.Nearest);
            var endIndex = dataSeries.FindIndex(endVertical, SciChart.Charting.Common.Extensions.SearchMode.Nearest);

            int subRangeLength = endIndex - startIndex + 1;

            List<double> yValues = new List<double>();

            for (int i = startIndex; i <= endIndex; i++)
            {
                yValues.Add((double)dataSeries.YValues[i]);
            }

            double min = yValues.Min();
            double max = yValues.Max();

            Min = Math.Round(min, 6);
            Value3 = String.Format("Min:{0:0.######}", Min);
            Max = Math.Round(max, 6);
            Value4 = String.Format("Max:{0:0.######}", Max);
            Height = Math.Round(Math.Abs(Min - Max), 6);
            Value1 = String.Format("Height:{0:0.######}", Height);
        }

        #region Events
        private void VerticalChanged(object sender, AnnotationDragDeltaEventArgs e)
        {
            Update();
        }
        #endregion

        #endregion
    }
}
