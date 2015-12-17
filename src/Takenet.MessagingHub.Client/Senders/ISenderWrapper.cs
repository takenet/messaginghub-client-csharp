namespace Takenet.MessagingHub.Client.Senders
{
    internal interface ISenderWrapper : IMessageSender, ICommandSender, INotificationSender
    {
    }
}