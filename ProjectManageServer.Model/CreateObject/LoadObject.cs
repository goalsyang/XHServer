using ProjectManageServer.Model.ObjectCreate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.CreateObject
{
    public class LoadObject
    {
        public dtObject dtObject { get; set; }

        public IEnumerable<FLC_LANG> Language { get; set; }

    }

    public class dtObject
    {
        public string obj_code { get; set; }

        public string is_Enable { get; set; }

        public string is_system { get; set; }

        public string is_enum { get; set; }

        public string tableNum { get; set; }
    }


    public class FLC_OBJECT
    {
        public string obj_code { get; set; }

        public string is_Enable { get; set; }

        public string is_system { get; set; }

        public string is_enum { get; set; }

    }

    public class FLC_LANG
    {
        public string key { get; set; }

        public string value { get; set; }

        public string lan { get; set; }
    }


    public class LoadObejctProperty
    {

        public IEnumerable<ObejctProperty> dtProperty { get; set; }

        public IEnumerable<Flc_Objects> dtName { get; set; }

    }


}
