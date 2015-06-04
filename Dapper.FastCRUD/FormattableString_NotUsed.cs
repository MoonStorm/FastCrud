#if !NET_46_OR_GREATER
namespace System
{
    using System.Globalization;

    /// <summary>
    /// Pollyfills for .NET 4.5 and below
    /// </summary>
    public class FormattableString: IFormattable
    {
        private readonly string _messageFormat;
        private readonly object[] _args;

        public FormattableString(string messageFormat, object[] args)
        {
            this._messageFormat = messageFormat;
            this._args = args;
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, this._messageFormat, this._args);
        }

        public override string ToString()
        {
            return string.Format(_messageFormat, _args, CultureInfo.CurrentCulture);
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, _messageFormat, _args);
        }
    }
}
#endif