using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public interface IContextProvider
    {
        IRequestContext GetContext(Node sender, Node destination);
    }
}