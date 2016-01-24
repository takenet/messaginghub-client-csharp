using Lime.Protocol;
using System.Globalization;
using System;

namespace Takenet.MessagingHub.Client.Textc
{
    /// <summary>
    /// Provides a default culture for all nodes.
    /// </summary>
    public sealed class DefaultCultureProvider : ICultureProvider
    {
        private readonly CultureInfo _defaultCultureInfo;

        public DefaultCultureProvider(CultureInfo defaultCultureInfo)
        {
            if (defaultCultureInfo == null) throw new ArgumentNullException(nameof(defaultCultureInfo));
            _defaultCultureInfo = defaultCultureInfo;
        }

        public CultureInfo GetCulture(Node node) => _defaultCultureInfo;
    }
}