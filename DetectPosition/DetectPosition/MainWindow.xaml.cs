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
    public partial class MainWindow : Window
    {
        #region Private Member Variables

        private MainViewModel _viewModel;

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

        private void ApplyMedianlick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyMedian?.Invoke();
        }

        private void ApplyMSERClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyMSER?.Invoke();
        }

        private void ApplyDFTClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyDFT?.Invoke();
        }

        private void ApplyBitwiseClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyBitwise?.Invoke();
        }

        private void ApplyQuantizationArithmatic( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyQuantizationArithmatic?.Invoke();
        }

        private void ApplyGuidedFilter( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyGuidedFilter?.Invoke();
        }

        private void DiffMasterSourceClick( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplySubtractMasterSource?.Invoke();
        }

        private void ApplyCubicSpline( object sender, RoutedEventArgs e )
        {
            ViewModel?.ApplyCubicSpline?.Invoke();
        }

        #endregion Private Methods

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }

        #endregion Constructors

        #region Public Properties

        public MainViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                this.DataContext = _viewModel;
            }
        }

        #endregion Public Properties
    }
}
