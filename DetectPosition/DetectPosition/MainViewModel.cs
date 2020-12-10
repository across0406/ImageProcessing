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

                    cv.Mat histoEqualized = new cv.Mat();

                    cv.Cv2.EqualizeHist( SourceMat, histoEqualized );

                    SourceMat = histoEqualized.Clone();

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
            dialog.Multiselect = true;
            result = dialog.ShowDialog();

            if ( (bool)result )
            {
                if ( dialog.FileNames.Length >= 1 )
                {
                    cv.Mat[] masters = new cv.Mat[ dialog.FileNames.Length ];
                    byte[][] data = new byte[ dialog.FileNames.Length ][];

                    Parallel.For(
                        0, dialog.FileNames.Length, i =>
                        {
                            masters[ i ] = new cv.Mat( dialog.FileNames[ i ], ImreadModes.Grayscale );
                            data[ i ] = new byte[ masters[ i ].Width * masters[ i ].Height ];
                            Marshal.Copy( masters[ i ].Data, data[ i ], 0, masters[ i ].Width * masters[ i ].Height );
                        }
                    );

                    byte[] averaged = new byte[ masters[ 0 ].Width * masters[ 0 ].Height ];

                    Parallel.For(
                        0, averaged.Length, i =>
                        {
                            int sum = 0;

                            for ( int j = 0; j < data.Length; ++j )
                            {
                                sum += data[ j ][ i ];
                            }

                            averaged[ i ] = (byte) ( sum / data.Length );
                        }
                    );

                    MasterMat = new cv.Mat( masters[ 0 ].Height, masters[ 0 ].Width, cv.MatType.CV_8UC1, averaged );

                    cv.Mat histoEqualized = new cv.Mat();

                    MasterMat.ImWrite( "D:/MasterImage.bmp" );

                    cv.Cv2.EqualizeHist( MasterMat, histoEqualized );

                    MasterMat = histoEqualized.Clone();

                    MasterMat.ImWrite( "D:/MasterImageHisoEqualized.bmp" );

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
                cv.Cv2.MedianBlur( DestinationMat, DestinationMat, 27 );

                float[] sharpenFilterData = new float[ 9 ]
                {
                    1, -2, 1,
                    -2, 5, -2,
                    1, -2, 1
                };

                cv.Mat sharpenFilter = new cv.Mat( 3, 3, MatType.CV_32FC1, sharpenFilterData );

                cv.Cv2.Filter2D( DestinationMat, DestinationMat, DestinationMat.Type(), sharpenFilter );

                byte[] data = new byte[ subtracted.Height * subtracted.Width ];
                int[] histo = new int[ 255 ];

                Parallel.For(
                    0, 255, i =>
                    {
                        histo[ i ] = 0;
                    }
                );

                Marshal.Copy( DestinationMat.Data, data, 0, DestinationMat.Height * DestinationMat.Width );

                Parallel.For(
                    0, DestinationMat.Height * DestinationMat.Width, i =>
                    {
                        object lockObject = new object();

                        lock ( lockObject )
                        {
                            histo[ data[ i ] ] += 1;
                        }
                    }
                );

                SetHistogram?.Invoke( histo );

                cv.Mat thresholded = new cv.Mat();
                cv.Mat morph = new cv.Mat();

                cv.Cv2.Threshold( DestinationMat, thresholded, 20.0, 255.0, ThresholdTypes.Binary );
                // cv.Cv2.AdaptiveThreshold( subtracted, thresholded, 255.0, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 25, 0 );

                cv.Cv2.MorphologyEx( thresholded, morph, MorphTypes.HitMiss, null );

                morph.ImWrite( "D:/morph.bmp" );

                cv.Mat labels = new cv.Mat();
                cv.Mat region = new cv.Mat();
                cv.Cv2.CvtColor(SourceMat, labels, ColorConversionCodes.GRAY2BGR );

                cv.Point[][] contours = null;
                cv.HierarchyIndex[] hierarchyIndices = null;
                cv.Cv2.FindContours( morph, out contours, out hierarchyIndices, 
                                     RetrievalModes.Tree, ContourApproximationModes.ApproxNone );

                

                if ( contours.Length >= 1 )
                {
                    int argMax = 0;
                    int maxCount = contours[ 0 ].Length;

                    for ( int i = 0; i < contours.Length; ++i )
                    {
                        if ( maxCount >= contours[ i ].Length )
                        {
                            continue;
                        }
                        else
                        {
                            argMax = i;
                            maxCount = contours[ i ].Length;
                        }
                    }

                    RotatedRect rotatedRect = cv.Cv2.MinAreaRect( contours[ argMax ] );
                    cv.Rect rect = rotatedRect.BoundingRect();

                    const int margin = 10;

                    int compensatedX = rect.X - margin;
                    int compensatedY = rect.Y - margin;

                    if ( rect.X - margin < 0 )
                    {
                        rect.X = 0;
                        compensatedX = 0;
                    }
                    else
                    {
                        rect.X -= margin;
                        compensatedX = rect.X;
                    }

                    if ( rect.X + rect.Width + margin >= labels.Cols )
                    {
                        rect.Width = labels.Cols - rect.X;
                    }
                    else
                    {
                        rect.Width += 2 * margin;
                    }

                    if ( rect.Y - margin < 0 )
                    {
                        rect.Y = 0;
                        compensatedY = 0;
                    }
                    else
                    {
                        rect.Y -= margin;
                        compensatedY = rect.Y;
                    }

                    if ( rect.Y + rect.Height + margin >= labels.Rows )
                    {
                        rect.Height = labels.Rows - rect.Y;
                    }
                    else
                    {
                        rect.Height += 2 * margin;
                    }

                    region = labels.SubMat( rect );

                    object lockObject = new object();
                    
                    Parallel.For(
                        0, region.Rows, i =>
                        {
                            for ( int j = 0; j < region.Cols; ++j )
                            {
                                lock ( lockObject )
                                {
                                    double dist = cv.Cv2.PointPolygonTest( contours[ argMax ], new Point2f( j + compensatedX, i + compensatedY ), false );
                    
                                    if ( dist < 0.0 )
                                    {
                                        var point = region.At<Vec3b>( i, j );
                                        point.Item0 = 50;
                                        point.Item1 = 50;
                                        point.Item2 = 50;

                                        region.Set<Vec3b>( i, j, point );
                                    }
                                    else
                                    {
                    
                                    }
                                }
                            }
                        }
                    );
                }

                for ( int i = 0; i < contours.Length; ++i )
                {
                    // cv.Cv2.DrawContours( labels, contours, i, Scalar.Red, 2, LineTypes.AntiAlias );
                }

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                    Result2Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( region.Clone() );
                    Result4Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( thresholded.Clone() );
                    Result5Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( morph.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.Background );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerApplyAbsDiffMasterSource()
        {
            try
            {
                var subtracted = new cv.Mat();

                cv.Cv2.Absdiff( MasterMat, SourceMat, subtracted );

                DestinationMat = subtracted.Clone();
                cv.Cv2.MedianBlur( DestinationMat, DestinationMat, 27 );

                float[] sharpenFilterData = new float[ 9 ]
                {
                    0, -1, 0,
                    -1, 5, -1,
                    0, -1, 0
                };

                cv.Mat sharpenFilter = new cv.Mat( 3, 3, MatType.CV_32FC1, sharpenFilterData );

                cv.Cv2.Filter2D( DestinationMat, DestinationMat, DestinationMat.Type(), sharpenFilter );

                byte[] data = new byte[ DestinationMat.Height * DestinationMat.Width ];
                int[] histo = new int[ 255 ];

                Parallel.For(
                    0, 255, i =>
                    {
                        histo[ i ] = 0;
                    }
                );

                Marshal.Copy( DestinationMat.Data, data, 0, DestinationMat.Height * DestinationMat.Width );

                Parallel.For(
                    0, DestinationMat.Height * DestinationMat.Width, i =>
                    {
                        object lockObject = new object();

                        lock ( lockObject )
                        {
                            histo[ data[ i ] ] += 1;
                        }
                    }
                );

                SetHistogram?.Invoke( histo );

                cv.Mat thresholded = new cv.Mat();
                cv.Mat morph = new cv.Mat();

                cv.Cv2.Threshold( DestinationMat, thresholded, 20.0, 255.0, ThresholdTypes.Binary );
                // cv.Cv2.AdaptiveThreshold( subtracted, thresholded, 255.0, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 25, 0 );

                cv.Cv2.MorphologyEx( thresholded, morph, MorphTypes.HitMiss, null );
                cv.Cv2.MorphologyEx( morph, morph, MorphTypes.Dilate, null );

                morph.ImWrite( "D:/morph.bmp" );

                cv.Mat labels = new cv.Mat();
                cv.Cv2.CvtColor( SourceMat, labels, ColorConversionCodes.GRAY2BGR );

                cv.Point[][] contours = null;
                cv.HierarchyIndex[] hierarchyIndices = null;
                cv.Cv2.FindContours( morph, out contours, out hierarchyIndices,
                                     RetrievalModes.Tree, ContourApproximationModes.ApproxNone );

                if ( contours.Length >= 1 )
                {
                    int argMax = 0;
                    int maxCount = contours[ 0 ].Length;

                    for ( int i = 0; i < contours.Length; ++i )
                    {
                        if ( maxCount >= contours[ i ].Length )
                        {
                            continue;
                        }
                        else
                        {
                            argMax = i;
                            maxCount = contours[ i ].Length;
                        }
                    }

                    object lockObject = new object();

                    Parallel.For(
                        0, labels.Rows, i =>
                        {
                            for ( int j = 0; j < labels.Cols; ++j )
                            {
                                lock ( lockObject )
                                {
                                    double dist = cv.Cv2.PointPolygonTest( contours[ argMax ], new Point2f( j, i ), false );

                                    if ( dist < 0.0 )
                                    {
                                        var point = labels.At<Vec3b>( i, j );
                                        point.Item0 = 50;
                                        point.Item1 = 50;
                                        point.Item2 = 50;
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                        }
                    );
                }

                for ( int i = 0; i < contours.Length; ++i )
                {
                    // cv.Cv2.DrawContours( labels, contours, i, Scalar.Red, 2, LineTypes.AntiAlias );
                }

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                    Result2Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( labels.Clone() );
                    Result4Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( thresholded.Clone() );
                    Result5Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( morph.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.Background );
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
                var subtracted = new cv.Mat();

                cv.Cv2.Subtract( MasterMat, SourceMat, subtracted );

                var filter = cv.XImgProc.GuidedFilter.Create( subtracted, 21, 0.09 );
                filter.Filter( SourceMat, filtered );

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( subtracted.Clone() );
                    Result2Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( filtered.Clone() );
                } ), System.Windows.Threading.DispatcherPriority.ApplicationIdle );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerApplyBilateralSubtract()
        {
            try
            {
                var bilateralMat = new cv.Mat();
                var gaussianMat = new cv.Mat();
                var subtracted = new cv.Mat();

                cv.Cv2.BilateralFilter( SourceMat, bilateralMat, 7, 10.0, 10.0 );
                cv.Cv2.GaussianBlur( SourceMat, gaussianMat, new cv.Size( 27, 27 ), 0 );

                cv.Cv2.Subtract( gaussianMat, SourceMat, subtracted );

                DestinationMat = subtracted.Clone();
                cv.Cv2.MedianBlur( DestinationMat, DestinationMat, 27 );
                
                float[] sharpenFilterData = new float[ 9 ]
                {
                    1, -2, 1,
                    -2, 5, -2,
                    1, -2, 1
                };
                
                cv.Mat sharpenFilter = new cv.Mat( 3, 3, MatType.CV_32FC1, sharpenFilterData );
                
                cv.Cv2.Filter2D( DestinationMat, DestinationMat, DestinationMat.Type(), sharpenFilter );
                
                //byte[] data = new byte[ subtracted.Height * subtracted.Width ];
                //int[] histo = new int[ 255 ];
                //
                //Parallel.For(
                //    0, 255, i =>
                //    {
                //        histo[ i ] = 0;
                //    }
                //);
                //
                //Marshal.Copy( DestinationMat.Data, data, 0, DestinationMat.Height * DestinationMat.Width );
                //
                //Parallel.For(
                //    0, DestinationMat.Height * DestinationMat.Width, i =>
                //    {
                //        object lockObject = new object();
                //
                //        lock ( lockObject )
                //        {
                //            histo[ data[ i ] ] += 1;
                //        }
                //    }
                //);
                //
                //SetHistogram?.Invoke( histo );
                
                cv.Mat thresholded = new cv.Mat();
                cv.Mat morph = new cv.Mat();
                
                cv.Cv2.Threshold( DestinationMat, thresholded, 10.0, 255.0, ThresholdTypes.Binary );
                // cv.Cv2.AdaptiveThreshold( subtracted, thresholded, 255.0, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 25, 0 );
                
                cv.Cv2.MorphologyEx( thresholded, morph, MorphTypes.HitMiss, null );
                
                morph.ImWrite( "D:/morph.bmp" );
                
                cv.Mat labels = new cv.Mat();
                cv.Mat region = new cv.Mat();
                cv.Cv2.CvtColor( SourceMat, labels, ColorConversionCodes.GRAY2BGR );
                
                cv.Point[][] contours = null;
                cv.HierarchyIndex[] hierarchyIndices = null;
                cv.Cv2.FindContours( morph, out contours, out hierarchyIndices,
                                     RetrievalModes.Tree, ContourApproximationModes.ApproxNone );
                
                
                
                if ( contours.Length >= 1 )
                {
                    int argMax = 0;
                    int maxCount = contours[ 0 ].Length;
                
                    for ( int i = 0; i < contours.Length; ++i )
                    {
                        if ( maxCount >= contours[ i ].Length )
                        {
                            continue;
                        }
                        else
                        {
                            argMax = i;
                            maxCount = contours[ i ].Length;
                        }
                    }
                
                    RotatedRect rotatedRect = cv.Cv2.MinAreaRect( contours[ argMax ] );
                    cv.Rect rect = rotatedRect.BoundingRect();
                
                    const int margin = 10;
                
                    int compensatedX = rect.X - margin;
                    int compensatedY = rect.Y - margin;
                
                    if ( rect.X - margin < 0 )
                    {
                        rect.X = 0;
                        compensatedX = 0;
                    }
                    else
                    {
                        rect.X -= margin;
                        compensatedX = rect.X;
                    }
                
                    if ( rect.X + rect.Width + margin >= labels.Cols )
                    {
                        rect.Width = labels.Cols - rect.X;
                    }
                    else
                    {
                        rect.Width += 2 * margin;
                    }
                
                    if ( rect.Y - margin < 0 )
                    {
                        rect.Y = 0;
                        compensatedY = 0;
                    }
                    else
                    {
                        rect.Y -= margin;
                        compensatedY = rect.Y;
                    }
                
                    if ( rect.Y + rect.Height + margin >= labels.Rows )
                    {
                        rect.Height = labels.Rows - rect.Y;
                    }
                    else
                    {
                        rect.Height += 2 * margin;
                    }
                
                    region = labels.SubMat( rect );
                
                    object lockObject = new object();
                
                    Parallel.For(
                        0, region.Rows, i =>
                        {
                            for ( int j = 0; j < region.Cols; ++j )
                            {
                                lock ( lockObject )
                                {
                                    double dist = cv.Cv2.PointPolygonTest( contours[ argMax ], new Point2f( j + compensatedX, i + compensatedY ), false );
                
                                    if ( dist < 0.0 )
                                    {
                                        var point = region.At<Vec3b>( i, j );
                                        point.Item0 = 50;
                                        point.Item1 = 50;
                                        point.Item2 = 50;
                
                                        region.Set<Vec3b>( i, j, point );
                                    }
                                    else
                                    {
                
                                    }
                                }
                            }
                        }
                    );
                }
                
                for ( int i = 0; i < contours.Length; ++i )
                {
                    // cv.Cv2.DrawContours( labels, contours, i, Scalar.Red, 2, LineTypes.AntiAlias );
                }

                Application.Current.Dispatcher.InvokeAsync( new Action( () =>
                {
                    Result1Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( DestinationMat.Clone() );
                    Result3Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( region.Clone() );
                    Result4Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( thresholded.Clone() );
                    Result5Image = cv.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap( morph.Clone() );


                } ), System.Windows.Threading.DispatcherPriority.Background );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void InnerSaveSourceImage()
        {
            if ( SourceImage == null )
            {

            }
            else
            {
                var sourceMat = cv.WpfExtensions.WriteableBitmapConverter.ToMat( SourceImage );
                sourceMat.ImWrite( "SourceImage.png" );
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
            ApplyAbsDiffMasterSource += new Action( InnerApplyAbsDiffMasterSource );
            ApplyBilateralSubtract += new Action( InnerApplyBilateralSubtract );

            SaveSourceImage += new Action( InnerSaveSourceImage );
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

        public Action ApplyAbsDiffMasterSource
        {
            get;
            set;
        }

        public Action ApplyBilateralSubtract
        {
            get;
            set;
        }

        public Action SaveSourceImage
        {
            get;
            set;
        }

        public Action<int[]> SetHistogram
        {
            get;
            set;
        }

        #endregion Public Methods
    }
}
