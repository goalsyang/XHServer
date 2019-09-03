using Dapper;
using ProjectManageServer.Common;
using ProjectManageServer.Model.ObjectCreate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ProjectManageServer.DataAccess
{
    public class ObjectCreateDataAccess
    {
        public static GetObj GetObj(string obj_code, string Language)
        {
            try
            {
                GetObj getObj = new GetObj();

                #region 对应UserControl 的编辑对象名
                string is_enable = "select is_enable from flc_object where obj_code='{0}'";
                string zn_CN1 = "select value from flc_lang where key='{0}' and lan='zn_CN'";
                string en_US1 = "select value from flc_lang where key='{0}' and lan='en_US'";

                is_enable = AppDataBase.ExecuteScalar(string.Format(is_enable, obj_code)).ToString();
                is_enable = (is_enable == "1" ? "是" : "否");
                zn_CN1 = AppDataBase.ExecuteScalar(string.Format(zn_CN1, obj_code)).ToString();
                en_US1 = AppDataBase.ExecuteScalar(string.Format(en_US1, obj_code)).ToString();

                Obj_Name obj_Name = new Obj_Name();
                obj_Name.Obj_Code = obj_code;
                obj_Name.zn_CN = zn_CN1;
                obj_Name.en_US = en_US1;
                obj_Name.Is_Enable = is_enable;

                getObj.obj_name = obj_Name;
                #endregion

                #region 对应UserControl 的对象表关系

                string getdata_objects = "Select * from FLC_OBJECTS where obj_code = :obj_code";
                var param = new DynamicParameters();
                param.Add(":obj_code", obj_code);
                IEnumerable<Flc_Objects> obj_relations = AppDataBase.Query<Flc_Objects>(getdata_objects, param);
                List<Obj_Relation> obj_Relations = new List<Obj_Relation>();

                foreach (Flc_Objects item in obj_relations)
                {
                    Obj_Relation obj_Relation = new Obj_Relation();
                        
                    obj_Relation.是否删除 = false;
                    obj_Relation.对象名 = item.Obj_Code;
                    obj_Relation.对象表名 = item.Obj_Table;

                    string code = obj_code + "." + item.Obj_Table;
                    string zn_CN2 = "select value from flc_lang where key='{0}' and lan='zn_CN'";
                    string en_US2 = "select value from flc_lang where key='{0}' and lan='en_US'";

                    zn_CN2 = AppDataBase.ExecuteScalar(string.Format(zn_CN2, code)).ToString();
                    en_US2 = AppDataBase.ExecuteScalar(string.Format(en_US2, code)).ToString();

                    obj_Relation.中文语言 = zn_CN2;
                    obj_Relation.英文语言 = en_US2;
                    string is_main = item.Is_Main.ToString();
                    is_main = (is_main == "1" ? "是" : "否");

                    obj_Relation.是否主表 = is_main;

                    obj_Relations.Add(obj_Relation);
                }

                getObj.obj_relation = obj_Relations;
                #endregion

                #region 对应UserControl 的对象属性

                string getdata_ObjTabProperty = "Select * from FLC_OBJECT_PROPERTY where obj_code = :obj_code";

                IEnumerable<Flc_Object_Property> obj_propertys = AppDataBase.Query<Flc_Object_Property>(getdata_ObjTabProperty, param);
                foreach (Flc_Object_Property item in obj_propertys)
                {
                    string code = obj_code + "." + item.Obj_Table + "." + item.Obj_Pro_Code.ToString().ToUpper();
                    string zn_CN2 = "select value from flc_lang where key='{0}' and lan='zn_CN'";
                    string en_US2 = "select value from flc_lang where key='{0}' and lan='en_US'";

                    zn_CN2 = AppDataBase.ExecuteScalar(string.Format(zn_CN2, code)).ToString();
                    en_US2 = AppDataBase.ExecuteScalar(string.Format(en_US2, code)).ToString();

                    item.Zn_Cn = zn_CN2;
                    item.en_Us = en_US2;
                    item.Is_Del = false;
                 
                }

                getObj.obj_property = obj_propertys;
                #endregion

                #region 获得所有对象名称
                string all_object = @"select obj_code,value from flc_object t left join flc_lang e on 
                                           t.obj_code=e.key and e.lan='" + Language + "'";
                IEnumerable<Allobj> allobjs = AppDataBase.Query<Allobj>(all_object);

                foreach (Allobj item in allobjs)
                {
                    if (item.Value == "")
                        item.Value = item.Obj_Code;
                }
                getObj.allobj = allobjs;
                #endregion

                #region 获得所有对象表
                string all_objecttab = "select obj_table from flc_objects where obj_code='" + obj_code + "'";
                IEnumerable<Allobjtab> allobjtab = AppDataBase.Query<Allobjtab>(all_objecttab);

                getObj.allobjtab = allobjtab;
                #endregion

                return getObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static Isexist_Obj isexist_Obj(string obj_code, string Language)
        {
            try
            {
                string is_exit = "select * from flc_object where obj_code='" + obj_code + "'";
                object o = AppDataBase.ExecuteScalar(is_exit);
                if (o == null || o.ToString() == string.Empty)
                    is_exit = "1";
                else
                    is_exit = "0";

                string all_object = @"select obj_code,value from flc_object t left join flc_lang e on 
                                           t.obj_code=e.key and e.lan='" + Language + "'";
                List<Allobj> allobjs = AppDataBase.Query<Allobj>(all_object).ToList();
                Allobj allobj = new Allobj();
                allobj.Obj_Code = obj_code;
                allobj.Value = "当前对象";
                allobjs.Insert(0, allobj);

                foreach (Allobj item in allobjs)
                {
                    if (item.Value == "")
                        item.Value = item.Obj_Code;
                }

                Isexist_Obj isexist_Obj = new Isexist_Obj();
                isexist_Obj.is_exit = is_exit;
                isexist_Obj.allobj = allobjs;
                return isexist_Obj;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string CrateObject_Drop(string obj_code)
        {

            //删除1.flc_object flc_objects flc_object_property flc_lang对应数据清除 2.对应表清除
            string del_obj = "delete from flc_object where obj_code like '" + obj_code + "%'";

            string del_objs = "delete from flc_objects where obj_code like '" + obj_code + "%'";

            string del_obj_pro = "delete from flc_object_property where obj_code like '" + obj_code + "%'";

            string del_lang = "delete from flc_lang where key like '" + obj_code + "%'";

            string sql;

            var param = new DynamicParameters();

            using (IDbConnection dbConnection = AppDataBase.DbConection)
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    sql = "Select * from FLC_OBJECTS where obj_code = :obj_code";
                    param.Add(":obj_code", obj_code);
                    IEnumerable<Flc_Objects> obj_relations = AppDataBase.Query<Flc_Objects>(sql, param, transaction);

                    foreach (Flc_Objects item in obj_relations)
                    {
                        string[] args = item.Obj_Table.Split('_');
                        if (!(args[0] == "FLC"))
                        {
                            AppDataBase.Execute(del_obj, null, transaction);
                            AppDataBase.Execute(del_objs, null, transaction);
                            AppDataBase.Execute(del_obj_pro, null, transaction);
                            AppDataBase.Execute(del_lang, null, transaction);
                            string setable = "select count(1) coun from user_tables where  table_name=:obj_table";
                            param = new DynamicParameters();
                            param.Add(":obj_table", item.Obj_Table);

                            int count = Convert.ToInt32(AppDataBase.ExecuteScalar(setable, dbConnection, param));
                            if (count > 0)
                            {
                                sql = "drop table " + item.Obj_Table;
                                AppDataBase.Execute(sql, null, transaction);
                            }
                        }

                    }
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

        public static string crateObject_alter(crateObject_alter crateObject_Alter)
        {
            try
            {
                using (IDbConnection dbConnection = AppDataBase.DbConection)
                {
                   // dbConnection=new 
                    dbConnection.Open();
                    IDbTransaction transaction = dbConnection.BeginTransaction();

                    try
                    {

                        #region 对象名称
                        Obj_Name obj_Name = crateObject_Alter.ObjectName;
                        if (obj_Name != null)
                        {
                            string obj_code = obj_Name.Obj_Code;
                            string zn_CN = obj_Name.zn_CN;
                            string en_US = obj_Name.en_US;

                            if (obj_Name.Is_Enable == "是")
                                obj_Name.Is_Enable = "1";
                            else
                                obj_Name.Is_Enable = "0";

                            int is_enable = Convert.ToInt32(obj_Name.Is_Enable);

                            string upobject = "update FLC_OBJECT set is_enable=:is_enable where obj_code = :obj_code";
                            var parm3 = new DynamicParameters();
                            parm3.Add(":is_enable", is_enable);
                            parm3.Add(":obj_code", obj_code);
                            AppDataBase.Execute(upobject, parm3, transaction);
                            //Update_FLC_Lang(obj_code, zn_CN, en_US, transaction);
                        }
                        #endregion

                        #region 对象表修改
                        List<ObjRelation> objRelations = crateObject_Alter.ObjRelation;
                        foreach (ObjRelation dr in objRelations)
                        {
                            int is_del = dr.is_del;
                            string obj_code = dr.obj_code;
                            string obj_table = dr.obj_table;
                            string zn_CN2 = dr.zn_CN;
                            string en_US2 = dr.en_US;
                            int is_main = dr.is_main;
                            //只做中英文修改 还有删除操作(flc_objects flc_obj_property flc_lang 中删除,删除表)
                            string signs = obj_code + "." + obj_table;

                            #region 删除操作
                            if (is_del == 1)
                            {
                                //主表不删除
                                if (is_main == 0)
                                {
                                    //flc_objects删除
                                    string del_objects = "delete from FLC_OBJECTS where obj_code=:obj_code and obj_table=:obj_table";
                                    //flc_obj_property删除
                                    string delvalue_FlcObjPro = "delete from FLC_OBJECT_PROPERTY where obj_code=:obj_code and obj_table=:obj_table";

                                    var parm1 = new DynamicParameters();
                                    parm1.Add(":obj_code", obj_code);
                                    parm1.Add(":obj_table", obj_table);

                                    //flc_lang删除
                                    string delvalue_FlcLang = "delete from FLC_LANG where key='" + signs + "'";

                                    string sele_tb = "select count(*) from user_tables where table_name='" + obj_table + "'";

                                    string drop_table = "drop table " + obj_table;

                                    AppDataBase.Execute(del_objects, parm1, transaction);

                                    AppDataBase.Execute(delvalue_FlcObjPro, parm1, transaction);

                                    AppDataBase.Execute(delvalue_FlcLang, transaction);

                                    //判断表是否存在
                                    int i = Convert.ToInt32(AppDataBase.ExecuteScalar(sele_tb, null, transaction));
                                    if (i > 0)
                                    {
                                        AppDataBase.Execute(drop_table, null, transaction);
                                    }
                                    else
                                        throw new Exception("此表不存在 ");

                                }
                            }
                            #endregion

                            #region 修改操作或新增
                            else
                            {
                               // Update_FLC_Lang(signs, zn_CN2, en_US2, transaction);

                                string seobjects = "Select count(*) from FLC_OBJECTS where obj_code = :obj_code and obj_table=:obj_table";
                                var parm10 = new DynamicParameters();
                                parm10.Add(":obj_code", obj_code);
                                parm10.Add(":obj_table", obj_table);

                                string inobjects = "insert into FLC_OBJECTS(obj_code,obj_table,is_main,id) values (:obj_code,:obj_table,:is_main,:id)";
                                //不存在就插入
                                int count = Convert.ToInt32(AppDataBase.ExecuteScalar(seobjects, dbConnection, parm10,transaction));
                                if (count < 1)
                                {
                                    int id = MethodGetSerial.getSerialNumInt("FLC_OBJECTS", transaction);
                                    var parm11 = new DynamicParameters();
                                    parm11.Add(":obj_code", obj_code);
                                    parm11.Add(":obj_table", obj_table);
                                    parm11.Add(":is_main", is_main);
                                    parm11.Add(":id", id);

                                    AppDataBase.Execute(inobjects,parm11);

                                    //新增之后构建表
                                    string creobjtab = "create table {0} (id number null,mid number null)";
                                    creobjtab = string.Format(creobjtab, obj_table,transaction);

                                    string setable = "select count(*) coun from user_tables where table_name=:obj_table";
                                    var parm12 = new DynamicParameters();
                                    parm12.Add(":obj_table", obj_table);
                                    int i = Convert.ToInt32(AppDataBase.ExecuteScalar(setable, dbConnection,parm12, transaction));
                                    if (count < 1)
                                    {
                                        AppDataBase.Execute(creobjtab,null,transaction);
                                    }

                                }
                            }


                            #endregion
                         
                        }

                        #endregion

                        #region 对象表属性修改
                        List<Flc_Object_Property> objproperty = crateObject_Alter.Objproperty;

                        foreach (Flc_Object_Property dr in objproperty)
                        {

                            #region 属性

                            string is_del = dr.Is_Del.ToString();
                            string obj_code = dr.Obj_Code.ToUpper();
                            string obj_table = dr.Obj_Table.ToUpper();
                            string obj_pro_code = dr.Obj_Pro_Code.ToUpper();
                            int obj_pro_type = Convert.ToInt32(dr.Obj_Pro_Type);

                            string obj_pro_length;
                            if (dr.Obj_Pro_Length.ToString() == null)
                                obj_pro_length = " ";
                            else
                                obj_pro_length = dr.Obj_Pro_Length.ToString();

                            int obj_pro_visible =dr.Obj_Pro_Visible;
                            int obj_pro_enable = dr.Obj_Pro_Enable;
                            int obj_pro_null = dr.Obj_Pro_Null;

                            if (dr.Obj_Pro_Default == null)
                                dr.Obj_Pro_Default = " ";
                            string obj_pro_default = dr.Obj_Pro_Default;

                            if (dr.Obj_Pro_Object == null)
                                dr.Obj_Pro_Object = " ";
                            string obj_pro_object = dr.Obj_Pro_Object;

                            string obj_pro_digit;
                            if (dr.Obj_Pro_Digit == null)
                                obj_pro_digit = " ";
                            else
                                obj_pro_digit = dr.Obj_Pro_Digit.ToString();

                            int obj_pro_using = dr.Obj_Pro_Using;

                            string zn_CN = dr.Zn_Cn;
                            string en_US = dr.en_Us;
                            string codes = obj_code + "." + obj_table + "." + obj_pro_code;
                            #endregion

                            #region 删除操作
                            if (is_del == "1" || is_del == "true")
                            {
                                string sefopro = "Select count(*) from FLC_OBJECT_PROPERTY where obj_code=:obj_code and obj_pro_code=:obj_pro_code";
                                var parm3 = new DynamicParameters();
                                parm3.Add(":obj_code", obj_code);
                                parm3.Add(":obj_pro_code", obj_pro_code);

                                int count = Convert.ToInt32(AppDataBase.ExecuteScalar(sefopro, dbConnection,parm3, transaction));
                                if (count < 1)
                                    continue;
                                else
                                {
                                    //1.属性表中删除，2.语言表删除 3.对象表中删除
                                    string delvalue_FlcObjPro = "delete from FLC_OBJECT_PROPERTY where obj_code=:obj_code and obj_pro_code=:obj_pro_code";
                                    AppDataBase.Execute(delvalue_FlcObjPro, parm3,transaction);
                                    string delvalue_FlcLang = "delete from FLC_LANG where key='{0}'";
                                    AppDataBase.Execute(string.Format(delvalue_FlcLang, codes),null,transaction);
                                    string delcolumn = "Alter table {0} drop column {1}";
                                    AppDataBase.Execute(string.Format(delcolumn, obj_table, obj_pro_code),null,transaction);
                                }

                            }

                            #endregion

                            #region 新增或更新操作
                            else
                            {

                                #region 操作语言表
                                //Update_FLC_Lang(codes, zn_CN, en_US, transaction);
                                codes = string.Empty;
                                #endregion

                                string sefopro = "Select count(*) from FLC_OBJECT_PROPERTY where obj_code=:obj_code and obj_pro_code=:obj_pro_code";
                                string infopro = @"insert into FLC_OBJECT_PROPERTY values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
                        :obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using)";
                                string upfopro = @"update FLC_OBJECT_PROPERTY set obj_code=:obj_code,obj_table=:obj_table,obj_pro_code=:obj_pro_code,obj_pro_type=:obj_pro_type,obj_pro_length=:obj_pro_length,
                        obj_pro_visible=:obj_pro_visible,obj_pro_enable=:obj_pro_enable,obj_pro_null=:obj_pro_null,obj_pro_default=:obj_pro_default,obj_pro_object=:obj_pro_object,
                        obj_pro_digit=:obj_pro_digit,obj_pro_using=:obj_pro_using where obj_code=:obj_code and obj_pro_code=:obj_pro_code";

                                var parm3 = new DynamicParameters();
                                parm3.Add(":obj_code", obj_code);
                                parm3.Add(":obj_pro_code", obj_pro_code);

                                var parm4 = new DynamicParameters();
                                parm4.Add(":obj_code", obj_code);
                                parm4.Add(":obj_table", obj_table);
                                parm4.Add(":obj_pro_code", obj_pro_code);
                                parm4.Add(":obj_pro_type", obj_pro_type);
                                parm4.Add(":obj_pro_length", obj_pro_length);
                                parm4.Add(":obj_pro_visible", obj_pro_visible);
                                parm4.Add(":obj_pro_enable", obj_pro_enable);
                                parm4.Add(":obj_pro_null", obj_pro_null);
                                parm4.Add(":obj_pro_default", obj_pro_default);
                                parm4.Add(":obj_pro_object", obj_pro_object);
                                parm4.Add(":obj_pro_digit", obj_pro_digit);
                                parm4.Add(":obj_pro_using", obj_pro_using);

                                string typess = string.Empty;
                                switch (obj_pro_type)
                                {
                                    case 1:
                                        typess = "nvarchar2(" + obj_pro_length + ")";
                                        break;
                                    case 2:
                                        typess = "nvarchar2(" + obj_pro_length + ")";
                                        break;
                                    case 3:
                                        typess = "number(" + obj_pro_length + ")";
                                        break;
                                    case 4:
                                        typess = "Date";
                                        break;
                                    case 5:
                                        typess = "nvarchar2(60)";
                                        break;
                                    //case 6:
                                    //typess = "nvarchar2(255)";
                                    //break;
                                    default:
                                        typess = "nvarchar2(255)";
                                        break;

                                }

                                int count = Convert.ToInt32(AppDataBase.ExecuteScalar(sefopro, dbConnection,parm3, transaction));
                                if (count < 1)
                                {
                                    AppDataBase.Execute(infopro, parm4,transaction);
                                    //判断表是否存在 不存在报错
                                    string setable = "select count(*) coun from user_tables where table_name='" + obj_table + "'";
                                    int i = Convert.ToInt32(AppDataBase.ExecuteScalar(setable,null,transaction));

                                    if (i > 0)
                                    {
                                        //在对象表中添加该属性
                                        string add = "alter table " + obj_table + " add " + obj_pro_code + " " + typess;
                                        AppDataBase.Execute(add,null,transaction);
                                    }
                                }
                                else
                                {
                                    //更新之后对应语言表也要更新(已完成) 同时对象表也要更新修改
                                    //此处修改对象表有1种 修改字段类型
                                    string gettype = "select obj_pro_type from flc_object_property where obj_code=:obj_code and obj_pro_code=:obj_pro_code";
                                    string getlength = "select obj_pro_length from flc_object_property where obj_code=:obj_code and obj_pro_code=:obj_pro_code";

                                    var parm5 = new DynamicParameters();
                                    parm5.Add(":obj_code", obj_code);
                                    parm5.Add(":obj_pro_code", obj_pro_code);

                                    int type = Convert.ToInt32(AppDataBase.ExecuteScalar(gettype, dbConnection,parm5, transaction));
       
                                    string length = AppDataBase.ExecuteScalar(getlength, dbConnection,parm5, transaction).ToString();
                                    if (length == "")
                                        length = "0";

                                    //更新属性表中值
                                    AppDataBase.Execute(upfopro, parm4,transaction);
                                    if (obj_pro_length == "")
                                        obj_pro_length = "0";
                                    if (type == obj_pro_type && Convert.ToInt32(length) >= Convert.ToInt32(obj_pro_length))
                                        continue;
                                    else
                                    {
                                        //此处异常会有很多
                                        try
                                        {
                                            string alter = "alter table {0} modify {1} {2}";
                                            alter = string.Format(alter, obj_table, obj_pro_code, typess);
                                            AppDataBase.Execute(alter,null,transaction);
                                        }
                                        catch (Exception e)
                                        {
                                            throw new Exception(e.ToString());
                                        }
                                    }

                                }
                                
                           }
                        }
                        #endregion

                        #endregion
                     
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw e;
                    }

                    transaction.Commit();
                }               

            }
            catch (Exception e)
            {
                throw e;
            }

            return "ok";
        }

        public static string crateObject_ADD(crateObject_alter crateObject_Alter)
        {
            try
            {
                using (IDbConnection dbConnection = (new AppDataBase()).connection)
                {
                    dbConnection.Open();
                    IDbTransaction transaction = dbConnection.BeginTransaction();
                    
                    try
                    {    
                        #region 对象名称                       
                        Obj_Name obj_Name = crateObject_Alter.ObjectName;
                        if (obj_Name != null)
                        {
                            string obj_code = obj_Name.Obj_Code;
                            string zn_CN1 = obj_Name.zn_CN;
                            string en_US1 = obj_Name.en_US;
                            string is_enable = obj_Name.Is_Enable;

                            //Update_FLC_Lang(dbConnection, obj_code, zn_CN1, en_US1, transaction);
                            string sql = "delete FLC_OBJECT where obj_code = :obj_code";

                            var parm1 = new DynamicParameters();
                            parm1.Add(":obj_code", obj_code);
                           // AppDataBase.ExecuteScalar(sql, dbConnection, parm1, transaction);

                            sql= "select count(*) from FLC_OBJECT where obj_code = :obj_code";
                            string str = AppDataBase.ExecuteScalar(sql, dbConnection, parm1, transaction).ToString();

                            int id1 = MethodGetSerial.getSerialNumInt("FLC_OBJECT", transaction, dbConnection);

                            string inobject = "insert into FLC_OBJECT(id,obj_code,is_enable) values (:id,:obj_code,:is_enable)";
                            var parm2 = new DynamicParameters();
                            parm2.Add(":id", id1);
                            parm2.Add(":obj_code", obj_code);
                            parm2.Add(":is_enable", is_enable);

                            AppDataBase.Execute(inobject, parm2,transaction);
                        }

                        //取得流水号
                        #endregion

                        #region 对象表新增
                        List<ObjRelation> objRelations = crateObject_Alter.ObjRelation;
                        foreach (ObjRelation dr in objRelations)
                        {                           
                            string obj_code = dr.obj_code;
                            string obj_table = dr.obj_table;
                            string zn_CN2 = dr.zn_CN;
                            string en_US2 = dr.en_US;
                            int is_main = dr.is_main;
                            //只做中英文修改 还有删除操作(flc_objects flc_obj_property flc_lang 中删除,删除表)
                            string sign = obj_code + "." + obj_table;
                           // Update_FLC_Lang(sign, zn_CN2, en_US2,transaction);

                            #region 在FLC_OBJECTS新增对应数据
                            //在FLC_OBJECTS删除对应数据
                            string sql = "delete FLC_OBJECTS where obj_code = :obj_code and obj_table=:obj_table";

                            var parm3 = new DynamicParameters();
                            parm3.Add(":obj_code", obj_code);
                            parm3.Add(":obj_table", obj_table);

                            AppDataBase.ExecuteScalar(sql, dbConnection,parm3, transaction);
                            //在FLC_OBJECTS新增对应数据
                            sql = "insert into FLC_OBJECTS(obj_code,obj_table,is_main,id) values (:obj_code,:obj_table,:is_main,:id)";
                            //取得流水号
                            int id2 = MethodGetSerial.getSerialNumInt("FLC_OBJECTS", transaction);
                            var parm4 = new DynamicParameters();
                            parm4.Add(":obj_code", obj_code);
                            parm4.Add(":obj_table", obj_table);
                            parm4.Add(":is_main", is_main);
                            parm4.Add(":id", id2);
                            AppDataBase.Execute(sql, parm4,transaction);

                            #endregion

                            #region 构建对象表
                            //查找此表是否存在语句
                            string setable = "select count(*) coun from user_tables where  table_name=:obj_table";

                            var parm5 = new DynamicParameters();
                            parm5.Add(":obj_table", obj_table);

                            //创建表语句
                            //主表
                            string creobjtab = "create table {0} (id number null)";
                            creobjtab = string.Format(creobjtab, obj_table);
                            //子表
                            string creobjtabs = "create table {0} (id number null,mid number null)";
                            creobjtabs = string.Format(creobjtabs, obj_table);

                            //查找此表是否存在 存在删除再创建。
                            int count = Convert.ToInt32(AppDataBase.ExecuteScalar(setable, dbConnection,parm5, transaction));
                            if (count > 0)
                            {
                                sql = "drop table " + obj_table;
                                AppDataBase.Execute(sql,null,transaction);
                            }
                            if (is_main == 1)
                                AppDataBase.Execute(creobjtab,null,transaction);
                            else
                                AppDataBase.Execute(creobjtabs,null,transaction);
                            #endregion

                        }
                        #endregion

                        #region 对象属性
                        List<Flc_Object_Property> objproperty = crateObject_Alter.Objproperty;

                        foreach (Flc_Object_Property dr in objproperty)
                        {
                            #region 属性                          
                            string obj_code = dr.Obj_Code.ToUpper();
                            string obj_table = dr.Obj_Table.ToUpper();
                            string obj_pro_code = dr.Obj_Pro_Code.ToUpper();
                            int obj_pro_type = Convert.ToInt32(dr.Obj_Pro_Type);

                            string obj_pro_length;
                            if (dr.Obj_Pro_Length.ToString() == null)
                                obj_pro_length = " ";
                            else
                                obj_pro_length = dr.Obj_Pro_Length.ToString();

                            int obj_pro_visible = dr.Obj_Pro_Visible;
                            int obj_pro_enable = dr.Obj_Pro_Enable;
                            int obj_pro_null = dr.Obj_Pro_Null;

                            if (dr.Obj_Pro_Default == null)
                                dr.Obj_Pro_Default = " ";
                            string obj_pro_default = dr.Obj_Pro_Default;

                            if (dr.Obj_Pro_Object == null)
                                dr.Obj_Pro_Object = " ";
                            string obj_pro_object = dr.Obj_Pro_Object;

                            string obj_pro_digit;
                            if (dr.Obj_Pro_Digit == null)
                                obj_pro_digit = " ";
                            else
                                obj_pro_digit = dr.Obj_Pro_Digit.ToString();

                            int obj_pro_using = dr.Obj_Pro_Using;

                            string zn_CN3 = dr.Zn_Cn;
                            string en_US3 = dr.en_Us;

                            #endregion

                            string codes = obj_code + "." + obj_table + "." + obj_pro_code;
                            //Update_FLC_Lang(codes, zn_CN3, en_US3,transaction);

                            #region 在FLC_OBJECT_PROPERTY插入数据
                            string sql = "delete FLC_OBJECT_PROPERTY where obj_code=:obj_code and obj_pro_code=:obj_pro_code";

                            var parm6 = new DynamicParameters();
                            parm6.Add(":obj_code", obj_code);
                            parm6.Add(":obj_pro_code", obj_pro_code);
                            AppDataBase.ExecuteScalar(sql, dbConnection,parm6, transaction);

                            string infopro = @"insert into FLC_OBJECT_PROPERTY values(:obj_code,:obj_table,:obj_pro_code,:obj_pro_type,:obj_pro_length,
                    :obj_pro_visible,:obj_pro_enable,:obj_pro_null,:obj_pro_default,:obj_pro_object,:obj_pro_digit,:obj_pro_using)";

                            var parm7 = new DynamicParameters();
                            parm7.Add(":obj_code", obj_code);
                            parm7.Add(":obj_table", obj_table);
                            parm7.Add(":obj_pro_code", obj_pro_code);
                            parm7.Add(":obj_pro_type", obj_pro_type);

                            parm7.Add(":obj_pro_length", obj_pro_length);
                            parm7.Add(":obj_pro_visible", obj_pro_visible);
                            parm7.Add(":obj_pro_enable", obj_pro_enable);
                            parm7.Add(":obj_pro_null", obj_pro_null);

                            parm7.Add(":obj_pro_default", obj_pro_default);
                            parm7.Add(":obj_pro_object", obj_pro_object);
                            parm7.Add(":obj_pro_digit", obj_pro_digit);
                            parm7.Add(":obj_pro_using", obj_pro_using);

                            AppDataBase.Execute(infopro, parm7,transaction);
                            #endregion

                            #region 在表中插入数据
                            //拼类型
                            string typess = string.Empty;
                            switch (obj_pro_type)
                            {
                                case 1:
                                    typess = "nvarchar2(" + obj_pro_length + ")";
                                    break;
                                case 2:
                                    typess = "nvarchar2(" + obj_pro_length + ")";
                                    break;
                                case 3:
                                    typess = "number(" + obj_pro_length + ")";
                                    break;
                                case 4:
                                    typess = "Date";
                                    break;
                                case 5:
                                    typess = "nvarchar2(60)";
                                    break;
                                default:
                                    typess = "nvarchar2(255)";
                                    break;
                            }

                            //在对象表中添加该属性
                            string add = "alter table " + obj_table + " add " + obj_pro_code + " {0}";
                            add = string.Format(add, typess);
                            AppDataBase.Execute(add,null,transaction);

                            #endregion

                        }

                        #endregion

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                       // dbConnection.Close(); 
                        //transaction.Dispose();
                        throw e;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return "ok";
        }

        private static void Update_FLC_Lang(IDbCommand dbCommand,string key, string zn_CN, string en_US, IDbTransaction dbTransaction=null)
        {
            string sql = "delete FLC_LANG where key=:key";

            var param = new DynamicParameters();
            param.Add(":key", key);
            AppDataBase.Execute(sql,param,dbTransaction);

            sql = "Insert into FLC_LANG(key,value,lan) values (:key,:zn_CN,:en_US)";
            //FLC_LANG中value列设置不允许为空，如果为空则不插入
            if (!(zn_CN == null || zn_CN == string.Empty))
            {
                sql = "Insert into FLC_LANG(key,value,lan) values (:key,:zn_CN,:en_US)";
                param.Add(":zn_CN", zn_CN);
                param.Add(":en_US", "zn_CN");
                AppDataBase.Execute(sql, param, dbTransaction);               
            }

            if (!(en_US == null || en_US == string.Empty))
            {
                param = new DynamicParameters();
                param.Add(":key", key);
                param.Add(":zn_CN", en_US);
                param.Add(":en_US", "En_US");
                AppDataBase.Execute(sql, param, dbTransaction);
            }

        }

    }
}
