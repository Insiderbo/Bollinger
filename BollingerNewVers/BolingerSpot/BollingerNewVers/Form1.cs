using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BollingerNewVers
{

    public partial class Form1 : Form
    {
        private string namePara;
        private int period;

        Dictionary<string, List<string>> allOrders = new Dictionary<string, List<string>>();

        public Form1()
        {
            AddAllOrders();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CycleWork();
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            //comboBox3.Enabled = false;
            //comboBox4.Enabled = false;
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
                 BollingerSpotMarket.Telegram.prozents = new Dictionary<string, string>() {
                        {"comboBox3", comboBox3.Text},
                        {"comboBox4", comboBox4.Text},
                        {"comboBox5", comboBox5.Text}
                    };
                 BollingerSpotMarket.Telegram.checkBoxs = new Dictionary<string, bool>()
                    {
                        {"checkBox1", checkBox1.Checked},
                        {"checkBox2", checkBox2.Checked}
                    };
                namePara = comboBox1.Text;
                period = Convert.ToInt32(comboBox2.Text);
                button1.Enabled = false;
                await StartWork();
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
        public async Task StartWork()
        {
            foreach (var item in allOrders[namePara])
            {
                Bollenger(item);
            }
        }
        public async Task Bollenger(string para)
        {
            label3.Text = allOrders[namePara].Count.ToString();

            string intervals = comboBox5.Text.ToString();
            dynamic d = await LoadUrlAsText($"https://api.binance.com/api/v1/klines?symbol={para}&interval={intervals}&limit=21");
            dynamic allOrder = JsonConvert.DeserializeObject(d);

            double totalAverage = 0;
            double totalSquares = 0;
            double lastprice = 0;
            double openPrice = 0;
            double closePrice = 0;
            double сlosedOpen = Convert.ToDouble(allOrder[19][1]);//[JSON].[19].[1] Open
            double сlosedClouse = Convert.ToDouble(allOrder[19][4]);//[JSON].[19].[4] Clouse
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
                closePrice = (Convert.ToDouble(item[4]));
                totalAverage += closePrice;//итоговая цена
                totalSquares += Math.Pow(Math.Round(closePrice, 8), 2);//возводим в квадрат средние цены закрытия
            }

            double average = totalAverage / allOrder.Count;
            double stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
            double up = average + 2 * stdev;
            double down = average - 2 * stdev;
            double bandWidth = (up - down) / average;
            double procup = 1 + double.Parse(comboBox4.Text) / 100;
            double upproc = Math.Round((up * procup), 8);
            double procdown = 1 + double.Parse(comboBox3.Text) / 100;
            double downproc = Math.Round((down / procdown), 8);

            label1.Text = "Pair " + para;

            if (upproc != double.NaN && downproc != double.NaN)
            {
                await BollingerSpotMarket.Telegram.IndexForTelegramm(para,
                     new Dictionary<string, double>()
                     {
                    {"average", average },
                    {"stdev", stdev },
                    {"up", up },
                    {"down", down},
                    {"bandWidth", bandWidth },
                    {"procup", procup},
                    {"upproc", upproc },
                    {"procdown", procdown },
                    {"downproc", downproc },
                    {"lastprice", lastprice },
                    {"openPrice", openPrice },
                    {"closePrice", closePrice },
                    {"сlosedOpen",сlosedOpen },
                    {"сlosedClouse", сlosedClouse }
                     });
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            BollingerSpotMarket.Telegram.ClearCheckedOrders();
            button1.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
            comboBox4.Enabled = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            BollingerSpotMarket.Telegram.AddOrderForMonitoring(textBox1.Text.ToString());
            textBox1.Text = "";
        }
        async void  AddAllOrders()
        {
            dynamic allPares = JsonConvert.DeserializeObject(await LoadUrlAsText("https://api.binance.com/api/v3/exchangeInfo"));
            
            foreach (var item in allPares.symbols)
            {
                if (allOrders.ContainsKey(item.quoteAsset.ToString()))
                {                    
                    allOrders[item.quoteAsset.ToString()].Add(item.symbol.ToString());
                }
                else
                {
                    List<string> orders = new List<string>();
                    orders.Add(item.symbol.ToString());
                    allOrders.Add(item.quoteAsset.ToString(), orders);
                }                
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
           var arg = "Test bot";
           await BollingerSpotMarket.Telegram.TelegramBotRepuschae(arg);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BollingerSpotMarket.Telegram.ClearMonitoring();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
