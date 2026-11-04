using System;
using System.Globalization;

namespace DX.Utils.Tests
{
    // Temporarily swaps CurrentCulture/CurrentUICulture for the current thread (async-local in .NET Core),
    // so culture-sensitive code can be exercised under e.g. nl-NL or tr-TR and restored afterwards.
    internal sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _culture;
        private readonly CultureInfo _uiCulture;

        public CultureScope(string name)
        {
            _culture = CultureInfo.CurrentCulture;
            _uiCulture = CultureInfo.CurrentUICulture;
            var ci = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiCulture;
        }
    }
}
