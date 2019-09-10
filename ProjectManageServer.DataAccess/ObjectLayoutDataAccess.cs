using Dapper;
using ProjectManageServer.Common;
using ProjectManageServer.Model.ObjectExport;
using ProjectManageServer.Model.ObjectLayout;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ProjectManageServer.DataAccess
{
    public class ObjectLayoutDataAccess
    {

        #region 通过对象ID获取属性
        public List<Dictionary<string,IEnumerable<V_FLC_OBJECTPROPERTY>>> GetPropertyByObjCode(string objectCode, string Lan)
        {
            if (string.IsNullOrEmpty(Lan))
            {
                Lan = "zn_CN";
            }

            List<Dictionary<string, IEnumerable<V_FLC_OBJECTPROPERTY>>> TableList = null;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                // IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    //ObjectsUnionLang
                    string sql = @"select t.*,l.value from FLC_Objects t
                    left join FLC_LANG l on Lower(l.key) = Lower(Concat(Concat(t.obj_code,'.'),t.obj_table))
                    where obj_code = :objectCode and l.lan = :LAN
                    union all
                    select t.*,l.value from FLC_Objects t
                    left join FLC_LANG l on Lower(l.key) = Lower(Concat(Concat(t.obj_code,'.'),t.obj_table))
                    where obj_code = :objectCode and l.lan is null
                    ";

                    var para = new DynamicParameters();
                    para.Add(":objectCode", objectCode);
                    para.Add(":LAN", Lan);

                    IEnumerable<ObjectsUnionLang> objectsUnionLangs = AppDataBase.Query<ObjectsUnionLang>(sql, para, null, dbConnection);

                    Dictionary<string, IEnumerable<V_FLC_OBJECTPROPERTY>> keyValues = new Dictionary<string, IEnumerable<V_FLC_OBJECTPROPERTY>>();

                    foreach (ObjectsUnionLang item in objectsUnionLangs)
                    {                       
                        IEnumerable<V_FLC_OBJECTPROPERTY> v_FLC_OBJECTPROPERTY = null;

                        sql = @"Select * from v_flc_objectproperty v where
                        lower(v.obj_code)=Lower(:objectCode) and V.LAN=:LAN and lower(v.obj_table) = lower(:objectTable) and v.obj_pro_visible='0'
                        union all
                        Select * from v_flc_objectproperty v
                        where lower(v.obj_code)=Lower(:objectCode) and V.LAN is null and lower(v.obj_table) = lower(:objectTable) and v.obj_pro_visible='0'
                    ";
                        para = new DynamicParameters();
                        para.Add(":objectCode", objectCode);
                        para.Add(":LAN", Lan);
                        para.Add(":objectTable",item.OBJ_TABLE);

                        v_FLC_OBJECTPROPERTY = AppDataBase.Query<V_FLC_OBJECTPROPERTY>(sql, para, null, dbConnection);

                        if (string.IsNullOrEmpty(item.VALUE))
                        {
                            item.VALUE = item.OBJ_TABLE;
                        }
                        keyValues.Add(item.VALUE, v_FLC_OBJECTPROPERTY);                     
                    }

                    TableList.Add(keyValues);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return TableList;
        }
        #endregion

        #region 获取当前布局
        public void GetLayoutByObjectCode(string objectCode, string Lan)
        {
            if (string.IsNullOrEmpty(Lan))
            {
                Lan = "zn_CN";
            }
            string sql;
            var para = new DynamicParameters();
            string count = "0";

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    sql = @"select t.*,t1.obj_pro_type,t1.obj_pro_length,t1.obj_pro_digit,t1.obj_pro_using,t1.obj_pro_enum as pro_using
,t.notnull as obj_pro_null  ,t1.obj_pro_default,t.OFFER_NUMBER ,t2.obj_pro_enable,t1.obj_pro_positive
from flc_obj_show t,FLC_OBJECT_PROPERTY t1,FLC_OBJECT_PROPERTY t2
where lower(t.obj_code) = lower(:objectCode)
and ((t.obj_pro_object like '%|'|| t1.obj_code and t.obj_pro_table like '%|'||t1.obj_table and t.obj_pro_source like '%.'||t1.obj_pro_code)
or (t.obj_pro_object = t1.obj_code and t.obj_pro_table =t1.obj_table and t.obj_pro_source =t1.obj_pro_code)
or (t1.obj_pro_type = 6 and t1.obj_code = t.obj_code and t1.obj_table = t.obj_table and t1.obj_pro_code = t.obj_pro_code))
and t.lan = :LAN
and t.obj_code= t2.obj_code and t.obj_table = t2.obj_table and 
(t.obj_pro_source like t2.obj_pro_code||'.%' or t.obj_pro_source = t2.obj_pro_code)
order by IORDER,layout_y,layout_x";

                    para.Add(":objectCode", objectCode);
                    para.Add(":LAN", Lan);
                    IEnumerable<dynamic> LayoutTable = AppDataBase.Query<dynamic>(sql, para, transaction, dbConnection);

                    var counst = LayoutTable.Where(x => x.obj_pro_type == 6);
                    if (counst.Count() > 0)
                    {
                        string enumObject = "'-999'";
                        foreach (var dr in counst)
                        {
                            enumObject += ",'" + dr.pro_using.Trim() + "'";
                        }
                        sql = "Select * from data_enummethon where enum_key in (" + enumObject + ") and lan = :Lan";
                        para = new DynamicParameters();
                        para.Add(":Lan", Lan);

                        IEnumerable<dynamic> Enum = AppDataBase.Query<dynamic>(sql, para, transaction, dbConnection);
                    }


                    sql = @"   
                         Select   rec.obj_code as objectcontrol,rec.iorder, rec.obj_pro_object,rec.obj_pro_name,
                        case when pro.obj_code is null then   rec.obj_pro_object else  pro.obj_code end as obj_code,
                        case when pro.obj_table is null then   rec.obj_pro_table else  pro.obj_table end as obj_table,
                        case when pro.obj_pro_code is null then  rec.obj_pro_source else  pro.obj_pro_code end as obj_pro_code,
                        pro.OBJ_PRO_TYPE,pro.obj_pro_enable,rec.obj_pro_name as   obj_name   
                        from FLC_LIST_RECORD rec
                         left join v_flc_objectproperty pro on pro.obj_code=rec.obj_pro_object and lan='zn_CN' and pro.obj_pro_code=rec.obj_pro_source  
                         where   rec.obj_code like :objCode   and rec.OBJ_LIST_CODE = :listCode and   type='Reference'
                         order by rec.obj_code,rec.iorder      ";

                    para = new DynamicParameters();
                    para.Add(":objCode", objectCode + ".%");
                    para.Add(":listCode", "test");




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

        #region 还原默认布局
        public GetDefaultLayout GetDefaultLayout(string objectCode, string Lan)
        {
            GetDefaultLayout getDefaultLayout = null;
            if (string.IsNullOrEmpty(Lan))
            {
                Lan = "zn_CN";
            }
            string sql;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                //IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    sql = @"select t.*,t1.obj_pro_type,t1.obj_pro_length,t1.obj_pro_digit,t1.obj_pro_enable,t1.obj_pro_using,t1.obj_pro_enum as pro_using
                        ,t.notnull as obj_pro_null  ,t1.obj_pro_default,t.OFFER_NUMBER
                        from FLC_OBJ_SHOW_DEFAULT t,FLC_OBJECT_PROPERTY t1
                        where lower(t.obj_code) = lower(:objectCode)
                        and ((t.obj_pro_object like '%|'|| t1.obj_code and t.obj_pro_table like '%|'||t1.obj_table and t.obj_pro_source like '%.'||t1.obj_pro_code)
                        or (t.obj_pro_object = t1.obj_code and t.obj_pro_table =t1.obj_table and t.obj_pro_source =t1.obj_pro_code)
                        or (t1.obj_pro_type = 6 and t1.obj_code = t.obj_code and t1.obj_table = t.obj_table and t1.obj_pro_code = t.obj_pro_code))
                        and t.lan = :LAN  order by IORDER,layout_y,layout_x";

                    var para = new DynamicParameters();
                    para.Add(":objectCode", objectCode);
                    para.Add(":LAN", Lan);

                    IEnumerable<dynamic> LayoutTable = AppDataBase.Query<dynamic>(sql, para, null, dbConnection);
                    getDefaultLayout.LayoutTable = LayoutTable;

                    IEnumerable<dynamic> drs = LayoutTable.Where(x => x.obj_pro_type == 6);

                    if (drs.Count() > 0)
                    {
                        string enumObject = "'-999'";
                        foreach (DataRow dr in drs)
                        {
                            enumObject += ",'" + dr["pro_using"].ToString().Trim() + "'";
                        }
                        sql = "Select * from data_enummethon where enum_key in (" + enumObject + ") and lan = :Lan";

                        para = new DynamicParameters();
                        para.Add(":Lan", Lan);

                        IEnumerable<dynamic> Enum = AppDataBase.Query<dynamic>(sql, para, null, dbConnection);
                        getDefaultLayout.Enum = Enum;
                    }

                    sql = @"   
                         Select   rec.obj_code as objectcontrol,rec.iorder, rec.obj_pro_object,rec.obj_pro_name,
                        case when pro.obj_code is null then   rec.obj_pro_object else  pro.obj_code end as obj_code,
                        case when pro.obj_table is null then   rec.obj_pro_table else  pro.obj_table end as obj_table,
                        case when pro.obj_pro_code is null then  rec.obj_pro_source else  pro.obj_pro_code end as obj_pro_code,
                        pro.OBJ_PRO_TYPE,pro.obj_pro_enable,rec.obj_pro_name as   obj_name   
                        from FLC_LIST_RECORD rec
                         left join v_flc_objectproperty pro on pro.obj_code=rec.obj_pro_object and lan='zn_CN' and pro.obj_pro_code=rec.obj_pro_source  
                         where   rec.obj_code like :objCode   and rec.OBJ_LIST_CODE = :listCode and   type='Reference'
                         order by rec.obj_code,rec.iorder      ";

                    para = new DynamicParameters();
                    para.Add(":objCode", objectCode + ".%");
                    para.Add(":listCode", "test");

                    IEnumerable<dynamic> dtShow = AppDataBase.Query<dynamic>(sql,para,null,dbConnection);
                    getDefaultLayout.dtShow = dtShow;
                    //transaction.Commit();
                }
                catch (Exception e)
                {
                   // transaction.Rollback();
                    throw e;
                }
            }

            return getDefaultLayout;

        }
        #endregion

    }
}
