using log4net.Repository; 

namespace ProjectManageServer.Common
{
    public class LogProperties
    {
        public static ILoggerRepository LogRepository
        {
            get;
            set;
        }

    }
}
