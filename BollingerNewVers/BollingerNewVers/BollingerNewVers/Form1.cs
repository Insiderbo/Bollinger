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
            int colvoSwitch = d.Count;
            double[] tp = new double[colvoSwitch];

            for (int i = 0; i < colvoSwitch; i++) //[JSON].[0].[2]
            {
                tp[i] = Convert.ToDouble(d[i][4]);                
            }

            double sred = 0f;

            foreach (var item in tp)
            {
                sred += item;
            }
             sred/= colvoSwitch;

            for (int i = 0; i < colvoSwitch; i++) //[JSON].[0].[2]
            {
                tp[i] -= sred;
                tp[i] = Math.Pow(tp[i], 2);
            }

            double sum = 0;

            foreach (var item in tp)
            {
                sum += item;
            }

            Math.Sqrt(sum);
            return sum / (colvoSwitch-1);




            return 0;
        }

      
    }
}
