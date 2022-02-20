using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;


namespace BollingerSpotMarket
{
    static class Telegram
    {
        static List<string> resalt = new List<string>();
        static List<string> monitoring = new List<string>();
        static List<string> controlavg = new List<string>();
        public static Dictionary<string, string> prozents;
        public static Dictionary<string, bool> checkBoxs;

        static ITelegramBotClient botClient;

        static public void ClearCheckedOrders()
        {
            resalt.Clear();
            monitoring.Clear();
            controlavg.Clear();
        }
        static public void AddOrderForMonitoring(string order)
        {
            monitoring.Add(order);
        }
        async static public Task IndexForTelegramm(string para, Dictionary<string, double> indicators)
        {
            if (resalt.Contains(para) == false)
            {
                if (indicators["lastprice"] < indicators["downproc"] && checkBoxs["checkBox2"] == true)
                {
                    var args = "Possibly Long ==-> " + prozents["comboBox3"] + " % " + "\n" + para.ToString() + "\n" + "Price ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Period ==-> " + prozents["comboBox5"];
                    TelegramBot(args);
                    resalt.Add(para);
                    controlavg.Add(para);
                }
                if (indicators["lastprice"] > indicators["upproc"] && checkBoxs["checkBox1"] == true)
                {
                    var args = "Possibly Short ==->  " + prozents["comboBox4"] + " % " + "\n" + para.ToString() + "\n" + "Price ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Period ==-> " + prozents["comboBox5"];
                    TelegramBot(args);
                    resalt.Add(para);
                }
            }
            else if (indicators["lastprice"] > indicators["downproc"] && indicators["lastprice"] < indicators["upproc"])
            {
                resalt.Remove(para);
            }

            if (monitoring.Contains(para) == true)
            {
                if (indicators["lastprice"] < indicators["downproc"])
                {
                    var arg = "Observed Coins " + "\n" + "Possibly Long ==-> " + prozents["comboBox3"] + " % " + "\n" + para.ToString() + "\n" + "Price ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Period ==-> " + prozents["comboBox5"];
                    TelegramBotRepuschae(arg);
                }
                if (indicators["lastprice"] > indicators["upproc"])
                {
                    var arg = "Observed Coins " + "\n" + "Possibly Short ==->  " + prozents["comboBox4"] + " % " + "\n" + para.ToString() + "\n" + "Price ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Period ==-> " + prozents["comboBox5"];
                    TelegramBotRepuschae(arg);

                }
            }

            if (controlavg.Contains(para) == true)
            {
                if (indicators["lastprice"] > indicators["down"] && indicators["сlosedClouse"] > indicators["сlosedOpen"])
                {
                    var arg = "Perhaps a pump ==->  " + para.ToString() + "\n" + "Price ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Period ==-> " + prozents["comboBox5"];
                    TelegramBotRepuschae(arg);
                    controlavg.Remove(para);
                }
            }
        }
        static async Task TelegramBot(string args)
        {
            botClient = new TelegramBotClient("5167308233:AAGF2mu55byq8XKBXxo7SKFOke7rB1tc5_8");
            var chat_id = -1001741001182;
            await SendMessageAsync(chat_id, args);
        }
        static async Task SendMessageAsync(long chatId, string args)
        {
            await botClient.SendTextMessageAsync(chatId: chatId, text: args);
        }
        public static async Task TelegramBotRepuschae(string arg)
        {
            botClient = new TelegramBotClient("5059700101:AAGpy77Pjg_vmX4aVSXYyS4oa00U_cyEMOA");
            var chat_id = -1001714789241;
            await SendMessageAsyncRepurchase(chat_id, arg);
        }
        static async Task SendMessageAsyncRepurchase(long chatId, string arg)
        {
            await botClient.SendTextMessageAsync(chatId: chatId, text: arg);
        }
    }
}