using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Binance.Net.Enums;

namespace BollingerSpotMarket
{
    static class Telegram
    {
        static List<string> resalt = new List<string>();
        static List<string> monitoring = new List<string>();
        static List<string> downmonitor = new List<string>();
        static List<string> upmonitor = new List<string>();
        public static Dictionary<string, string> prozents;
        public static Dictionary<string, bool> checkBoxs;

        static ITelegramBotClient botClient;

        static public void ClearMonitoring()
        {
            monitoring.Clear();
        }

        static public void ClearCheckedOrders()
        {
            resalt.Clear();
            monitoring.Clear();
            downmonitor.Clear();
            downmonitor.Clear();
            upmonitor.Clear();
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
                    var args = "Возможенo Long. Точность 70%. Пробой ==-> " + prozents["comboBox3"] + " % " + "\n" + para.ToString() + "\n" + "Цена ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Таймфрейм ==-> " + prozents["comboBox5"];
                    TelegramBot(args);
                    resalt.Add(para);
                    downmonitor.Add(para);
                }
                if (indicators["lastprice"] > indicators["upproc"] && checkBoxs["checkBox1"] == true)
                {
                    var args = "Возможно Short. Точность 70%. Пробой ==->  " + prozents["comboBox4"] + " % " + "\n" + para.ToString() + "\n" + "Цена ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Таймфрейм ==-> " + prozents["comboBox5"];
                    TelegramBot(args);
                    resalt.Add(para);
                    upmonitor.Add(para);
                }
            }
            if (indicators["lastprice"] < indicators["average"] && upmonitor.Contains(para) == true)
            {
                resalt.Remove(para);
            }
            if (indicators["lastprice"] > indicators["average"] && downmonitor.Contains(para) == true)
            {
                resalt.Remove(para);
            }

            if (downmonitor.Contains(para) == true)
            {
                if (indicators["lastprice"] > indicators["down"] && indicators["сlosedClouse"] > indicators["сlosedOpen"])
                {
                    var args = "Возможенo Long. Точность 90%. ==->  " + para.ToString() + "\n" + "Цена ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Таймфрейм ==-> " + prozents["comboBox5"];
                    TelegramBot(args);
                    downmonitor.Remove(para);
                }
            }
            if (upmonitor.Contains(para) == true)
            {
                if (indicators["lastprice"] < indicators["up"] && indicators["сlosedClouse"] < indicators["сlosedOpen"])
                {
                    var args = "Возможно Short. Точность 90% ==->  " + para.ToString() + "\n" + "Цена ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Таймфрейм ==-> " + prozents["comboBox5"];
                    TelegramBot(args);
                    upmonitor.Remove(para);
                }
            }

            if (monitoring.Contains(para) == true)
            {
                if (indicators["lastprice"] < indicators["downproc"])
                {
                    var arg = "Монета на контроле " + "\n" + "Возможно Long ==-> " + prozents["comboBox3"] + " % " + "\n" + para.ToString() + "\n" + "Цена ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Таймфрейм ==-> " + prozents["comboBox5"];
                    TelegramBotRepuschae(arg);
                }
                if (indicators["lastprice"] > indicators["upproc"])
                {
                    var arg = "Монета на контроле " + "\n" + "Возможно Short ==->  " + prozents["comboBox4"] + " % " + "\n" + para.ToString() + "\n" + "Цена ==-> " + Math.Round(indicators["lastprice"], 8).ToString() + "\n" + "Таймфрейм ==-> " + prozents["comboBox5"];
                    TelegramBotRepuschae(arg);

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