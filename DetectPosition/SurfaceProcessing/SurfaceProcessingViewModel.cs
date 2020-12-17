using Base;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using cv = OpenCvSharp;

namespace SurfaceProcessing
{
    public class SurfaceProcessingViewModel : Base.BasePropertyChanged
    {
        #region Private Methods

        private void InnerLoadSurface()
        {
            bool loadResult = false;
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            OpenFileDialog dialog = new OpenFileDialog();
            Nullable<bool> result = false;
            dialog.Filter = "Binary files (*.bin)|*.bin|CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            dialog.Multiselect = false;
            result = dialog.ShowDialog();

            if ( !(bool)result )
            {
                Logging.Instance.Error(
                    methodBase.ReflectedType.Name + "." + methodBase.Name + " -> " +
                    "Failed to load surface file." );

                loadResult = false;
            }
            else
            {
                if ( dialog.FileNames.Length != 1 )
                {
                    Logging.Instance.Error(
                        methodBase.ReflectedType.Name + "." + methodBase.Name + " -> " +
                        "It is not enough data or being overflowed data to simulate setup process" );

                    loadResult = false;
                }
                else
                {
                    try
                    {
                        SizeInputTypeWindow sizeInputWindow = new SizeInputTypeWindow( this );
                        bool? isGetSized = sizeInputWindow.ShowDialog();

                        int height = sizeInputWindow.SizeHeight;
                        int width = sizeInputWindow.SizeWidth;

                        if ( !(bool)isGetSized )
                        {
                            throw new Exception( "The input size is canceled." );
                        }
                        else
                        {
                            Type = sizeInputWindow.SelectedType;

                            float[] loadData = new float[ width * height ];
                            string[] splitName = dialog.FileName.Split( '.' );

                            switch ( splitName[ splitName.Length - 1 ] )
                            {
                                case "csv":
                                case "CSV":
                                case "txt":
                                case "TXT":
                                    using ( StreamReader file = new StreamReader( dialog.FileName ) )
                                    {
                                        int i = 0;

                                        while ( !file.EndOfStream && i < height )
                                        {
                                            string s = file.ReadLine();
                                            string[] lineSplit = s.Split( '\t' );

                                            Parallel.For(
                                                0, lineSplit.Length, j =>
                                                {
                                                    float data = 0.0f;

                                                    if ( !float.TryParse( lineSplit[ j ], out data ) )
                                                    {
                                                        throw new Exception( "Failed to parse." );
                                                    }
                                                    else
                                                    {
                                                        loadData[ i * width + j ] = data;
                                                    }
                                                }
                                            );

                                            ++i;
                                        }
                                    }

                                    break;

                                case "bin":
                                default:
                                    using ( FileStream fs = new FileStream( dialog.FileName, FileMode.Open, FileAccess.Read ) )
                                    {
                                        using ( BinaryReader br = new BinaryReader( fs ) )
                                        {
                                            for ( int i = 0; i < height; i++ )
                                            {
                                                for ( int j = 0; j < width; j++ )
                                                {
                                                    loadData[ i * width + j ] = br.ReadSingle();
                                                }
                                            }
                                        }
                                    }

                                    break;
                            }

                            // float average = loadData.Average();
                            // 
                            // Parallel.For(
                            //     0, loadData.Length, i =>
                            //     {
                            //         loadData[ i ] -= average;
                            //     }
                            // );

                            cv.Mat loadedMat = new cv.Mat( height, width, cv.MatType.CV_32FC1, loadData );

                            CurrentWidth = height;
                            CurrentHeight = width;

                            switch ( Type )
                            {
                                case "Intensity":
                                    IntensitySurfaceData = loadedMat.Clone();
                                    ProcessedIntensityData = loadedMat.Clone();

                                    InnerUpdateView( IntensitySurfaceData.Clone() );
                                    break;

                                case "Phase":
                                    PhaseSurfaceData = loadedMat.Clone();
                                    ProcessedPhaseData = loadedMat.Clone();

                                    InnerUpdateView( PhaseSurfaceData.Clone() );
                                    break;

                                default:
                                    break;
                            }
                            

                            loadResult = true;
                        }
                    }
                    catch ( Exception ex )
                    {
                        Logging.Instance.Error(
                            methodBase.ReflectedType.Name + "." + methodBase.Name + " -> " +
                            "System Exception: " + ex.Message );

                        loadResult = false;
                    }
                }
            }
        }

