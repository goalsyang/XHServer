using Dapper;
using ProjectManageServer.Common;
using ProjectManageServer.Model.Authorization;
using ProjectManageServer.Model.ObjectExport;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProjectManageServer.DataAccess
{
    public class ObjectExportDataAccess
    {

        public GetExportData GetExportData(string objectCode)
        {
            GetExportData getExportData = new GetExportData();
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {             
                dbConnection.Open();
               // IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = @"select * from V_FLC_OBJECTPROPERTY   where obj_code = '" + objectCode + @"'  and lan = 'zn_CN'
                 order by obj_table ,obj_pro_using desc,obj_pro_null desc  ";
                    getExportData.v_FLC_s = AppDataBase.Query<V_FLC_OBJECTPROPERTY>(sql, null, null, dbConnection);

                    string sql2 = @"select *from flc_objects   objs
                                    left join flc_lang lan on lan.key=objs.obj_code||'.'||objs.obj_table
                                      where obj_code='" + objectCode + "'   and lan='zn_CN'  and is_main=0         ";
                    getExportData.objectsUnions = AppDataBase.Query<ObjectsUnionLang>(sql2, null, null, dbConnection);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return getExportData;
        }

        public IEnumerable<dynamic> GetObjectDataL()
        {
            IEnumerable<dynamic> dynamics = null; 
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = @"Select Obj.Id,Obj.Obj_Code,Lng.Key,Lng.Value as Chinese,Flc.Value as English,
                                case Obj.Is_Enable when 1 then '是' when 0 then '否' end Is_Enable,
                                case Obj.Is_System when 1 then '是' when 0 then '否' end Is_System
                                        From FLC_OBJECT Obj 
                                        Left Join FLC_LANG Lng On Lng.Key=Obj.Obj_Code And Lng.lan='zn_CN'
                                        Left Join FLC_LANG Flc On Flc.Key=Lng.Key And Flc.lan='en_US'
                                        where is_show='1'
                                        Order By Obj.Obj_Code Asc";
                    dynamics = AppDataBase.Query<dynamic>(sql, null, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }


            return dynamics;
        }

        public IEnumerable<V_FLC_MENU> GetObjectList(string lan)
        {
            IEnumerable<V_FLC_MENU> v_FLC_MENUs = null;
            lan = string.IsNullOrEmpty(lan) ? "zn_CN" : lan;
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = @"select t1.* from v_FLC_MENU t1 where lan = :lan 
                        and is_sys <> 1 and is_enable = 1 and is_show = 1  and is_from_model <> 0   order by disp_order";

                    var parm = new DynamicParameters();
                    parm.Add(":lan", lan);
                    v_FLC_MENUs = AppDataBase.Query<V_FLC_MENU>(sql, parm, null, dbConnection);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return v_FLC_MENUs;
        }

        public GetProperty GetProperty(string obj_code)
        {
            GetProperty getExportData = new GetProperty();
            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                // IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string sql = @"select * from V_FLC_OBJECTPROPERTY 
                           where obj_code = :obj_code    and lan = 'zn_CN' ";
                    var para = new DynamicParameters();
                    para.Add(":obj_code", obj_code);
               
                    getExportData.objects = AppDataBase.Query<V_FLC_OBJECTPROPERTY>(sql, para, null, dbConnection);

                    string sql2 = @"select *from flc_objects   objs
                                    left join flc_lang lan on lan.key=objs.obj_code||'.'||objs.obj_table
                                      where obj_code=:obj_code   and lan='zn_CN'  and is_main=0         ";
                    getExportData.tables = AppDataBase.Query<ObjectsUnionLang>(sql2, para, null, dbConnection);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return getExportData;
        }

        public IEnumerable<dynamic> GetProductclineDA(string opseq,string cinvcode)
        {
            IEnumerable<dynamic> ProductclineDA = null;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = @"select *from Data_Productcline_Item0 item0
                            inner join data_PRODUCTCLINE duc on item0.mid=duc.id 
                            where item0.opseq='" + opseq + "' and duc.cinvcode='" + cinvcode + @"' and  versioncode=(select max(versioncode) from 
                            (select *from Data_Productcline_Item0 item0
                            inner join data_PRODUCTCLINE duc on item0.mid=duc.id 
                            where item0.opseq='" + opseq + "' and duc.cinvcode='" + cinvcode + @"') t
                            ) ";
                    ProductclineDA = AppDataBase.Query<dynamic>(sql, null, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return ProductclineDA;
        }

        public IEnumerable<dynamic> GetProdclineDA(string opseq, string cinvcode)
        {
            IEnumerable<dynamic> GetProdclineDA = null;

            using (IDbConnection dbConnection = (new AppDataBase()).connection)
            {
                dbConnection.Open();
                try
                {
                    string sql = @"select *from Data_PRODCLLIN_Item0 item0 
                            inner join data_PRODCLLIN duc on item0.mid=duc.id 
                            inner join data_INVENTORY inv on inv.cinvccode=duc.ccinvcode
                            where item0.opseq='" + opseq + "' and inv.cinvcode='" + cinvcode + @"' and versioncode=(select max(versioncode) from 
                            (select *from Data_PRODCLLIN_Item0 item0 
                            inner join data_PRODCLLIN duc on item0.mid=duc.id 
                            inner join data_INVENTORY inv on inv.cinvccode=duc.ccinvcode
                            where item0.opseq='" + opseq + "' and inv.cinvcode='" + cinvcode + @"') t
                            )";
                    GetProdclineDA = AppDataBase.Query<dynamic>(sql, null, null, dbConnection);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return GetProdclineDA;
        }


    }
}
