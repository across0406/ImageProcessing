using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// SizeInputWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SizeInputTypeWindow : Window, INotifyPropertyChanged
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

        private int _sizeWidth;
        private int _sizeHeight;

        private ObservableCollection<string> _types;
        private string _selectedType;

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
                _parentViewModel.CurrentWidth = SizeWidth;
                _parentViewModel.CurrentHeight = SizeHeight;
                _parentViewModel.LoadType = SelectedType;
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

        public SizeInputTypeWindow( SurfaceProcessingViewModel parentViewModel )
        {
            InitializeComponent();

            _parentViewModel = parentViewModel;

            Types = new ObservableCollection<string>();
            Types.Add( "Intensity" );
            Types.Add( "Phase" );
            SelectedType = Types[ 0 ];

            this.DataContext = this;
        }

        #endregion Constructors

        #region Public Properties

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

        public ObservableCollection<string> Types
        {
            get => _types;
            set => _types = value;
        }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged( "SelectedType" );
            }
        }

        #endregion Public Properties
    }
}
