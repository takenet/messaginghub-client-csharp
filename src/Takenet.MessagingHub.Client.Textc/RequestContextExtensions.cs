using System;
using System.Collections.Generic;
using Lime.Protocol;
using Takenet.Textc;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Lime.Protocol.Serialization;

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
        public const string CLEARSESSION_VARIABLE_NAME = "$clearSession";

        public static string GetMessageId(this IRequestContext context)
        {
            return context.GetVariable<string>(ID_VARIABLE_NAME);
        }

        public static void SetMessageId(this IRequestContext context, string id)
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

        public static T GetMessageContent<T>(this IRequestContext context) where T : Document
        {
            var content = context.GetVariable<string>(CONTENT_VARIABLE_NAME);
            var mediaType = context.GetVariable<MediaType>(TYPE_VARIABLE_NAME);

            if (mediaType.IsJson)
            {
                return JsonConvert.DeserializeObject<T>(content, JsonNetSerializer.Settings);
            }
            else
            {
                object document;
                if (TypeUtil.TryParseString(content, typeof(T), out document))
                {
                    return (T)document;
                }
                return null;
            }
        }

        public static void SetMessageContent(this IRequestContext context, Document content)
        {
            if (content.GetMediaType().IsJson)
            {
                context.SetVariable(CONTENT_VARIABLE_NAME,
                    JsonConvert.SerializeObject(content, JsonNetSerializer.Settings));
            }
            else
            {
                context.SetVariable(CONTENT_VARIABLE_NAME, content.ToString());
            }
        }

        public static IDictionary<string, string> GetMessageMetadata(this IRequestContext context)
        {
            return context.GetVariable<IDictionary<string, string>>(METADATA_VARIABLE_NAME);
        }

        public static void SetMessageMetadata(this IRequestContext context, IDictionary<string, string> metadata)
        {
            context.SetVariable(METADATA_VARIABLE_NAME, metadata);
        }

        public static void SetMessage(this IRequestContext context, Message message)
        {
            context.SetMessageId(message.Id);
            context.SetMessageFrom(message.From);
            context.SetMessageTo(message.To);
            context.SetMessagePp(message.Pp);
            context.SetMessageType(message.Type);
            context.SetMessageContent(message.Content);
            context.SetMessageMetadata(message.Metadata);
        }

        public static bool HasToClearSession(this IRequestContext context)
        {
            return context.GetVariable<bool>(CLEARSESSION_VARIABLE_NAME) == true;
        }

        public static void SetClearSession(this IRequestContext context)
        {
            context.SetVariable(CLEARSESSION_VARIABLE_NAME, true);
        }
    }
}