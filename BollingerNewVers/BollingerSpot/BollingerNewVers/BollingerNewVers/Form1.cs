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

        public static double InterestUp;
        public static double InterestDown;

        public static List<string> resalt = new List<string>();
        public static List<string> monitoring = new List<string>();
        public static List<string> controlavg = new List<string>();


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

            DataPara dataPara = new DataPara();

            try
            {
                dynamic www = await LoadUrlAsText($"https://api.binance.com/api/v3/ticker/price?symbol={para}");
                dynamic lastPare = JsonConvert.DeserializeObject(www);
                dataPara.lastprice = (Convert.ToDouble(lastPare.price));
            }
            catch { }

            dataPara.СalculationsRara(allOrder);

            label1.Text = "Pair " + para;

            if (dataPara.upproc != double.NaN && dataPara.downproc != double.NaN)
            {
                string message = dataPara.CheckingConditionsForAll(checkBox1.Enabled, checkBox2.Enabled);

                if (message != null)
                {
                    BollingerSpotMarket.Telegram.TelegramBot(message + "Period ==-> " + comboBox5.Text); 
                    message = null;
                }

                message = dataPara.CheckingConditionsForMe();

                if (message != null)
                {
                    BollingerSpotMarket.Telegram.TelegramBotRepuschae(message + "Period ==-> " + comboBox5.Text);
                }

            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            resalt.Clear();
            monitoring.Clear();
            controlavg.Clear();

            button1.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
            comboBox4.Enabled = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            monitoring.Add(textBox1.Text.ToString());
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
    }
}
