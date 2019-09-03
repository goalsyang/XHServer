using ProjectManageServer.Model.ObjectCreate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Interface
{
    public interface IObjectCreate
    {
        GetObj GetObj(string obj_code, string Language);

        Isexist_Obj Isexist_Obj(string obj_code, string Language);

        string crateObject_drop(string obj_code);

        Dictionary<string, string> crateObject_alter(crateObject_alter createObject_Alter);

        Dictionary<string, string> crateObject_ADD(crateObject_alter createObject_Alter);
    }
}
