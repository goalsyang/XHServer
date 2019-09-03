using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.CreateObject
{
    public class ModifyObject
    {
        public List<dtObject> dtObject { get; set; }

        public List<FLC_LANG> Language { get; set; }

        public List<Flc_Obj_Operation> checkboxbutton { get; set; }

    }
}
