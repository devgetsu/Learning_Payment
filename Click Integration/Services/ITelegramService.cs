namespace Click_Integration.Services
{
    public interface ITelegramService
    {
        public Task SendMessage(string message);
    }
}
