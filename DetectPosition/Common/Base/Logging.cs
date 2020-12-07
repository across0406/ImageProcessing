using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Base
{
    public sealed class Logging : IDisposable
    {
        #region Log Queue Limit Constant

        private const int QUEUE_LIMIT = 10;

        #endregion Log Queue Limit Constant

        #region Logging Type Enumeration

        public enum ELoggingType { Debug, Info, Warn, Error, Fatal };

        #endregion Logging Type Enumeration

        #region IDisposable Implementation

        private bool disposedValue = false; // 중복 호출을 검색하려면

        void Dispose( bool disposing )
        {
            if ( !disposedValue )
            {
                if ( disposing )
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.

                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~Log()
        // {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose( true );
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Implementation

        #region Singleton Implementation
        private static Logging _instance = new Logging();
        public static Logging Instance
        {
            get => _instance;
        }

        #endregion Singleton Implementation

        #region Private Members Variables

        private string _path;
        private ILog _logger;

        #endregion Private Members Variables

        #region Private Methods

        private void InnerAddLog( string message )
        {
            AddLog?.Invoke( message );
            // if ( AddLog == null )
            // {
            //     MessageBox.Show( message );
            // }
            // else
            // {
            //     AddLog?.Invoke( message );
            // }
        }

        #endregion Private Methods

        #region Constructor

        public Logging()
        {
            // Initialize( @"D:\Log\", "HSI-300" );
        }

        #endregion Constructor

        #region Public Properties

        public string Path
        {
            get => _path; set => _path = value;
        }
        public ILog Logger
        {
            get => _logger; set => _logger = value;
        }

        #endregion Public Properties

        #region Public Methods

        public bool Initialize( string path, string id )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool initializeResult = false;

            try
            {
                log4net.Config.XmlConfigurator.Configure();

                Logger = LogManager.GetLogger( id );
                Path = path;

                Logger.Info( "        =============  Started Logging  =============        " );

                initializeResult = true;
            }
            catch ( Exception ex )
            {
                string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                            "System Exception: " + ex.Message;

                InnerAddLog( exceptionMessage );

                initializeResult = false;
            }

            return initializeResult;
        }

        public bool Write( string message, ELoggingType type = ELoggingType.Debug )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool logResult = false;

            if ( Logger == null || Path == null )
            {
                string errorMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                        "Logger or path is null.";

                InnerAddLog( errorMessage );

                logResult = false;
            }
            else
            {
                try
                {
                    switch ( type )
                    {
                        case ELoggingType.Debug:
                            Logger.Debug( message );
                            break;

                        default:
                        case ELoggingType.Info:
                            Logger.Info( message );
                            break;

                        case ELoggingType.Warn:
                            Logger.Warn( message );
                            break;

                        case ELoggingType.Error:
                            Logger.Error( message );
                            break;

                        case ELoggingType.Fatal:
                            Logger.Fatal( message );
                            break;
                    }

                    InnerAddLog( message );

                    logResult = true;
                }
                catch ( Exception ex )
                {
                    string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                                "System Exception: " + ex.Message;

                    InnerAddLog( exceptionMessage );

                    logResult = false;
                }
            }

            return logResult;
        }

        public bool Debug( string message )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool logResult = false;

            if ( Logger == null || Path == null )
            {
                string errorMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                        "Logger or path is null.";

                InnerAddLog( errorMessage );

                logResult = false;
            }
            else
            {
                try
                {
                    Logger.Debug( message );
                    InnerAddLog( message );

                    logResult = true;
                }
                catch ( Exception ex )
                {
                    string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                                "System Exception: " + ex.Message;

                    InnerAddLog( exceptionMessage );

                    logResult = false;
                }
            }

            return logResult;
        }

        public bool Info( string message )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool logResult = false;

            if ( Logger == null || Path == null )
            {
                string errorMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                        "Logger or path is null.";

                InnerAddLog( errorMessage );

                logResult = false;
            }
            else
            {
                try
                {
                    Logger.Info( message );
                    InnerAddLog( message );

                    logResult = true;
                }
                catch ( Exception ex )
                {
                    string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                                "System Exception: " + ex.Message;

                    InnerAddLog( exceptionMessage );

                    logResult = false;
                }
            }

            return logResult;
        }

        public bool Warn( string message )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool logResult = false;

            if ( Logger == null || Path == null )
            {
                string errorMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                        "Logger or path is null.";

                InnerAddLog( errorMessage );

                logResult = false;
            }
            else
            {
                try
                {
                    Logger.Warn( message );
                    InnerAddLog( message );

                    logResult = true;
                }
                catch ( Exception ex )
                {
                    string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                                "System Exception: " + ex.Message;

                    InnerAddLog( exceptionMessage );

                    logResult = false;
                }
            }

            return logResult;
        }

        public bool Error( string message, bool isExiting = false )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool logResult = false;

            if ( Logger == null || Path == null )
            {
                string errorMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                        "Logger or path is null.";

                if ( isExiting )
                {
                    InnerAddLog( errorMessage );
                }

                logResult = false;
            }
            else
            {
                try
                {
                    Logger.Error( message );

                    if ( isExiting )
                    {
                        InnerAddLog( message );
                    }

                    logResult = true;
                }
                catch ( Exception ex )
                {
                    string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                                "System Exception: " + ex.Message;

                    if ( isExiting )
                    {
                        InnerAddLog( exceptionMessage );
                    }

                    logResult = false;
                }
            }

            return logResult;
        }

        public bool Fatal( string message )
        {
            MethodBase methodBase = MethodBase.GetCurrentMethod();
            bool logResult = false;

            if ( Logger == null || Path == null )
            {
                string errorMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                        "Logger or path is null.";

                InnerAddLog( errorMessage );

                logResult = false;
            }
            else
            {
                try
                {
                    Logger.Fatal( message );
                    InnerAddLog( message );

                    logResult = true;
                }
                catch ( Exception ex )
                {
                    string exceptionMessage = methodBase.ReflectedType + "." + methodBase.Name + " -> " +
                                                "System Exception: " + ex.Message;

                    InnerAddLog( exceptionMessage );

                    logResult = false;
                }
            }

            return logResult;
        }

        public Action<string> AddLog
        {
            get;
            set;
        }

        #endregion Public Methods
    }
}
