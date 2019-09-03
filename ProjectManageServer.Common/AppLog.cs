using Dapper;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ProjectManageServer.Common
{
    public class AppLog
    {
        private static readonly ILog log = LogManager.GetLogger(LogProperties.LogRepository.Name, typeof(AppLog));

        public static void WriteLog(string info)
        {
            AppLog.WriteLogAsyn(info, AppLog.log);
        }

        public static void WriteLog(Exception se)
        {
            AppLog.WriteLogAsyn(se.ToString(), AppLog.log);
        }

        private static void WriteLogAsyn(string info, ILog log)
        {
            Thread thread = new Thread(() =>
            {
                bool isInfoEnabled = log.IsInfoEnabled;

                if (isInfoEnabled)
                {
                    log.Info(info);
                }

            });

            thread.IsBackground = true;

            thread.Start();
        }

        public static void WriteDbLog(string db, string strSql, string result, object paramters)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("\r\n=========================================开始=====================================================\r\n");

            stringBuilder.AppendLine($"数据库:{db}");

            stringBuilder.AppendFormat("查询语句：{0}", strSql);

            bool flag = paramters != null;

            if (flag)
            {
                StringBuilder stringBuilder2 = new StringBuilder();

                Type type = paramters.GetType();

                bool flag2 = type == typeof(DynamicParameters);

                if (flag2)
                {
                    DynamicParameters dynamicParameters = paramters as DynamicParameters;

                    using (IEnumerator<string> enumerator = dynamicParameters.ParameterNames.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;

                            string currentvalue = (dynamicParameters.Get<object>(current) != null) ? dynamicParameters.Get<object>(current).ToString() : string.Empty;

                            stringBuilder2.Append($"\t参数名：{current}<->参数值：{currentvalue}");
                        }
                    }

                    stringBuilder.AppendLine($"\r\n参数:{stringBuilder2.ToString()}");

                }

            }

            stringBuilder.AppendLine($"执行结果:{result}");

            stringBuilder.AppendLine("\r\n=========================================结束=====================================================");

            AppLog.WriteLog(stringBuilder.ToString());
        }
    }
}
