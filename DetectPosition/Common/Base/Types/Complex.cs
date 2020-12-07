using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Types
{
    public class Complex
    {
        #region Private Member Variables

        private float _real;
        private float _imag;

        #endregion Private Member Variables

        #region Public Properties

        public float Real
        {
            get => _real;
            set => _real = value;
        }

        public float Imag
        {
            get => _imag;
            set => _imag = value;
        }

        public float Amplitude
        {
            get => (float)Math.Sqrt( _real * _real + _imag * _imag );
        }

        public float AmplitudeLog
        {
            get => (float)Math.Log( Amplitude, 2 );
        }

        public float Phase
        {
            get => (float)Math.Atan2( _imag, _real );
        }

        #endregion Public Properties
    }
}
