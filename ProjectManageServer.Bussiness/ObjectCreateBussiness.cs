using ProjectManageServer.DataAccess;
using ProjectManageServer.Interface;
using ProjectManageServer.Model.ObjectCreate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Bussiness
{
    public class ObjectCreateBussiness : IObjectCreate
    {
        public Dictionary<string, string> crateObject_ADD(crateObject_alter createObject_Alter)
        {
            try
            {
                string Result = ObjectCreateDataAccess.crateObject_ADD(createObject_Alter);
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("REEE", Result);
                return keyValuePairs;
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        public Dictionary<string, string> crateObject_alter(crateObject_alter createObject_Alter)
        {
            try
            {
                string Result = ObjectCreateDataAccess.crateObject_alter(createObject_Alter);
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("REEES", Result);
                return keyValuePairs;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string crateObject_drop(string obj_code)
        {
            try
            {
                string Result = ObjectCreateDataAccess.CrateObject_Drop(obj_code);
                if (Result == "ok")
                    return "删除成功!";
                else
                    return "删除失败!";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        GetObj IObjectCreate.GetObj(string obj_code, string Language)
        {
            try
            {
                GetObj getObj = ObjectCreateDataAccess.GetObj(obj_code, Language);

                return getObj;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        Isexist_Obj IObjectCreate.Isexist_Obj(string obj_code, string Language)
        {
            try
            {                
                return ObjectCreateDataAccess.isexist_Obj(obj_code, Language);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
