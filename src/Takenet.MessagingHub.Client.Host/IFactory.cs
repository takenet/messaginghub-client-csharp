using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Defines a factory for instance of <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFactory<T> where T : class
    {
        /// <summary>
        /// Creates the asynchronous.
        /// </summary>
        /// <param name="serviceProvider">A service provider to allow resolving references.</param>
        /// <param name="settings">A settings dictionary.</param>
        /// <returns></returns>
        Task<T> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings);
    }
}