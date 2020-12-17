using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SurfaceProcessing
{
    /// <summary>
    /// SizeCropWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SizeCropWindow : Window, INotifyPropertyChanged
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

        private SurfaceProcessingViewModel _parentViewModel;

        private int _startX;
        private int _startY;
        private int _sizeWidth;
        private int _sizeHeight;

        #endregion Private Member Variables

        #region Private Methods

        private void DownMouseLeftButton( object sender, MouseButtonEventArgs e )
        {
            base.OnMouseLeftButtonDown( e );
            this.DragMove();
        }

        private void ClickOK( object sender, RoutedEventArgs e )
        {
            if ( _parentViewModel == null )
            {
                this.DialogResult = false;
            }
            else
            {
                // _parentViewModel.StartXManualCrop = StartX;
                // _parentViewModel.StartYManualCrop = StartY;
                // _parentViewModel.SizeWidthManualCrop = SizeWidth;
                // _parentViewModel.SizeHeightManualCrop = SizeHeight;
                this.DialogResult = true;
            }

            Close();
        }

        private void ClickCancel( object sender, RoutedEventArgs e )
        {
            this.DialogResult = false;
            Close();
        }

        #endregion Private Methods

        #region Constructors

        public SizeCropWindow( SurfaceProcessingViewModel parentViewModel )
        {
            InitializeComponent();

            _parentViewModel = parentViewModel;

            this.DataContext = this;
        }

        #endregion Constructors

        #region Public Properties

        public int StartX
        {
            get => _startX;
            set
            {
                _startX = value;
                OnPropertyChanged( "StartX" );
            }
        }

        public int StartY
        {
            get => _startY;
            set
            {
                _startY = value;
                OnPropertyChanged( "StartY" );
            }
        }

        public int SizeWidth
        {
            get => _sizeWidth;
            set
            {
                _sizeWidth = value;
                OnPropertyChanged( "SizeWidth" );
            }
        }

        public int SizeHeight
        {
            get => _sizeHeight;
            set
            {
                _sizeHeight = value;
                OnPropertyChanged( "SizeHeight" );
            }
        }

        #endregion Public Properties
    }
}
