using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.ObjectExport
{
    public class FUNCTION
    {
    }

    public class GetExportData
    {
        public IEnumerable<V_FLC_OBJECTPROPERTY> v_FLC_s { get; set; }

        public IEnumerable<ObjectsUnionLang> objectsUnions { get; set; }
    }

    public class GetProperty
    {
        public IEnumerable<V_FLC_OBJECTPROPERTY> objects { get; set; }

        public IEnumerable<ObjectsUnionLang> tables { get; set; }
    }

}
