namespace Click_Integration
{
    public interface ITelegramService
    {
        public Task SendMessage(string message);
    }
}
