using System;
using System.Text;
using System.Windows;

using System.Runtime.InteropServices;

namespace Base
{
    public class IniFile
    {
        #region Dll Import

        [DllImport( "kernel32" )]
        private static extern int GetPrivateProfileString( string section, string key, string def, StringBuilder retVal, int Size, string filePat );

        [DllImport( "Kernel32" )]
        private static extern long WritePrivateProfileString( string section, string key, string val, string filePath );

        #endregion Dll import

        #region Private Member Variables

        private string _path;

        #endregion Private Member Variables

        #region Constructors

        public IniFile( string strPath )
        {
            _path = strPath;
        }

        #endregion Constructors

        #region Public Methods

        public void WriteValue( string section, string key, string writtenValue, string path )
        {
            WritePrivateProfileString( section, key, writtenValue, path );
        }

        public string ReadValue( string section, string key, string path )
        {
            StringBuilder temp = new StringBuilder( 2000 );
            int i = GetPrivateProfileString( section, key, "", temp, 2000, path );
            return temp.ToString();
        }

        public bool ReadRectValue( string section, string key, out Rect result )
        {
            char[] delimiterChars = { '=', ',', ' ' };
            string[] data = { this.ReadValue( section, key, _path ) };
            string[] sub_data = data[ 0 ].Split( delimiterChars );

            double dX, dY, dWidth, dHeight;

            if ( sub_data.Length >= 4 )
            {
                if ( double.TryParse( sub_data[ 0 ], out dX ) && double.TryParse( sub_data[ 1 ], out dY ) &&
                    double.TryParse( sub_data[ 2 ], out dWidth ) && double.TryParse( sub_data[ 3 ], out dHeight ) )
                {
                    result = new Rect( dX, dY, dWidth, dHeight );
                    return true;
                }
            }

            result = new Rect( 0, 0, 1, 1 );
            return false;
        }

        public bool ReadUShortValue( string section, string key, out ushort result )
        {
            string s = this.ReadValue( section, key, _path );
            return ushort.TryParse( s, out result );
        }

        public bool ReadIntValue( string section, string key, out int result )
        {
            string s = this.ReadValue( section, key, _path );
            return int.TryParse( s, out result );
        }

        public bool ReadDoubleValue( string section, string key, out double result )
        {
            string s = this.ReadValue( section, key, _path );
            return double.TryParse( s, out result );
        }

        public bool ReadBoolValue( string section, string key, out bool result )
        {
            string s = this.ReadValue( section, key, _path );
            return bool.TryParse( s, out result );
        }

        public bool ReadStringValue( string section, string key, out string result )
        {
            result = this.ReadValue( section, key, _path );
            return ( result.Length > 0 ) ? true : false;
        }

        public void WriteRectValue( string section, string key, Rect data )
        {
            this.WriteValue( section, key, data.ToString(), _path );
        }

        public void WriteIntValue( string section, string key, int data )
        {
            this.WriteValue( section, key, data.ToString(), _path );
        }

        public void WriteDoubleValue( string section, string key, double data )
        {
            this.WriteValue( section, key, data.ToString(), _path );
        }

        public void WriteBoolValue( string section, string key, bool data )
        {
            this.WriteValue( section, key, data.ToString(), _path );
        }

        public void WriteStringValue( string section, string key, string data )
        {
            this.WriteValue( section, key, data, _path );
        }

        public bool CheckFileAvailability( bool isLoad = false )
        {
            bool isChecked = false;
            bool isFileExisted = System.IO.File.Exists( _path );

            if ( isLoad )
            {
                isChecked = isFileExisted;
            }
            else
            {
                if ( isFileExisted )
                {
                    System.IO.File.Delete( _path );
                }

                isChecked = true;
            }

            return isChecked;
        }

        #endregion Public Methods
    }
}
