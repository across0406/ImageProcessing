using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Base.Controls.ImageViewer
{
    public class DisplayInfo : Base.BasePropertyChanged
    {
        #region Private Member Variables

        private EOverlayType _overlayTypeInfo;
        private System.Windows.Rect _position;
        private Color _colorInfo;
        private long _thickness;
        private string _displayText;

        #endregion Private Member Variables

        #region Constructor

        public DisplayInfo()
        {
            OverlayTypeInfo = EOverlayType.POINT;
            Position = new System.Windows.Rect( 0, 0, 0, 0 );
            ColorInfo = Color.FromArgb( 255, 255, 0, 0 );
            Thickness = 1;
            DisplayText = string.Empty;
        }

        #endregion Constructor

        #region Public Properties

        public EOverlayType OverlayTypeInfo
        {
            get => _overlayTypeInfo;
            set
            {
                _overlayTypeInfo = value;
                OnPropertyChanged( "OverlayTypeInfo" );
            }
        }

        public System.Windows.Rect Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged( "Position" );
            }
        }

        public Color ColorInfo
        {
            get => _colorInfo;
            set
            {
                _colorInfo = value;
                OnPropertyChanged( "ColorInfo" );
            }
        }
        public long Thickness
        {
            get => _thickness;
            set
            {
                _thickness = value;
                OnPropertyChanged( "Thickness" );
            }
        }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                OnPropertyChanged( "DisplayText" );
            }
        }

        #endregion Public Properties
    }
}
