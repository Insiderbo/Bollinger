using Newtonsoft.Json;
using System;
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
        private string timerTick;

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
                namePara = comboBox1.Text;
                period = Convert.ToInt32(comboBox2.Text);

                if (namePara == "")
                {
                    MessageBox.Show("No order");
                    return;
                }
                if (period == 0)
                {
                    MessageBox.Show("No period");
                    return;
                }
                button1.Enabled = false;
                await Get_Pairs();
                await Task.Delay(period * 1000);
            }

        }



        public async Task Get_Pairs()
        {           
            dynamic allPares = JsonConvert.DeserializeObject(await LoadUrlAsText("https://api.binance.com/api/v3/exchangeInfo"));


            int i = 0;

            dataGridView1.Rows.Clear();

            foreach (var item in allPares.symbols)
            {
                
                if (item.quoteAsset == namePara)//[JSON].symbols.[0].quoteAsset
                {
                    i++;
                    string para = item.symbol.ToString();

                    var allOrder =await LoadUrlAsText($"https://api.binance.com/api/v1/klines?symbol={para}&interval=5m&limit=20");

                    dataGridView1.Rows.Add(item.symbol, Bollenger(JsonConvert.DeserializeObject(allOrder)));

                }
                textBox1.Text =i.ToString();
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

        public double Bollenger(dynamic d)
        {
            double[] tp = new double[20];

            for (int i = 0; i < 20; i++) //[JSON].[0].[2]
            {
                double sum = Convert.ToDouble(d[i][2]) + Convert.ToDouble(d[i][3]) + Convert.ToDouble(d[i][4]);
                tp[i] = sum/3;
            }

            double rez = 0f;

            foreach (var item in tp)
            {
                rez += item;
            }

            return rez/20;
        }

      
    }
}
