using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.ObjectCreate
{
    public class crateObject_alter
    {
        public Obj_Name ObjectName { get; set; }

        public List<ObjRelation> ObjRelation { get; set; }

        public List<Flc_Object_Property> Objproperty { get; set; }

    }


    public class ObjRelation
    {

        public dynamic is_del { get; set; }

        public dynamic obj_code { get; set; }

        public dynamic obj_table { get; set; }

        public dynamic zn_CN { get; set; }

        public dynamic en_US { get; set; }

        public dynamic is_main { get; set; }

    }


}
