using System.Net;

namespace SdgElasticPoller
{
    //https://github.com/davidfowl/StreamingSample/blob/master/client/PostStreamContent.cs
    internal class PushStreamContent : HttpContent
    {
        private readonly Func<Stream, Task> _handler;

        public PushStreamContent(Func<Stream, Task> handler) => _handler = handler;

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => _handler(stream);

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