        private void InnerUpdateView( cv.Mat data )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();

            if ( data == null )
            {

            }
            else
            {
                try
                {
                    cv.Mat normalizedMat = new cv.Mat();

                    cv.Cv2.Normalize( data, normalizedMat, 0, 255, cv.NormTypes.MinMax );
                    normalizedMat.ConvertTo( normalizedMat, cv.MatType.CV_8UC1 );

                    Application.Current.Dispatcher.InvokeAsync(
                        new Action( () =>
                        {
                            SetSurface3DData?.Invoke( data.Clone() );
                            SetSurface2DData?.Invoke( normalizedMat.Clone() );
                        } ), System.Windows.Threading.DispatcherPriority.Background
                    );
                }
                catch ( Exception ex )
                {
                    Logging.Instance.Error(
                        methodBase.ReflectedType.Name + "." + methodBase.Name + " -> " +
                        "System Exception: " + ex.Message );
                }
            }
        }

        private void InnerUpdateView( cv.Mat data, cv.Mat display, cv.Point[] contour )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();

            if ( data == null )
            {

            }
            else
            {
                try
                {
                    cv.Mat normalizedMat = new cv.Mat();

                    cv.Cv2.Normalize( display, normalizedMat, 0, 255, cv.NormTypes.MinMax );
                    normalizedMat.ConvertTo( normalizedMat, cv.MatType.CV_8UC1 );
                    cv.Cv2.CvtColor( normalizedMat, normalizedMat, cv.ColorConversionCodes.GRAY2BGR );

                    if ( contour == null )
                    {

                    }
                    else
                    {
                        cv.Point[][] contours = new cv.Point[ 1 ][];
                        contours[ 0 ] = contour;

                        cv.Cv2.DrawContours( normalizedMat, contours, -1, new cv.Scalar( 0, 0, 255 ), 5, cv.LineTypes.AntiAlias );
                    }

                    Application.Current.Dispatcher.InvokeAsync(
                        new Action( () =>
                        {
                            SetSurface3DData?.Invoke( data.Clone() );
                            SetSurface2DData?.Invoke( normalizedMat.Clone() );
                        } ), System.Windows.Threading.DispatcherPriority.Background
                    );
                }
                catch ( Exception ex )
                {
                    Logging.Instance.Error(
                        methodBase.ReflectedType.Name + "." + methodBase.Name + " -> " +
                        "System Exception: " + ex.Message );
                }
            }
        }

        private void InnerUpdateView( cv.Mat data, cv.Mat display, cv.Point[][] contours = null )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();

            if ( data == null )
            {

            }
            else
            {
                try
                {
                    cv.Mat normalizedMat = new cv.Mat();

                    cv.Cv2.Normalize( display, normalizedMat, 0, 255, cv.NormTypes.MinMax );
                    normalizedMat.ConvertTo( normalizedMat, cv.MatType.CV_8UC1 );
                    cv.Cv2.CvtColor( normalizedMat, normalizedMat, cv.ColorConversionCodes.GRAY2BGR );

                    if ( contours == null )
                    {

                    }
                    else
                    {
                        cv.Cv2.DrawContours( normalizedMat, contours, -1, new cv.Scalar( 0, 0, 255 ), 5, cv.LineTypes.AntiAlias );
                    }

                    Application.Current.Dispatcher.InvokeAsync(
                        new Action( () =>
                        {
                            SetSurface3DData?.Invoke( data.Clone() );
                            SetSurface2DData?.Invoke( normalizedMat.Clone() );
                        } ), System.Windows.Threading.DispatcherPriority.Background
                    );
                }
                catch ( Exception ex )
                {
                    Logging.Instance.Error(
                        methodBase.ReflectedType.Name + "." + methodBase.Name + " -> " +
                        "System Exception: " + ex.Message );
                }
            }
        }

        private cv.Mat GetSubtractedIntensityMat( cv.Mat source )
        {
            cv.Mat subtracted = cv.Mat.Zeros( ProcessedIntensityData.Rows, ProcessedIntensityData.Cols - 1, ProcessedIntensityData.Type() );
            cv.Mat.Indexer<float> surfaceIndexer = ProcessedIntensityData.GetGenericIndexer<float>();
            cv.Mat.Indexer<float> subtractedIndexer = subtracted.GetGenericIndexer<float>();

            Parallel.For(
                0, ProcessedIntensityData.Rows, i =>
                {
                    for ( int j = 0; j < ProcessedIntensityData.Cols - 1; ++j )
                    {
                        subtractedIndexer[ i, j ] = (float)Math.Pow( surfaceIndexer[ i, j + 1 ] - surfaceIndexer[ i, j ], 2.0f );

                        if ( subtractedIndexer[ i, j ] < 2 &&
                             subtractedIndexer[ i, j ] > -2 )
                        {
                            subtractedIndexer[ i, j ] = 0.0f;
                        }
                    }
                }
            );

            return subtracted.Clone();
        }

        private cv.Mat GetSubtractedPhaseMat( cv.Mat source )
        {
            cv.Mat subtracted = cv.Mat.Zeros( source.Rows, source.Cols - 1, source.Type() );
            cv.Mat.Indexer<float> surfaceIndexer = source.GetGenericIndexer<float>();
            cv.Mat.Indexer<float> subtractedIndexer = subtracted.GetGenericIndexer<float>();

            Parallel.For(
                0, source.Rows, i =>
                {
                    for ( int j = 0; j < source.Cols - 1; ++j )
                    {
                        subtractedIndexer[ i, j ] = surfaceIndexer[ i, j + 1 ] - surfaceIndexer[ i, j ];

                        if ( subtractedIndexer[ i, j ] > (float)Math.PI )
                        {
                            subtractedIndexer[ i, j ] = subtractedIndexer[ i, j ] - (float)( 2.0f * Math.PI );
                        }
                    }
                }
            );

            return subtracted.Clone();
        }

        private void InnerFindContours()
        {
            switch ( Type )
            {
                case "Intensity":
                    InnerFindContoursIntensity();
                    break;

                case "Phase":
                    InnerFindContoursPhase();
                    break;

                default:
                    break;
            }
        }

        private void InnerFindContoursIntensity()
        {
            if ( IntensitySurfaceData == null )
            {

            }
            else
            {
                float average = (float)IntensitySurfaceData.Mean().Val0;
                cv.Point[] contour = GetContour();

                if ( contour == null )
                {
                    InnerUpdateView( IntensitySurfaceData.Clone(), IntensitySurfaceData.Clone() );
                }
                else
                {
                    cv.Mat.Indexer<float> surfaceIndexer = IntensitySurfaceData.GetGenericIndexer<float>();
                    cv.Mat gaussianed = IntensitySurfaceData.Clone();
                    // cv.Cv2.BoxFilter( gaussianed, gaussianed, gaussianed.Type(), new cv.Size( 21, 21 ) );
                    cv.Cv2.GaussianBlur( gaussianed, gaussianed, new cv.Size( 3, 3 ), 0.0 );
                    cv.MatIndexer<float> gaussianedIndexer = gaussianed.GetGenericIndexer<float>();

                    Parallel.For(
                        0, IntensitySurfaceData.Rows, i =>
                        {
                            for ( int j = 0; j < IntensitySurfaceData.Cols; ++j )
                            {
                                double distance = cv.Cv2.PointPolygonTest( contour, new cv.Point2f( j, i ), false );

                                if ( distance < 0.0 )
                                {
                                    surfaceIndexer[ i, j ] = ( gaussianedIndexer[ i, j ] - average ) / 2.0f;
                                }
                                else
                                {
                                    surfaceIndexer[ i, j ] = gaussianedIndexer[ i, j ] - average;
                                }
                            }
                        }
                    );

                    InnerSaveData( IntensitySurfaceData, "Intensity", contour );
                    InnerUpdateView( IntensitySurfaceData.Clone(), IntensitySurfaceData.Clone(), contour );
                }

                // InnerUpdateView( SurfaceData.Clone(), found.Clone(), contours );
            }
        }

        private void InnerFindContoursPhase()
        {
            if ( PhaseSurfaceData == null || ProcessedPhaseData == null )
            {

            }
            else
            {
                cv.Point[] contour = GetContour();

                if ( contour == null )
                {
                    InnerUpdateView( PhaseSurfaceData.Clone(), PhaseSurfaceData.Clone() );
                }
                else
                {
                    cv.Mat subtracted = GetSubtractedPhaseMat( PhaseSurfaceData.Clone() );
                    cv.Mat.Indexer<float> subtractedIndexer = subtracted.GetGenericIndexer<float>();
                    cv.Mat.Indexer<float> surfaceIndexer = PhaseSurfaceData.GetGenericIndexer<float>();

                    Parallel.For(
                        0, subtracted.Rows, i =>
                        {
                            for ( int j = 0; j < subtracted.Cols; ++j )
                            {
                                if ( j == 0 )
                                {
                                    surfaceIndexer[ i, 0 ] = subtractedIndexer[ i, j ];
                                }

                                surfaceIndexer[ i, j + 1 ] = subtractedIndexer[ i, j ];
                            }
                        }
                    );

                    float average = (float)PhaseSurfaceData.Mean().Val0;

                    cv.Mat gaussianed = PhaseSurfaceData.Clone();
                    // cv.Cv2.BoxFilter( gaussianed, gaussianed, gaussianed.Type(), new cv.Size( 2, 2 ) );
                    cv.Cv2.GaussianBlur( gaussianed, gaussianed, new cv.Size( 3, 3 ), 0.0 );
                    cv.MatIndexer<float> gaussianedIndexer = gaussianed.GetGenericIndexer<float>();

                    Parallel.For(
                        0, PhaseSurfaceData.Rows, i =>
                        {
                            for ( int j = 0; j < PhaseSurfaceData.Cols; ++j )
                            {
                                double distance = cv.Cv2.PointPolygonTest( contour, new cv.Point2f( j, i ), false );
                    
                                if ( distance < 0.0 )
                                {
                                    surfaceIndexer[ i, j ] = ( gaussianedIndexer[ i, j ] - average ) / 2.0f;
                                }
                                else
                                {
                                    surfaceIndexer[ i, j ] = gaussianedIndexer[ i, j ] - average;
                                }
                            }
                        }
                    );

                    InnerSaveData( PhaseSurfaceData, "Phase", contour );
                    InnerUpdateView( PhaseSurfaceData.Clone(), PhaseSurfaceData.Clone(), contour );
                }
            }
        }

        private cv.Point[] GetContour()
        {
            cv.Point[] resultContour = null;

            cv.Mat found = GetSubtractedIntensityMat( IntensitySurfaceData.Clone() );
            float average = (float)IntensitySurfaceData.Mean().Val0;

            cv.Cv2.Threshold( found, found, 4, 255, cv.ThresholdTypes.Binary );
            found.ConvertTo( found, cv.MatType.CV_8UC1 );

            int iteration = 10;
            cv.Cv2.MorphologyEx( found, found, cv.MorphTypes.Dilate, null, null, iteration );
            // cv.Cv2.MorphologyEx( found, found, cv.MorphTypes.Erode, null, null, iteration );

            cv.Point[][] contours = null;
            cv.HierarchyIndex[] hierarchies = null;

            cv.Cv2.FindContours( found, out contours, out hierarchies,
                                 cv.RetrievalModes.Tree, cv.ContourApproximationModes.ApproxTC89KCOS );

            cv.Point[] contour = null;

            if ( contours == null )
            {

            }
            else
            {
                object lockObject = new object();
                cv.Mat.Indexer<float> surfaceIndexer = IntensitySurfaceData.GetGenericIndexer<float>();
                contour = contours[ 0 ];

                Parallel.For(
                    1, contours.Length, i =>
                    {
                        lock ( lockObject )
                        {
                            if ( contour.Length < contours[ i ].Length )
                            {
                                contour = contours[ i ];
                            }
                        }
                    }
                );

                Parallel.For(
                    0, contour.Length, i =>
                    {
                        contour[ i ].X += 1;
                    }
                );

                resultContour = contour;
            }

            return resultContour;
        }

        private void InnerSaveData( cv.Mat data, string type, cv.Point[] contour = null )
        {
            string toSaveDrive = "D:/SurfaceProcessing";
            string toSaveFolder = DateTime.Now.ToString( "yyyy-MM-dd" ) + "/" + DateTime.Now.ToString( "HH-mm-ss" );
            DirectoryInfo di = new DirectoryInfo( toSaveDrive + "/" + toSaveFolder + "/Result/" );

            if ( !di.Exists )
            {
                di.Create();
            }

            string surfacePhaseBinaryFilePath = di.FullName + "/" + "surface_map.bin";
            string surfacePhaseCSVFilePath = di.FullName + "/" + "surface_map.csv";
            string featuresFilePath = di.FullName + "/" + "result_features.csv";

            InnerSaveBinaryData( data, surfacePhaseBinaryFilePath );
            InnerSaveCSVData( data, surfacePhaseCSVFilePath );
            InnerSaveFeatureData( data, contour, featuresFilePath, type );
        }

        private void InnerSaveBinaryData( float[] data, string fullPath, int width, int height )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();

            byte[] binaryData = new byte[ sizeof( float ) * width * height ];

            FileStream fs = new FileStream( fullPath, FileMode.Create, FileAccess.Write );
            BinaryWriter bw = new BinaryWriter( fs );
            Buffer.BlockCopy( data, 0, binaryData, 0, width * height * sizeof( float ) );
            bw.Write( binaryData );
            bw.Flush();
            bw.Close();
        }

        private void InnerSaveBinaryData( cv.Mat data, string fullPath )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            int width = data.Cols;
            int height = data.Rows;

            byte[] binaryData = new byte[ sizeof( float ) * width * height ];

            FileStream fs = new FileStream( fullPath, FileMode.Create, FileAccess.Write );
            BinaryWriter bw = new BinaryWriter( fs );
            Marshal.Copy( data.Data, binaryData, 0, width * height );
            bw.Write( binaryData );
            bw.Flush();
            bw.Close();
        }
        private void InnerSaveCSVData( float[] data, string fullPath, int width, int height )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();

            using ( StreamWriter file = new StreamWriter( fullPath, false ) )
            {
                for ( int i = 0; i < height; i++ )
                {
                    for ( int j = 0; j < width; j++ )
                    {
                        if ( j == width - 1 )
                        {
                            file.Write( data[ i * width + j ].ToString() );
                        }
                        else
                        {
                            file.Write( data[ i * width + j ].ToString() + "\t" );
                        }
                    }

                    file.WriteLine();
                }
            }
        }

        private void InnerSaveCSVData( cv.Mat data, string fullPath )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            int width = data.Cols;
            int height = data.Rows;
            cv.Mat.Indexer<float> dataIndexer = data.GetGenericIndexer<float>();

            using ( StreamWriter file = new StreamWriter( fullPath, false ) )
            {
                for ( int i = 0; i < height; i++ )
                {
                    for ( int j = 0; j < width; j++ )
                    {
                        if ( j == width - 1 )
                        {
                            file.Write( dataIndexer[ i, j ].ToString() );
                        }
                        else
                        {
                            file.Write( dataIndexer[ i, j ].ToString() + "\t" );
                        }
                    }

                    file.WriteLine();
                }
            }
        }

        private void InnerSaveFeatureData( cv.Mat data, cv.Point[] contour, string fullPath, string type )
        {
            cv.RotatedRect rotatedRect = cv.Cv2.MinAreaRect( contour );
            int centerX = (int)rotatedRect.Center.X;
            int centerY = (int)rotatedRect.Center.Y;
            int length = (int)( rotatedRect.Size.Width > rotatedRect.Size.Height ? rotatedRect.Size.Width : rotatedRect.Size.Height );
            int width = (int)( rotatedRect.Size.Width <= rotatedRect.Size.Height ? rotatedRect.Size.Width : rotatedRect.Size.Height );
            float heightMin = 0.0f;
            float heightMax = 0.0f;
            float heightAverage = 0.0f;
            int upperCount = 0;
            int lowerCount = 0;
            float upperArea = 0.0f;
            float lowerArea = 0.0f;

            cv.Rect rect = cv.Cv2.BoundingRect( contour );
            cv.Mat subData = data.SubMat( rect );
            cv.Mat.Indexer<float> subDataIndexer = subData.GetGenericIndexer<float>();
            int compensatedX = rect.X;
            int compensatedY = rect.Y;
            object lockObject = new object();
            float[] outerContourHeights = new float[ rect.Width * rect.Height ];
            float[] innerContourHeights = new float[ rect.Width * rect.Height ];
            float[] onlyOuters = null;
            float[] onlyInners = null;
            int innerCount = 0;
            int outerCount = 0;

            Parallel.For(
                0, subData.Rows, i =>
                {
                    for ( int j = 0; j < subData.Cols; ++j )
                    {
                        double distance = cv.Cv2.PointPolygonTest( contour, new cv.Point2f( i + compensatedX, j + compensatedY ), false );

                        lock ( lockObject )
                        {
                            if ( distance < 0.0 )
                            {
                                outerContourHeights[ outerCount ] = subDataIndexer[ i, j ];
                                outerCount++;
                            }
                            else
                            {

                                innerContourHeights[ innerCount ] = subDataIndexer[ i, j ];
                                innerCount++;
                            }
                        }
                    }
                }
            );

            onlyOuters = new float[ outerCount ];
            onlyInners = new float[ innerCount ];

            Array.Copy( outerContourHeights, onlyOuters, outerCount );
            Array.Copy( innerContourHeights, onlyInners, innerCount );

            float surfaceAverage = onlyOuters.Average();
            heightMax = onlyInners.Max();
            heightMin = onlyInners.Min();
            heightAverage = onlyInners.Average();

            Parallel.For(
                0, onlyInners.Length, i =>
                {
                    lock ( lockObject )
                    {
                        if ( onlyInners[ i ] <= surfaceAverage )
                        {
                            upperCount++;
                        }
                        else
                        {
                            lowerCount++;
                        }
                    }
                }
            );

            if ( type.Equals( "Intensity" ) )
            {
                heightMin *= 0.344f;
                heightMax *= 0.344f;
                heightAverage *= 0.344f;
            }

            using ( StreamWriter file = new StreamWriter( fullPath, true ) )
            {
                file.WriteLine( "Length,Width,Height_Min,Height_Max,Height_Average," + 
                                "Center_X,Center_Y,Upper_Count,Lower_Count,Upper_Area,Upper_Area" );
                file.WriteLine( "{0:F3},{1:F3},{2:F3},{3:F3},{4:F3},{5},{6},{7},{8},{9:F3},{10:F3}",
                                length * 0.258f, width * 0.258f, heightMin, heightMax, heightAverage,
                                centerX, centerY, upperCount, lowerCount, upperArea, lowerArea );
            }
        }

        #endregion Private Methods

        #region Constructors

        public SurfaceProcessingViewModel()
        {
            Type = string.Empty;

            LoadSurface += new Action( InnerLoadSurface );
            // SubtractMap += new Action( InnerSubtractMap );
            FindContours += new Action( InnerFindContours );
        }

        #endregion Constructors

        #region Public Properties

        public cv.Mat IntensitySurfaceData
        {
            get;
            set;
        }

        public cv.Mat PhaseSurfaceData
        {
            get;
            set;
        }

        public cv.Mat ProcessedIntensityData
        {
            get;
            set;
        }

        public cv.Mat ProcessedPhaseData
        {
            get;
            set;
        }

        public cv.Point[] Contour
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public int CurrentWidth
        {
            get;
            set;
        }

        public int CurrentHeight
        {
            get;
            set;
        }

        public string LoadType
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Public Methods

        public Action<cv.Mat> SetSurface3DData
        {
            get;
            set;
        }

        public Action<cv.Mat> SetSurface2DData
        {
            get;
            set;
        }

        public Action<float[]> SetProfileXData
        {
            get;
            set;
        }

        public Action<float[]> SetProfileYData
        {
            get;
            set;
        }

        public Action LoadSurface
        {
            get;
            set;
        }

        public Action SubtractMap
        {
            get;
            set;
        }

        public Action FindContours
        {
            get;
            set;
        }

        #endregion Public Methods
    }
}
