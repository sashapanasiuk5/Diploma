using System.Reflection;
using DBGuard.Common.Attributes;

namespace DBGuard.Common.Extensions;

public static class EnumExtensions
{
    public static string GetTextRepresentation(this Enum value)
    {
        var member = value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault();

        var attribute = member?
            .GetCustomAttribute<TextRepresentationAttribute>();

        return attribute?.Text ?? value.ToString();
    }
}