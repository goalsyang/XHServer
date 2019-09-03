using System;
using System.ComponentModel;
using System.Reflection;

namespace ProjectManageServer.Attributes
{
    public static class AttributeCommonValidate
    {

        /// <summary>
        /// 特性统一验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        public static void Validates<T>(this T entitys)
        {
            Type type = entitys.GetType();

            foreach (var item in type.GetProperties())
            {
                if (item.IsDefined(typeof(BaseValidateAttribute), true))
                {

                    object value = item.GetValue(entitys, null);

                    foreach (BaseValidateAttribute items in item.GetCustomAttributes(typeof(BaseValidateAttribute), true))
                    {
                        if (!items.Validate(value))
                        {
                            throw new Exception(items.MsgInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 枚举转换
        /// </summary>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum enumeration)
        {
            Type type = enumeration.GetType();

            MemberInfo[] memInfo = type.GetMember(enumeration.ToString());

            if (null != memInfo && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (null != attrs && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return enumeration.ToString();
        }

        /// <summary>
        /// 状态转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public static void ConvertDescription<T>(this T entitys)
        {
            if (entitys != null)
            {
                Type type = entitys.GetType();

                foreach (var item in type.GetProperties())
                {
                    if (item.IsDefined(typeof(BaseDescriptionAttribute), true))
                    {
                        object value = item.GetValue(entitys, null);

                        object description = value;

                        foreach (BaseDescriptionAttribute items in item.GetCustomAttributes(typeof(BaseDescriptionAttribute), true))
                        {
                            if (value != null)
                            {
                                description = items.ConvertDescription(value);

                                if (description != value)
                                {
                                    item.SetValue(entitys, description);
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
