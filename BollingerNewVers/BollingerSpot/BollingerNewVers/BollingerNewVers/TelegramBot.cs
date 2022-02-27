using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;


namespace BollingerSpotMarket
{
    static class Telegram
    {      

        static ITelegramBotClient botClient;

        
        public static async Task TelegramBot(string args)
        {
            botClient = new TelegramBotClient("5167308233:AAGF2mu55byq8XKBXxo7SKFOke7rB1tc5_8");
            var chat_id = -1001741001182;
            await SendMessageAsync(chat_id, args);
        }
        public static async Task SendMessageAsync(long chatId, string args)
        {
            await botClient.SendTextMessageAsync(chatId: chatId, text: args);
        }
        public static async Task TelegramBotRepuschae(string arg)
        {
            botClient = new TelegramBotClient("5059700101:AAGpy77Pjg_vmX4aVSXYyS4oa00U_cyEMOA");
            var chat_id = -1001714789241;
            await SendMessageAsync(chat_id, arg);
        }
        //static async Task SendMessageAsyncRepurchase(long chatId, string arg)
        //{
        //    await botClient.SendTextMessageAsync(chatId: chatId, text: arg);
        //}
    }
}