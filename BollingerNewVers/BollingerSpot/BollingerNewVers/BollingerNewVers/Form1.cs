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
        bool work;
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
            work = true;
            CycleWork();
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            comboBox3.Enabled = false;
            comboBox4.Enabled = false;
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
            dynamic allPares = JsonConvert.DeserializeObject(await LoadUrlAsText("https://api.binance.com/api/v3/exchangeInfo"));
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

            dynamic d = await LoadUrlAsText($"https://api.binance.com/api/v1/klines?symbol={para}&interval=15m&limit=22");
            dynamic allOrder = JsonConvert.DeserializeObject(d);
            double totalAverage = 0;
            double totalSquares = 0;
            double lastprice = 0;
            double openPrice = 0;
            try
            {
                dynamic www = await LoadUrlAsText($"https://api.binance.com/api/v3/ticker/price?symbol={para}");
                dynamic lastPare = JsonConvert.DeserializeObject(www);
                lastprice = (Convert.ToDouble(lastPare.price));
            }
            catch { }

            //[JSON].[0].[4]
            foreach (dynamic item in allOrder)
            {
                openPrice = (Convert.ToDouble(item[1]));//[JSON].[0].[1]
                double closePrice = (Convert.ToDouble(item[4]));
                totalAverage += closePrice;//итоговая цена
                totalSquares += Math.Pow(Math.Round(closePrice, 8), 2);//возводим в квадрат средние цены закрытия
            }

            double average = totalAverage / allOrder.Count;
            double stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
            double up = average + 2 * stdev;
            double down = average - 2 * stdev;
            double bandWidth = (up - down) / average;
            double procup = 1+double.Parse(comboBox4.Text)/100;
            double upproc = Math.Round((up * procup),8);
            double procdown = 1+double.Parse(comboBox3.Text)/100;
            double downproc = Math.Round((down / procdown),8);
            double proclast = Math.Round((openPrice /1.03), 8);

            ///label1.Text = "Pair " + para + "\n" + "UP " + up + "\n" + "AVG " + average + "\n" + "DOWN " + down + "\n" + "Last Price " + lastprice;
            label1.Text = "Pair " + para;

            if (upproc != double.NaN && downproc != double.NaN && proclast != double.NaN)
            {
                Telegramm(para, upproc, downproc, lastprice, up, proclast);
            }
        }
        void Telegramm(string para, double upproc, double downproc, double lastprice, double up, double proclast)
        {
            if (lastprice < downproc && resalt.Contains(para) == false && checkBox2.Checked == true)
            {
               var args = "DOWN ==-> " + comboBox3.Text.ToString()+ " % " + "\n" + para.ToString() + "\n" + "PRICE ==-> " + Math.Round(lastprice,8).ToString();
               TelegramBot(args);
               resalt.Add(para);
            }
            else
            {
                if (lastprice > upproc && resalt.Contains(para) == false && checkBox1.Checked == true)
                {
                    var args = "UP ==->  " + comboBox4.Text.ToString() + " % " + "\n" + para.ToString() + "\n" + "PRICE ==-> " + Math.Round(lastprice, 8).ToString();
                    TelegramBot(args);
                    resalt.Add(para);
                }
            }
            if (lastprice > downproc && lastprice < upproc)
            {
                if (resalt.Contains(para) == true)
                {
                    resalt.Remove(para);
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
        private void button2_Click(object sender, EventArgs e)
        {
            resalt.Clear();
            button1.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
            comboBox4.Enabled = true;
        }
    }
}
