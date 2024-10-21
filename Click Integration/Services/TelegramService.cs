using Telegram.Bot;

namespace Click_Integration.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly long _groupId = 5569322769;
        private readonly TelegramBotClient _botClient;

        public TelegramService(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SendMessage(string message)
        {
            await _botClient.SendTextMessageAsync(chatId: _groupId, text: message);
        }
    }
}
