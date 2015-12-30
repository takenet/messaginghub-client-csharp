using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Takenet.Textc;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class SyntaxTextcMessageReceiverBuilder
    {
        private readonly List<Syntax> _syntaxes;
        private readonly TextcMessageReceiverBuilder _textcMessageReceiverBuilder;

        internal SyntaxTextcMessageReceiverBuilder(List<Syntax> syntaxes, TextcMessageReceiverBuilder textcMessageReceiverBuilder)
        {
            _syntaxes = syntaxes;
            _textcMessageReceiverBuilder = textcMessageReceiverBuilder;
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T>(Action<T> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2>(Action<T1, T2> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3>(Action<T1, T2, T3> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do(Func<Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T>(Func<T, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2>(Func<T1, T2, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3>(Func<T1, T2, T3, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an action to be executed when there is a syntax match.
        /// </summary>
        public TextcMessageReceiverBuilder Do<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<TResult>(Func<TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T, TResult>(Func<T, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<TResult>(Func<Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T, TResult>(Func<T, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder Return<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textcMessageReceiverBuilder.OutputProcessor, _syntaxes.ToArray()));
        }

        /// <summary>
        /// Specify an return value when there is a syntax match.
        /// The returned object will be handled by the output processor associated to the message receiver.
        /// </summary>
        public TextcMessageReceiverBuilder ProcessWith(ICommandProcessor commandProcessor)
        {
            _textcMessageReceiverBuilder.CommandProcessors.Add(commandProcessor);
            return _textcMessageReceiverBuilder;
        }
    }
}