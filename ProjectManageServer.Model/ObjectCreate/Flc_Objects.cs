using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.ObjectCreate
{
    public class Flc_Objects
    {
        //Select * from FLC_OBJECTS 

        public int ID { get; set; }

        public string Obj_Code { get; set; }

        public string Obj_Table { get; set; }

        public int Is_Main { get; set; }

        public string Data_View { get; set; }

    }
}
