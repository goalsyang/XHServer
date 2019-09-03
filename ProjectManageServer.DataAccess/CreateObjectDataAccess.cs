using Dapper;
using ProjectManageServer.Common;
using ProjectManageServer.Model.CreateObject;
using ProjectManageServer.Model.ObjectCreate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectManageServer.DataAccess
{
    public class CreateObjectDataAccess
    {

        #region 读取对象
        public LoadObject LoadObject(string objectCode)
        {
            LoadObject loadObject = new LoadObject();

            string sql = "Select * from FLC_OBJECT where obj_code = :objectCode";
            var parm = new DynamicParameters();
            parm.Add(":objectCode", objectCode);
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {                   
                    List<FLC_OBJECT> fLC_OBJECTs = AppDataBase.Query<FLC_OBJECT>(sql, parm, transaction, dbConnection).ToList();

                    if (fLC_OBJECTs.Count == 0 || fLC_OBJECTs == null)
                        throw new Exception("对象获取失败");

                    dtObject dtObject = new dtObject();
                    dtObject.obj_code = fLC_OBJECTs[0].obj_code;
                    dtObject.is_Enable = fLC_OBJECTs[0].is_Enable;
                    dtObject.is_system = fLC_OBJECTs[0].is_system;
                    dtObject.is_enum = fLC_OBJECTs[0].is_enum;

                    sql = "Select count(*) from FLC_OBJECTS where obj_code = :objectCode and is_main = 0";
                    string tableNum = AppDataBase.ExecuteScalar(sql, dbConnection, parm, transaction).ToString();

                    if (tableNum == null || string.IsNullOrEmpty(tableNum))
                        throw new Exception("对象获取失败");

                    dtObject.tableNum = tableNum;

                    sql = "Select * from flc_lang where key like '{0}.%' or key = '{0}' order by key";
                    IEnumerable<FLC_LANG> fLC_LANGs = AppDataBase.Query<FLC_LANG>(string.Format(sql, objectCode), null, transaction, dbConnection);

                    loadObject.dtObject = dtObject;
                    loadObject.Language = fLC_LANGs;

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return loadObject;
        }
        #endregion

        #region 删除对象
        public void Object_Delete(string OBJ_CODE)
        {
            string sql = string.Empty;
            var parm = new DynamicParameters();

            #region 数据交互
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {

                    #region 检查对象是否被引用，如果被引用则不能被删除
                    sql = @"Select lang.value from flc_object_property pro
                            left join flc_lang lang  on lang.key=pro.obj_code  and lan='zn_CN'
                            where OBJ_PRO_OBJECT=:OBJ_PRO_OBJECT";

                    parm.Add(":OBJ_PRO_OBJECT", OBJ_CODE);
                    IEnumerable<object> value = AppDataBase.Query(sql, parm, transaction, dbConnection);
                    if (value != null && value.AsList().Count > 0)
                    {
                        string obj_using = "";
                        foreach (object item in value)
                        {
                            obj_using += item.ToString() + ",";
                        }

                        if (obj_using.Length > 0)
                            obj_using = obj_using.Substring(0, obj_using.Length - 1);
                        throw new Exception("对象已被" + obj_using + "引用，不允许删除！");
                    }
                    #endregion

                    #region 检查对象是否被使用过，如果有数据则不允许删除
                    sql = string.Format("select Count(*) from data_{0}", OBJ_CODE);
                    object o = AppDataBase.ExecuteScalar(sql, dbConnection, null, transaction);
                    if (o != null && o.ToString() != "0")
                    {
                        throw new Exception("对象已经使用，请删除对象数据后重试！");
                    }
                    #endregion

                    #region 删除对象数据
                    parm = new DynamicParameters();
                    parm.Add(":OBJ_CODE", OBJ_CODE);

                    sql = "select obj_table from flc_objects where obj_code=:OBJ_CODE";
                    IEnumerable<object> obj_tables = AppDataBase.Query(sql, parm, transaction, dbConnection);

                    //主表
                    sql = "delete flc_object where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm,transaction,dbConnection);

                    sql = "delete from FLC_OBJECT  where obj_code like '{0}_%'";//删除子表对象
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE), null, transaction, dbConnection);

                    //子表
                    sql = "delete flc_objects where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    sql = "delete from FLC_OBJECTS where obj_code like '{0}_%'";//删除子表对象的表
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //属性表
                    sql = "delete FLC_OBJECT_PROPERTY where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    sql = "delete from FLC_OBJECT_PROPERTY where obj_code like '{0}_%'";//删除子表对象的表属性
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //多语
                    //sql = "delete FLC_LANG where key like :OBJ_CODE||'.%' or key = :OBJ_CODE";
                    //oracletool.ExecuteNonQuery(sql, parm);

                    sql = "delete from FLC_LANG where key = '{0}' or key like '{0}.%'  or key like '{0}_%' ";
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //flc_authorization_role
                    sql = "delete from flc_authorization_role where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //事件 flc_customevent
                    sql = "delete from flc_customevent where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_list_print
                    sql = "delete from flc_list_print where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_list_record
                    sql = "delete from flc_list_record where obj_code like '{0}%'";
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //flc_list_setting
                    sql = "delete from flc_list_setting where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_menu_relevance
                    sql = "delete from flc_menu_relevance where objectcode=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_obj_show
                    sql = "delete from flc_obj_show where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_obj_show_default
                    sql = "delete from flc_obj_show_default where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_obj_show_print
                    sql = "delete from flc_obj_show_print where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_operation_auth
                    sql = "delete from flc_obj_show_print where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    // from flc_plugsql t where t.keyid like '%%'
                    sql = "delete from flc_plugsql t where t.keyid like '%{0}%'";
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //flc_printtemplate
                    sql = "delete from flc_printtemplate t where t.keyid like '%{0}%'";
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //flc_property_table
                    sql = "delete from flc_printtemplate t where t.keyid like 'V_{0}%'";
                    AppDataBase.Execute(string.Format(sql, OBJ_CODE),null, transaction, dbConnection);

                    //flc_pro_reference
                    sql = "delete from flc_pro_reference where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_special_operation
                    sql = "delete from flc_special_operation where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    //flc_vouch_setting
                    sql = "delete from flc_vouch_setting where obj_code=:OBJ_CODE";
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);
                    #endregion

                    #region 删除对象物理表

                    foreach (object obj_table in obj_tables)
                    {
                        sql = @"drop table  " + obj_table.ToString();
                        AppDataBase.Execute(sql, null, transaction, dbConnection);
                    }
                    #endregion

                    #region 删除脱靶

                    sql = "delete from flc_obj_operation where obj_code = :objectCode";
                    parm = new DynamicParameters();
                    parm.Add(":objectCode", OBJ_CODE);
                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    #endregion

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
            #endregion

        }

        #endregion

        #region 预制的系统字段
        /// <summary>
        /// 预制的系统字段
        /// </summary>
        /// <param name="obj_code">对象名称</param>
        /// <param name="obj_table">对象表名</param>
        /// <param name="tool">oracle连接串</param>
        /// <param name="Enable">是否执行此代码</param>
        public void SysProperty(string obj_code, string obj_table,bool Enable, IDbTransaction transaction, IDbConnection dbConnection)
        {
            string sql;
            var para = new DynamicParameters();           
            if (Enable)
            {
                DataTable DT = new DataTable();
                DT.Columns.Add("OBJ_CODE");
                DT.Columns.Add("OBJ_TABLE");
                DT.Columns.Add("OBJ_PRO_CODE");
                DT.Columns.Add("OBJ_PRO_TYPE");
                DT.Columns.Add("OBJ_PRO_LENGTH");
                DT.Columns.Add("OBJ_PRO_VISIBLE");
                DT.Columns.Add("OBJ_PRO_ENABLE");
                DT.Columns.Add("OBJ_PRO_NULL");
                DT.Columns.Add("OBJ_PRO_CHECK");

                DT.Columns.Add("OBJ_PRO_DEFAULT");
                DT.Columns.Add("OBJ_PRO_OBJECT");
                DT.Columns.Add("OBJ_PRO_DIGIT");
                DT.Columns.Add("OBJ_PRO_USING");

                DT.Columns.Add("OBJ_PRO_ENUM");
                DT.Columns.Add("zn_CN");
                DT.Columns.Add("en_US");
                DT.Columns.Add("mbshow");//IS_SYSTEM
                DT.Columns.Add("IS_SYSTEM");

                DT.Rows.Add(obj_code, obj_table, "CREATE_USER", "1", "60", "0", "1", "0", "0", "", "", "0", "0", "", "创建人", "CREATE_USER", "0", "1");
                DT.Rows.Add(obj_code, obj_table, "MODIFY_USER", "1", "60", "0", "1", "0", "0", "", "", "0", "0", "", "修改人", "MODIFY_USER", "0", "1");

                DT.Rows.Add(obj_code, obj_table, "CREATE_TIME", "4", "0", "0", "1", "0", "0", "", "", "0", "0", "", "创建日期", "CREATE_TIME", "0", "1");
                DT.Rows.Add(obj_code, obj_table, "MODIFY_TIME", "4", "0", "0", "1", "0", "0", "", "", "0", "0", "", "修改日期", "MODIFY_TIME", "0", "1");

                DT.Rows.Add(obj_code, obj_table, "VERIFY_TIME", "4", "0", "0", "1", "0", "0", "", "", "0", "0", "", "确定时间", "VERIFY_TIME", "0", "1");
                DT.Rows.Add(obj_code, obj_table, "VERIFY_USER", "1", "60", "0", "1", "0", "0", "", "", "0", "0", "", "确定人", "VERIFY_USER", "0", "1");

                DT.Rows.Add(obj_code, obj_table, "STATUS", "6", "60", "0", "1", "0", "0", "", "", "0", "0", "__object.status", "状态", "STATUS", "0", "1");

                #region 插入

                foreach (DataRow dr1 in DT.Rows)
                {
                    string OBJ_CODE = dr1["OBJ_CODE"].ToString();
                    string OBJ_TABLE = dr1["OBJ_TABLE"].ToString();
                    string OBJ_PRO_CODE = dr1["OBJ_PRO_CODE"].ToString();
                    string OBJ_PRO_TYPE = dr1["OBJ_PRO_TYPE"].ToString();
                    string OBJ_PRO_LENGTH = dr1["OBJ_PRO_LENGTH"].ToString();
                    string OBJ_PRO_VISIBLE = dr1["OBJ_PRO_VISIBLE"].ToString();
                    string OBJ_PRO_ENABLE = dr1["OBJ_PRO_ENABLE"].ToString();
                    string OBJ_PRO_NULL = dr1["OBJ_PRO_NULL"].ToString();
                    string OBJ_PRO_CHECK = dr1["OBJ_PRO_CHECK"].ToString();
                    string OBJ_PRO_DEFAULT = dr1["OBJ_PRO_DEFAULT"].ToString();
                    string OBJ_PRO_OBJECT = dr1["OBJ_PRO_OBJECT"].ToString();
                    string OBJ_PRO_DIGIT = dr1["OBJ_PRO_DIGIT"].ToString();
                    string OBJ_PRO_USING = dr1["OBJ_PRO_USING"].ToString();
                    string OBJ_PRO_ENUM = dr1["OBJ_PRO_ENUM"].ToString();
                    string zn_CN = dr1["zn_CN"].ToString();
                    string en_US = dr1["en_US"].ToString();
                    string mbshow = dr1["mbshow"].ToString();
                    string IS_SYSTEM = dr1["IS_SYSTEM"].ToString();

                    #region 插入前校验

                    sql = "select count(*) from (select * from flc_object_property t  where obj_code ='{0}' or obj_code like '{1}' ) where obj_pro_code='{2}'";
                    object o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, OBJ_CODE + "_ITEM%", OBJ_PRO_CODE), dbConnection, null, transaction);

                    if (o != null && o.ToString() != "0")
                    {
                        throw new Exception("属性已存在，请检查！" + OBJ_CODE + OBJ_PRO_CODE);
                    }

                    sql = @"select count(*) from  (Select distinct obj_code,obj_table,obj_pro_code,laZ.Value CH from flc_object_property v
                            left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='zn_CN'
                             where obj_code like '{0}'  or obj_code like '{0}_ITEM%') where CH='{1}'";
                    o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, zn_CN),dbConnection,null,transaction);
                    if (o != null && o.ToString() != "0")
                    {
                        throw new Exception("属性中文名重复，请检查！" + OBJ_CODE + zn_CN);
                    }

                    if (!string.IsNullOrEmpty(en_US))
                    {
                        sql = @"select count(*) from  (Select distinct obj_code,obj_table,obj_pro_code,laZ.Value US from flc_object_property v
                            left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='en_US'
                             where obj_code like '{0}'  or obj_code like '{0}_ITEM%') where lower(US)=lower('{1}')";
                        o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, en_US),dbConnection,null,transaction);
                        if (o != null && o.ToString() != "0")
                        {
                            throw new Exception("属性英文名重复，请检查！" + OBJ_CODE + en_US);
                        }
                    }
                    #endregion

                    Regex reg = new Regex("^[0-9]*$");

                    if (!reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                    {

                        #region 插入属性表
                        sql = @"insert into flc_object_property(obj_code,obj_table,obj_pro_code,obj_pro_type,obj_pro_length,
            obj_pro_visible,obj_pro_enable,obj_pro_null,obj_pro_check,obj_pro_default,obj_pro_object,obj_pro_digit,obj_pro_using,obj_pro_enum,obj_pro_show,is_system)
                                                     values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
            :obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_check,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using,:obj_pro_enum,:obj_pro_show,:is_system)";
                        para = new DynamicParameters();
                        para.Add(":obj_code", OBJ_CODE);
                        para.Add(":obj_table", OBJ_TABLE);
                        para.Add(":obj_pro_code", OBJ_PRO_CODE);
                        para.Add(":obj_pro_type", OBJ_PRO_TYPE);
                        para.Add(":obj_pro_length", string.IsNullOrEmpty(OBJ_PRO_LENGTH) ? "0" : OBJ_PRO_LENGTH);

                        para.Add(":obj_pro_visible", string.IsNullOrEmpty(OBJ_PRO_VISIBLE) ? "1" : OBJ_PRO_VISIBLE);
                        para.Add(":obj_pro_enable", string.IsNullOrEmpty(OBJ_PRO_ENABLE) ? "1" : OBJ_PRO_ENABLE);
                        para.Add(":obj_pro_null", string.IsNullOrEmpty(OBJ_PRO_NULL) ? "1" : OBJ_PRO_NULL);
                        para.Add(":obj_pro_check", string.IsNullOrEmpty(OBJ_PRO_CHECK) ? "1" : OBJ_PRO_CHECK);
                        para.Add(":obj_pro_default", OBJ_PRO_DEFAULT);

                        para.Add(":obj_pro_object", OBJ_PRO_OBJECT);
                        para.Add(":obj_pro_digit", OBJ_PRO_DIGIT);
                        para.Add(":obj_pro_using", string.IsNullOrEmpty(OBJ_PRO_USING) ? "0" : OBJ_PRO_USING);
                        para.Add(":obj_pro_enum", OBJ_PRO_ENUM);
                        para.Add(":obj_pro_show", mbshow);
                        para.Add(":is_system", IS_SYSTEM);
            
                        AppDataBase.Execute(sql, para, transaction, dbConnection);

                        #endregion

                        #region 插入子表对象属性
                        //判断是否为子表     DATA_AA_ITEM0

                        //   if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                        //    {

                        /*sql = @"insert into flc_object_property(obj_code,obj_table,obj_pro_code,obj_pro_type,obj_pro_length,
obj_pro_visible,obj_pro_enable,obj_pro_null,obj_pro_check,obj_pro_default,obj_pro_object,obj_pro_digit,obj_pro_using,obj_pro_enum,obj_pro_show)
                                         values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
:obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_check,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using,:obj_pro_enum,:obj_pro_show)";
                        para = new OracleParameter[] { 
        new OracleParameter(":obj_code",OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length-1,1)),
        new OracleParameter(":obj_table",OBJ_TABLE),
        new OracleParameter(":obj_pro_code",OBJ_PRO_CODE),
        new OracleParameter(":obj_pro_type",OBJ_PRO_TYPE),
        //new OracleParameter(":obj_pro_length",string.IsNullOrEmpty(OBJ_PRO_LENGTH)?"0":OBJ_PRO_LENGTH),
        new OracleParameter(":obj_pro_length",string.IsNullOrEmpty(OBJ_PRO_LENGTH)?"0":OBJ_PRO_LENGTH),
        new OracleParameter(":obj_pro_visible",string.IsNullOrEmpty(OBJ_PRO_VISIBLE)?"1":OBJ_PRO_VISIBLE),
        new OracleParameter(":obj_pro_enable",string.IsNullOrEmpty(OBJ_PRO_ENABLE)?"1":OBJ_PRO_ENABLE),
        new OracleParameter(":obj_pro_null",string.IsNullOrEmpty(OBJ_PRO_NULL)?"1":OBJ_PRO_NULL),
        new OracleParameter(":obj_pro_check",string.IsNullOrEmpty(OBJ_PRO_CHECK)?"1":OBJ_PRO_CHECK),
        new OracleParameter(":obj_pro_default",OBJ_PRO_DEFAULT),
        new OracleParameter(":obj_pro_object",OBJ_PRO_OBJECT),
        new OracleParameter(":obj_pro_digit",OBJ_PRO_DIGIT),
        new OracleParameter(":obj_pro_using",string.IsNullOrEmpty(OBJ_PRO_USING)?"0":OBJ_PRO_USING),
        new OracleParameter(":obj_pro_enum",OBJ_PRO_ENUM),
        new OracleParameter(":obj_pro_show",mbshow)

        };

                        oracletool.ExecuteNonQuery(sql, para);*/
                        //  }
                        #endregion

                        #region 插入中英文
                        string key = OBJ_CODE + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;
                        string zikey = OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1) + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;

                        sql = string.Format("delete flc_lang where key='{0}'", key);
                        AppDataBase.Execute(sql, null, transaction, dbConnection);
                        if (!string.IsNullOrEmpty(en_US))
                        {
                            sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";
                            para = new DynamicParameters();
                            para.Add(":key", key);
                            para.Add(":value", en_US);
                            para.Add(":lan", "en_US");

                            AppDataBase.Execute(sql, para,transaction,dbConnection);

                        }
                        if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                        {
                            /* sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                             para = new OracleParameter[] { 
                     new OracleParameter(":key",zikey),
                     new OracleParameter(":value",en_US),
                     new OracleParameter(":lan","en_US")
                     };
                             oracletool.ExecuteNonQuery(sql, para);
                         }*/
                        }

                        if (!string.IsNullOrEmpty(zn_CN))
                        {
                            sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";
                            para = new DynamicParameters();
                            para.Add(":key", key);
                            para.Add(":value", zn_CN);
                            para.Add(":lan", "zn_CN");

                            AppDataBase.Execute(sql, para,transaction,dbConnection);

                            /*  if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                              {
                                  sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";
                                  para = new OracleParameter[] { 
                  new OracleParameter(":key",zikey),
                  new OracleParameter(":value",zn_CN),
                  new OracleParameter(":lan","zn_CN")
                  };
                                  oracletool.ExecuteNonQuery(sql, para);
                              }*/
                        }
                        #endregion

                    }
                }
                #endregion

            }
        }

        #endregion

        #region 修改对象
        public string ModifyObject(ModifyObject modifyObject)
        {
            string sql = "";
            var para = new DynamicParameters();
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    List<dtObject> flc_objects = modifyObject.dtObject;
                    string objectCode = flc_objects[0].obj_code.ToUpper();

                    #region 新建对象信息
                    string enable = flc_objects[0].is_Enable;
                    string is_system = flc_objects[0].is_system;
                    string is_enum = flc_objects[0].is_enum;
                    int id = MethodGetSerial.getSerialNumInt("OBJECT", transaction, dbConnection);

                    sql = "update FLC_OBJECT set is_enable = :is_enable,is_system = :is_system,is_enum = :is_enum where obj_code = :objectCode";
                    para.Add("objectCode", objectCode);
                    para.Add("is_enable", enable);
                    para.Add("is_system", is_system);
                    para.Add("is_enum", is_enum);
                    AppDataBase.Execute(sql, para, transaction, dbConnection);

                    para = new DynamicParameters();
                    para.Add("objectCode", objectCode);
                    sql = "Select count(*) from FLC_OBJECTS where obj_code = :objectCode and is_main  = 0";
                    string oldTableNum = AppDataBase.ExecuteScalar(sql, dbConnection, para, transaction).ToString();
                    int oldNum;
                    if (oldTableNum == null || string.IsNullOrEmpty(oldTableNum))
                    {
                        oldNum = 0;
                    }
                    else
                    {
                        oldNum = Convert.ToInt32(oldTableNum);
                    }

                    string nums = flc_objects[0].tableNum;
                    int tableNum = string.IsNullOrEmpty(nums) ? 0 : Convert.ToInt32(nums);
                    if (tableNum < oldNum)
                        throw new Exception("不允许删除明细表!");

                    for (int i = oldNum; i < tableNum; i++)
                    {

                        id = MethodGetSerial.getSerialNumInt("OBJECT", transaction, dbConnection);
                        sql = "Insert into FLC_OBJECT(id,OBJ_CODE,is_enable,is_system,is_enum,is_show) values (:id,:objectCode,:is_enable,:is_system,:is_enum,0)";
                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":objectCode", objectCode + "_ITEM" + i.ToString());
                        para.Add(":is_enable", enable);
                        para.Add(":is_system", is_system);
                        para.Add(":is_enum", is_enum);

                        AppDataBase.Execute(sql, para, transaction, dbConnection);//插入子表对象

                        id = MethodGetSerial.getSerialNumInt("OBJECT_TABLE", transaction, dbConnection);
                        sql = "Insert into FLC_OBJECTS(id,OBJ_CODE,OBJ_TABLE,IS_MAIN) values (:id,:objectCode,:objectTable,1)";
                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":objectCode", objectCode + "_ITEM" + i.ToString());
                        para.Add(":objectTable", "DATA_" + objectCode + "_ITEM" + i.ToString());

                        AppDataBase.Execute(sql, para, transaction, dbConnection);//插入子表对象的表


                        id = MethodGetSerial.getSerialNumInt("OBJECT_TABLE", transaction, dbConnection);
                        sql = "Insert into FLC_OBJECTS(id,OBJ_CODE,OBJ_TABLE,IS_MAIN)values(:id,:objectCode,:objectTable,0)";
                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":objectCode", objectCode);
                        para.Add(":objectTable", "DATA_" + objectCode + "_ITEM" + i.ToString());

                        AppDataBase.Execute(sql, para, transaction, dbConnection);

                        //默认字段id，mid,创建人，创建时间，修改人，修改时间，时间戳，状态
                        sql = "create table DATA_" + objectCode + "_ITEM" + i.ToString() + @"(id number,mid number,create_user nvarchar2(60),modify_user nvarchar2(60),
                            create_time date,modify_time date,ufts timestamp default systimestamp,status nvarchar2(60),verify_user nvarchar2(60),verify_time date)";
                        AppDataBase.Execute(sql, null, transaction, dbConnection);
                    }

                    #endregion

                    #region 新增语言表内容前校验语言跟其他对象是否存在相同
                    sql = @"select t1.obj_code key,t.value,t.lan from flc_lang t right join flc_object t1 on t.key = t1.obj_code WHERE t1.is_model='1' and t1.obj_code <> '" + objectCode + "' and t1.obj_code not like '" + objectCode + @"_ITEM%'
                               union  
                    select 'DATA_'||t3.obj_code key,t2.value,t2.lan from flc_lang t2 right join flc_objects t3 on t2.key = t3.obj_code||'.'||t3.obj_table WHERE t3.is_main='1' and t3.obj_code <>'" + objectCode + "'";
                    IEnumerable<FLC_LANG> fLC_LANGs = AppDataBase.Query<FLC_LANG>(sql,null,transaction,dbConnection);
                    #endregion

                    #region 新增语言表内容

                    sql = "Delete from FLC_LANG where key = '{0}' or key = '{0}.DATA_{0}' or key like '{0}.DATA_{0}_ITEM_' or key like '{0}_ITEM_' or key like '{0}_ITEM_.DATA_{0}_ITEM_'";
                    AppDataBase.Execute(string.Format(sql, objectCode),null,transaction,dbConnection);

                    sql = "Insert into FLC_LANG (key,value,lan) values(:key,:value,:lan)";
                    List<FLC_LANG> fLC_LANGs1 = modifyObject.Language;

                    foreach (FLC_LANG item in fLC_LANGs1)
                    {
                        para = new DynamicParameters();
                        para.Add(":key",item.key);
                        para.Add(":value", item.value);
                        para.Add(":lan", item.lan);

                        foreach (FLC_LANG item1 in fLC_LANGs)
                        {
                            if (item1.value == item.value)
                            {
                                throw new Exception("不允许跟对象编码为" + item.key + "的名称相同");
                            }
                        }

                        AppDataBase.Execute(sql, para,transaction,dbConnection);

                        string[] key = item.key.Split('.');                      
                        if (key.Length > 0)
                        {
                            if (item.key.Length > 5 && item.key.Substring(item.key.Length - 5, 4).ToString().ToUpper() == "ITEM")
                            {
                                para = new DynamicParameters();
                                para.Add(":key", objectCode + "_ITEM" + item.key.Substring(item.key.ToString().Length - 1, 1).ToString());
                                para.Add(":value", item.value);
                                para.Add(":lan", item.lan);

                                foreach (FLC_LANG item1 in fLC_LANGs)
                                {
                                    if (item1.value == item.value)
                                    {
                                        throw new Exception("不允许跟对象编码为" + item.key + "的名称相同");
                                    }
                                }

                                AppDataBase.Execute(sql, para, transaction, dbConnection);
                            }
                        }

                    }
                    #endregion

                    #region 拖靶
                    sql = "delete from flc_obj_operation where obj_code = :objectCode";
                    para = new DynamicParameters();
                    para.Add(":objectCode", objectCode);

                    AppDataBase.Execute(sql, para,transaction,dbConnection);

                    List<Flc_Obj_Operation> flc_Obj_Operations = modifyObject.checkboxbutton;
                    foreach (Flc_Obj_Operation item in flc_Obj_Operations)
                    {
                        sql = @"Insert into flc_obj_operation (obj_code,operation_id,from_system,btn_index,page) values 
                                (:obj_code,:operation_id,:from_system,:btn_index,:page)";
                        para = new DynamicParameters();
                        para.Add(":obj_code", objectCode);
                        para.Add(":operation_id",item.operation_id);
                        para.Add(":from_system", item.from_system);
                        para.Add(":btn_index", item.btn_index);
                        para.Add(":page", item.page);

                        AppDataBase.Execute(sql,para,transaction,dbConnection);
                    }

                    #endregion

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return "ok";
        }
        #endregion

        #region 保存对象
        public string SaveObject(ModifyObject modifyObject)
        {
            string sql = "";
            var para = new DynamicParameters();
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    List<dtObject> flc_objects = modifyObject.dtObject;
                    string objectCode = flc_objects[0].obj_code.ToUpper();

                    #region 清除对象残余信息
                    para = new DynamicParameters();
                    para.Add(":objectCode", objectCode);

                    sql = "delete from FLC_OBJECT where obj_code = :objectCode";
                    AppDataBase.Execute(sql, para,transaction,dbConnection);

                    sql = "delete from FLC_OBJECT  where obj_code like '{0}_%'";//删除子表对象
                    AppDataBase.Execute(string.Format(sql, objectCode),null,transaction,dbConnection);

                    sql = "delete from FLC_OBJECTS where obj_code = :objectCode";
                    AppDataBase.Execute(sql, para,transaction,dbConnection);

                    sql = "delete from FLC_OBJECTS where obj_code like '{0}_%'";//删除子表对象的表
                    AppDataBase.Execute(string.Format(sql, objectCode),null,transaction,dbConnection);

                    sql = "delete from FLC_OBJECT_PROPERTY where obj_code  = :objectCode";
                    AppDataBase.Execute(sql, para,transaction,dbConnection);

                    sql = "delete from FLC_OBJECT_PROPERTY where obj_code like '{0}_%'";//删除子表对象的表属性
                    AppDataBase.Execute(string.Format(sql, objectCode),null,transaction,dbConnection);

                    sql = "delete from FLC_OBJ_SHOW where obj_code = :objectCode";
                    AppDataBase.Execute(sql, para,transaction,dbConnection);

                    sql = "delete from FLC_LANG where key = '{0}' or key like '{0}.%'  or key like '{0}_%' ";
                    AppDataBase.Execute(string.Format(sql, objectCode),null,transaction,dbConnection);

                    sql = "Select * from user_tables where table_name = 'DATA_{0}' or table_name like 'DATA_{0}_ITEM%'";

                    IEnumerable<object> vs = AppDataBase.Query<object>(string.Format(sql, objectCode), null, transaction, dbConnection);
                    foreach (object item in vs)
                    {
                        sql = "drop table {0}";
                        AppDataBase.Execute(string.Format(sql, item.ToString()), null, transaction, dbConnection);
                    }
                    #endregion

                    #region 新建对象信息
                    string enable = flc_objects[0].is_Enable;
                    string is_system = flc_objects[0].is_system;
                    string is_enum = flc_objects[0].is_enum;
                    int id = MethodGetSerial.getSerialNumInt("OBJECT", transaction, dbConnection);
                    sql = "Insert into FLC_OBJECT(id,obj_code,is_enable,is_system,is_enum,is_show) values(:id,:objectCode,:is_enable,:is_system,:is_enum,1)";
                    para = new DynamicParameters();
                    para.Add("id", id);
                    para.Add("objectCode", objectCode);
                    para.Add("is_enable", enable);
                    para.Add("is_system", is_system);
                    para.Add("is_enum", is_enum);
                    AppDataBase.Execute(sql, para, transaction, dbConnection);

                    string nums = flc_objects[0].tableNum;
                    int tableNum = string.IsNullOrEmpty(nums) ? 0 : Convert.ToInt32(nums);
                    id = MethodGetSerial.getSerialNumInt("OBJECT_TABLE", transaction, dbConnection);
                    sql = "Insert into FLC_OBJECTS(id,OBJ_CODE,OBJ_TABLE,IS_MAIN)values(:id,:objectCode,:objectTable,1)";
                    para = new DynamicParameters();
                    para.Add(":id", id);
                    para.Add(":objectCode", objectCode);
                    para.Add(":objectTable", "DATA_" + objectCode);

                    AppDataBase.Execute(sql, para, transaction, dbConnection);
                    //默认字段id 创建人，创建时间，修改人，修改时间，时间戳，状态,审核人，审核时间
                    sql = "create table DATA_" + objectCode + @"(id number,create_user nvarchar2(60),modify_user nvarchar2(60),
                        create_time date,modify_time date,ufts timestamp default systimestamp,status nvarchar2(60),verify_user nvarchar2(60),verify_time date)";
                    AppDataBase.Execute(sql, null, transaction, dbConnection);

                    #region  只将主表的这些属性添加至flc_object_property和预制语言
                    try
                    {
                        SysProperty(objectCode, "DATA_" + objectCode, true, transaction, dbConnection);
                    }
                    catch (Exception ex)
                    {
                        //tool.Rollback();
                        throw ex;
                    }

                    #endregion

                    for (int i = 0; i < tableNum; i++)
                    {
                        id = MethodGetSerial.getSerialNumInt("OBJECT", transaction,dbConnection);
                        sql = "Insert into FLC_OBJECT(id,OBJ_CODE,is_enable,is_system,is_enum,is_show) values(:id,:objectCode,:is_enable,:is_system,:is_enum,0)";
                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":objectCode", objectCode + "_ITEM" + i.ToString());
                        para.Add(":is_enable", enable);
                        para.Add(":is_system", is_system);
                        para.Add(":is_enum", is_enum);

                        AppDataBase.Execute(sql, para,transaction,dbConnection);//插入子表对象

                        id = MethodGetSerial.getSerialNumInt("OBJECT_TABLE", transaction,dbConnection);
                        sql = "Insert into FLC_OBJECTS(id,OBJ_CODE,OBJ_TABLE,IS_MAIN)values(:id,:objectCode,:objectTable,1)";

                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":objectCode", objectCode + "_ITEM" + i.ToString());
                        para.Add(":objectTable", "DATA_" + objectCode + "_ITEM" + i.ToString());

                        AppDataBase.Execute(sql, para,transaction,dbConnection);//插入子表对象的表

                        id = MethodGetSerial.getSerialNumInt("OBJECT_TABLE", transaction,dbConnection);
                        sql = "Insert into FLC_OBJECTS(id,OBJ_CODE,OBJ_TABLE,IS_MAIN)values(:id,:objectCode,:objectTable,0)";

                        para = new DynamicParameters();
                        para.Add(":id", id);
                        para.Add(":objectCode", objectCode);
                        para.Add(":objectTable", "DATA_" + objectCode + "_ITEM" + i.ToString());

                        AppDataBase.Execute(sql, para,transaction,dbConnection);
                        //默认字段id，mid,创建人，创建时间，修改人，修改时间，时间戳，状态,审核人，审核时间
                        sql = "create table DATA_" + objectCode + "_ITEM" + i.ToString() + @"(id number,mid number,create_user nvarchar2(60),modify_user nvarchar2(60),
                        create_time date,modify_time date,ufts timestamp default systimestamp,status nvarchar2(60),verify_user nvarchar2(60),verify_time date)";
                        AppDataBase.Execute(sql,null,transaction,dbConnection);
                    }

                    #endregion

                    #region 新增语言表内容前校验语言跟其他对象是否存在相同
                    sql = @"select t1.obj_code key,t.value,t.lan from flc_lang t right join flc_object t1 on t.key = t1.obj_code WHERE t1.is_model='1' and t1.obj_code <> '" + objectCode + "' and t1.obj_code not like '" + objectCode + @"_ITEM%'
                               union  
                    select 'DATA_'||t3.obj_code key,t2.value,t2.lan from flc_lang t2 right join flc_objects t3 on t2.key = t3.obj_code||'.'||t3.obj_table WHERE t3.is_main='1' and t3.obj_code <>'" + objectCode + "'";
                    IEnumerable<FLC_LANG> fLC_LANGs = AppDataBase.Query<FLC_LANG>(sql, null, transaction, dbConnection);
                    #endregion

                    #region 新增语言表内容

                    sql = "Insert into FLC_LANG (key,value,lan) values(:key,:value,:lan)";
                    List<FLC_LANG> fLC_LANGs1 = modifyObject.Language;

                    foreach (FLC_LANG item in fLC_LANGs1)
                    {
                        para = new DynamicParameters();
                        para.Add(":key", item.key);
                        para.Add(":value", item.value);
                        para.Add(":lan", item.lan);

                        foreach (FLC_LANG item1 in fLC_LANGs)
                        {
                            if (item1.value == item.value)
                            {
                                throw new Exception("不允许跟对象编码为" + item.key + "的名称相同");
                            }
                        }

                        AppDataBase.Execute(sql, para, transaction, dbConnection);

                        string[] key = item.key.Split('.');
                        if (key.Length > 0)
                        {
                            if (item.key.Length > 5 && item.key.Substring(item.key.Length - 5, 4).ToString().ToUpper() == "ITEM")
                            {
                                para = new DynamicParameters();
                                para.Add(":key", objectCode + "_ITEM" + item.key.Substring(item.key.ToString().Length - 1, 1).ToString());
                                para.Add(":value", item.value);
                                para.Add(":lan", item.lan);

                                foreach (FLC_LANG item1 in fLC_LANGs)
                                {
                                    if (item1.value == item.value)
                                    {
                                        throw new Exception("不允许跟对象编码为" + item.key + "的名称相同");
                                    }
                                }

                                AppDataBase.Execute(sql, para, transaction, dbConnection);
                            }
                        }

                    }
                    #endregion

                    #region 拖靶
                    sql = "delete from flc_obj_operation where obj_code = :objectCode";
                    para = new DynamicParameters();
                    para.Add(":objectCode", objectCode);

                    AppDataBase.Execute(sql, para, transaction, dbConnection);

                    List<Flc_Obj_Operation> flc_Obj_Operations = modifyObject.checkboxbutton;
                    foreach (Flc_Obj_Operation item in flc_Obj_Operations)
                    {
                        sql = @"Insert into flc_obj_operation (obj_code,operation_id,from_system,btn_index,page) values 
                                (:obj_code,:operation_id,:from_system,:btn_index,:page)";
                        para = new DynamicParameters();
                        para.Add(":obj_code", objectCode);
                        para.Add(":operation_id", item.operation_id);
                        para.Add(":from_system", item.from_system);
                        para.Add(":btn_index", item.btn_index);
                        para.Add(":page", item.page);

                        AppDataBase.Execute(sql, para, transaction, dbConnection);
                    }

                    #endregion

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
            return "ok";
        }
        #endregion

        private string ConvertType(string TypeName, string Len, string digit)
        {
            string ColumnTypeName = string.Empty;
            Len = string.IsNullOrEmpty(Len) ? "0" : Len;
            switch (TypeName)
            {
                case "1"://字符串
                    ColumnTypeName = string.Format("NVARCHAR2({0})", Len);
                    break;
                case "2"://数字
                    ColumnTypeName = string.Format("number({0})", Len);
                    break;
                case "3"://金额
                    ColumnTypeName = string.Format("number({0},{1})", Len, digit);
                    break;
                case "7"://时间
                case "4"://日期
                    ColumnTypeName = "date";
                    break;
                case "26"://文本
                    ColumnTypeName = "CLOB";
                    break;
                case "27"://二进制流                  
                    ColumnTypeName = "BLOB";
                    break;
                default:
                    ColumnTypeName = string.Format("NVARCHAR2({0})", Len);
                    break;
            }
            return ColumnTypeName;

        }

        #region 读取对象属性
        public LoadObejctProperty LoadObejctProperty(string objectCode, string Lan)
        {
            LoadObejctProperty loadObejctProperty = new LoadObejctProperty();
            if (string.IsNullOrEmpty(Lan))
            {
                Lan = "zn_CN";
            }

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                //IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = @"Select distinct obj_code,obj_table,obj_pro_code,obj_pro_type,obj_pro_length,obj_pro_order 
,case when obj_pro_using = 1 then '是' else '否' end as obj_pro_using 
,case when obj_pro_visible = 1 then '是' else '否' end as obj_pro_visible 
,case when obj_pro_enable = 1 then '是' else '否' end as obj_pro_enable  
,case when obj_pro_null = 1 then '是' else '否' end as obj_pro_null
,case when obj_pro_check = 1 then '是' else '否' end as obj_pro_check
,case when obj_pro_show = 1 then '是' else '否' end as obj_pro_show
,case when is_system = 1 then '是' else '否' end as is_system
,case when obj_pro_positive = 1 then '是' else '否' end as obj_pro_positive

,obj_pro_default,obj_pro_object, obj_pro_enum,
obj_pro_digit,obj_pro_type_name ,laY.value ying ,laZ.Value zhong
from v_flc_objectproperty v
left join flc_lang laY on laY.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laY.lan='en_US'
left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='zn_CN'
where obj_code = :objectCode order by v.obj_pro_order,NLSSORT(v.obj_pro_code,'NLS_SORT = SCHINESE_RADICAL_M')";

                    var parm = new DynamicParameters();
                    parm.Add(":objectCode", objectCode);

                    loadObejctProperty.dtProperty = AppDataBase.Query<ObejctProperty>(sql, parm, null, dbConnection);

                    parm.Add(":lan", Lan);

                    sql = @"select * from flc_objects t 
left join flc_lang t1 on t1.key = Concat(Concat( t.obj_code,'.'),t.obj_table) and (lan = :lan or lan is null)
where t.obj_code = :objectCode order by is_main desc,obj_table";

                    loadObejctProperty.dtName = AppDataBase.Query<Flc_Objects>(sql, parm, null, dbConnection);

                    return loadObejctProperty;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

        }
        #endregion

        #region 新增对象属性
        public string ObjectProperty_Add(ObejctProperty obejctProperty)
        {
            string sql = string.Empty;
            var parm=new DynamicParameters();

            #region 获取传入参数

            string OBJ_CODE = obejctProperty.OBJ_CODE.ToUpper();
            string OBJ_TABLE = obejctProperty.OBJ_TABLE.ToUpper();
            string OBJ_PRO_CODE = obejctProperty.OBJ_PRO_CODE.ToUpper();
            string OBJ_PRO_TYPE = obejctProperty.OBJ_PRO_TYPE;//类型
            string OBJ_PRO_LENGTH = string.Empty;
            if (OBJ_PRO_TYPE == "25")
            {
                OBJ_PRO_LENGTH = "300";//长度
            }
            else
            {
                OBJ_PRO_LENGTH = obejctProperty.OBJ_PRO_LENGTH;
            }

            string OBJ_PRO_VISIBLE = obejctProperty.OBJ_PRO_VISIBLE;
            string OBJ_PRO_ENABLE = obejctProperty.OBJ_PRO_ENABLE;
            string OBJ_PRO_NULL = obejctProperty.OBJ_PRO_NULL;
            string OBJ_PRO_CHECK = obejctProperty.OBJ_PRO_CHECK;
            string OBJ_PRO_DEFAULT = obejctProperty.OBJ_PRO_DEFAULT;//默认值
            string OBJ_PRO_OBJECT = obejctProperty.OBJ_PRO_OBJECT.ToUpper();
            string OBJ_PRO_DIGIT = obejctProperty.OBJ_PRO_DIGIT;
            string OBJ_PRO_USING = obejctProperty.OBJ_PRO_USING;
            string OBJ_PRO_ENUM = obejctProperty.OBJ_PRO_ENUM;
            string zn_CN = obejctProperty.zn_CN;
            string en_US = obejctProperty.en_US;
            string mbshow = obejctProperty.mbshow;
            string is_system = obejctProperty.IS_SYSTEM;
            string OBJ_PRO_POSITIVE = obejctProperty.OBJ_PRO_POSITIVE;
            #endregion

            #region 数据完整性检查

            if (string.IsNullOrEmpty(OBJ_CODE))
            {
                throw new Exception("对象编码不能为空！");
            }
            if (string.IsNullOrEmpty(OBJ_TABLE))
            {
                throw new Exception("对象表名不能为空！");
            }
            if (string.IsNullOrEmpty(OBJ_PRO_CODE))
            {
                throw new Exception("对象属性名不能为空！");
            }
            if (string.IsNullOrEmpty(zn_CN))
            {
                throw new Exception("中文名称不能为空！");
            }

            #endregion

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {

                    #region 检查flc_object_property 是否存在相同属性，存在抛错
                    sql = "select count(*) from (select * from flc_object_property t  where (obj_code like '{0}_ITEM%' or obj_code = '{0}') ) where obj_pro_code='{1}'";
                    object o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, OBJ_PRO_CODE), dbConnection, null, transaction);

                    if (o != null && o.ToString() != "0")
                    {
                        throw new Exception("属性已存在，请检查！");
                    }

                    //检测语言是否相同
                    sql = @"select count(*) from  (Select distinct obj_code,obj_table,obj_pro_code,laZ.Value CH from flc_object_property v
                        left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='zn_CN'
                         where obj_code like '{0}'  or obj_code like '{0}_ITEM%') where CH='{1}'";
                    o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, zn_CN), dbConnection, null, transaction);
                    if (o != null && o.ToString() != "0")
                    {
                        throw new Exception("属性中文名重复，请检查！");
                    }

                    if (!string.IsNullOrEmpty(en_US))
                    {
                        sql = @"select count(*) from  (Select distinct obj_code,obj_table,obj_pro_code,laZ.Value US from flc_object_property v
                        left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='en_US'
                         where obj_code like '{0}'  or obj_code like '{0}_ITEM%') where lower(US)=lower('{1}')";
                        o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, en_US), dbConnection, null, transaction);
                        if (o != null && o.ToString() != "0")
                        {
                            throw new Exception("属性英文名重复，请检查！");
                        }
                    }


                    #endregion

                    #region 检查flc_object_property 是否存在外键，存在抛错

                    if (OBJ_PRO_USING == "1")
                    {
                        sql = @"select  Count(*) from flc_object_property t where OBJ_CODE=:OBJ_CODE and OBJ_TABLE =:OBJ_TABLE and obj_pro_using='1'";

                        parm = new DynamicParameters();
                        parm.Add(":OBJ_CODE", OBJ_CODE);
                        parm.Add(":OBJ_TABLE", OBJ_TABLE);

                        o = AppDataBase.ExecuteScalar(sql, dbConnection, parm, transaction);
                        if (o != null && o.ToString() != "0")
                        {
                            throw new Exception("该对象已存在主键，请检查！");
                        }
                    }
                    #endregion

                    string typename = OBJ_PRO_TYPE;//字段类型
                    string typelen = OBJ_PRO_LENGTH;//字段长度
                    string typedigit = OBJ_PRO_DIGIT;//字段小数位数

                    #region 如果属性是引用类型，则需要获取引用对象的外键，获取其数据类型
                    if (OBJ_PRO_TYPE == "5")
                    {
                        if (string.IsNullOrEmpty(OBJ_PRO_OBJECT))
                        {
                            throw new Exception("属性类型为引用类型的请设置其引用对象！");
                        }
                        sql = "select OBJ_PRO_TYPE,OBJ_PRO_LENGTH from flc_object_property t where obj_code='{0}' and obj_table='{1}' and obj_pro_USING=1";
                        sql = string.Format(sql, OBJ_PRO_OBJECT, "DATA_" + OBJ_PRO_OBJECT);

                        object dt = AppDataBase.Query(sql, null, transaction, dbConnection).FirstOrDefault();
                                            
                        if (dt == null)
                        {
                            throw new Exception("引用对象没有设置外键属性,不能被引用，请检查！");
                        }
                        typename = ((object[])((System.Collections.Generic.IDictionary<string, object>)dt).Values)[0].ToString();
                        typelen = ((object[])((System.Collections.Generic.IDictionary<string, object>)dt).Values)[1].ToString();
                    }
                    #endregion

                    #region 添加物理表字段

                    sql = string.Format(" alter table {0} add ({1} {2})",
                        OBJ_TABLE, OBJ_PRO_CODE, ConvertType(typename, typelen, typedigit));

                    AppDataBase.Execute(sql, null, transaction, dbConnection);

                    if (OBJ_PRO_USING == "1")
                    {
                        sql = string.Format(" alter table {0} add primary key({1})", OBJ_TABLE, OBJ_PRO_CODE);
                        AppDataBase.Execute(sql, null, transaction, dbConnection);
                    }
                    #endregion

                    #region 插入属性表
                    sql = @"insert into flc_object_property(obj_code,obj_table,obj_pro_code,obj_pro_type,obj_pro_length,
        obj_pro_visible,obj_pro_enable,obj_pro_null,obj_pro_check,obj_pro_default,obj_pro_object,obj_pro_digit,obj_pro_using,obj_pro_enum,obj_pro_show,is_system,obj_pro_positive)
                                                 values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
        :obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_check,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using,:obj_pro_enum,:obj_pro_show,:is_system,:obj_pro_positive)";

                    parm = new DynamicParameters();
                    parm.Add(":obj_code", OBJ_CODE);
                    parm.Add(":obj_table", OBJ_TABLE);
                    parm.Add(":obj_pro_code", OBJ_PRO_CODE);
                    parm.Add(":obj_pro_type", OBJ_PRO_TYPE);
                    parm.Add(":obj_pro_length", string.IsNullOrEmpty(OBJ_PRO_LENGTH) ? "0" : OBJ_PRO_LENGTH);

                    parm.Add(":obj_pro_visible", string.IsNullOrEmpty(OBJ_PRO_VISIBLE) ? "1" : OBJ_PRO_VISIBLE);
                    parm.Add(":obj_pro_enable", string.IsNullOrEmpty(OBJ_PRO_ENABLE) ? "1" : OBJ_PRO_ENABLE);
                    parm.Add(":obj_pro_null", string.IsNullOrEmpty(OBJ_PRO_NULL) ? "1" : OBJ_PRO_NULL);
                    parm.Add(":obj_pro_check", string.IsNullOrEmpty(OBJ_PRO_CHECK) ? "1" : OBJ_PRO_CHECK);
                    parm.Add(":obj_pro_default", OBJ_PRO_DEFAULT);

                    parm.Add(":obj_pro_object", OBJ_PRO_OBJECT);
                    parm.Add(":obj_pro_digit", OBJ_PRO_DIGIT);
                    parm.Add(":obj_pro_using", string.IsNullOrEmpty(OBJ_PRO_USING) ? "0" : OBJ_PRO_USING);
                    parm.Add(":obj_pro_enum", OBJ_PRO_ENUM);
                    parm.Add(":obj_pro_show", mbshow);

                    parm.Add(":is_system", is_system);
                    parm.Add(":obj_pro_positive", OBJ_PRO_POSITIVE);

                    AppDataBase.Execute(sql, parm,transaction,dbConnection);

                    #endregion

                    #region 插入子表对象属性
                    //判断是否为子表     DATA_AA_ITEM0
                    Regex reg = new Regex("^[0-9]*$");
                    if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                    {
                        sql = @"insert into flc_object_property(obj_code,obj_table,obj_pro_code,obj_pro_type,obj_pro_length,
        obj_pro_visible,obj_pro_enable,obj_pro_null,obj_pro_check,obj_pro_default,obj_pro_object,obj_pro_digit,obj_pro_using,obj_pro_enum,obj_pro_show,is_system,obj_pro_positive)
                                                 values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
        :obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_check,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using,:obj_pro_enum,:obj_pro_show,:is_system,:obj_pro_positive)";

                        parm = new DynamicParameters();
                        parm.Add(":obj_code", OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1));
                        parm.Add(":obj_table", OBJ_TABLE);
                        parm.Add(":obj_pro_code", OBJ_PRO_CODE);
                        parm.Add(":obj_pro_type", OBJ_PRO_TYPE);
                        parm.Add(":obj_pro_length", string.IsNullOrEmpty(obejctProperty.OBJ_PRO_LENGTH) ? "0" : obejctProperty.OBJ_PRO_LENGTH);

                        parm.Add(":obj_pro_visible", string.IsNullOrEmpty(OBJ_PRO_VISIBLE) ? "1" : OBJ_PRO_VISIBLE);
                        parm.Add(":obj_pro_enable", string.IsNullOrEmpty(OBJ_PRO_ENABLE) ? "1" : OBJ_PRO_ENABLE);
                        parm.Add(":obj_pro_null", string.IsNullOrEmpty(OBJ_PRO_NULL) ? "1" : OBJ_PRO_NULL);
                        parm.Add(":obj_pro_check", string.IsNullOrEmpty(OBJ_PRO_CHECK) ? "1" : OBJ_PRO_CHECK);
                        parm.Add(":obj_pro_default", OBJ_PRO_DEFAULT);

                        parm.Add(":obj_pro_object", OBJ_PRO_OBJECT);
                        parm.Add(":obj_pro_digit", OBJ_PRO_DIGIT);
                        parm.Add(":obj_pro_using", string.IsNullOrEmpty(OBJ_PRO_USING) ? "0" : OBJ_PRO_USING);
                        parm.Add(":obj_pro_enum", OBJ_PRO_ENUM);
                        parm.Add(":obj_pro_show", mbshow);

                        parm.Add(":is_system", is_system);
                        parm.Add(":obj_pro_positive", OBJ_PRO_POSITIVE);                    
                        AppDataBase.Execute(sql, parm,transaction,dbConnection);


                    }
                    #endregion

                    #region 插入中英文
                    string key = OBJ_CODE + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;
                    string zikey = OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1) + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;

                    sql = string.Format("delete flc_lang where key='{0}'", key);
                    AppDataBase.Execute(sql,null,transaction,dbConnection);
                    if (!string.IsNullOrEmpty(en_US))
                    {
                        sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                        parm = new DynamicParameters();
                        parm.Add(":key", key);
                        parm.Add(":value", en_US);
                        parm.Add(":lan", "en_US");
                        AppDataBase.Execute(sql, parm, transaction, dbConnection);


                        if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                        {
                            sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                            parm = new DynamicParameters();
                            parm.Add(":key", zikey);
                            parm.Add(":value", en_US);
                            parm.Add(":lan", "en_US");
                            AppDataBase.Execute(sql, parm,transaction,dbConnection);
                        }
                    }

                    if (!string.IsNullOrEmpty(zn_CN))
                    {
                        sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                        parm = new DynamicParameters();
                        parm.Add(":key", key);
                        parm.Add(":value", zn_CN);
                        parm.Add(":lan", "zn_CN");
                        AppDataBase.Execute(sql, parm,transaction,dbConnection);

                        if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                        {
                            sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                            parm = new DynamicParameters();
                            parm.Add(":key", zikey);
                            parm.Add(":value", zn_CN);
                            parm.Add(":lan", "zn_CN");
                            AppDataBase.Execute(sql, parm,transaction,dbConnection);
                        }
                    }
                    #endregion

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    sql = @"select count(1)    
                                 from user_Tab_Columns t  
                                where t.table_name = '{0}' and t.COLUMN_NAME='{1}'  ";
                    object obj = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_TABLE, OBJ_PRO_CODE), dbConnection, null, transaction);
                    if (obj.ToString() != "0")
                    {
                        sql = @"alter table {0} drop column {1}";
                        AppDataBase.Execute(string.Format(sql, OBJ_TABLE, OBJ_PRO_CODE), null, transaction, dbConnection);
                    }
                    throw e;
                }
            }

            return "ok";
        }

        #endregion

        #region 修改对象属性
        public string ObjectProperty_Modify(ObejctProperty obejctProperty)
        {
            string sql = string.Empty;
            var parm = new DynamicParameters();

            #region 获取传入参数

            string OBJ_CODE = obejctProperty.OBJ_CODE.ToUpper();
            string OBJ_TABLE = obejctProperty.OBJ_TABLE.ToUpper();
            string OBJ_PRO_CODE = obejctProperty.OBJ_PRO_CODE.ToUpper();
            string OBJ_PRO_TYPE = obejctProperty.OBJ_PRO_TYPE;//类型
            string OBJ_PRO_LENGTH = string.Empty;
            if (OBJ_PRO_TYPE == "25")
            {
                OBJ_PRO_LENGTH = "300";//长度
            }
            else
            {
                OBJ_PRO_LENGTH = obejctProperty.OBJ_PRO_LENGTH;
            }

            string OBJ_PRO_VISIBLE = obejctProperty.OBJ_PRO_VISIBLE;
            string OBJ_PRO_ENABLE = obejctProperty.OBJ_PRO_ENABLE;
            string OBJ_PRO_NULL = obejctProperty.OBJ_PRO_NULL;
            string OBJ_PRO_CHECK = obejctProperty.OBJ_PRO_CHECK;
            string OBJ_PRO_DEFAULT = obejctProperty.OBJ_PRO_DEFAULT;//默认值
            string OBJ_PRO_OBJECT = obejctProperty.OBJ_PRO_OBJECT.ToUpper();
            string OBJ_PRO_DIGIT = obejctProperty.OBJ_PRO_DIGIT;
            string OBJ_PRO_USING = obejctProperty.OBJ_PRO_USING;
            string OBJ_PRO_ENUM = obejctProperty.OBJ_PRO_ENUM;
            string zn_CN = obejctProperty.zn_CN;
            string en_US = obejctProperty.en_US;
            string mbshow = obejctProperty.mbshow;
            string is_system = obejctProperty.IS_SYSTEM;
            string OBJ_PRO_POSITIVE = obejctProperty.OBJ_PRO_POSITIVE;
            #endregion

            #region 数据完整性检查

            if (string.IsNullOrEmpty(OBJ_CODE))
            {
                throw new Exception("对象编码不能为空！");
            }
            if (string.IsNullOrEmpty(OBJ_TABLE))
            {
                throw new Exception("对象表名不能为空！");
            }
            if (string.IsNullOrEmpty(OBJ_PRO_CODE))
            {
                throw new Exception("对象属性名不能为空！");
            }
            if (string.IsNullOrEmpty(zn_CN))
            {
                throw new Exception("中文名称不能为空！");
            }

            #endregion

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();

                try
                {
                    string typename = OBJ_PRO_TYPE;//字段类型
                    string typelen = OBJ_PRO_LENGTH;//字段长度
                    string typedigit = OBJ_PRO_DIGIT;//字段小数位数  

                    #region 如果属性是引用类型，则需要获取引用对象的外键，获取其数据类型
                    if (OBJ_PRO_TYPE == "5")
                    {
                        if (string.IsNullOrEmpty(OBJ_PRO_OBJECT))
                        {
                            throw new Exception("属性类型为引用类型的请设置其引用对象！");
                        }
                        sql = "select OBJ_PRO_TYPE,OBJ_PRO_LENGTH from flc_object_property t where obj_code='{0}' and obj_table='{1}' and obj_pro_USING=1";
                        sql = string.Format(sql, OBJ_PRO_OBJECT, "DATA_" + OBJ_PRO_OBJECT);

                        object dt = AppDataBase.Query(sql, null, transaction, dbConnection).FirstOrDefault();

                        if (dt == null)
                        {
                            throw new Exception("引用对象没有设置外键属性,不能被引用，请检查！");
                        }
                        typename = ((object[])((System.Collections.Generic.IDictionary<string, object>)dt).Values)[0].ToString();
                        typelen = ((object[])((System.Collections.Generic.IDictionary<string, object>)dt).Values)[1].ToString();
                    }
                    #endregion

                    #region 修改物理表字段

                    try
                    {
                        sql = string.Format(" alter table {0} modify ({1} {2})",
                            OBJ_TABLE, OBJ_PRO_CODE, ConvertType(typename, typelen, typedigit));

                        AppDataBase.Execute(sql, null, transaction, dbConnection);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.ToString() == "ORA-01439: 要更改数据类型, 则要修改的列必须为空")
                        {
                            throw new Exception("要更改数据类型, 则要修改的列必须为空");
                        }
                        else
                            throw new Exception("此处属性长度不允许变小", e);
                    }


                    //查询主键对应的列 
                    sql = @"select column_name   from   user_cons_columns where   table_name   =   '{0}'   
                        and constraint_name in (select constraint_name from user_constraints where table_name = '{1}' and constraint_type ='P')";
                    sql = string.Format(sql, OBJ_TABLE, OBJ_TABLE);
                    string PK_column = AppDataBase.ExecuteScalar(sql, dbConnection, null, transaction).ToString();
                    //如果存在多个主键  会抛出异常
                    if (OBJ_PRO_USING == "1")
                    {
                        if (string.IsNullOrEmpty(PK_column))
                        {
                            try
                            {
                                sql = string.Format(" alter table {0} add  primary key({1})", OBJ_TABLE, OBJ_PRO_CODE);
                                AppDataBase.Execute(sql, null, transaction, dbConnection);
                            }
                            catch
                            {
                                throw new Exception("主键列中数据重复,违反主键唯一性");
                            }
                        }
                        if (!string.IsNullOrEmpty(PK_column) && PK_column != OBJ_PRO_CODE)
                            throw new Exception("已存在其他主键,请检查!");
                    }

                    else
                    {
                        if (PK_column == OBJ_PRO_CODE)
                        {
                            sql = "alter table DATA_AGV drop primary key";
                            sql = string.Format("alter table {0} drop primary key", OBJ_TABLE);
                            AppDataBase.Execute(sql, null, transaction, dbConnection);
                        }
                    }
                    #endregion

                    Regex reg = new Regex("^[0-9]*$");

                    bool is_insert = false;

                    string OBJ_CODES = "";

                    #region 如果本身是主键 修改类型或者长度之后 引用此主键的对象全部更改

                    if (OBJ_PRO_USING == "1")
                    {
                        if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                            //拼成子表对象名
                            OBJ_CODES = OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1);
                        else
                            OBJ_CODES = OBJ_CODE;

                        sql = "select * from flc_object_property where obj_pro_object=:OBJ_CODE";
                        parm = new DynamicParameters();
                        parm.Add(":OBJ_CODE", OBJ_CODES);

                        IEnumerable<ObejctProperty> obejcts = AppDataBase.Query<ObejctProperty>(sql, parm, transaction, dbConnection);

                        if (obejcts.Count() > 0)
                        {
                            foreach (ObejctProperty item in obejcts)
                            {

                                string codesign =item.OBJ_CODE + item.OBJ_TABLE + item.OBJ_PRO_CODE;

                                sql = "update flc_object_property t set t.obj_pro_length=:obj_pro_length where obj_code||obj_table||obj_pro_code=:codesign ";

                                parm = new DynamicParameters();
                                parm.Add(":obj_pro_length", OBJ_PRO_LENGTH);
                                parm.Add(":codesign", codesign);

                                AppDataBase.Execute(sql, parm, transaction, dbConnection);

                                try
                                {
                                    sql = string.Format(" alter table {0} modify ({1} {2})",
                                        item.OBJ_TABLE, item.OBJ_PRO_CODE, ConvertType(typename, typelen, typedigit));

                                    AppDataBase.Execute(sql,null,transaction,dbConnection);
                                }
                                catch (Exception e)
                                {
                                    if (e.Message.ToString() == "ORA-01439: 要更改数据类型, 则要修改的列必须为空")
                                    {
                                        throw new Exception("要更改数据类型, 则要修改对象【" + item.OBJ_CODE + "】的列【" + item.OBJ_PRO_CODE + "】必须为空");
                                    }
                                    throw new Exception("此处属性长度不允许变小", e);
                                }

                            }
                        }

                    }



                    #endregion

                    #region 删除flc_object_property  属性

                    sql = @"delete flc_object_property t where OBJ_CODE=:OBJ_CODE
                                and OBJ_TABLE=:OBJ_TABLE and OBJ_PRO_CODE=:OBJ_PRO_CODE";

                    parm = new DynamicParameters();
                    parm.Add(":OBJ_CODE", OBJ_CODE);
                    parm.Add(":OBJ_TABLE", OBJ_TABLE);
                    parm.Add(":OBJ_PRO_CODE", OBJ_PRO_CODE);

                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                    {
                        is_insert = true;
                        //拼成子表对象名
                        OBJ_CODES = OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1);

                        parm = new DynamicParameters();
                        parm.Add(":OBJ_CODE", OBJ_CODES);
                        parm.Add(":OBJ_TABLE", OBJ_TABLE);
                        parm.Add(":OBJ_PRO_CODE", OBJ_PRO_CODE);

                        AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    }

                    #endregion

                    #region 插入属性表
                    sql = @"insert into flc_object_property(obj_code,obj_table,obj_pro_code,obj_pro_type,obj_pro_length,
        obj_pro_visible,obj_pro_enable,obj_pro_null,obj_pro_check,obj_pro_default,obj_pro_object,obj_pro_digit,obj_pro_using,obj_pro_enum,obj_pro_show,is_system,obj_pro_positive)
                                                 values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
        :obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_check,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using,:obj_pro_enum,:obj_pro_show,:is_system,:obj_pro_positive)";

                    parm = new DynamicParameters();
                    parm.Add(":obj_code", OBJ_CODE);
                    parm.Add(":obj_table", OBJ_TABLE);
                    parm.Add(":obj_pro_code", OBJ_PRO_CODE);
                    parm.Add(":obj_pro_type", OBJ_PRO_TYPE);
                    parm.Add(":obj_pro_length", string.IsNullOrEmpty(OBJ_PRO_LENGTH) ? "0" : OBJ_PRO_LENGTH);

                    parm.Add(":obj_pro_visible", string.IsNullOrEmpty(OBJ_PRO_VISIBLE) ? "1" : OBJ_PRO_VISIBLE);
                    parm.Add(":obj_pro_enable", string.IsNullOrEmpty(OBJ_PRO_ENABLE) ? "1" : OBJ_PRO_ENABLE);
                    parm.Add(":obj_pro_null", string.IsNullOrEmpty(OBJ_PRO_NULL) ? "1" : OBJ_PRO_NULL);
                    parm.Add(":obj_pro_check", string.IsNullOrEmpty(OBJ_PRO_CHECK) ? "1" : OBJ_PRO_CHECK);
                    parm.Add(":obj_pro_default", OBJ_PRO_DEFAULT);

                    parm.Add(":obj_pro_object", OBJ_PRO_OBJECT);
                    parm.Add(":obj_pro_digit", OBJ_PRO_DIGIT);
                    parm.Add(":obj_pro_using", string.IsNullOrEmpty(OBJ_PRO_USING) ? "0" : OBJ_PRO_USING);
                    parm.Add(":obj_pro_enum", OBJ_PRO_ENUM);
                    parm.Add(":obj_pro_show", mbshow);

                    parm.Add(":is_system", is_system);
                    parm.Add(":obj_pro_positive", OBJ_PRO_POSITIVE);

                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    if (is_insert)
                    {
                        parm = new DynamicParameters();
                        parm.Add(":obj_code", OBJ_CODES);
                        parm.Add(":obj_table", OBJ_TABLE);
                        parm.Add(":obj_pro_code", OBJ_PRO_CODE);
                        parm.Add(":obj_pro_type", OBJ_PRO_TYPE);
                        parm.Add(":obj_pro_length", string.IsNullOrEmpty(obejctProperty.OBJ_PRO_LENGTH) ? "0" : obejctProperty.OBJ_PRO_LENGTH);

                        parm.Add(":obj_pro_visible", string.IsNullOrEmpty(OBJ_PRO_VISIBLE) ? "1" : OBJ_PRO_VISIBLE);
                        parm.Add(":obj_pro_enable", string.IsNullOrEmpty(OBJ_PRO_ENABLE) ? "1" : OBJ_PRO_ENABLE);
                        parm.Add(":obj_pro_null", string.IsNullOrEmpty(OBJ_PRO_NULL) ? "1" : OBJ_PRO_NULL);
                        parm.Add(":obj_pro_check", string.IsNullOrEmpty(OBJ_PRO_CHECK) ? "1" : OBJ_PRO_CHECK);
                        parm.Add(":obj_pro_default", OBJ_PRO_DEFAULT);

                        parm.Add(":obj_pro_object", OBJ_PRO_OBJECT);
                        parm.Add(":obj_pro_digit", OBJ_PRO_DIGIT);
                        parm.Add(":obj_pro_using", string.IsNullOrEmpty(OBJ_PRO_USING) ? "0" : OBJ_PRO_USING);
                        parm.Add(":obj_pro_enum", OBJ_PRO_ENUM);
                        parm.Add(":obj_pro_show", mbshow);

                        parm.Add(":is_system", is_system);
                        parm.Add(":obj_pro_positive", OBJ_PRO_POSITIVE);

                        AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    }

                    #endregion

                    #region 插入中英文
                    string key = OBJ_CODE + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;
                    sql = string.Format("delete flc_lang where key='{0}'", key);
                    AppDataBase.Execute(sql,null,transaction,dbConnection);

                    string zikey = OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1) + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;
                    sql = string.Format("delete flc_lang where key='{0}'", zikey);
                    AppDataBase.Execute(sql, null, transaction, dbConnection);

                    #region 校验语言是否相同
                    sql = @"select count(*) from  (Select distinct obj_code,obj_table,obj_pro_code,laZ.Value CH from flc_object_property v
                        left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='zn_CN'
                        where obj_code like '{0}'  or obj_code like '{0}_ITEM%' ) where CH='{1}'";
                    object o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, zn_CN), dbConnection, null, transaction);
                    if (o != null && o.ToString() != "0")
                    {
                        throw new Exception("属性中文名重复，请检查！");
                    }
                    if (!string.IsNullOrEmpty(en_US))
                    {
                        sql = @"select count(*) from  (Select distinct obj_code,obj_table,obj_pro_code,laZ.Value US from flc_object_property v
                        left join flc_lang laZ on laZ.key=v.obj_code||'.'||v.obj_table||'.'||obj_pro_code and laZ.lan='en_US'
                         where obj_code like '{0}'  or obj_code like '{0}_ITEM') where lower(US)=lower('{1}')";
                        o = AppDataBase.ExecuteScalar(string.Format(sql, OBJ_CODE, en_US), dbConnection, null, transaction);
                        if (o != null && o.ToString() != "0")
                        {
                            throw new Exception("属性英文名重复，请检查！");
                        }
                    }

                    #endregion


                    if (!string.IsNullOrEmpty(en_US))
                    {
                        sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                        parm = new DynamicParameters();
                        parm.Add(":key", key);
                        parm.Add(":value", en_US);
                        parm.Add(":lan", "en_US");
   
                        AppDataBase.Execute(sql, parm, transaction, dbConnection);

                        if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                        {
                            sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";
                            parm = new DynamicParameters();
                            parm.Add(":key", zikey);
                            parm.Add(":value", en_US);
                            parm.Add(":lan", "en_US");
                            AppDataBase.Execute(sql, parm, transaction, dbConnection);
                        }
                    }

                    if (!string.IsNullOrEmpty(zn_CN))
                    {
                        sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                        parm = new DynamicParameters();
                        parm.Add(":key", key);
                        parm.Add(":value", zn_CN);
                        parm.Add(":lan", "zn_CN");

                        AppDataBase.Execute(sql, parm, transaction, dbConnection);

                        if (reg.IsMatch(OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1)))
                        {
                            sql = @"Insert into flc_lang(key,value,lan)values(:key,:value,:lan)";

                            parm = new DynamicParameters();
                            parm.Add(":key", zikey);
                            parm.Add(":value", zn_CN);
                            parm.Add(":lan", "zn_CN");

                            AppDataBase.Execute(sql, parm, transaction, dbConnection);
                        }
                    }
                    #endregion

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return "ok";
        }

        #endregion

        #region 删除对象属性
        public void ObjectProperty_Delete(string OBJ_CODE, string OBJ_TABLE, string OBJ_PRO_CODE, string bcheck)
        {
            string sql;

            #region 获取传入的参数
            bool bchecks = bcheck == "" ? false : Convert.ToBoolean(bcheck);
            #endregion

            #region 数据完整性检查

            if (string.IsNullOrEmpty(OBJ_CODE))
            {
                throw new Exception("对象编码不能为空！");
            }
            if (string.IsNullOrEmpty(OBJ_TABLE))
            {
                throw new Exception("对象表名不能为空！");
            }
            if (string.IsNullOrEmpty(OBJ_PRO_CODE))
            {
                throw new Exception("对象属性名不能为空！");
            }

            #endregion

            var parm = new DynamicParameters();
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    if (bchecks)
                    {
                        sql = string.Format(" select count(*) from {0} where {1} is  not null", OBJ_TABLE, OBJ_PRO_CODE);
                        object o = AppDataBase.ExecuteScalar(sql, dbConnection, null, transaction);
                        if (o != null && o.ToString() != "0")
                        {
                            throw new Exception(string.Format("属性[{0}] 已有数据，是否删除？", OBJ_PRO_CODE));
                        }
                    }

                    #region 删除flc_object_property  属性
                    sql = @"delete flc_object_property t where OBJ_CODE=:OBJ_CODE
and OBJ_TABLE=:OBJ_TABLE and OBJ_PRO_CODE=:OBJ_PRO_CODE";

                    parm = new DynamicParameters();
                    parm.Add(":OBJ_CODE", OBJ_CODE);
                    parm.Add(":OBJ_TABLE", OBJ_TABLE);
                    parm.Add(":OBJ_PRO_CODE", OBJ_PRO_CODE);

                    AppDataBase.Execute(sql, parm, transaction, dbConnection);

                    #endregion

                    #region 删除子表对象flc_object_property  属性
                    sql = @"delete flc_object_property t where OBJ_CODE=:OBJ_CODE
and OBJ_TABLE=:OBJ_TABLE and OBJ_PRO_CODE=:OBJ_PRO_CODE";

                    parm = new DynamicParameters();
                    parm.Add(":OBJ_CODE", OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1));
                    parm.Add(":OBJ_TABLE", OBJ_TABLE);
                    parm.Add(":OBJ_PRO_CODE", OBJ_PRO_CODE);                   

                    AppDataBase.Execute(sql, parm,transaction,dbConnection);

                    #endregion

                    #region 删除多语
                    string key = OBJ_CODE + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;
                    sql = string.Format("delete flc_lang where key='{0}'", key);
                    AppDataBase.Execute(sql, null, transaction, dbConnection);
                    #endregion

                    #region 删除子表多语
                    string zikey = OBJ_CODE + "_ITEM" + OBJ_TABLE.Substring(OBJ_TABLE.Length - 1, 1) + "." + OBJ_TABLE + "." + OBJ_PRO_CODE;

                    sql = string.Format("delete flc_lang where key='{0}'", zikey);
                    AppDataBase.Execute(sql,null,transaction,dbConnection);
                    #endregion

                    #region 删除物理表属性
                    sql = string.Format("alter table {0} drop column {1}", OBJ_TABLE, OBJ_PRO_CODE);
                    AppDataBase.Execute(sql,null,transaction,dbConnection);
                    #endregion

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

        }
        #endregion

        #region 获取对象接口数据
        public IEnumerable<FLC_OBJECTEVENT> GetObjectInterface(string OBJ_CODE, string bVouchControl)
        {
            IEnumerable<FLC_OBJECTEVENT> fLC_OBJECTEVENTs;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                // IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = @"Select * from FLC_OBJECTEVENT
                     where OBJ_CODE=:OBJ_CODE 
                     and obj_eventtype = :obj_eventtype
                     and RUN_ENABLE=1 order by RUN_INDEX ";

                    var parm = new DynamicParameters();
                    parm.Add(":OBJ_CODE", OBJ_CODE);
                    parm.Add(":obj_eventtype", bVouchControl);

                    fLC_OBJECTEVENTs = AppDataBase.Query<FLC_OBJECTEVENT>(sql, parm, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return fLC_OBJECTEVENTs;
        }
        #endregion

        #region 删除钱检查本身对象属性是否允许被删除
        public int bDelete(string OBJ_CODE, string OBJ_TABLE, string OBJ_PRO_CODE)
        {
            OBJ_CODE = OBJ_CODE.ToUpper();
            OBJ_TABLE = OBJ_TABLE.ToUpper();
            OBJ_PRO_CODE = OBJ_PRO_CODE.ToUpper();
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = string.Format(" select count(*) from {0} where {1} is  not null", OBJ_TABLE, OBJ_PRO_CODE);
                    object o = AppDataBase.ExecuteScalar(sql, dbConnection);
                    if (o != null && o.ToString() != "0")
                    {
                        return -1;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return 0;
        }
        #endregion

    }
}
