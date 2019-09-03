using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Common
{
    public static class Utils
    {

        public static string NullValue(this object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

    }
}
