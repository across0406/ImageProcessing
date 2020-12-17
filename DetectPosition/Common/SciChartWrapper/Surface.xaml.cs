// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2017. All rights reserved.
//  
// Web: http://www.scichart.com
//   Support: support@scichart.com
//   Sales:   sales@scichart.com
// 
// TenorCurves3DChart.xaml.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use. 
// 
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied. 
// *************************************************************************************
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting3D;
using SciChart.Charting3D.Model;
using SciChart.Charting3D.Model.ChartData;
using SciChart.Charting3D.PointMarkers;
using SciChart.Charting3D.RenderableSeries;
using SciChart.Core.Extensions;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using cv = OpenCvSharp;

namespace SciChartWrapper
{
    public enum ECheckShape
    {
        None = 0, Line, Rect, Circle_2P, Circle_3P
    }

    public struct PositionInfo
    {
        public List<Coords> points;
        public Coords centerPoints; // For Circle_3P            

        public double length; // For Line
        public double width, height; // For Rect
        public double radius; // For Circle

        public ECheckShape checkShape;

        public PositionInfo( ECheckShape checkShape )
        {
            points = new List<Coords>();
            centerPoints = new Coords();

            length = 0;
            width = 0;
            height = 0;
            radius = 0;

            this.checkShape = checkShape;
        }
    }

    public struct Coords
    {
        public float x, y, z;

        public Coords( float x, float y, float z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class PositionEventArgs : EventArgs
    {
        public PositionInfo Data
        {
            get; set;
        }

        public PositionEventArgs()
        {
            Data = new PositionInfo();
        }

        ~PositionEventArgs()
        {

        }
    }

    public class Vector3ComponentConverterEx : Vector3ComponentConverter
    {
        public new object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var seriesInfo = value as BaseXyzSeriesInfo3D;

            if ( seriesInfo == null )
                return null;

            var ds = seriesInfo.RenderableSeries.DataSeries as XyzDataSeries3D<double>;

            Vector3 convertedVector3 = new Vector3(
                (float)ds.ZValues[ (int)seriesInfo.VertexId - 1 ],
                (float)ds.XValues[ (int)seriesInfo.VertexId - 1 ],
                (float)ds.YValues[ (int)seriesInfo.VertexId - 1 ] );

            return convertedVector3;
        }
    }

    public partial class Surface : UserControl, INotifyPropertyChanged
    {
        public object _viewPage;

        public UniformGridDataSeries3D<double, double, double> _meshDataSeries;

        public cv.Mat zMap;
        public int xSize;
        public float yMax;
        public int zSize;
        public List<PositionInfo> positionInfo;
        public List<DrawGeometry> drawInfo;
        public int annotIdx;
        public DrawGeometry test;
        public XyzDataSeries3D<double> marker;
        public bool movePoint;
        public int selectedIdx;

        private float _dH;
        private float _dW;
        private float _maxWidth;
        private float _maxHeight;

        public event EventHandler<PositionEventArgs> PositionEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        private const int MaxSeriesAmount = 4;

        public UniformGridDataSeries3D<double, double, double> MeshDataSeries
        {
            get
            {
                return _meshDataSeries;
            }
            set
            {
                _meshDataSeries = value;
                OnPropertyChanged( "MeshDataSeries" );
            }
        }

        public bool CanAddSeries
        {
            get
            {
                return sciChart.RenderableSeries.Count < MaxSeriesAmount;
            }
        }

        public bool CanRemoveSeries
        {
            get
            {
                return sciChart.RenderableSeries.Count > 0;
            }
        }

        private IRange _xAxisRange;
        private IRange _yAxisRange;
        private IRange _zAxisRange;

        public IRange XAxisRange
        {
            get
            {
                return _xAxisRange;
            }
            set
            {
                if ( _xAxisRange != value )
                {
                    _xAxisRange = value;
                    OnPropertyChanged( "XAxisRange" );
                }
            }
        }

