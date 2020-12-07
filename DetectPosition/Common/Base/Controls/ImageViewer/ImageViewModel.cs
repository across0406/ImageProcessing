using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.XPhoto;
using cv = OpenCvSharp;

namespace Base.Controls.ImageViewer
{
    public class ImageViewModel : Base.BasePropertyChanged
    {
        #region Constants

        // Max => 512 MB
        private const int _MAX_SIZE = 536870912;

        #endregion

        #region Private Member Variables

        private int _scale;
        private double _zoomRatio;
        private WriteableBitmap _wb;
        private ImageSource _viewimage;
        private cv.Mat _imageOrigin;
        private Dictionary<string, DisplayInfo> _displayInfoDictionary;
        private string _viewInfo;
        private double _gridWidth;
        private double _gridHeight;
        private double _scrollWidth;
        private double _scrollHeight;
        private DrawingGroup _imageGroup;

        #endregion Private Member Variables

        #region Private Methods

        private void ShowPixelInfo( System.Windows.Point point )
        {
            if ( ImageOrigin == null )
            {
                return;
            }

            Mat.Indexer<cv.Vec3b> indexer = ImageOrigin.GetGenericIndexer<cv.Vec3b>();
            cv.Vec3b color = indexer[ (int)( point.Y * Scale ), (int)( point.X * Scale ) ];
            ViewInfo = string.Format(
                "X:{0:0.0}\t\tY:{1:0.0}\t\tR:{2}\tG:{3}\tB:{4}\t{5}",
                point.X * Scale, point.Y * Scale,
                color.Item2, color.Item1, color.Item0, Scale );
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

            }

            return loadImageResult;
        }

        private void ClearOverlay()
        {
            DisplayInfoDictionary.Clear();
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

        #endregion Private Methods

        #region Constructor

        public ImageViewModel()
        {
            Wb = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            Viewimage = new DrawingImage();
            ImageOrigin = new cv.Mat( new cv.Size( 10, 10 ), cv.MatType.CV_8SC3 );
            ViewInfo = string.Empty;
            DisplayInfoDictionary = new Dictionary<string, DisplayInfo>();
            ImageGroup = new DrawingGroup();
        }

        #endregion Constructor

        #region Public Properties

        public int Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                OnPropertyChanged( "Scale" );
            }
        }
        public double ZoomRatio
        {
            get => _zoomRatio;
            set
            {
                _zoomRatio = value;
                OnPropertyChanged( "ZoomRatio" );
            }
        }

        public ImageSource Viewimage
        {
            get => _viewimage;
            set
            {
                _viewimage = value;
                OnPropertyChanged( "ViewImage" );
            }
        }

        public WriteableBitmap Wb
        {
            get => _wb;
            set
            {
                _wb = value;
                OnPropertyChanged( "Wb" );
            }
        }
        public Mat ImageOrigin
        {
            get => _imageOrigin;
            set
            {
                _imageOrigin = value.Clone();
                cv.Mat displayImage = new cv.Mat();
                Scale = _imageOrigin.Width * _imageOrigin.Height / _MAX_SIZE + 1;

                if ( Scale != 0 )
                {
                    cv.Cv2.Resize( _imageOrigin, displayImage, new cv.Size( ImageOrigin.Width / Scale, ImageOrigin.Height / Scale ) );
                }

                if ( displayImage.Width != Viewimage.Width || displayImage.Height != Viewimage.Height )
                {
                    if ( displayImage.Channels() > 1 )
                    {
                        Wb = new WriteableBitmap( displayImage.Width, displayImage.Height, 96, 96, PixelFormats.Bgr24, null );	// color                        
                    }
                    else
                    {
                        Wb = new WriteableBitmap( displayImage.Width, displayImage.Height, 96, 96, PixelFormats.Gray8, null );	// Gray
                    }

                    OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( displayImage, Wb );
                }

                if ( ImageGroup == null )
                {
                    ImageGroup = new DrawingGroup();
                }
                else
                {
                    ImageGroup.Children.Clear();
                }
                
                ImageDrawing drawingImage = new ImageDrawing( Wb, new System.Windows.Rect( 0, 0, Wb.Width, Wb.Height ) );
                ImageGroup.Children.Add( drawingImage );

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
                            ImageGroup.Children.Add( new GeometryDrawing( brush, null, overlay ) );
                        }
                        else
                        {
                            if ( display.Thickness == 0 )
                            {
                                ImageGroup.Children.Add( new GeometryDrawing( brush, new Pen( brush, display.Thickness ), overlay ) );
                            }
                            else
                            {
                                ImageGroup.Children.Add( new GeometryDrawing( null, new Pen( brush, display.Thickness ), overlay ) );
                            }
                        }
                    }
                }

                Viewimage = new DrawingImage( ImageGroup );
                OnPropertyChanged( "ImageOrigin" );
            }
        }
        public Dictionary<string, DisplayInfo> DisplayInfoDictionary
        {
            get => _displayInfoDictionary;
            set
            {
                _displayInfoDictionary = value;
                OnPropertyChanged( "DisplayInfoDictionary" );
            }
        }

        public string ViewInfo
        {
            get => _viewInfo;
            set
            {
                _viewInfo = value;
                OnPropertyChanged( "ViewInfo" );
            }
        }

        public double GridWidth
        {
            get => _gridWidth;
            set
            {
                _gridWidth = value;
                OnPropertyChanged( "GridWidth" );
            }
        }
        public double GridHeight
        {
            get => _gridHeight;
            set
            {
                _gridHeight = value;
                OnPropertyChanged( "GridHeight" );
            }
        }

        public double ScrollWidth
        {
            get => _scrollWidth;
            set
            {
                _scrollWidth = value;
                OnPropertyChanged( "ScrollWidth" );
            }
        }
        public double ScrollHeight
        {
            get => _scrollHeight;
            set
            {
                _scrollHeight = value;
                OnPropertyChanged( "ScrollHeight" );
            }
        }

        public DrawingGroup ImageGroup
        {
            get => _imageGroup;
            set
            {
                _imageGroup = value;
                OnPropertyChanged( "ImageGroup" );
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void ZoomFit()
        {
            double widthRatio = ScrollWidth / GridWidth;
            double heightRatio = ScrollHeight / GridHeight;
            ZoomRatio = widthRatio > heightRatio ? heightRatio : widthRatio;
            //Zoom
        }

        #endregion Public Methods
    }
}
