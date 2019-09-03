using System;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectManageServer.Attributes
{
    public class ValueConvertAttribute : Attribute
    {
        public string MsgInfo { get; }

        private Dictionary<object, object> keyValuePairs;

        private string convertObject;

        public ValueConvertAttribute(Dictionary<object, object> _keyValuePairs, string _convertObject)
        {
            this.keyValuePairs = _keyValuePairs;

            this.convertObject = _convertObject;

        }

        public void Converter<T>(T inmodel, out T outmodel)
        {
            Type type = typeof(T);

            object obj = Activator.CreateInstance(type);

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propert in props)
            {
                if (propert.Name == convertObject)
                {
                    string convert = NullValue(keyValuePairs[propert.GetValue(propert, null)]);

                    propert.SetValue(propert, convert, null);
                }
            }

            outmodel = inmodel;
        }

        private string NullValue(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

    }
}
