using Dapper;
using Oracle.ManagedDataAccess.Client;
using ProjectManageServer.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProjectManageServer.DataAccess
{
    public class MethodGetSerial
    {

        #region 根据流水表的主键直接获取流水号
        /// <summary>
        /// 根据流水表的主键直接获取流水号
        /// 如果obj不存在则插入
        /// 返回值 1  说明是新插入的流水行值
        /// </summary>
        /// <param name="obj">对象名</param>
        /// <param name="connString">连接串</param>
        /// <returns></returns>
        public static int getSerialNumInt(string obj, IDbTransaction transaction = null, IDbConnection dbConnection = null)
        {
            int type = 1;
            var par = new DynamicParameters();
            par.Add("@keyCode", obj);
            par.Add("@Type", type);
            par.Add("@SerialNum", 0,DbType.Int32, ParameterDirection.Output);
            par.Add("@SerialLength",0, DbType.Int32, ParameterDirection.Output);
            DynamicParameters Last_par = (DynamicParameters)AppDataBase.QueryStoredProcedure("GetSerialNum", par, transaction, dbConnection);

            return Last_par.Get<int>("@SerialNum");
        }

        #endregion
    }
}
