using System;

namespace ProjectManageServer.Attributes
{
    public abstract class BaseDescriptionAttribute:Attribute
    { 
        public abstract object ConvertDescription(object key);

    }
}
