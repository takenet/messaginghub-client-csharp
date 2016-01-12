using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.Textc;
using Takenet.Textc.Csdl;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    /// <summary>
    /// Builder for instances of <see cref="TextcMessageReceiver"/> class. 
    /// </summary>
    public sealed class TextcMessageReceiverBuilder
    {
        private readonly MessagingHubSenderBuilder _senderBuilder;
        private IContextProvider _contextProvider;
        private Func<Message, MessageReceiverBase, Task> _matchNotFoundHandler;

        internal readonly List<ICommandProcessor> CommandProcessors;
        internal IOutputProcessor OutputProcessor;

        public TextcMessageReceiverBuilder(MessagingHubSenderBuilder senderBuilder, IOutputProcessor outputProcessor = null)
        {
            _senderBuilder = senderBuilder;
            if (senderBuilder == null) throw new ArgumentNullException(nameof(senderBuilder));
            CommandProcessors = new List<ICommandProcessor>();
            OutputProcessor = outputProcessor ?? new MessageOutputProcessor(_senderBuilder.EnvelopeListener);
        }
        
        /// <summary>
        /// Adds a new command syntax to the <see cref="TextcMessageReceiver"/> builder.
        /// </summary>
        /// <param name="syntaxPattern">The CSDL statement. Please refer to <seealso cref="https://github.com/takenet/textc-csharp#csdl"/> about the notation.</param>
        /// <returns></returns>
        public SyntaxTextcMessageReceiverBuilder ForSyntax(string syntaxPattern)
        {
            return ForSyntax(CsdlParser.Parse(syntaxPattern));
        }

        /// <summary>
        /// Adds a new command syntax to the <see cref="TextcMessageReceiver"/> builder.
        /// </summary>
        /// <param name="culture">The syntax culture.</param>
        /// <param name="syntaxPattern">The CSDL statement. Please refer to <seealso cref="https://github.com/takenet/textc-csharp#csdl"/> about the notation.</param>        
        /// <returns></returns>
        public SyntaxTextcMessageReceiverBuilder ForSyntax(CultureInfo culture, string syntaxPattern)
        {
            return ForSyntax(CsdlParser.Parse(syntaxPattern, culture));
        }

        /// <summary>
        /// Adds a new command syntax to the <see cref="TextcMessageReceiver"/> builder.
        /// </summary>
        /// <param name="syntax">The syntax instance to be added.</param>
        /// <returns></returns>
        public SyntaxTextcMessageReceiverBuilder ForSyntax(Syntax syntax)
        {
            return new SyntaxTextcMessageReceiverBuilder(new List<Syntax> { syntax }, this);
        }

        /// <summary>
        /// Adds multiple command syntaxes to the <see cref="TextcMessageReceiver"/> builder.
        /// The added syntaxes should be related and will be associated to a same command processor.
        /// </summary>
        /// <param name="syntaxPatterns">The CSDL statements. Please refer to <seealso cref="https://github.com/takenet/textc-csharp#csdl"/> about the notation.</param>
        /// <returns></returns>
        public SyntaxTextcMessageReceiverBuilder ForSyntaxes(params string[] syntaxPatterns)
        {
            return new SyntaxTextcMessageReceiverBuilder(syntaxPatterns.Select(CsdlParser.Parse).ToList(), this);
        }

        /// <summary>
        /// Adds multiple command syntaxes to the <see cref="TextcMessageReceiver"/> builder.
        /// The added syntaxes should be related and will be associated to a same command processor.
        /// </summary>
        /// <param name="culture">The syntaxes culture.</param>
        /// <param name="syntaxPatterns">The CSDL statements. Please refer to <seealso cref="https://github.com/takenet/textc-csharp#csdl"/> about the notation.</param>
        /// <returns></returns>
        public SyntaxTextcMessageReceiverBuilder ForSyntaxes(CultureInfo culture, params string[] syntaxPatterns)
        {
            return new SyntaxTextcMessageReceiverBuilder(syntaxPatterns.Select(s => CsdlParser.Parse(s, culture)).ToList(), this);
        }

        /// <summary>
        /// Adds multiple command syntaxes to the <see cref="TextcMessageReceiver"/> builder.
        /// The added syntaxes should be related and will be associated to a same command processor.
        /// </summary>
        /// <param name="syntaxes">The syntax instances to be added.</param>
        /// <returns></returns>
        public SyntaxTextcMessageReceiverBuilder ForSyntaxes(params Syntax[] syntaxes)
        {
            return new SyntaxTextcMessageReceiverBuilder(syntaxes.ToList(), this);
        }

        /// <summary>
        /// Sets the message text to be returned in case of no match of the user input.
        /// </summary>
        /// <param name="matchNotFoundMessage">The message text.</param>
        /// <returns></returns>
        public TextcMessageReceiverBuilder WithMatchNotFoundMessage(string matchNotFoundMessage)
        {
            return WithMatchNotFoundHandler(
                (message, receiver) =>
                    receiver.EnvelopeSender.SendMessageAsync(matchNotFoundMessage, message.Pp ?? message.From));
        }

        /// <summary>
        /// Sets a handler to be called in case of no match of the user input.
        /// </summary>
        /// <param name="matchNotFoundHandler">The handler.</param>
        /// <returns></returns>
        public TextcMessageReceiverBuilder WithMatchNotFoundHandler(Func<Message, MessageReceiverBase, Task> matchNotFoundHandler)
        {
            _matchNotFoundHandler = matchNotFoundHandler;
            return this;
        }

        /// <summary>
        /// Defines the user context validity, which is used to store the conversation variables.
        /// </summary>
        /// <param name="contextValidity">The context validity.</param>
        /// <returns></returns>
        public TextcMessageReceiverBuilder WithContextValidityOf(TimeSpan contextValidity)
        {
            return WithContextProvider(new ContextProvider(contextValidity));            
        }

        /// <summary>
        /// Defines a context provider to be used by the instance of <see cref="TextcMessageReceiver"/>.
        /// </summary>
        /// <param name="contextProvider">The context provider instance.</param>
        /// <returns></returns>
        public TextcMessageReceiverBuilder WithContextProvider(IContextProvider contextProvider)
        {
            if (contextProvider == null) throw new ArgumentNullException(nameof(contextProvider));
            _contextProvider = contextProvider;
            return this;
        }

        /// <summary>
        /// Builds a new instance of <see cref="TextcMessageReceiver"/> using the defined configurations.
        /// </summary>
        /// <returns></returns>
        public TextcMessageReceiver Build()
        {
            var textProcessor = new TextProcessor();
            foreach (var commandProcessor in CommandProcessors)
            {
                textProcessor.CommandProcessors.Add(commandProcessor);
            }
            return new TextcMessageReceiver(
                textProcessor, 
                _contextProvider ?? new ContextProvider(TimeSpan.FromMinutes(5)), 
                _matchNotFoundHandler);
        }
        
        /// <summary>
        /// Builds a new instance of <see cref="TextcMessageReceiver"/> using the defined configurations and adds it to the associated <see cref="MessagingHubClient"/> instance.
        /// </summary>
        /// <returns></returns>
        public MessagingHubSenderBuilder BuildAndAddTextcMessageReceiver()
        {
            _senderBuilder.AddMessageReceiver(Build(), MediaTypes.PlainText);
            return _senderBuilder;
        }
    }
}
