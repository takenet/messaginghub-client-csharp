﻿using Lime.Messaging.Resources;
using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Extensions.Contacts
{
    /// <summary>
    /// Manage contact's roster
    /// </summary>
    public interface IContactExtension
    {
        /// <summary>
        /// Get contact info from roster
        /// </summary>
        /// <param name="identity">Contact identity</param>
        /// <returns>Contact info</returns>
        Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken);

        /// <summary>
        /// Set contact info into roster
        /// </summary>
        /// <param name="identity">Contact identity</param>
        /// <param name="contact">Contact info</param>
        Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken);

        /// <summary>
        /// Delete a contact info from roster
        /// </summary>
        /// <param name="identity">Contact identity</param>
        Task DeleteAsync(Identity identity, CancellationToken cancellationToken);
    }
}
