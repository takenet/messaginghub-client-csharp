using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Takenet.Textc;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class SyntaxTextcMessageReceiverBuilder
    {
        private readonly List<Func<IOutputProcessor, ICommandProcessor>> _commandProcessorFactories;
        private readonly List<Syntax> _syntaxes;
        private readonly TextcMessageReceiverBuilder _textcMessageReceiverBuilder;

        internal SyntaxTextcMessageReceiverBuilder(List<Func<IOutputProcessor, ICommandProcessor>> commandProcessorFactories, List<Syntax> syntaxes, TextcMessageReceiverBuilder textcMessageReceiverBuilder)
        {
            _commandProcessorFactories = commandProcessorFactories;
            _syntaxes = syntaxes;
            _textcMessageReceiverBuilder = textcMessageReceiverBuilder;
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do(Func<Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T>(Func<T, Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2>(Func<T1, T2, Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3>(Func<T1, T2, T3, Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<TResult>(Func<Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T, TResult>(Func<T, Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func) => 
            ProcessWith(CreateCommandProcessorFactory(func));

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder ProcessWith(ICommandProcessor commandProcessor) => 
            ProcessWith(o => commandProcessor);

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder ProcessWith(Func<IOutputProcessor, ICommandProcessor> commandProcessorFactory)
        {
            _commandProcessorFactories.Add(commandProcessorFactory);
            return _textcMessageReceiverBuilder;
        }

        private Func<IOutputProcessor, ICommandProcessor> CreateCommandProcessorFactory(Delegate @delegate) => 
            o => new DelegateCommandProcessor(@delegate, outputProcessor: o, syntaxes: _syntaxes.ToArray());
    }
}