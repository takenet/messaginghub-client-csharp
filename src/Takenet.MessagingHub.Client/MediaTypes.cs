using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public static class MediaTypes
    {
        static MediaType _any = new MediaType("*", "*");
        static MediaType _plainText = new MediaType(MediaType.DiscreteTypes.Text, MediaType.SubTypes.Plain);

        public static MediaType Any => _any;

        public static MediaType PlainText => _plainText;
    }
}
