using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;

namespace BollingerNewVers
{

    public partial class Form1 : Form
    {
        private string namePara;
        private int period;
        List<string> resalt = new List<string>();
        static ITelegramBotClient botClient;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
        private  void button1_Click(object sender, EventArgs e)
        {
            CycleWork();
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
        }
        private async void CycleWork()
        {
            while (true)
            {   
                if (comboBox1.Text == "")
                {
                    MessageBox.Show("No order");
                    return;
                }
                if (comboBox2.Text == "")
                {
                    MessageBox.Show("No period");
                    return;
                }

                namePara = comboBox1.Text;
                period = Convert.ToInt32(comboBox2.Text);
                button1.Enabled = false;
                await Get_Pairs();
                await Task.Delay(period * 1000);
            }

        }
        public async Task<string> LoadUrlAsText(string url)
        {
            var request = WebRequest.Create(url);
            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    var sr = new StreamReader(stream);
                    return await sr.ReadToEndAsync();
                }
            }
        }
        public async Task Get_Pairs()
        {           
            dynamic allPares = JsonConvert.DeserializeObject(await LoadUrlAsText("https://testnet.binancefuture.com/fapi/v1/exchangeInfo"));
            int i = 0;
            foreach (var item in allPares.symbols)
            {
                if (item.quoteAsset == namePara)//[JSON].symbols.[0].quoteAsset
                {
                    i++;
                    string para = item.symbol.ToString();
                    Bollenger(para);
                }
                label2.Text =i.ToString();
            }
        }
        public async Task Bollenger(string para)
        {

            dynamic d = await LoadUrlAsText($"https://testnet.binancefuture.com/fapi/v1/markPriceKlines?symbol={para}&interval=15m&limit=22");
            dynamic allOrder = JsonConvert.DeserializeObject(d);
            double totalAverage = 0;
            double totalSquares = 0;
            double lastprice = 0;

            try
            {
                dynamic www = await LoadUrlAsText($"https://testnet.binancefuture.com/fapi/v1/premiumIndex?symbol={para}");
                dynamic lastPare = JsonConvert.DeserializeObject(www);
                lastprice = (Convert.ToDouble(lastPare.markPrice));
            }
            catch { }

            //[JSON].[0].[4]
            foreach (dynamic item in allOrder)
            {
                double closePrice= (Convert.ToDouble(item[4]));
                totalAverage += closePrice;//итоговая цена
                totalSquares += Math.Pow(Math.Round(closePrice, 8), 2);//возводим в квадрат средние цены закрытия
            }

            double average = totalAverage / allOrder.Count;
            double stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
            double up = average + 2 * stdev;
            double down = average - 2 * stdev;
            double bandWidth = (up - down) / average;
            double friproc = Math.Round((up * 1.03),8);
            double sixproc = Math.Round((up * 1.06),8);

            label1.Text = "Pair " + para;

            if (friproc != double.NaN && sixproc != double.NaN)
            {
                Telegramm(para, friproc, sixproc, lastprice, up);
            }
        }
        void Telegramm(string para, double friproc, double sixproc, double lastprice, double up)
        {
            if (lastprice > friproc)
            {
                if(resalt.Contains(para) == false)
                {
                    var args = "UP ==->  " + para.ToString() + "\n" + "PRICE ==-> " + Math.Round(lastprice, 8).ToString();
                    TelegramBot(args);
                    resalt.Add(para);
                }
            }
            else
            {
                if (resalt.Contains(para) == true)
                {
                    if (lastprice < friproc)
                    {
                        resalt.Remove(para);
                    }
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            resalt.Clear();
        }
        static async Task TelegramBot(string args)
        {
            botClient = new TelegramBotClient("1873622145:AAETGH-oWv2PkkDJrAdNVAm9nMnNNRMWvbQ");
            var chat_id = -596734253;
            await SendMessageAsync(chat_id, args);
        }
        static async Task SendMessageAsync(long chatId, string args)
        {
            await botClient.SendTextMessageAsync(chatId: chatId, text: args);
        }
    }
}
