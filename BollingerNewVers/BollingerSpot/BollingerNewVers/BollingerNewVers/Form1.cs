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

            Dictionary<string, double> indicators = new Dictionary<string, double>(10);
            indicators.Add("average", totalAverage / allOrder.Count);
            indicators.Add("stdev", Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count));
            indicators.Add("up", indicators["average"] + 2 * indicators["stdev"]);
            indicators.Add("down", indicators["average"] - 2 * indicators["stdev"]);
            indicators.Add("bandWidth", (indicators["up"] - indicators["stdev"]) / indicators["average"]);
            indicators.Add("procup", 1 + double.Parse(comboBox4.Text) / 100);
            indicators.Add("upproc", Math.Round(indicators["up"] * indicators["procup"], 8));
            indicators.Add("procdown", 1 + double.Parse(comboBox3.Text) / 100);
            indicators.Add("downproc", Math.Round((indicators["down"] / indicators["procdown"]), 8));
            indicators.Add("lastprice", lastprice);

            label1.Text = "Pair " + para;


            if (indicators["upproc"] != double.NaN && indicators["downproc"] != double.NaN)
            {
                BollingerSpotMarket.Telegram.IndexForTelegramm(para,
                    indicators,
                    new Dictionary<string, string>() {
                        {"comboBox3", comboBox3.Text},
                        {"comboBox4", comboBox4.Text}
                    },
                    new Dictionary<string, bool>()
                    {
                        {"checkBox1", checkBox1.Enabled},
                        {"checkBox2", checkBox2.Enabled}
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

        private void button4_Click(object sender, EventArgs e)
        {
            var arg = "Test bot";
            BollingerSpotMarket.Telegram.TelegramBotRepuschae(arg);

        }
    }
}
