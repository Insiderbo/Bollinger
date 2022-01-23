using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

//https://www.binance.com/ru/trade/EUR_USDT?layout=basic&type=spot

namespace RSI_test
{
    public partial class Form1 : Form
    {
        
        readonly Dictionary<string, HashSet<string>> pairs = new Dictionary<string, HashSet<string>>(); // вся информация по парам
        readonly Dictionary<string, DataGridViewRow> rows = new Dictionary<string, DataGridViewRow>();// 
        readonly Dictionary<string, List<Candle>> candles = new Dictionary<string, List<Candle>>();
        private int period = 1;
        private int period_slow = 3;
        private int period_normal = 5;
       

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            cbUpdateTime.Text = "300000";
            CandlePeriod.Text = "1d";

            
            pairs["EUR"] = new HashSet<string>();
            pairs["RUB"] = new HashSet<string>();
            pairs["USDT"] = new HashSet<string>();
            pairs["BTC"] = new HashSet<string>();
            pairs["BNB"] = new HashSet<string>();
            pairs["ETH"] = new HashSet<string>();
            await Get_Pairs();

            foreach (var currency in pairs.Keys)
            {
                comboBox1.Items.Add(currency);
            }
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false;

            await LoadAllCandles(comboBox1.Text);
            await LoadOneCandle(comboBox1.Text);


            timer1.Start();
        }


        public async Task Get_Pairs()
        {
            var ddd = await LoadUrlAsText("https://api.binance.com/api/v3/exchangeInfo");
            dynamic d = JsonConvert.DeserializeObject(ddd);

            int count = d.rateLimits[0]["limit"];
            foreach (var master in pairs.Keys)
            {
                for (var i = 0; i < count; i++)
                {
                    string symbol = (d.symbols[i]["symbol"]).ToString();

                    if (symbol.Contains(master))
                    {
                        pairs[master].Add(symbol);
                        if (!candles.ContainsKey(symbol))
                        {
                            candles.Add(symbol, new List<Candle>());
                        }
                    }
                }
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

        public async Task<List<Candle>> LoadCandles(string pair, string interval, int limit)
        {
            dynamic d = JsonConvert.DeserializeObject(await LoadUrlAsText($"https://api.binance.com/api/v1/klines?symbol={pair}&interval={interval}&limit={limit}"));
            var candles = new List<Candle>();
            foreach (var candle in d)
            {
                candles.Add(Candle.Create(candle));
            }
            return candles;
        }

        public async Task LoadAllCandles(string pair)
        {
            candles[pair] = await LoadCandles(pair, CandlePeriod.Items.ToString(), 213);
            UpdateRow(pair);
        }

        public async Task LoadOneCandle(string pair)
        {
            var all = await LoadCandles(pair, CandlePeriod.Items.ToString(), 1);
            var new_last = all.Single();
            var old_last = candles[pair].Last();
            candles[pair].RemoveAt(candles[pair].Count - 1);
            var previous = candles[pair].Last();
            candles[pair].Add(new_last);
            UpdateRow(pair);
        }

        private void UpdateRow(string pair)
        {
            if (!rows.ContainsKey(pair))
            {
                return;
            }
            var candles_for_pair = candles[pair];

            var row = rows[pair];
           
        }

        static void telegramBotMessage(string message)
        {
            var path = @"C:\Users\Администратор\source\repos\Telegramm_Bot\bin\Debug\net5.0\Telegramm_Bot.exe";
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = $"\"{message}\"";
            p.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            

        }
    }
}
