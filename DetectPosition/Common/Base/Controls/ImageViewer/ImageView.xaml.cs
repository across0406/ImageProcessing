using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Threading;
using cv = OpenCvSharp;

namespace Base.Controls.ImageViewer
{
    /// <summary>
    /// ImageView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageView : UserControl
    {
        #region Private Member Constants

        // Max => 512 MB
        private const int _MAX_SIZE = 536870912;

        #endregion Private Member Constants

        #region Private Member Variables

        // Should be checked for MVVM pattern.
        // private ImageViewModel _viewModel;

        private int _scale;
        private double _zoomRatio;
        private WriteableBitmap _wb;
        private cv.Mat _imageOrigin;
        private Dictionary<string, DisplayInfo> _displayInfoDictionary;
        private string _viewInfo;
        private bool _isShowROI;

        #endregion Private Member Variables

        #region Private Methods

        private void ShowPixelInfo( System.Windows.Point point )
        {
            if ( ImageOrigin == null )
            {
                return;
            }

            cv.Mat.Indexer<cv.Vec3b> indexer = ImageOrigin.GetGenericIndexer<cv.Vec3b>();
            cv.Vec3b color = indexer[ (int)( point.Y * Scale ), (int)( point.X * Scale ) ];

            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                ViewInfo = string.Format(
                    "X:{0:0.0}\t\tY:{1:0.0}\t\tR:{2}\tG:{3}\tB:{4}\t{5}",
                point.X * Scale, point.Y * Scale,
                color.Item2, color.Item1, color.Item0, Scale );
                uiViewInfo.Text = ViewInfo;
            } ), DispatcherPriority.ApplicationIdle );
        }

        private void SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desiredCursor = Cursors.Arrow;

            // Tracker
            // switch ( MouseHitType )
            // {
            //     case HitType.Body:
            //     case HitType.ST:
            //     case HitType.ED:
            //         desiredCursor = Cursors.ScrollAll;
            //         break;
            // 
            //     case HitType.T:
            //     case HitType.B:
            //         desiredCursor = Cursors.SizeNS;
            //         break;
            // 
            //     case HitType.L:
            //     case HitType.R:
            //         desiredCursor = Cursors.SizeWE;
            //         break;
            // }
            // 
            // // Measure
            // switch ( MeasureHitType )
            // {
            //     case HitMeasureType.Pt:
            //         desiredCursor = Cursors.ScrollAll;
            //         break;
            // }

            // Display the desired cursor.
            if ( Cursor != desiredCursor )
            {
                Cursor = desiredCursor;
            }
        }

        private bool SaveImage()
        {
            bool saveImageResult = false;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "BMP Images (*.bmp)|*.bmp";
            Nullable<bool> result = dialog.ShowDialog();

            if ( result == true )
            {
                saveImageResult = ImageOrigin.SaveImage( dialog.FileName );
            }
            else
            {
                saveImageResult = false;
            }

            return saveImageResult;
        }

        private bool LoadImage()
        {
            bool loadImageResult = false;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "BMP Images (*.bmp)|*.bmp";
            Nullable<bool> result = dialog.ShowDialog();

            if ( result == true )
            {
                try
                {
                    cv.Mat image = new cv.Mat( dialog.FileName );

                    ClearOverlay();
                    SetViewImage( image );
                    _UpdateView();
                    ZoomFit();

                    loadImageResult = true;
                }
                catch ( Exception ex )
                {
                    // Log
                    Console.WriteLine( ex.ToString() );
                    loadImageResult = false;
                }
            }
            else
            {
                loadImageResult = false;
            }

            return loadImageResult;
        }

        private System.Windows.Rect CheckRect( System.Windows.Rect input )
        {
            System.Windows.Rect interRect = new System.Windows.Rect();
            interRect.X = Math.Max( input.X, 0 );
            interRect.Y = Math.Max( input.Y, 0 );
            interRect.Width = Math.Min( input.Width, ImageOrigin.Width / Scale );
            interRect.Height = Math.Min( input.Height, ImageOrigin.Height / Scale );

            return interRect;
        }

        private void _UpdateView()
        {
            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                cv.Mat displayImage = new cv.Mat();

                if ( Scale != 0 )
                {
                    cv.Cv2.Resize( ImageOrigin, displayImage, new cv.Size( ImageOrigin.Width / Scale, ImageOrigin.Height / Scale ) );
                }

                if ( displayImage.Width != uiViewImage.Width || displayImage.Height != uiViewImage.Height )
                {
                    if ( displayImage.Channels() > 1 )
                    {
                        Wb = new WriteableBitmap( displayImage.Width, displayImage.Height, 96, 96, PixelFormats.Bgr24, null );	// color                        
                    }
                    else
                    {
                        Wb = new WriteableBitmap( displayImage.Width, displayImage.Height, 96, 96, PixelFormats.Gray8, null );	// Gray
                    }

                    try
                    {
                        OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( displayImage, Wb );
                        uiViewGrid.Width = Wb.Width;
                        uiViewGrid.Height = Wb.Height;
                        uiViewGrid.UpdateLayout();
                    }
                    catch ( Exception ex )
                    {
                        Logging.Instance.Error( "Base.ImageView -> _UpdateView : " + ex.Message );
                        // Console.WriteLine( "HMLib -> ImageView : " + ex.ToString() );
                    }
                }

                DrawingGroup drawingGroup = new DrawingGroup();
                ImageDrawing drawingImage = new ImageDrawing( Wb, new System.Windows.Rect( 0, 0, Wb.Width, Wb.Height ) );
                drawingGroup.Children.Add( drawingImage );

                Geometry overlay = null;
                DisplayInfo display;
                SolidColorBrush brush;
                System.Windows.Rect position;

                if ( DisplayInfoDictionary != null )
                {
                    foreach ( KeyValuePair<string, DisplayInfo> item in DisplayInfoDictionary )
                    {
                        display = item.Value;
                        brush = new SolidColorBrush( display.ColorInfo );
                        position = CheckRect( display.Position );

                        switch ( display.OverlayTypeInfo )
                        {
                            case EOverlayType.TEXT:
                                FormattedText formattedText = new FormattedText(
                                    display.DisplayText,
                                    CultureInfo.GetCultureInfo( "en-us" ),
                                    FlowDirection.LeftToRight,
                                    new Typeface( "Arial" ),
                                    display.Thickness,
                                    brush, Scale );
                                overlay = formattedText.BuildGeometry( new System.Windows.Point( position.X, position.Y ) );
                                break;

                            default:
                            case EOverlayType.POINT:
                                overlay = new LineGeometry(
                                    new System.Windows.Point( position.X, position.Y ),
                                    new System.Windows.Point( position.X + 1, position.Y + 1 ) );
                                break;

                            case EOverlayType.LINE:
                                overlay = new LineGeometry(
                                    new System.Windows.Point( position.X, position.Y ),
                                    new System.Windows.Point( position.Width, position.Height ) );
                                break;

                            case EOverlayType.RECT:
                                overlay = new RectangleGeometry( position );
                                break;

                            case EOverlayType.CIRCLE:
                                overlay = new EllipseGeometry(
                                    new System.Windows.Point( ( position.Left + position.Right ) / 2,
                                    ( position.Top + position.Bottom ) / 2 ), position.Width / 2, position.Height / 2 );
                                break;

                            case EOverlayType.ELLIPSE:
                                overlay = new EllipseGeometry(
                                    new System.Windows.Point( ( position.Left + position.Right ) / 2,
                                    ( position.Top + position.Bottom ) / 2 ), position.Width / 2, position.Height / 2 );
                                break;
                        }

                        if ( display.OverlayTypeInfo == EOverlayType.TEXT )
                        {
                            drawingGroup.Children.Add( new GeometryDrawing( brush, null, overlay ) );
                        }
                        else
                        {
                            if ( display.Thickness == 0 )
                            {
                                drawingGroup.Children.Add( new GeometryDrawing( brush, new Pen( brush, display.Thickness ), overlay ) );
                            }
                            else
                            {
                                drawingGroup.Children.Add( new GeometryDrawing( null, new Pen( brush, display.Thickness ), overlay ) );
                            }
                        }
                    }
                }

                uiViewImage.Source = new DrawingImage( drawingGroup );
            } ), DispatcherPriority.ApplicationIdle );
        }

        #endregion Private Methods

        #region Private Events

        private void ZoomInClick( object sender, RoutedEventArgs e )
        {
            ZoomRatio *= 2.0;
            Zoom( ZoomRatio );
        }

        private void ZoomOutClick( object sender, RoutedEventArgs e )
        {
            ZoomRatio /= 2.0;
            Zoom( ZoomRatio );
        }

        private void ZoomFitClick( object sender, RoutedEventArgs e )
        {
            ZoomFit();
        }

        private void ROIClick( object sender, RoutedEventArgs e )
        {

        }

        private void CrossLineClick( object sender, RoutedEventArgs e )
        {

        }

        private void ViewMouseMove( object sender, MouseEventArgs e )
        {
            if ( _isShowROI )
            {
                TrackerMouseMove( sender, e );
            }

            MeasureMouseMove( sender, e );

            System.Windows.Point point = Mouse.GetPosition( uiViewCanvas );
            ShowPixelInfo( point );

            SetMouseCursor();
        }

        private void ViewMouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( _isShowROI )
            {
                TrackerMouseDown( sender, e );
            }

            MeasureMouseDown( sender, e );
        }

        private void ViewMouseUp( object sender, MouseButtonEventArgs e )
        {
            if ( _isShowROI )
            {
                TrackerMouseUp( sender, e );
            }

            MeasureMouseUp( sender, e );
        }

        private void TrackerMouseDown( object sender, MouseEventArgs e )
        {
            // FindHit( Mouse.GetPosition( uiViewCanvas ) );
            // if ( MouseHitType == HitType.None )
            // {
            //     return;
            // }
            // 
            // Mouse.Capture( uiViewCanvas );
            // LastPoint = Mouse.GetPosition( uiViewCanvas );
            // bDragInProgress = true;
            // ShowControlRect();
        }

        private void TrackerMouseMove( object sender, MouseEventArgs e )
        {
            // if ( !bDragInProgress )
            // {
            //     FindHit( Mouse.GetPosition( uiViewCanvas ) );
            // }
            // else
            // {
            //     Point point = Mouse.GetPosition( uiViewCanvas );
            //     if ( HitTracker.type == TrackType.LINE )
            //     {
            //         switch ( MouseHitType )
            //         {
            //             case HitType.ST:
            //                 HitTracker.line.X1 = point.X;
            //                 HitTracker.line.Y1 = point.Y;
            //                 break;
            // 
            //             case HitType.ED:
            //                 HitTracker.line.X2 = point.X;
            //                 HitTracker.line.Y2 = point.Y;
            //                 break;
            //         }
            // 
            //         // Save the mouse's new location.
            //         LastPoint = point;
            // 
            //         ShowControlRect();
            //     }
            //     else if ( HitTracker.type == TrackType.RECT )
            //     {
            //         // See how much the mouse has moved.
            //         double offset_x = point.X - LastPoint.X;
            //         double offset_y = point.Y - LastPoint.Y;
            // 
            //         // Get the rectangle's current position.
            //         double new_x = Canvas.GetLeft( HitTracker.rect );
            //         double new_y = Canvas.GetTop( HitTracker.rect );
            //         double new_width = HitTracker.rect.Width;
            //         double new_height = HitTracker.rect.Height;
            // 
            //         // Update the rectangle.
            //         switch ( MouseHitType )
            //         {
            //             case HitType.Body:
            //                 new_x += offset_x;
            //                 new_y += offset_y;
            //                 break;
            // 
            //             case HitType.L:
            //                 new_x += offset_x;
            //                 new_width -= offset_x;
            //                 break;
            // 
            //             case HitType.R:
            //                 new_width += offset_x;
            //                 break;
            // 
            //             case HitType.B:
            //                 new_height += offset_y;
            //                 break;
            // 
            //             case HitType.T:
            //                 new_y += offset_y;
            //                 new_height -= offset_y;
            //                 break;
            //         }
            // 
            //         // Don't use negative width or height.
            //         if ( ( new_width > 0 ) && ( new_height > 0 ) )
            //         {
            //             // Update the rectangle.
            //             Canvas.SetLeft( HitTracker.rect, new_x );
            //             Canvas.SetTop( HitTracker.rect, new_y );
            //             HitTracker.rect.Width = new_width;
            //             HitTracker.rect.Height = new_height;
            // 
            //             // Save the mouse's new location.
            //             LastPoint = point;
            // 
            //             ShowControlRect();
            //         }
            //     }
            // }
        }

        private void TrackerMouseUp( object sender, MouseEventArgs e )
        {
            // Mouse.Capture( null );
            // bDragInProgress = false;
        }

        private void MeasureMouseDown( object sender, MouseEventArgs e )
        {
            // Point point = Mouse.GetPosition( uiViewCanvas );
            // HitMeasureIndex = FindMeasureHit( point );
            // if ( MeasureHitType == HitMeasureType.None )
            // {
            //     return;
            // }
            // 
            // Mouse.Capture( uiViewCanvas );
            // LastMeasurePoint = point;
            // bDragInMeasure = true;
        }

        private void MeasureMouseMove( object sender, MouseEventArgs e )
        {
            // Point point = Mouse.GetPosition( uiViewCanvas );
            // 
            // HitMeasureIndex = FindMeasureHit( point );
            // if ( Mouse.LeftButton == MouseButtonState.Pressed && HitMeasureIndex != -1 )
            // {
            //     if ( bDragInMeasure )
            //     {
            //         if ( HitMeasureIndex >= 0 && HitMeasureSubIndex >= 0 )
            //         {
            //             m_MeasureData[ HitMeasureIndex ].pt_List[ HitMeasureSubIndex ] = point;
            //         }
            //     }
            // 
            //     MakeRoundRect();
            //     DrawCanvas();
            // }
        }

        private void MeasureMouseUp( object sender, MouseEventArgs e )
        {
            // Mouse.Capture( null );
            // 
            // bDragInMeasure = false;
            // HitMeasureIndex = -1;
            // HitMeasureSubIndex = -1;
            // 
            // // Add Point
            // Point point = Mouse.GetPosition( uiViewCanvas );
            // AddMeasurePt( point );
            // 
            // MakeRoundRect();
            // 
            // // Check Measure Data
            // HitMeasureIndex = FindMeasureHit( point );
            // if ( IsEndMeasure( HitMeasure ) )
            // {
            //     MeasureToolsEventArgs args = new MeasureToolsEventArgs();
            //     args.Data = HitMeasure;
            //     OnMeasureData( args );
            // }
        }

        #endregion Private Events

        #region Constructor

        public ImageView()
        {
            InitializeComponent();

            // Should consider for MVVM pattern.
            // ViewModel = new ImageViewModel();
            // this.DataContext = ViewModel;

            Wb = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            ImageOrigin = new cv.Mat( new cv.Size( 10, 10 ), cv.MatType.CV_8SC3 );
            ViewInfo = string.Empty;
            DisplayInfoDictionary = new Dictionary<string, DisplayInfo>();
            _isShowROI = false;

            Simul3DButton.Visibility = Visibility.Hidden;
            uiROI.IsEnabled = false;
            uiCrossLine.IsEnabled = false;
        }

        #endregion Constructor

        #region Public Properties

        // Should consider for MVVM pattern.
        // public ImageViewModel ViewModel
        // {
        //     get => _viewModel;
        //     set => _viewModel = value;
        // }

        public int Scale
        {
            get => _scale;
            set
            {
                _scale = value;
            }
        }
        public double ZoomRatio
        {
            get => _zoomRatio;
            set
            {
                _zoomRatio = value;
            }
        }

        public WriteableBitmap Wb
        {
            get => _wb;
            set
            {
                _wb = value;
            }
        }

        public cv.Mat ImageOrigin
        {
            get => _imageOrigin;
            set
            {
                _imageOrigin = value.Clone();
                Scale = _imageOrigin.Width * _imageOrigin.Height / _MAX_SIZE + 1;
            }
        }

        public Dictionary<string, DisplayInfo> DisplayInfoDictionary
        {
            get => _displayInfoDictionary;
            set
            {
                _displayInfoDictionary = value;
            }
        }

        public string ViewInfo
        {
            get => _viewInfo;
            set
            {
                _viewInfo = value;
            }
        }

        public Button Simul3DButton
        {
            get
            {
                return ui3D;
            }
        }

        public int OverlayCount
        {
            get
            {
                int overlayCount = 0;

                if ( DisplayInfoDictionary == null )
                {
                    // Log
                    overlayCount = 0;
                }
                else
                {
                    overlayCount = DisplayInfoDictionary.Count;
                }

                return overlayCount;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Zoom()
        {
            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                uiViewGrid.LayoutTransform = new ScaleTransform( ZoomRatio, ZoomRatio );
                uiViewScroll.UpdateLayout();
            } ), DispatcherPriority.ApplicationIdle );
        }

        public void Zoom( double zoomRatio )
        {
            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                ZoomRatio = zoomRatio;
                uiViewGrid.LayoutTransform = new ScaleTransform( ZoomRatio, ZoomRatio );
                uiViewScroll.UpdateLayout();
            } ), DispatcherPriority.ApplicationIdle );
        }

        public void ZoomFit()
        {
            double widthRatio = uiViewScroll.ActualWidth / uiViewGrid.ActualWidth;
            double heightRatio = uiViewScroll.ActualHeight / uiViewGrid.ActualHeight;
            ZoomRatio = widthRatio > heightRatio ? heightRatio : widthRatio;
            Zoom( ZoomRatio );
        }

        public void GetImageValue_Color( Point pt, ref long r, ref long g, ref long b )
        {
            var gv_value = new cv.Vec3b( 0, 0, 0 );

            gv_value = ImageOrigin.Get<cv.Vec3b>( (int)pt.Y, (int)pt.X );
            r = (long)gv_value.Item0;
            g = (long)gv_value.Item1;
            b = (long)gv_value.Item2;
        }

        public void SetViewImage( cv.Mat img )
        {
            ImageOrigin = img.Clone();
        }

        public void SetView( cv.Mat img )
        {
            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                ImageOrigin = img.Clone();
                ClearOverlay();
                _UpdateView();
                ZoomFit();
            } ), DispatcherPriority.ApplicationIdle );
        }

        public void ClearOverlay()
        {
            DisplayInfoDictionary.Clear();
        }

        public void UpdateView()
        {
            _UpdateView();
        }

        public void DeleteOverlay( string key )
        {
            if ( DisplayInfoDictionary == null )
            {
                // Log
                return;
            }

            if ( DisplayInfoDictionary.Count < 1 )
            {
                // Log
                return;
            }

            if ( !DisplayInfoDictionary.Remove( key ) )
            {
                // Log
                return;
            }            
        }

        public void SetAllButtonSize( int size )
        {
            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                uiControl.Width = new GridLength( size );
            } ), DispatcherPriority.ApplicationIdle );
        }

        public void DrawCanvas()
        {
            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                uiViewCanvas.Children.Clear();

                if ( _isShowROI )
                {
                    //_DrawRectTracker();
                }

                //_DrawMeasureTools();
            } ), DispatcherPriority.ApplicationIdle );
        }

        public void ShowROI()
        {
            _isShowROI = !_isShowROI;
            DrawCanvas();
        }

        #endregion Public Methods
    }
}
