using System;
using System.Collections.Generic;
using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public static class RequestContextExtensions
    {
        public const string ID_VARIABLE_NAME = "$id";
        public const string FROM_VARIABLE_NAME = "$from";
        public const string TO_VARIABLE_NAME = "$to";
        public const string PP_VARIABLE_NAME = "$pp";
        public const string TYPE_VARIABLE_NAME = "$type";
        public const string CONTENT_VARIABLE_NAME = "$content";
        public const string METADATA_VARIABLE_NAME = "$metadata";

        public static Guid GetMessageId(this IRequestContext context)
        {
            return context.GetVariable<Guid>(ID_VARIABLE_NAME);
        }

        public static void SetMessageId(this IRequestContext context, Guid id)
        {
            context.SetVariable(ID_VARIABLE_NAME, id);
        }

        public static Node GetMessageFrom(this IRequestContext context)
        {
            return context.GetVariable<Node>(FROM_VARIABLE_NAME);
        }

        public static void SetMessageFrom(this IRequestContext context, Node from)
        {
            context.SetVariable(FROM_VARIABLE_NAME, from);
        }

        public static Node GetMessageTo(this IRequestContext context)
        {
            return context.GetVariable<Node>(TO_VARIABLE_NAME);
        }

        public static void SetMessageTo(this IRequestContext context, Node to)
        {
            context.SetVariable(TO_VARIABLE_NAME, to);
        }

        public static Node GetMessagePp(this IRequestContext context)
        {
            return context.GetVariable<Node>(PP_VARIABLE_NAME);
        }

        public static void SetMessagePp(this IRequestContext context, Node pp)
        {
            context.SetVariable(PP_VARIABLE_NAME, pp);
        }

        public static MediaType GetMessageType(this IRequestContext context)
        {
            return context.GetVariable<MediaType>(TYPE_VARIABLE_NAME);
        }

        public static void SetMessageType(this IRequestContext context, MediaType type)
        {
            context.SetVariable(TYPE_VARIABLE_NAME, type);
        }

        public static Document GetMessageContent(this IRequestContext context)
        {
            return context.GetVariable<Document>(CONTENT_VARIABLE_NAME);
        }

        public static void SetMessageContent(this IRequestContext context, Document content)
        {
            context.SetVariable(CONTENT_VARIABLE_NAME, content);
        }

        public static IDictionary<string, string> GetMessageMetadata(this IRequestContext context)
        {
            return context.GetVariable<IDictionary<string, string>>(METADATA_VARIABLE_NAME);
        }

        public static void SetMessageMetadata(this IRequestContext context, IDictionary<string, string> metadata)
        {
            context.SetVariable(METADATA_VARIABLE_NAME, metadata);
        }
    }
}