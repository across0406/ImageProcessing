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

namespace DetectPosition
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DetectPositionView : Window
    {
        #region Private Member Variables

        private DetectPositionViewModel _viewModel;

        #endregion Private Member Variables

        #region Private Methods

        private void ImageOpenClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ImageOpen?.Invoke();
        }

        private void MasterImageOpenClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.MasterImageOpen?.Invoke();
        }

        private void ApplyMSERClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyMSER?.Invoke();
        }

        private void ApplyGuidedFilter( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyGuidedFilter?.Invoke();
        }

        private void ClickDiffMasterSource( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplySubtractMasterSource?.Invoke();
        }

        private void OnSourceImageMouseRightButtonDown( object sender, MouseButtonEventArgs e )
        {
            ViewModel?.SaveSourceImage?.Invoke();
        }

        private void ApplyBilateralSubtract( object sender, RoutedEventArgs e )
        {
            _viewModel?.ApplyBilateralSubtract?.Invoke();
        }

        #endregion Private Methods

        #region Constructors

        public DetectPositionView()
        {
            InitializeComponent();
            ViewModel = new DetectPositionViewModel();
        }

        #endregion Constructors

        #region Public Properties

        public DetectPositionViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;

                if ( _viewModel == null )
                {
                }
                else
                {
                    _viewModel.SetHistogram += new Action<int[]>( _histogram.SetData );
                }

                this.DataContext = _viewModel;
            }
        }


        #endregion Public Properties
    }
}
