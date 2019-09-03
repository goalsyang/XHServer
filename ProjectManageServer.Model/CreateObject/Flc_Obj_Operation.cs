using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.CreateObject
{
    public class Flc_Obj_Operation
    {
        //拖把按钮实体表
        //obj_code,operation_id,from_system,btn_index,page

        public string obj_code { get; set; } 

        public string operation_id { get; set; }

        public string from_system { get; set; }

        public string btn_index { get; set; }

        public string page { get; set; }

    }
}
