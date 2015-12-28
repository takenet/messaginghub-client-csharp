using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Takenet.Text;
using Takenet.Text.Csdl;
using Takenet.Text.Processors;

namespace Takenet.MessagingHub.Client.Text
{
    public sealed class SyntaxTextMessageReceiverBuilder
    {
        private readonly List<Syntax> _syntaxes;
        private readonly TextMessageReceiverBuilder _textMessageReceiverBuilder;

        internal SyntaxTextMessageReceiverBuilder(List<Syntax> syntaxes, TextMessageReceiverBuilder textMessageReceiverBuilder)
        {
            _syntaxes = syntaxes;
            _textMessageReceiverBuilder = textMessageReceiverBuilder;
        }

        public TextMessageReceiverBuilder Do<T>(Action<T> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2>(Action<T1, T2> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3>(Action<T1, T2, T3> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do(Func<Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T>(Func<T, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2>(Func<T1, T2, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3>(Func<T1, T2, T3, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Do<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<TResult>(Func<TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T, TResult>(Func<T, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<TResult>(Func<Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T, TResult>(Func<T, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder Return<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func)
        {
            return ProcessWith(
                DelegateCommandProcessor.Create(func, _textMessageReceiverBuilder.MessageOutputProcessor, _syntaxes.ToArray()));
        }

        public TextMessageReceiverBuilder ProcessWith(ICommandProcessor commandProcessor)
        {
            _textMessageReceiverBuilder.CommandProcessors.Add(commandProcessor);
            return _textMessageReceiverBuilder;
        }
    }
}