using System;
namespace server
{
    public class MessageEvent
    {
        public MessageData Data;

        public MessageEvent(object source, MessageData data)
        {
            this.Data = data;
        }
    }
}
