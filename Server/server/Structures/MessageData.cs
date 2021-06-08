using System;
namespace server
{
    public class MessageData
    {
        public int Id { get; set; }
        public String Message { get; set; }

        public MessageData(int id, String message)
        {
            this.Id = id;
            this.Message = message;
        }
    }
}