        public IRange YAxisRange
        {
            get
            {
                return _yAxisRange;
            }
            set
            {
                if ( _yAxisRange != value )
                {
                    _yAxisRange = value;
                    OnPropertyChanged( "YAxisRange" );
                }
            }
        }

        public IRange ZAxisRange
        {
            get
            {
                return _zAxisRange;
            }
            set
            {
                if ( _zAxisRange != value )
                {
                    _zAxisRange = value;
                    OnPropertyChanged( "ZAxisRange" );
                }
            }
        }

        public Surface()
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
            </LicenseContract>" );
            //SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
            //<Customer>HUMEN INC</Customer>
            //<OrderId>ABT180824-6591-16103</OrderId>
            //<LicenseCount>1</LicenseCount>
            //<IsTrialLicense>false</IsTrialLicense>
            //<SupportExpires>08/24/2019 00:00:00</SupportExpires>
            //<ProductCode>SC-WPF-SDK-ENTERPRISE-SRC</ProductCode>
            //<KeyCode>lwAAAAEAAACfOArucTvUAXUAQ3VzdG9tZXI9SFVNRU4gSU5DO09yZGVySWQ9QUJUMTgwODI0LTY1OTEtMTYxMDM7U3Vic2NyaXB0aW9uVmFsaWRUbz0yNC1BdWctMjAxOTtQcm9kdWN0Q29kZT1TQy1XUEYtU0RLLUVOVEVSUFJJU0UtU1JDAEhyOOgf4VGIFWsSYM3pcvwC/wDfDVvKNfvmtI3mSY77keeN/QvfDCWcLV/kuY8O</KeyCode>
            //</LicenseContract>");

            InitializeComponent();

            //XAx3D.TextFormatting = "#";
            //YAx3D.TextFormatting = "#";
            YAx3D.GrowBy = new DoubleRange( 0.1, 0.1 );
            //ZAx3D.TextFormatting = "#";

            scatterRenderableSeries.PointMarker = (BasePointMarker3D)Activator.CreateInstance( (Type)( typeof( SpherePointMarker3D ) ) );
            scatterRenderableSeries.PointMarker.Fill = Colors.Pink;
            scatterRenderableSeries.PointMarker.Size = 5.0f;
            scatterRenderableSeries.PointMarker.Opacity = 1.0f;

            sciChart.Camera.OrthoHeight = 1000.0f;
            sciChart.Camera.OrthoWidth = 1000.0f;

            sciChart.Camera.FieldOfView = 100.0f;

            _dH = (float)sciChart.Camera.OrthoHeight / 10;
            _dW = (float)sciChart.Camera.OrthoWidth / 10;
            _maxHeight = (float)sciChart.Camera.OrthoHeight * 2;
            _maxWidth = (float)sciChart.Camera.OrthoWidth * 2;

            uiROIBtn.IsEnabled = false;
            uiLineBtn.IsEnabled = false;
            uiHColorBtn.IsEnabled = false;
            uiRulerBtn.IsEnabled = false;
            uiSetBtn.IsEnabled = false;

            XAxisRange = new DoubleRange( 0, 2048 );
            YAxisRange = new DoubleRange( -50, 50 );
            ZAxisRange = new DoubleRange( 0, 2048 );

            this.DataContext = this;
        }

