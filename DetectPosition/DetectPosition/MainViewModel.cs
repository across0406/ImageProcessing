using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OpenCvSharp;
using cv = OpenCvSharp;

namespace DetectPosition
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation

        protected virtual void SetProperty<T>( ref T member, T val, [CallerMemberName] string propertyName = null )
        {
            if ( object.Equals( member, val ) )
            {
                return;
            }

            member = val;
            PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected virtual void OnPropertyChanged( string propertyName )
        {
            PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion INotifyPropertyChanged Implementation

        #region Private Member Variables

        private cv.Mat _sourceMat;
        private cv.Mat _masterMat;
        private cv.Mat _destinationMat;

        private WriteableBitmap _sourceImage;
        private WriteableBitmap _masterImage;
        private WriteableBitmap _destinationImage;

        private WriteableBitmap _result1Image;
        private WriteableBitmap _result2Image;
        private WriteableBitmap _result3Image;
        private WriteableBitmap _result4Image;
        private WriteableBitmap _result5Image;
        private WriteableBitmap _result6Image;

        // Parameters
        private int _width;
        private int _height;

        // MSER Parameters
        private int _deltaMSER;
        private int _minAreaMSER;
        private int _maxAreaMSER;
        private double _maxVariationMSER;
        private double _minDiversityMSER;
        private int _maxEvolutionMSER;
        private double _areaThresholdMSER;
        private double _minMarginMSER;
        private int _edgeBlurSizeMSER;

        #endregion Private Member Variables

        #region Private Methods

        private void InnerImageOpen()
        {
            bool openResult = false;
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            OpenFileDialog dialog = new OpenFileDialog();
            Nullable<bool> result = false;
            dialog.Filter = "PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp";
            dialog.Multiselect = false;
            result = dialog.ShowDialog();

            if ( (bool)result )
            {
                if ( dialog.FileNames.Length >= 1 )
                {
                    SourceMat = new cv.Mat( dialog.FileName, ImreadModes.Grayscale );

                    Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                    {
                        SourceImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( SourceMat.Clone() );
                    } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );

                    openResult = true;
                }
                else
                {
                    openResult = false;
                }
            }
            else
            {
                openResult = false;
            }
        }

        private void InnerMasterImageOpen()
        {
            bool openResult = false;
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            OpenFileDialog dialog = new OpenFileDialog();
            Nullable<bool> result = false;
            dialog.Filter = "PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp";
            dialog.Multiselect = false;
            result = dialog.ShowDialog();

            if ( (bool)result )
            {
                if ( dialog.FileNames.Length >= 1 )
                {
                    MasterMat = new cv.Mat( dialog.FileName, ImreadModes.Grayscale );

                    Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                    {
                        MasterImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( MasterMat.Clone() );
                    } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );

                    openResult = true;
                }
                else
                {
                    openResult = false;
                }
            }
            else
            {
                openResult = false;
            }
        }

        private void InnerApplySubtractMasterSource()
        {
            try
            {
                var subtracted = new cv.Mat();

                cv.Cv2.Subtract( MasterMat, SourceMat, subtracted );

                DestinationMat = subtracted.Clone();

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }


        private void InnerApplyMSER()
        {


            try
            {

                var blurred = new cv.Mat();

                cv.Cv2.MedianBlur( SourceMat, blurred, 27 );

                var mser = cv.MSER.Create( DeltaMSER, MinAreaMSER );
                var regions = mser.Detect( blurred );

                if ( regions == null )
                {

                }
                else
                {
                    DestinationMat = blurred.Clone();

                    // foreach ( var region in regions )
                    // {
                    //     Point2f point = region.Pt;
                    // 
                    //     cv.Cv2.Circle( DestinationMat, (int)point.X, (int)point.Y, 20, Scalar.White );
                    // }

                    Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                    {
                        DestinationImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                    } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerApplyMedian()
        {
            DestinationMat = new cv.Mat();
            cv.Cv2.MedianBlur( SourceMat, DestinationMat, 27 );

            Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            {
                DestinationImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
            } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
        }

        private void InnerApplyDFT()
        {
            // DestinationMat = new cv.Mat();
            // var merged = new cv.Mat();
            // var converted = new cv.Mat();
            // var zeroMat = cv.Mat.Zeros( SourceMat.Height, SourceMat.Width, MatType.CV_32FC1 ).ToMat();
            // var dft = new cv.Mat();
            // SourceMat.ConvertTo( converted, MatType.CV_32FC1 );
            // cv.Mat[] planes = new cv.Mat[ 2 ];
            // planes[ 0 ] = converted.Clone();
            // planes[ 1 ] = zeroMat.Clone();
            // cv.Cv2.Merge( planes, merged );
            // cv.Cv2.Dft( merged, dft, DftFlags.ComplexOutput );
            // 
            // 
            // 
            // 
            // Application.Current.Dispatcher.InvokeAsync( new Action( () =>
            // {
            //     DestinationImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
            // } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
        }

        private void InnerApplyBitwise()
        {
            try
            {
                var blurred = new cv.Mat();
                var xorBitWised = new cv.Mat();
                var andBitWised = new cv.Mat();
                var orBitWised = new cv.Mat();
                var xorInvert = new cv.Mat();
                var andInvert = new cv.Mat();
                var orInvert = new cv.Mat();

                cv.Cv2.MedianBlur( SourceMat, blurred, 33 );
                cv.Cv2.BitwiseXor( SourceMat, blurred, xorBitWised );
                cv.Cv2.BitwiseAnd( SourceMat, blurred, andBitWised );
                cv.Cv2.BitwiseOr( SourceMat, blurred, orBitWised );

                cv.Cv2.BitwiseNot( xorBitWised, xorInvert );
                cv.Cv2.BitwiseNot( andBitWised, andInvert );
                cv.Cv2.BitwiseNot( orBitWised, orInvert );

                DestinationMat = blurred.Clone();

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    DestinationImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( xorBitWised.Clone() );
                    Result3Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( andBitWised.Clone() );
                    Result5Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( orBitWised.Clone() );
                    Result2Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( xorInvert.Clone() );
                    Result4Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( andInvert.Clone() );
                    Result6Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( orInvert.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerApplyQuantizationArithmatic()
        {
            try
            {
                // var blurred = new cv.Mat();
                // 
                // cv.Cv2.MedianBlur( SourceMat, blurred, 33 );
                // 
                // byte[] data = new byte[ SourceMat.Height * SourceMat.Width ];
                // Marshal.Copy( blurred.Data, data, 0, SourceMat.Height * SourceMat.Width );
                // 
                // Parallel.For( 0, SourceMat.Height * SourceMat.Width, i =>
                // {
                //     data[ i ] = (byte)( data[ i ] / quantizedNum * quantizedNum + quantizedNum / 2 );
                // } );
                // 
                // quantized = cv.Mat.Zeros( SourceMat.Height, SourceMat.Width, SourceMat.Type() ).ToMat();
                // Marshal.Copy( data, 0, quantized.Data, SourceMat.Height * SourceMat.Width );
                // 
                // DestinationMat = blurred.Clone();

                var subtracted = new cv.Mat();
                var quantized = new cv.Mat();
                int quantizedNum = 32;

                cv.Cv2.Subtract( MasterMat, SourceMat, subtracted );

                byte[] data = new byte[ SourceMat.Height * SourceMat.Width ];
                Marshal.Copy( subtracted.Data, data, 0, SourceMat.Height * SourceMat.Width );
                
                Parallel.For( 0, SourceMat.Height * SourceMat.Width, i =>
                {
                    data[ i ] = (byte)( data[ i ] / quantizedNum * quantizedNum + quantizedNum / 2 );
                } );
                
                quantized = cv.Mat.Zeros( SourceMat.Height, SourceMat.Width, SourceMat.Type() ).ToMat();
                Marshal.Copy( data, 0, quantized.Data, SourceMat.Height * SourceMat.Width );

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( subtracted.Clone() );
                    Result2Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( quantized.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerApplyGuidedFilter()
        {
            try
            {
                var blurred = new cv.Mat();
                var filtered = new cv.Mat();
                var xor = new cv.Mat();

                cv.Cv2.MedianBlur( SourceMat, blurred, 33 );

                var filter = cv.XImgProc.GuidedFilter.Create( blurred, 21, 0.09 );
                filter.Filter( blurred, filtered );

                cv.Cv2.BitwiseXor( filtered, blurred, xor );

                DestinationMat = blurred.Clone();

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    DestinationImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( filtered.Clone() );
                    Result2Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( xor.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerApplyCubicSpline()
        {
            try
            {
                var blurred = new cv.Mat();

                cv.Cv2.MedianBlur( SourceMat, blurred, 33 );



                DestinationMat = blurred.Clone();

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    DestinationImage = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        #endregion Private Methods

        #region Constructors

        public MainViewModel()
        {
            SourceImage = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            DestinationImage = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );

            Result1Image = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            Result2Image = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            Result3Image = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            Result4Image = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            Result5Image = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );
            Result6Image = new WriteableBitmap( 10, 10, 96, 96, PixelFormats.Bgr24, null );

            DeltaMSER = 1;
            MinAreaMSER = 3;

            ImageOpen += new Action( InnerImageOpen );
            MasterImageOpen += new Action( InnerMasterImageOpen );
            ApplyMSER += new Action( InnerApplyMSER );
            ApplyMedian += new Action( InnerApplyMedian );
            ApplyDFT += new Action( InnerApplyDFT );
            ApplyBitwise += new Action( InnerApplyBitwise );
            ApplyQuantizationArithmatic += new Action( InnerApplyQuantizationArithmatic );
            ApplyGuidedFilter += new Action( InnerApplyGuidedFilter );
            ApplySubtractMasterSource += new Action( InnerApplySubtractMasterSource );
        }

        #endregion Constructors

        #region Public Properties

        public Mat SourceMat
        {
            get => _sourceMat;
            set => _sourceMat = value;
        }

        public Mat MasterMat
        {
            get => _masterMat;
            set => _masterMat = value;
        }

        public Mat DestinationMat
        {
            get => _destinationMat;
            set => _destinationMat = value;
        }

        public WriteableBitmap SourceImage
        {
            get => _sourceImage;
            set
            {
                _sourceImage = value;
                OnPropertyChanged( "SourceImage" );
            }
        }

        public WriteableBitmap MasterImage
        {
            get => _masterImage;
            set
            {
                _masterImage = value;
                OnPropertyChanged( "MasterImage" );
            }
        }

        public WriteableBitmap DestinationImage
        {
            get => _destinationImage;
            set
            {
                _destinationImage = value;
                OnPropertyChanged( "DestinationImage" );
            }
        }

        public WriteableBitmap Result1Image
        {
            get => _result1Image;
            set
            {
                _result1Image = value;
                OnPropertyChanged( "Result1Image" );
            }
        }

        public WriteableBitmap Result2Image
        {
            get => _result2Image;
            set
            {
                _result2Image = value;
                OnPropertyChanged( "Result2Image" );
            }
        }

        public WriteableBitmap Result3Image
        {
            get => _result3Image;
            set
            {
                _result3Image = value;
                OnPropertyChanged( "Result3Image" );
            }
        }

        public WriteableBitmap Result4Image
        {
            get => _result4Image;
            set
            {
                _result4Image = value;
                OnPropertyChanged( "Result4Image" );
            }
        }

        public WriteableBitmap Result5Image
        {
            get => _result5Image;
            set
            {
                _result5Image = value;
                OnPropertyChanged( "Result5Image" );
            }
        }

        public WriteableBitmap Result6Image
        {
            get => _result6Image;
            set
            {
                _result6Image = value;
                OnPropertyChanged( "Result6Image" );
            }
        }

        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public int DeltaMSER
        {
            get => _deltaMSER;
            set => _deltaMSER = value;
        }

        public int MinAreaMSER
        {
            get => _minAreaMSER;
            set => _minAreaMSER = value;
        }

        #endregion Public Properties

        #region Public Methods

        public Action ImageOpen
        {
            get;
            set;
        }

        public Action MasterImageOpen
        {
            get;
            set;
        }

        public Action ApplyMSER
        {
            get;
            set;
        }

        public Action ApplyMedian
        {
            get;
            set;
        }

        public Action ApplyDFT
        {
            get;
            set;
        }

        public Action ApplyBitwise
        {
            get;
            set;
        }

        public Action ApplyQuantizationArithmatic
        {
            get;
            set;
        }

        public Action ApplyGuidedFilter
        {
            get;
            set;
        }

        public Action ApplySubtractMasterSource
        {
            get;
            set;
        }

        #endregion Public Methods
    }
}
