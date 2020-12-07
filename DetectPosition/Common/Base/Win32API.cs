using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Base
{
    public class Win32API
    {
        #region Private Static Member Variables

        private static long _freq;
        private static long _timeStart;
        private static long _end;
        private static double _timeElapsed = 0;

        #endregion Private Static Member Variables

        #region Protected Static Methods

        [DllImport( "kernel32.dll" )]
        extern static short QueryPerformanceCounter( ref long x );

        [DllImport( "kernel32.dll" )]
        extern static short QueryPerformanceFrequency( ref long x );

        #endregion Protected Static Methods

        #region Public Static Methods

        [DllImport( "kernel32.dll", EntryPoint = "LoadLibrary" )]
        public static extern IntPtr LoadLibrary( [MarshalAs( UnmanagedType.LPStr )] string lpLibFileName );

        [DllImport( "kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi )]
        public static extern IntPtr GetProcAddress( IntPtr hModule, [MarshalAs( UnmanagedType.LPStr )] string lpProcName );

        [DllImport( "kernel32.dll", EntryPoint = "FreeLibrary" )]
        public static extern bool FreeLibrary( IntPtr hModule );


        [DllImport( "user32.dll", EntryPoint = "SendMessageA" )]
        public static extern Int32 SendMessage( Int32 hWnd, Int32 Msg, Int32 wParam, Int32 lParam );

        public static void CheckTimeStart()
        {
            // start
            if ( QueryPerformanceFrequency( ref _freq ) > 0 )
                QueryPerformanceCounter( ref _timeStart );
        }

        public static string CheckTimeEnd()
        {
            // stop
            QueryPerformanceCounter( ref _end );
            _timeElapsed = (double)( _end - _timeStart ) / (double)_freq * 1000.0;
            return String.Format( "{0,0:F3}", _timeElapsed );
        }

        #endregion Public Static Methods
    }
}
