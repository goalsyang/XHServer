using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class StatusDescriptionAttribute : BaseDescriptionAttribute
    {
        private Dictionary<object, object> keyValuePairs { get; set; } = new Dictionary<object, object>();

        public StatusDescriptionAttribute(object keys,object values)
        {  
            keyValuePairs.Add(keys, values);
        }

        public override object ConvertDescription(object key)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                return keyValuePairs[key];
            }
            else
            {
                return key;
            }

        }
    }
}