        protected virtual void OnPropertyChanged( string propertyName )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if ( handler != null )
                handler( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public void Init( int row, int col )
        {
            xSize = row;
            zSize = col;
            yMax = float.MinValue;

            if ( positionInfo == null )
            {
                positionInfo = new List<PositionInfo>();
            }
            else
            {
                positionInfo.Clear();
            }

            if ( drawInfo == null )
            {
                drawInfo = new List<DrawGeometry>();
            }
            else
            {
                drawInfo.Clear();
            }

            if ( MeshDataSeries == null )
            {
                MeshDataSeries = new UniformGridDataSeries3D<double, double, double>( xSize, zSize );
            }
            else
            {
                MeshDataSeries.Clear();
            }

            if ( marker == null )
            {
                marker = new XyzDataSeries3D<double>();
                marker.DataSeriesChanged += OnScatterSelected;
            }
            else
            {
                marker.Clear();
            }

            movePoint = false;
            annotIdx = -1; // 0 base            
            selectedIdx = -1;
        }

        public void ApplyMinMaxCalc( float[] input )
        {
            float zMin = input.Min();
            float zMax = input.Max();

            Parallel.For( 0, xSize - 1, x =>
             {
                 for ( int z = 0; z < zSize; z++ )
                 {
                     input[ x * 2048 + zSize ] -= zMin;
                     input[ x * 2048 + zSize ] *= ( zMax - zMin );
                     MeshDataSeries[ z, x ] = input[ x * 2048 + zSize ];
                 }
             } );
        }

        public void ApplyMinMaxCalc( cv.Mat input )
        {
            double zMax = double.MinValue;
            double zMin = double.MaxValue;

            for ( int x = 0; x < xSize; ++x )
            {
                for ( int z = 0; z < zSize; ++z )
                {
                    if ( zMax < input.At<double>( x, z ) )
                        zMax = input.At<double>( x, z );

                    if ( zMin > input.At<double>( x, z ) )
                        zMin = input.At<double>( x, z );
                }
            }

            for ( int x = 0; x < xSize; ++x )
            {
                for ( int z = 0; z < zSize; ++z )
                {
                    input.Set<float>( x, z, input.At<float>( x, z ) - (float)zMin );
                    input.Set<float>( x, z, input.At<float>( x, z ) * 1000.0f / (float)( zMax - zMin ) );
                    MeshDataSeries[ z, x ] = input.Get<float>( x, z );
                }
            }
        }

        public Coords ConvertCoordToVec( Coords coord )
        {
            coord.x = ( coord.x - xSize / 2 ) * 2 / xSize * sciChart.WorldDimensions.X / 2;
            coord.y = coord.y / yMax * sciChart.WorldDimensions.Y;
            coord.z = ( coord.z - zSize / 2 ) * 2 / zSize * sciChart.WorldDimensions.Z / 2;

            return coord;
        }

        public bool IsHitPointValid( HitTestInfo3D hitVertex )
        {
            return !hitVertex.IsEmpty() && hitVertex.IsHit;
        }

        public double GetLength()
        {
            return Math.Sqrt( Math.Pow( positionInfo[ annotIdx ].points[ 1 ].x - positionInfo[ annotIdx ].points[ 0 ].x, 2 ) + Math.Pow( positionInfo[ annotIdx ].points[ 1 ].z - positionInfo[ annotIdx ].points[ 0 ].z, 2 ) );
        }

        public double GetWidth()
        {
            return Math.Abs( positionInfo[ annotIdx ].points[ 1 ].x - positionInfo[ annotIdx ].points[ 0 ].x );
        }

        public double GetHeight()
        {
            return Math.Abs( positionInfo[ annotIdx ].points[ 1 ].z - positionInfo[ annotIdx ].points[ 0 ].z );
        }

        public double GetRadius2P()
        {
            return GetLength();
        }

        public double[] GetRadiusCenter3P()
        {
            int dataSize = positionInfo[ annotIdx ].points.Count;
            double a, b, r;

            cv.Mat A = cv.Mat.Zeros( dataSize, 3, cv.MatType.CV_64FC1 );
            cv.Mat B = cv.Mat.Zeros( dataSize, 1, cv.MatType.CV_64FC1 );
            cv.Mat X = cv.Mat.Zeros( dataSize, 1, cv.MatType.CV_64FC1 );

            for ( int i = 0; i < dataSize; i++ )
            {
                A.Set<double>( i, 0, positionInfo[ annotIdx ].points[ i ].x );
                A.Set<double>( i, 1, positionInfo[ annotIdx ].points[ i ].z );
                A.Set<double>( i, 2, 1 );
                B.Set<double>( i, -Math.Pow( positionInfo[ annotIdx ].points[ i ].x, 2 ) - Math.Pow( positionInfo[ annotIdx ].points[ i ].z, 2 ) );
            }

            cv.Cv2.Solve( A, B, X, cv.DecompTypes.SVD );

            a = -X.At<double>( 0, 0 ) / 2;
            b = -X.At<double>( 1, 0 ) / 2;
            r = Math.Sqrt( Math.Pow( a, 2 ) + Math.Pow( b, 2 ) - X.At<double>( 2, 0 ) );

            double[] data = { a, b, r };

            return data;
        }

        public void ClearPoints()
        {
            if ( positionInfo == null || drawInfo == null || marker == null )
                return;

            positionInfo.Clear();
            for ( int i = 0; i < drawInfo.Count; i++ )
            {
                sciChart.Viewport3D.RootEntity.Children.Remove( drawInfo[ i ] );
            }
            drawInfo.Clear();
            marker.Clear();
            scatterRenderableSeries.DataSeries = null;
            annotIdx = -1;

            if ( sciChart.RenderableSeries.Any() )
            {
                var rSeries = sciChart.RenderableSeries.LastOrDefault();

                if ( rSeries == null || rSeries.DataSeries == null )
                    return;

                //sciChart.RenderableSeries.Remove(rSeries);

                MeshDataSeries.Clear();

                OnPropertyChanged( "CanAddSeries" );
                OnPropertyChanged( "CanRemoveSeries" );

                //sciChart.ZoomExtents();
                sciChart.ZoomExtentsX();
                sciChart.ZoomExtentsZ();
            }
        }

        private void SurfaceMouseUp( object sender, MouseButtonEventArgs e )
        {
            if ( positionInfo.Count < 1 )
            {
                return;
            }

            // using SciChart.Charting3D.RenderableSeries
            HitTestInfo3D hitVertex2 = backgroundSurfaceMesh.HitTest( e.GetPosition( sciChart ) );
            HitTestInfo3D hitVertex = surfaceMeshRenderableSeries.HitTest( e.GetPosition( sciChart ) );
            var xyzInfo2 = backgroundSurfaceMesh.ToSeriesInfo( hitVertex2 );
            var xyzInfo = surfaceMeshRenderableSeries.ToSeriesInfo( hitVertex );



            if ( IsHitPointValid( hitVertex ) && !drawInfo.IsNullOrEmptyList() )
            {
                if ( movePoint && selectedIdx > -1 )
                {
                    int idx = sciChart.Viewport3D.RootEntity.Children.Count; // sciChart.Viewport3D.RootEntity.Children.Count?

                    if ( positionInfo[ selectedIdx / 2 ].points[ 0 ].y > positionInfo[ selectedIdx / 2 ].points[ 1 ].y )
                    {
                        marker.Update( selectedIdx, xyzInfo.HitVertex.X, positionInfo[ selectedIdx / 2 ].points[ 0 ].y + 25, xyzInfo.HitVertex.Z );
                    }
                    else
                    {
                        marker.Update( selectedIdx, xyzInfo.HitVertex.X, positionInfo[ selectedIdx / 2 ].points[ 1 ].y + 25, xyzInfo.HitVertex.Z );
                    }

                    Coords tt = positionInfo[ selectedIdx / 2 ].points[ selectedIdx % 2 ];
                    tt.x = xyzInfo.HitVertex.X;
                    tt.y = xyzInfo.HitVertex.Y;
                    tt.z = xyzInfo.HitVertex.Z;

                    positionInfo[ selectedIdx / 2 ].points[ selectedIdx % 2 ] = tt;
                    tt = ConvertCoordToVec( tt );

                    if ( selectedIdx % 2 == 0 )
                    {
                        drawInfo[ selectedIdx / 2 ].firstPos.X = tt.x;
                        drawInfo[ selectedIdx / 2 ].firstPos.Y = tt.y;
                        drawInfo[ selectedIdx / 2 ].firstPos.Z = tt.z;
                    }
                    else
                    {
                        drawInfo[ selectedIdx / 2 ].secondPos.X = tt.x;
                        drawInfo[ selectedIdx / 2 ].secondPos.Y = tt.y;
                        drawInfo[ selectedIdx / 2 ].secondPos.Z = tt.z;
                    }

                    drawInfo[ selectedIdx / 2 ].Update();

                    movePoint = false;
                    selectedIdx = -1;
                }
            }

            //if (drawInfo.Count > 0)
            //{
            //    MessageBox.Show(e.GetPosition(sciChart).X.ToString() + ", " + e.GetPosition(sciChart).Y.ToString());
            //    SciChart.Charting3D.Primitives.HeightMapIndex t = new SciChart.Charting3D.Primitives.HeightMapIndex();
            //    t.XIndex = 647;
            //    t.ZIndex = 375;
            //    hitVertex.EntityId = 7;
            //    hitVertex.IjIndices = t;
            //    hitVertex.IsHit = true;
            //    hitVertex.VertexId = 0;
            //
            //    //var tt = surfaceMeshRenderableSeries.HitTest(new Point(647, 375));
            //    var tt = surfaceMeshRenderableSeries.ToSeriesInfo(hitVertex);
            //}

            if ( IsHitPointValid( hitVertex ) && positionInfo[ annotIdx ].checkShape != ECheckShape.None )
            {
                PositionInfo temp = positionInfo[ annotIdx ];

                temp.points.Add( new Coords( xyzInfo.HitVertex.X, xyzInfo.HitVertex.Y, xyzInfo.HitVertex.Z ) );

                // 마우스 따라가는 거 필요함                

                if ( temp.points.Count == 2 && temp.checkShape != ECheckShape.Circle_3P )
                {
                    PositionEventArgs args = new PositionEventArgs();

                    switch ( temp.checkShape )
                    {
                        case ECheckShape.Line:
                            Coords[] LineCoords = { temp.points[ 0 ], temp.points[ 1 ] };
                            temp.length = GetLength();
                            LineCoords[ 0 ] = ConvertCoordToVec( temp.points[ 0 ] );
                            LineCoords[ 1 ] = ConvertCoordToVec( temp.points[ 1 ] );
                            drawInfo.Add( new DrawGeometry( new Vector3( LineCoords[ 0 ].x, LineCoords[ 0 ].y, LineCoords[ 0 ].z ), new Vector3( LineCoords[ 1 ].x, LineCoords[ 1 ].y, LineCoords[ 1 ].z ), Color.FromArgb( 128, 0, 0, 255 ), ECheckShape.Line ) );
                            sciChart.Viewport3D.RootEntity.Children.Add( drawInfo[ annotIdx ] );
                            AddMarker( temp );
                            args.Data = temp;
                            OnMeasureData( args );
                            break;
                        case ECheckShape.Rect:
                            Coords[] rectCoords = { temp.points[ 0 ], temp.points[ 1 ] };
                            temp.width = GetWidth();
                            temp.height = GetHeight();
                            rectCoords[ 0 ] = ConvertCoordToVec( temp.points[ 0 ] );
                            rectCoords[ 1 ] = ConvertCoordToVec( temp.points[ 1 ] );
                            drawInfo.Add( new DrawGeometry( new Vector3( rectCoords[ 0 ].x, rectCoords[ 0 ].y, rectCoords[ 0 ].z ), new Vector3( rectCoords[ 1 ].x, rectCoords[ 1 ].y, rectCoords[ 1 ].z ), Color.FromArgb( 128, 255, 0, 0 ), ECheckShape.Rect ) );
                            sciChart.Viewport3D.RootEntity.Children.Add( drawInfo[ annotIdx ] );
                            AddMarker( temp );
                            args.Data = temp;
                            OnMeasureData( args );
                            break;
                        case ECheckShape.Circle_2P:
                            Coords[] circleCoords = { temp.points[ 0 ], temp.points[ 1 ] };
                            temp.radius = GetRadius2P();
                            circleCoords[ 0 ] = ConvertCoordToVec( temp.points[ 0 ] );
                            circleCoords[ 1 ] = ConvertCoordToVec( temp.points[ 1 ] );
                            drawInfo.Add( new DrawGeometry( new Vector3( circleCoords[ 0 ].x, circleCoords[ 0 ].y, circleCoords[ 0 ].z ), new Vector3( circleCoords[ 1 ].x, circleCoords[ 1 ].y, circleCoords[ 1 ].z ), Color.FromArgb( 255, 255, 0, 0 ), ECheckShape.Circle_2P ) );
                            sciChart.Viewport3D.RootEntity.Children.Add( drawInfo[ annotIdx ] );
                            break;
                        default:
                            break;
                    }
                }
                else if ( temp.points.Count == 3 )
                {
                    double[] ret = GetRadiusCenter3P();

                    temp.centerPoints.x = (float)ret[ 0 ];
                    temp.centerPoints.y = xyzInfo.HitVertex.Y;
                    temp.centerPoints.z = (float)ret[ 1 ];
                    temp.radius = ret[ 2 ];
                }

                positionInfo[ annotIdx ] = temp;
            }
        }

        public void AddLine()
        {
            positionInfo.Add( new PositionInfo( ECheckShape.Line ) );
        }

        public void AddRect()
        {
            positionInfo.Add( new PositionInfo( ECheckShape.Rect ) );
        }

        public void AddCircle2P()
        {
            positionInfo.Add( new PositionInfo( ECheckShape.Circle_2P ) );
        }

        public void AddCircle3P()
        {
            positionInfo.Add( new PositionInfo( ECheckShape.Circle_3P ) );
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            String ButtonName = ( (Button)sender ).Name;

            switch ( ButtonName )
            {
                case "LoadBtn":
                    LoadImage();
                    sciChart.ZoomExtents();
                    break;
                case "DrawLineBtn":
                    AddLine();
                    break;
                case "DrawRectBtn":
                    AddRect();
                    break;
                case "DrawCircle2PBtn":
                    AddCircle2P();
                    break;
                case "DrawCircle3PBtn":
                    AddCircle3P();
                    break;
                case "uiClearBtn":
                    Clear();
                    break;
                case "uiZoomInBtn":
                    if ( sciChart.Camera.OrthoHeight > _dH &&
                       sciChart.Camera.OrthoWidth > _dW )
                    {
                        sciChart.Camera.OrthoHeight -= _dH;
                        sciChart.Camera.OrthoWidth -= _dW;
                    }
                    else
                    {
                        sciChart.Camera.OrthoHeight = 0.0f;
                        sciChart.Camera.OrthoWidth = 0.0f;
                    }

                    break;
                case "uiZoomOutBtn":
                    if ( _maxHeight > sciChart.Camera.OrthoHeight + _dH &&
                       _maxWidth > sciChart.Camera.OrthoWidth + _dW )
                    {
                        sciChart.Camera.OrthoHeight += _dH;
                        sciChart.Camera.OrthoWidth += _dW;
                    }
                    else
                    {
                        sciChart.Camera.OrthoHeight = _maxHeight;
                        sciChart.Camera.OrthoWidth = _maxWidth;
                    }

                    break;
                case "uiZoomFitBtn":
                    sciChart.Camera.OrthoHeight = _maxHeight / 2;
                    sciChart.Camera.OrthoWidth = _maxWidth / 2;
                    //sciChart.ZoomExtents();
                    sciChart.ZoomExtentsX();
                    sciChart.ZoomExtentsZ();
                    break;
                case "uiTopViewBtn":
                    sciChart.Camera.Position.X = 0.0f;
                    sciChart.Camera.Position.Y = 1000.0f;
                    sciChart.Camera.Position.Z = 0.0f;
                    sciChart.Camera.Target.X = 0.0f;
                    sciChart.Camera.Target.Y = 10.0f;
                    sciChart.Camera.Target.Z = 0.0f;
                    sciChart.Camera.OrbitalPitch = 90.0f;
                    sciChart.Camera.OrbitalYaw = 180.0f;
                    sciChart.Camera.Radius = 800.0f;
                    sciChart.UpdateLayout();
                    break;
                default:
                    break;
            }

            if ( ButtonName == "LoadBtn" || ButtonName == "ClearBtn" )
            {
                return;
            }

            annotIdx++;

        }

        public void SetData( float[] input, int row, int col )
        {
            //if (xSize != -1 || zSize != -1)
            //{
            //    surfaceMeshRenderableSeries.DataSeries.Clear();
            //}

            Init( row, col );

            double min = 0.0f, max = 0.0f;
            min = input.Min();
            max = input.Max();

            Parallel.For( 0, xSize, x =>
             {
                 for ( int z = 0; z < zSize; ++z )
                 {
                     input[ x * row + z ] = input[ x * row + z ] + (float)Math.Abs( min );
                     input[ x * row + z ] = input[ x * row + z ] * 255.0f / (float)( max - min );

                     if ( yMax < input[ x * row + z ] )
                     {
                         yMax = input[ x * row + z ];
                     }
                 }
             } );

            // 값을 맞춘다...

            Parallel.For( 0, xSize, x =>
             {
                 for ( int z = 0; z < zSize; ++z )
                 {
                     MeshDataSeries[ z, x ] = input[ x * row + z ];

                     if ( yMax < input[ x * row + z ] )
                     {
                         yMax = input[ x * row + z ];
                     }
                 }
             } );


            min = input.Min();
            max = input.Max();

            surfaceMeshRenderableSeries.Maximum = max;
            surfaceMeshRenderableSeries.Minimum = min;

            if ( YAx3D.VisibleRange != null )
            {
                if ( min > (double)YAx3D.VisibleRange.Max )
                {
                    YAx3D.VisibleRange.Max = max;
                    YAx3D.VisibleRange.Min = min;
                }
                else
                {
                    YAx3D.VisibleRange.Min = min;
                    YAx3D.VisibleRange.Max = max;
                }
            }
        }

        public void SetData( cv.Mat input )
        {
            //if (xSize != -1 || zSize != -1)
            //{
            //    surfaceMeshRenderableSeries.DataSeries.Clear();
            //}

            zMap = new cv.Mat( new cv.Size( input.Width, input.Height ), cv.MatType.CV_32FC1 );
            input.ConvertTo( zMap, cv.MatType.CV_32FC1 );
            //Init(zMap.Rows, zMap.Cols);
            Init( zMap.Rows, zMap.Cols );

            Parallel.For( 0, xSize, x =>
             {
                 for ( int z = 0; z < zSize; ++z )
                 {
                     MeshDataSeries[ z, x ] = zMap.At<float>( x, z );

                     if ( yMax < zMap.Get<float>( x, z ) )
                     {
                         yMax = zMap.Get<float>( x, z );
                     }
                 }
             } );

            double min = 0.0f, max = 0.0f;
            zMap.MinMaxLoc( out min, out max );
            surfaceMeshRenderableSeries.Maximum = max;
            surfaceMeshRenderableSeries.Minimum = min;
            //backgroundSurfaceMesh.IsVisible = false;
        }

        public void SetData( cv.Mat input, float minValue )
        {
            //if (xSize != -1 || zSize != -1)
            //{
            //    surfaceMeshRenderableSeries.DataSeries.Clear();
            //}

            zMap = new cv.Mat( new cv.Size( input.Width, input.Height ), cv.MatType.CV_32FC1 );
            input.ConvertTo( zMap, cv.MatType.CV_32FC1 );
            //Init(zMap.Rows, zMap.Cols);
            Init( zMap.Rows, zMap.Cols );

            Parallel.For( 0, xSize, x =>
            {
                for ( int z = 0; z < zSize; ++z )
                {
                    MeshDataSeries[ z, x ] = zMap.At<float>( x, z );

                    if ( yMax < zMap.Get<float>( x, z ) )
                    {
                        yMax = zMap.Get<float>( x, z );
                    }
                }
            } );

            double min = 0.0f, max = 0.0f;
            zMap.MinMaxLoc( out min, out max );
            min = minValue;
            surfaceMeshRenderableSeries.Maximum = max;
            surfaceMeshRenderableSeries.Minimum = min;
            //backgroundSurfaceMesh.IsVisible = false;
        }

        public void AddMarker( PositionInfo position )
        {
            if ( position.checkShape == ECheckShape.Line )
            {
                marker.Append( position.points[ 0 ].x, position.points[ 0 ].y + 25, position.points[ 0 ].z, new PointMetadata3D( Colors.Pink, 1.0f ) );
                marker.Append( position.points[ 1 ].x, position.points[ 1 ].y + 25, position.points[ 1 ].z, new PointMetadata3D( Colors.Pink, 1.0f ) );
            }
            else if ( position.checkShape == ECheckShape.Rect )
            {
                marker.Append( position.points[ 0 ].x, position.points[ 0 ].y + 25, position.points[ 0 ].z, new PointMetadata3D( Colors.Pink, 1.0f ) );
                marker.Append( position.points[ 1 ].x, position.points[ 0 ].y + 25, position.points[ 0 ].z, new PointMetadata3D( Colors.Pink, 1.0f ) );
                marker.Append( position.points[ 1 ].x, position.points[ 1 ].y + 25, position.points[ 1 ].z, new PointMetadata3D( Colors.Pink, 1.0f ) );
                marker.Append( position.points[ 0 ].x, position.points[ 1 ].y + 25, position.points[ 1 ].z, new PointMetadata3D( Colors.Pink, 1.0f ) );
            }

            scatterRenderableSeries.DataSeries = marker;
        }

        private void LoadImage()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "bmp Images (*.bmp)|*.bmp|All Files (*.*)|*.*";
            Nullable<bool> result = dlg.ShowDialog();

            if ( result == true )
            {
                cv.Mat img = new cv.Mat( dlg.FileName, cv.ImreadModes.Grayscale );
                SetData( img );
            }
        }

        protected virtual void OnMeasureData( PositionEventArgs e )
        {
            EventHandler<PositionEventArgs> handler = PositionEvent;
            if ( handler != null )
            {
                handler( this, e );
            }
        }

        private void OnScatterSelected( object sender, DataSeriesChangedEventArgs e )
        {
            for ( int i = 0; i < marker.Count; i++ )
            {
                if ( marker.WValues[ i ] != null && marker.WValues[ i ].IsSelected )
                {
                    selectedIdx = i;
                    movePoint = true;
                    break;
                }
            }
        }

        public void Clear()
        {
            if ( sciChart.RenderableSeries == null )
                return;

            if ( sciChart.RenderableSeries.Any() )
            {
                var rSeries = sciChart.RenderableSeries.LastOrDefault();

                if ( rSeries == null || rSeries.DataSeries == null )
                    return;

                MeshDataSeries.Clear();
                sciChart.RenderableSeries.Remove( rSeries );

                OnPropertyChanged( "CanAddSeries" );
                OnPropertyChanged( "CanRemoveSeries" );

                sciChart.ZoomExtents();
            }
        }
    }
}
