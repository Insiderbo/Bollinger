using Newtonsoft.Json;
using System;
using System.Globalization;
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

            dataGridView1.Rows.Clear();

            foreach (var item in allPares.symbols)
            {
                if (item.quoteAsset == namePara)//[JSON].symbols.[0].quoteAsset
                {
                    i++;
                    string para = item.symbol.ToString();
                    await Bollenger(para);
                }
                textBox1.Text =i.ToString();
                //break;
            }
        }



        public async Task Bollenger(string para)
        {
            dynamic d = await LoadUrlAsText($"https://testnet.binancefuture.com/fapi/v1/klines?symbol={para}&interval=15m&limit=21");
            dynamic allOrder = JsonConvert.DeserializeObject(d);
            double totalAverage = 0;
            double totalSquares = 0;
            double lastprice = 0;

            try
            {
                dynamic www = await LoadUrlAsText($"https://testnet.binancefuture.com/fapi/v1/ticker/price?symbol={para}");
                dynamic lastPare = JsonConvert.DeserializeObject(www);
                lastprice = lastPare.price;
            }
            catch
            {

            }

            //[JSON].[0].[4]
            foreach (dynamic item in allOrder)
            {
                double closePrice= (Convert.ToDouble(item[4]));
                totalAverage += closePrice;//итоговая цена
                totalSquares += Math.Pow(Math.Round(closePrice, 2), 2);//возводим в квадрат средние цены закрытия
            }
            double average = totalAverage / allOrder.Count;
            double stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
            double up = average + 2 * stdev;
            double down = average - 2 * stdev;
            double bandWidth = (up - down) / average;
            double friproc = up * 1.03;
            double sixproc = up * 1.06;

            dataGridView1.Rows.Add(para,Math.Round(friproc, 8), Math.Round(sixproc, 8), lastprice);

            if (friproc != double.NaN && sixproc != double.NaN)
            {
                Telegramm(para, friproc, sixproc, lastprice);
            }
        }
        void Telegramm(string para, double friproc, double sixproc, double lastprice)
        {
            string path = @"C:\Users\insiderbo\Documents\Telegramm_Bot\bin\Debug\net5.0\Telegramm_Bot.exe";
            if (lastprice > friproc && lastprice > sixproc)
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = $"\"{para}\"";
                p.Start();
            }
        }

    }
}
