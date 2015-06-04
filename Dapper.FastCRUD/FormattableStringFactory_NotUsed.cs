#if !NET_46_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Pollyfills for .NET 4.5 and below
    /// </summary>
    public class FormattableStringFactory
    {
        public static FormattableString Create(string messageFormat, params object[] args)
        {
            return new FormattableString(messageFormat, args);
        }
    }
}
#endif