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
                    Bollenger(para);

                }
                textBox1.Text =i.ToString();
            }
            
            
        }
        

        public async void Bollenger(string para)
        {
            dynamic allOrder = JsonConvert.DeserializeObject(await LoadUrlAsText($"https://testnet.binancefuture.com/fapi/v1/klines?symbol={para}&interval=5m&limit=20"));

           

            int colvoSwitch = allOrder.Count;
            double[] tp = new double[colvoSwitch];

            for (int i = 0; i < colvoSwitch; i++) //[JSON].[0].[2]
            {
                tp[i] = Convert.ToDouble(allOrder[i][4]);                
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
            

            dataGridView1.Rows.Add(para, (sum / (colvoSwitch-1))/2);
        }

      
    }
}
