using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Telegram_Bot
{
	class Program
	{
		static ITelegramBotClient botClient;
		static async Task Main(string[] args)
		{
			var key = "1873622145:AAETGH-oWv2PkkDJrAdNVAm9nMnNNRMWvbQ";
			botClient = new TelegramBotClient(key);
			var chat_id = -596734253;
			var message = (args.Length == 0) ? "hello" : args[0];
			await SendMessageAsync(chat_id, message);
			botClient.StartReceiving();
			botClient.StopReceiving();
		}
		static async Task SendMessageAsync(long chatId, string text)
		{
			await botClient.SendTextMessageAsync(chatId: chatId,text:text);
		}
	}
}
