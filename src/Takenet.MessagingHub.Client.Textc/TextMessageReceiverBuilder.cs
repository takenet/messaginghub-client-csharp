using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc;
using Takenet.Textc.Csdl;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class TextMessageReceiverBuilder
    {
        private readonly MessagingHubClient _client;
        
        internal readonly List<ICommandProcessor> CommandProcessors;
        internal readonly IOutputProcessor MessageOutputProcessor;

        private Func<Message, MessageReceiverBase, Task> _matchNotFoundHandler;

        public TextMessageReceiverBuilder(MessagingHubClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
            CommandProcessors = new List<ICommandProcessor>();
            MessageOutputProcessor = new MessageOutputProcessor(client);
        }

        public SyntaxTextMessageReceiverBuilder ForSyntax(string syntaxPattern)
        {
            return ForSyntax(CsdlParser.Parse(syntaxPattern));
        }

        public SyntaxTextMessageReceiverBuilder ForSyntax(Syntax syntax)
        {
            return new SyntaxTextMessageReceiverBuilder(new List<Syntax> { syntax }, this);
        }

        public SyntaxTextMessageReceiverBuilder ForSyntaxes(params string[] syntaxPatterns)
        {
            return new SyntaxTextMessageReceiverBuilder(syntaxPatterns.Select(CsdlParser.Parse).ToList(), this);
        }

        public SyntaxTextMessageReceiverBuilder ForSyntaxes(params Syntax[] syntaxes)
        {
            return new SyntaxTextMessageReceiverBuilder(syntaxes.ToList(), this);
        }


        public TextMessageReceiverBuilder WithMatchNotFoundMessage(string matchNotFoundMessage)
        {
            return WithMatchNotFoundHandler(
                (message, receiver) =>
                    receiver.MessageSender.SendMessageAsync(matchNotFoundMessage, message.Pp ?? message.From));
        }

        public TextMessageReceiverBuilder WithMatchNotFoundHandler(Func<Message, MessageReceiverBase, Task> matchNotFoundHandler)
        {
            _matchNotFoundHandler = matchNotFoundHandler;
            return this;
        }
        
        public TextMessageReceiver Build()
        {
            var textProcessor = new TextProcessor();
            foreach (var commandProcessor in CommandProcessors)
            {
                textProcessor.AddCommandProcessor(commandProcessor);
            }
            return new TextMessageReceiver(textProcessor, _matchNotFoundHandler);
        }
        
        public MessagingHubClient BuildAndAddMessageReceiver()
        {
            _client.AddMessageReceiver(Build(), MediaTypes.PlainText);
            return _client;
        }
    }
}
