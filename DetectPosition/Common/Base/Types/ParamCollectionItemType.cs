using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Types
{
    public class ParamCollectionItemType : Base.BasePropertyChanged
    {
        #region Private Member Variables

        private string _paramName;
        private string _paramValue;

        #endregion Private Member Variables

        #region Constructor

        public ParamCollectionItemType()
        {

        }

        #endregion Consturctor

        #region Public Properties

        public string ParamName
        {
            get => _paramName;
            set
            {
                _paramName = value;
                OnPropertyChanged( "ParamName" );
            }
        }
        public string ParamValue
        {
            get => _paramValue;
            set
            {
                _paramValue = value;
                OnPropertyChanged( "ParamValue" );
            }
        }

        #endregion Public Properties
    }
}
