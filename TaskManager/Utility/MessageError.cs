namespace TaskManager.Utility
{
    public class MessageError
    {
        public string Message { get; set; }
        public string Stack { get; set; }
        public MessageError(string msg, string stack)
        {
            Message = msg;
            Stack = stack;
        }
    }
}
