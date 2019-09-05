using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.ObjectExport
{
    public class V_FLC_OBJECTPROPERTY
    {
        public string obj_code { get; set; }

        public string obj_table { get; set; }

        public string obj_pro_code { get; set; }

        public string obj_pro_type { get; set; }

        public string obj_pro_length { get; set; }

        public string obj_pro_visible { get; set; }

        public string obj_pro_enable { get; set; }

        public string obj_pro_null { get; set; }

        public string obj_pro_default { get; set; }

        public string obj_pro_object { get; set; }

        public string obj_pro_digit { get; set; }

        public string obj_pro_using { get; set; }

        public string obj_pro_enum { get; set; }

        public string obj_pro_show { get; set; }

        public string obj_name { get; set; }

        public string obj_pro_type_name { get; set; }

        public string lan { get; set; }

        public string obj_pro_check { get; set; }

        public string is_system { get; set; }

        public string obj_pro_positive { get; set; }

        public string obj_pro_order { get; set; }

    }


    public class ObjectsUnionLang
    {
        //OBJ_CODE OBJ_TABLE   IS_MAIN DATA_VIEW   ID KEY VALUE LAN

        public int? ID { get; set; }

        public string OBJ_CODE { get; set; }

        public string OBJ_TABLE { get; set; }

        public int? IS_MAIN { get; set; }

        public string DATA_VIEW { get; set; }

        public string KEY { get; set; }

        public string VALUE { get; set; }

        public string LAN { get; set; }

    }
}
