using System.ComponentModel;

namespace MauiCoreBluetooth.Extensions;

/// <summary>
/// Extensions to the Enum class.
/// </summary>
public static class EnumEx
{
    /// <summary>
    /// Retrieves the value in the DescriptionAttribute of the enum member, if there is one.
    /// </summary>
    /// <param name="e">This Enum.</param>
    /// <returns>The DescriptionAttribute value, or null if there is no DescriptionAttribute.</returns>
    public static string? GetDescriptionValue(this Enum e)
    {
        var attr = e.GetFirstAttribute<DescriptionAttribute>();
        return attr?.Description ?? null;
    }


    /// <summary>
    /// Retrieves the first attribute that matches the specified type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="src">The enum member to retrieve the attribute from.</param>
    /// <returns>The attribute or null if it doesn't exist or if something goes wrong in 
    /// retrieving it.</returns>
    public static TAttribute? GetFirstAttribute<TAttribute>(this Enum src)
    {
        var enumType = src.GetType();
        var member = enumType.GetMember(src.ToString());
        var enumMember = Array.Find(member, m => m.DeclaringType == enumType);
        if (enumMember == null)
        {
            return default(TAttribute);
        }
        else
        {
            var attr = enumMember
                .GetCustomAttributes(typeof(TAttribute), false)
                .Cast<TAttribute>()
                .FirstOrDefault();

            return attr;
        }
    }

    /// <summary>
    /// Retrieves all of the members of the enumeration, cast to the specified type.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
    /// <returns>The enumeration members, properly typecast.</returns>
    public static TEnum[] GetValues<TEnum>()
        where TEnum : Enum
    {
        return (TEnum[])Enum.GetValues(typeof(TEnum));
    }
}