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
        List<string> upcoin = new List<string>();
        List<string> downcoin = new List<string>();

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
            dynamic allPares = JsonConvert.DeserializeObject(await LoadUrlAsText("https://fapi.binance.com/fapi/v1/exchangeInfo"));
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
            dynamic d = await LoadUrlAsText($"https://fapi.binance.com/fapi/v1/markPriceKlines?symbol={para}&interval=15m&limit=21");
            dynamic allOrder = JsonConvert.DeserializeObject(d);
            double totalAverage = 0;
            double totalSquares = 0;
            double lastprice = 0;
            double highprice = 0;
            double lowprice = 0;


            try
            {
                dynamic www = await LoadUrlAsText($"https://fapi.binance.com/fapi/v1/premiumIndex?symbol={para}");
                dynamic lastPare = JsonConvert.DeserializeObject(www);
                lastprice = (Convert.ToDouble(lastPare.markPrice));
            }
            catch { }

            //[JSON].[0].[4]
            foreach (dynamic item in allOrder)
            {
                highprice = (Convert.ToDouble(item[2]));
                lowprice = (Convert.ToDouble(item[3]));
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

            label1.Text = "Pair " + para + "\n" + "UP " + up + "\n" + "AVG " + average + "\n" + "DOWN " + down
                + "\n" + "Last Price " + lastprice + "\n" + "High Price " + highprice + "\n" + "Low Price " + lowprice;
            //label1.Text = "Pair " + para;

            if (upproc != double.NaN && downproc != double.NaN)
            {
                Telegramm(para, upproc, downproc, lastprice, highprice, lowprice, average);
            }
        }
        void Telegramm(string para, double upproc, double downproc, double lastprice, double highprice, double lowprice, double average)
        {
            if (lastprice < downproc || lowprice < downproc)
            {
                if (downcoin.Contains(para) == false && checkBox2.Checked == true)
                {
                    var args = "DOWN ==-> " + comboBox3.Text.ToString() + " % " + "\n" + para.ToString() + "\n" + "PRICE ==-> " + Math.Round(lastprice, 8).ToString();
                    TelegramBot(args);
                    downcoin.Add(para);
                }
            }
            if (highprice > upproc || lastprice > upproc)
            {
                if (upcoin.Contains(para) == false && checkBox1.Checked == true)
                {
                    var args = "UP ==->  " + comboBox4.Text.ToString() + " % " + "\n" + para.ToString() + "\n" + "PRICE ==-> " + Math.Round(lastprice, 8).ToString();
                    TelegramBot(args);
                    upcoin.Add(para);
                }
            }
            if (downcoin.Contains(para) == true && lastprice > average)
            {
                downcoin.Remove(para);
            }
            if (upcoin.Contains(para) == true && lastprice < average)
            {
                upcoin.Remove(para);
            }
        }
        static async Task TelegramBot(string args)
        {
            var chat_id = -1001795291190;
            botClient = new TelegramBotClient("1873622145:AAETGH-oWv2PkkDJrAdNVAm9nMnNNRMWvbQ");
            await SendMessageAsync(chat_id, args);
        }
        static async Task SendMessageAsync(long chatId, string args)
        {
            await botClient.SendTextMessageAsync(chatId: chatId, text: args);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
            comboBox4.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var args = "Test bot";
            TelegramBot(args);
        }
    }
}
