using System;

namespace ProjectManageServer.Attributes
{
    public abstract class BaseValidateAttribute : Attribute
    { 
        public abstract string MsgInfo { get; }
         
        public abstract bool Validate(object value);
         
    }
}
