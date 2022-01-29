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
                    dynamic allOrder = JsonConvert.DeserializeObject(await LoadUrlAsText($"https://testnet.binancefuture.com/fapi/v1/klines?symbol={para}&interval=15m&limit=21"));

                    dataGridView1.Rows.Add(para, Bollenger(allOrder));

                }
                textBox1.Text =i.ToString();
            }
            
            
        }
        

        public string Bollenger(dynamic allOrder)
        {
            
            double totalAverage = 0;
            double totalSquares = 0;

            //[JSON].[0].[2]
            foreach (dynamic item in allOrder)
            {
                double closePrice= (Convert.ToDouble(item[2]) + Convert.ToDouble(item[3]) + Convert.ToDouble(item[4]))/3;
                totalAverage += closePrice;//итоговая цена
                totalSquares += Math.Pow(closePrice, 2);//возводим в квадрат средние цены закрытия
            }
            double average = totalAverage / allOrder.Count;
            double stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
            double up = average + 2 * stdev;

            return up.ToString();
        }

      
    }
}
