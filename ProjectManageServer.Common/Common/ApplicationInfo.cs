using log4net.Repository;

namespace ProjectManageServer.Common
{
    public class ApplicationInfo
    {

        private static ApplicationInfo _CurrentApplicationInfo = null;

        /// <summary>
        /// 单例
        /// </summary>
        public static ApplicationInfo CurrentApplicationInfo
        {
            get
            {
                if (_CurrentApplicationInfo == null)
                {
                    _CurrentApplicationInfo = new ApplicationInfo();
                }
                return _CurrentApplicationInfo;
            }
            set
            {
                _CurrentApplicationInfo = value;
            }
        }

        /// <summary>
        /// 数据库连接串
        /// </summary>
        public string ConnString { get; set; } = string.Empty;

        /// <summary>
        /// 日志服务器
        /// </summary>
        public string LogConnString { get; set; } = string.Empty;

    }
}
