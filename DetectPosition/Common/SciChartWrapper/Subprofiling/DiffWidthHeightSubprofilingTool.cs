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
    public class DiffWidthHeightSubprofilingTool : BasePropertyChanged, ISubprofilingTool
    {
        #region Members

        #region Variables
        private SciChartProfile2DView _chartView;
        private object _owner;
        
        private int _no;
        private string _measurementType;
        private string _value1;
        private string _value2;

        private bool _isEditable;
        private Color _toolColor;
        
        private HorizontalLineAnnotation[] _horizontals;
        private VerticalLineAnnotation[] _verticals;

        private double _height;
        private double _width;
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
            get
            {
                _value1 = "Height:" + Height.ToString();
                return _value1;
            }

            set
            {
                _value1 = value;
                OnPropertyChanged("Value1");
            }
        }

        public string Value2
        {
            get
            {
                _value2 = "Width:" + Width.ToString();
                return _value2;
            }

            set
            {
                _value2 = value;
                OnPropertyChanged("Value2");
            }
        }

        public string Value3
        {
            get
            {
                return "";
            }

            set
            {
                OnPropertyChanged("Value3");
            }
        }

        public string Value4
        {
            get
            {                
                return "";
            }

            set
            {
                OnPropertyChanged("Value2");
            }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;

                if(_horizontals != null)
                {
                    _horizontals[0].IsEditable = value;
                    _horizontals[1].IsEditable = value;
                }

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

                if (_horizontals != null)
                {
                    _horizontals[0].Stroke = new SolidColorBrush(value);
                    _horizontals[1].Stroke = new SolidColorBrush(value);
                }

                if (_verticals != null)
                {
                    _verticals[0].Stroke = new SolidColorBrush(value);
                    _verticals[1].Stroke = new SolidColorBrush(value);
                }

                OnPropertyChanged("ToolColor");
            }
        }

        public HorizontalLineAnnotation[] Horizontals
        {
            get { return _horizontals; }
            set { _horizontals = value; OnPropertyChanged("Horizontals"); }
        }

        public VerticalLineAnnotation[] Verticals
        {
            get { return _verticals; }
            set { _verticals = value; OnPropertyChanged("Verticals"); }
        }

        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }        
        #endregion

        #endregion

        #region Constructor
        public DiffWidthHeightSubprofilingTool()
        {
            ToolColor = Colors.Orange;
            IsEditable = true;
            Init();
        }

        public DiffWidthHeightSubprofilingTool( SciChartWrapper.Views.SciChartProfile2DView view)
        {
            ChartView = view;
            ToolColor = Colors.Orange;
            IsEditable = true;
            Init();
        }

        public DiffWidthHeightSubprofilingTool(Color color)
        {
            ToolColor = color;
            IsEditable = true;
            Init();
        }

        public DiffWidthHeightSubprofilingTool(Color color, bool isEditable)
        {
            ToolColor = color;
            IsEditable = isEditable;
            Init();
        }
        #endregion

        #region Methods

        #region Init
        private void Init()
        {
            _horizontals = new HorizontalLineAnnotation[2]
            {
                new HorizontalLineAnnotation()
                {
                    Stroke = new SolidColorBrush(_toolColor),
                    StrokeThickness = 2,
                    Y1 = -0.5,
                    IsEditable = _isEditable
                },

                new HorizontalLineAnnotation()
                {
                    Stroke = new SolidColorBrush(_toolColor),
                    StrokeThickness = 2,
                    Y1 = 0.5,
                    IsEditable = _isEditable
                }
            };

            _verticals = new VerticalLineAnnotation[2]
            {
                new VerticalLineAnnotation()
                {
                    Stroke = new SolidColorBrush(_toolColor),
                    StrokeThickness = 2,
                    X1 = -0.5,
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

            _horizontals[0].DragDelta += HorizontalChanged;
            _horizontals[1].DragDelta += HorizontalChanged;
            _verticals[0].DragDelta += VerticalChanged;
            _verticals[1].DragDelta += VerticalChanged;

            Update();
        }
        #endregion

        public void Update()
        {
            var xList = ChartView.GetDataKeyList();
            var yList = ChartView.GetDataValueList();

            if ((double)_horizontals[0].Y1 < yList.Min())
            {
                _horizontals[0].Y1 = yList.Min();
            }

            else if ((double)_horizontals[0].Y1 >= yList.Max())
            {
                _horizontals[0].Y1 = yList.Max();
            }

            if ((double)_horizontals[1].Y1 < yList.Min())
            {
                _horizontals[1].Y1 = yList.Min();
            }

            else if ((double)_horizontals[1].Y1 >= yList.Max())
            {
                _horizontals[1].Y1 = yList.Max();
            }

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

            Height = Math.Round(Math.Abs(
                        (double)(_horizontals[0].Y1) -
                        (double)(_horizontals[1].Y1)), 6);
            Value1 = String.Format("Height:{0:0.######}", Height.ToString());

            Width = Math.Round(Math.Abs(
                        (double)(_verticals[0].X1) -
                        (double)(_verticals[1].X1)), 6);
            Value2 = String.Format("Width:{0:0.######}", Width.ToString());
        }

        #region Events
        private void HorizontalChanged(object sender, AnnotationDragDeltaEventArgs e)
        {
            Update();
        }

        private void VerticalChanged(object sender, AnnotationDragDeltaEventArgs e)
        {
            Update();
        }
        #endregion

        #endregion
    }
}
