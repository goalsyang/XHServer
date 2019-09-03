using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.ObjectCreate
{
    public class GetObj
    {

        public Obj_Name obj_name { get; set; }

        public List<Obj_Relation> obj_relation { get; set; }

        public IEnumerable<Flc_Object_Property> obj_property { get; set; }

        public IEnumerable<Allobj> allobj { get; set; }

        public IEnumerable<Allobjtab> allobjtab { get; set; }

    }

    public class Obj_Name
    {

        public string Obj_Code { get; set; }

        public string zn_CN { get; set; }

        public string en_US { get; set; }

        public string Is_Enable { get; set; }
    }

    public class Obj_Relation
    {
        public bool 是否删除 { get; set; }

        public string 对象名 { get; set; }

        public string 对象表名 { get; set; }

        public string 中文语言 { get; set; }

        public string 英文语言 { get; set; }

        public string 是否主表 { get; set; }

    }

    public class Allobj
    {
        public string Obj_Code { get; set; }

        public string Value { get; set; }
    }

    public class Allobjtab
    {
        public string Obj_Table { get; set; }

    }

}
