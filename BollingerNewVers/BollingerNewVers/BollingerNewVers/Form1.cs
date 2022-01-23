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
        private string period;
        private string timerTick;

        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {            
            namePara = comboBox1.Text;
            period = comboBox2.Text;
            timerTick = comboBox3.Text;
            if (namePara=="")
            {
                MessageBox.Show("No order");
                return;
            }
            await Get_Pairs();
        }

        public async Task Get_Pairs()
        {
            var ddd = await LoadUrlAsText("https://api.binance.com/api/v3/exchangeInfo");
            dynamic d = JsonConvert.DeserializeObject(ddd);

            int i = 0;

            dataGridView1.Rows.Clear();

            foreach (var item in d.symbols)
            {
                
                if (item.quoteAsset == namePara)//[JSON].symbols.[0].quoteAsset
                {
                    i++;
                    dataGridView1.Rows.Add(item.symbol);//, await (item.symbol));
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



        //async Task<string> OrderTime(string namePara)
        //{
        //    dynamic d = JsonConvert.DeserializeObject(await LoadUrlAsText($"https://api.binance.com/api/v1/klines?symbol={namePara}&interval={15m}&limit={1920}"));           
        //    return await d;
        //}

    }
}
