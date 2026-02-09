using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;

namespace EcoMob.McpServer.Infra.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string? GetDescription<T>(this T enumValue) where T : Enum
        {
            FieldInfo? fi = enumValue.GetType().GetField(enumValue.ToString());
            DescriptionAttribute?[] attributes = fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[] ?? Array.Empty<DescriptionAttribute>();
            if (attributes.Length > 0)
            {
                return attributes[0]!.Description;
            }
            else
            {
                return enumValue.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string? GetDisplayName<T>(this T enumValue) where T : Enum
        {
            FieldInfo? fi = enumValue.GetType().GetField(enumValue.ToString());
            DisplayAttribute?[] attributes = fi?.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[] ?? Array.Empty<DisplayAttribute>();
            if (attributes.Length > 0)
            {
                return attributes[0]!.Name;
            }
            else
            {
                return enumValue.ToString();
            }
        }
    }
}
