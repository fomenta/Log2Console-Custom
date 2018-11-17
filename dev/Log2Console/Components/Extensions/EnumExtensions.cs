using System;

namespace Log2Console.Components.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum ToEnum<TEnum>(this int value) where TEnum : struct, IConvertible
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            { throw new ArgumentOutOfRangeException(nameof(value), $"Cannot cast '{value}' to enum {typeof(TEnum).Name}"); }
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }

        public static object ToEnum(this string value, Type enumType)
            => ToEnumInternal(value, enumType, () => throw new ArgumentOutOfRangeException(nameof(value), $"Cannot cast '{value}' to enum {enumType.Name}"));

        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, IConvertible
            => (TEnum)ToEnumInternal(value, typeof(TEnum), () => throw new ArgumentOutOfRangeException(nameof(value), $"Cannot cast '{value}' to enum {typeof(TEnum).Name}"));

        public static TEnum ToEnum<TEnum>(this string value, TEnum defaultValue) where TEnum : struct, IConvertible
            => (TEnum)ToEnumInternal(value, typeof(TEnum), () => defaultValue);

        #region Private
        private static object ToEnumInternal(string value, Type enumType, Func<object> funcNotFound)
        {
            if (!enumType.IsEnum) { throw new ArgumentException(enumType.Name, $"The type '{enumType.Name}' must be an enum"); }

            if (!Enum.IsDefined(enumType, value))
            {
                return funcNotFound();
            }
            else
            {
                return Enum.Parse(enumType, value, ignoreCase: true);
            }
        }
        #endregion
    }
}
