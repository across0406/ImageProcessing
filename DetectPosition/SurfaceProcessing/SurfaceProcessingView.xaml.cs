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
using cv = OpenCvSharp;

namespace SurfaceProcessing
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SurfaceProcessingView : Window
    {
        #region Private Member Variables

        private SurfaceProcessingViewModel _viewModel;

        #endregion Private Member Variables

        #region Private Methods

        private void ClickOpenIntensity( object sender, RoutedEventArgs e )
        {
            _viewModel?.LoadSurface?.Invoke();
        }

        private void ClickSubtractMap( object sender, RoutedEventArgs e )
        {
            _viewModel?.SubtractMap?.Invoke();
        }

        private void ClickFindContours( object sender, RoutedEventArgs e )
        {
            _viewModel?.FindContours?.Invoke();
        }

        #endregion Private Methods

        #region Constructors

        public SurfaceProcessingView()
        {
            InitializeComponent();

            ViewModel = new SurfaceProcessingViewModel();
        }

        #endregion Constructors

        #region Public Properties

        public SurfaceProcessingViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;

                if ( _viewModel == null )
                {
                    this.DataContext = null;
                }
                else
                {
                    _viewModel.SetSurface3DData += new Action<cv.Mat>( _surface3DViewer.SetData );
                    _viewModel.SetSurface2DData += new Action<cv.Mat>( _surface2DViewer.SetView );
                    _viewModel.SetProfileXData += new Action<float[]>( _profileX.SetData );
                    _viewModel.SetProfileYData += new Action<float[]>( _profileY.SetData );

                    this.DataContext = _viewModel;
                }
            }
        }


        #endregion Public Properties
    }
}
