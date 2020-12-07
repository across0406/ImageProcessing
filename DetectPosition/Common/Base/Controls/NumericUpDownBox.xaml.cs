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

namespace Base.Controls
{
    /// <summary>
    /// NumericUpDown.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NumericUpDownBox : UserControl
    {
        #region Private Member Variables

        private int _numValue;

        #endregion Private Member Variables

        #region Private Methods

        private void NumChanged( object sender, TextChangedEventArgs e )
        {
            if ( txtNum == null )
            {
                return;
            }

            if ( int.TryParse( txtNum.Text, out _numValue ) )
            {
                txtNum.Text = _numValue.ToString();
            }
        }

        private void NumUp( object sender, RoutedEventArgs e )
        {
            NumValue += 1;
        }

        private void NumDown( object sender, RoutedEventArgs e )
        {
            NumValue -= 1;
        }

        #endregion Private Methods

        #region Constructors

        public NumericUpDownBox()
        {
            InitializeComponent();

            NumValue = 0;
        }

        #endregion Constructors

        #region Public Properties

        public int NumValue
        {
            get => _numValue;
            set
            {
                _numValue = value;
                txtNum.Text = _numValue.ToString();
            }
        }

        #endregion Public Properties
    }
}
