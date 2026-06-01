namespace DBGuard.Common.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TextRepresentationAttribute : Attribute
{
    public string Text { get; }

    public TextRepresentationAttribute(string text)
    {
        Text = text;
    }
}