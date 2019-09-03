using Dapper;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ProjectManageServer.Common
{
    public class AppDataBase
    {

        public IDbConnection connection=null;

        public  AppDataBase()
        {
            connection = GetDbConnection(ConnStrings);
        }

        #region 字段属性
        private static readonly object SyncObject = new object();

        private static readonly int CommandTimeout = 300;//1200

        private static readonly bool Buffered = true;

        private static IDbConnection _dbConection;

        private static Dictionary<string, IDbConnection> _dbConnectionDict = new Dictionary<string, IDbConnection>();

        public static string ConnStrings { get; set; }

        public static string LogConnString { get; set; }

        public static IDbConnection DbConection
        {
            get
            {
                bool flag = AppDataBase._dbConection == null;

                if (flag)
                {
                    object syncObject = AppDataBase.SyncObject;

                    lock (syncObject)
                    {
                        AppDataBase._dbConection = AppDataBase.GetDbConnection(AppDataBase.ConnStrings);
                    }
                }
                return AppDataBase._dbConection;
            }
        }

        #endregion

        #region 方法函数

        private static IDbConnection GetDbConnection(string conectStrName)
        {
            bool flag = string.IsNullOrEmpty(conectStrName);

            if (flag)
            {
                conectStrName = ConnStrings;
            }
            bool flag2 = AppDataBase._dbConnectionDict.ContainsKey(conectStrName);

            bool flag3 = flag2;

            IDbConnection result;

            if (!AppDataBase._dbConnectionDict.TryGetValue(conectStrName, out result))
            {  
                object syncObject = AppDataBase.SyncObject;

                lock (syncObject)
                {
                    IDbConnection dbConnection;
                    if (conectStrName.Contains("DBType=SqlServer"))
                    {
                        conectStrName = conectStrName.Replace("DBType=SqlServer", "");
                        dbConnection = new SqlConnection(conectStrName);
                    }

                    else if (conectStrName.Contains("DBType=MySql"))
                    {
                        conectStrName = conectStrName.Replace("DBType=MySql", "");
                        dbConnection = new MySqlConnection(conectStrName);
                    }

                    else if (conectStrName.Contains("DBType=Oracle"))
                    {
                        conectStrName = conectStrName.Replace("DBType=Oracle", "");
                        dbConnection = new OracleConnection(conectStrName);
                    }
                    else
                    {
                        dbConnection = new SqlConnection(conectStrName);
                    }
                
                   

                  /*  if (flag5)
                    {
                        conectStrName = conectStrName.Replace("DBType=SqlServer", "");

                        dbConnection = new SqlConnection(conectStrName);
                    }
                    else
                    {
                        conectStrName = conectStrName.Replace("DBType=Oracle", "");//MySql

                        //dbConnection = new MySqlConnection(conectStrName);

                        dbConnection = new OracleConnection(conectStrName);                 }*/

                    result = dbConnection;
                }
            }
            return result;
        }

        public static object QueryStoredProcedure(string strSql,object param, IDbTransaction trans = null, IDbConnection dbConnection = null)
        {
            if (dbConnection == null)
                dbConnection = AppDataBase.GetDbConnection(ConnStrings);
            try
            {
                dbConnection.Query(strSql, param, null, Buffered, null, CommandType.StoredProcedure).FirstOrDefault();
                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, "成功", param);
            }
            catch (Exception e)
            {
                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, $"失败:{e.Message}", param);
            }
            return param;
        }

        public static IEnumerable<object> Query(string strSql, object param = null, IDbTransaction trans = null, IDbConnection dbConnection = null)
        {
            string result = "成功";
            if (dbConnection == null)
                dbConnection = AppDataBase.GetDbConnection(ConnStrings);
            try
            {
              
                IEnumerable<object> enumerable = dbConnection.Query(typeof(Object), strSql, param, trans, Buffered, CommandTimeout, CommandType.Text); //dbConnection.Query(strSql, param, trans, Buffered, CommandTimeout, CommandType.Text);

                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, result, param);

                return enumerable;

            }
            catch (Exception ex)
            {
                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, $"失败:{ex.Message}", param);
            }
            return null;
        }

        public static IEnumerable<T> Query<T>(string strSql, object param = null, IDbTransaction trans = null, IDbConnection dbConnection=null)
        {
            string result = "成功";
            if (dbConnection == null)
                dbConnection = AppDataBase.GetDbConnection(ConnStrings);
            
            IEnumerable<T> result2;

            try
            {
                IEnumerable<T> enumerable = dbConnection.Query<T>(strSql, param, trans, Buffered, CommandTimeout, CommandType.Text);

                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, result, param);

                result2 = enumerable;

                return result2;

            }
            catch (Exception ex)
            {
                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, $"失败:{ex.Message}", param);
            }

            return result2 = null;
        }

        public static T QuerySingle<T>(string strSql, object param = null, IDbTransaction trans = null, IDbConnection dbConnection = null)
        {
            string result = "成功";
            if (dbConnection == null)
                dbConnection = AppDataBase.GetDbConnection(ConnStrings);

            try
            {

                IEnumerable<T> enumerable = dbConnection.Query<T>(strSql, param, trans,Buffered, CommandTimeout, CommandType.Text);

                if (enumerable!=null)
                {
                    AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, result, param);

                    return enumerable.FirstOrDefault();
                }
                else
                {
                    AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, $"失败:{enumerable}未找到数据!", param);

                    return default(T);
                }

                //T result2 = dbConnection.QuerySingle<T>(strSql, param, trans, CommandTimeout, CommandType.Text);

                //AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, result, param);

                //return result2;
            }
            catch (Exception ex)
            {
                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, $"失败:{ex.Message}", param);

                return default(T);
            }
        }

        public static Object ExecuteScalar(string sql, IDbConnection dbConnection = null, object param = null, IDbTransaction trans = null)
        {
            string result = "成功";
            if (dbConnection == null)
                dbConnection = AppDataBase.GetDbConnection(ConnStrings);
            //dbConnection.Open();
            //trans = dbConnection.BeginTransaction();

            try
            {
                object obj = dbConnection.ExecuteScalar(sql, param, trans, CommandTimeout, CommandType.Text);
                AppLog.WriteDbLog(dbConnection.ConnectionString, sql, result, param);

                //trans.Commit();
                return obj == null ? "" : obj;

            }
            catch (Exception ex)
            {
                //trans.Rollback();
                AppLog.WriteDbLog(dbConnection.ConnectionString, sql, $"失败:{ex.Message}", param);
                throw ex;
            }
        }

        public static int Execute(string sql, object param = null, IDbTransaction trans = null, IDbConnection dbConnection = null)
        {
            string result = "成功";

            if (dbConnection == null)
                dbConnection = AppDataBase.GetDbConnection(ConnStrings);

            try
            {             

                int num = dbConnection.Execute(sql, param, trans, CommandTimeout, CommandType.Text);

                AppLog.WriteDbLog(dbConnection.ConnectionString, sql, result, param);

                return num;
            }
            catch (Exception ex)
            {
                
                AppLog.WriteDbLog(dbConnection.ConnectionString, sql, $"失败:{ex.Message}", param);
                throw ex;
            }
            //return 0;
        }

        public static IEnumerable<T> QueryStored<T>(string storedName, object param)
        {
            string result = "成功";

            IDbConnection dbConnection = AppDataBase.GetDbConnection(ConnStrings);

            IEnumerable<T> result2 = null;

            try
            {

                bool flag = !string.IsNullOrEmpty(storedName);

                if (flag)
                {
                    bool flag2 = param != null;

                    if (flag2)
                    {
                        result2 = dbConnection.Query<T>(storedName, param, null, Buffered, CommandTimeout, CommandType.StoredProcedure);
                    }
                    else
                    {
                        result2 = dbConnection.Query<T>(storedName, null, null, Buffered, CommandTimeout, CommandType.StoredProcedure);
                    }

                    AppLog.WriteDbLog(dbConnection.ConnectionString, storedName, result, param);
                }

                return result2;

            }
            catch (Exception ex)
            {
                AppLog.WriteDbLog(dbConnection.ConnectionString, storedName, $"失败:{ex.Message}", param);
            }

            return result2;

        }

        public static Tuple<IEnumerable<T1>, IEnumerable<T2>> Query<T1, T2>(string sql, object param)
        {
            string result = "成功";

            IDbConnection dbConnection = AppDataBase.GetDbConnection(ConnStrings);

            IEnumerable<T1> enumerable = null;

            IEnumerable<T2> enumerable2 = null;

            bool flag = !string.IsNullOrEmpty(sql);

            if (flag)
            {
                try
                {
                    using (SqlMapper.GridReader gridReader = dbConnection.QueryMultiple(sql, param, null, CommandTimeout, CommandType.Text))
                    {
                        enumerable = gridReader.Read<T1>(true);

                        enumerable2 = gridReader.Read<T2>(true);

                        AppLog.WriteDbLog(dbConnection.ConnectionString, sql, result, param);
                    }

                }
                catch (Exception ex)
                {
                    AppLog.WriteDbLog(dbConnection.ConnectionString, sql, $"失败:{ex.Message}", param);
                }
            }

            return Tuple.Create<IEnumerable<T1>, IEnumerable<T2>>(enumerable, enumerable2);

        }

        public static string ExecuteTrans(List<string> list, List<DynamicParameters> parms = null)
        {
            string result = "成功";

            string connectstring = ConnStrings.Replace("DBType=SqlServer", "").Replace("DBType=Oracle", "").Replace("DBType=MySql", "");

            using (IDbConnection dbConnection = new SqlConnection(connectstring))
            {
                dbConnection.Open();

                IDbTransaction dbTransaction = dbConnection.BeginTransaction();

                try
                {
                    int num = 0;

                    using (List<string>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;

                            bool flag2 = parms != null && parms.Count - 1 >= num;

                            if (flag2)
                            {
                                dbConnection.Execute(current, parms[num], dbTransaction, CommandTimeout, CommandType.Text);
                            }
                            else
                            {
                                dbConnection.Execute(current, null, dbTransaction, CommandTimeout, CommandType.Text);
                            }

                            num++;
                        }
                    }

                    dbTransaction.Commit();

                    AppLog.WriteDbLog(dbConnection.ConnectionString, string.Join(",", list.ToArray()), result, null);

                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();

                    AppLog.WriteDbLog(dbConnection.ConnectionString, string.Join(",", list.ToArray()), result + "->" + ex.Message, null);

                    return result;
                }
            }

            return "OK";
        }

        public static DataTable GetTable(string strSql, object param = null, IDbTransaction trans = null, string connectString = "")
        {
            string result = "成功";

            IDbConnection dbConnection = AppDataBase.GetDbConnection(connectString);

            DataTable dataTable = new DataTable(); 

            try
            {
                using (IDataReader dataReader = dbConnection.ExecuteReader(strSql, param, trans))
                {
                    dataTable.Load(dataReader);

                    AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, result, param);

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteDbLog(dbConnection.ConnectionString, strSql, $"失败：{ex.Message}", param);
            }

            return dataTable;
        }

        public static DataSet GetDataSet(string strSql, object param = null, IDbTransaction trans = null, string connectString = "")
        {
            IDbConnection dbConnection = AppDataBase.GetDbConnection(connectString);

            DataSet dataSet = new DataSet();

            using (IDataReader dataReader = dbConnection.ExecuteReader(strSql, param, trans))
            {
                dataSet.Load(dataReader, LoadOption.OverwriteChanges, null, new DataTable[0]);
            }

            return dataSet;
        }

        #endregion

    }

}
