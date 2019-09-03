using System;

namespace ProjectManageServer.Attributes
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class StringIsNullAttribute : BaseValidateAttribute
    {
        public override string MsgInfo { get; } 

        public StringIsNullAttribute(string _MsgInfo)
        {
            this.MsgInfo = _MsgInfo;
        }

        public override bool Validate(object value)
        {
            return !string.IsNullOrWhiteSpace(NullValue(value));
        }

        private string NullValue(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }
         
    }
}
