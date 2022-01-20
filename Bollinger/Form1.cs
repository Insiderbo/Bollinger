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
        string ccy;
        string candle_period;
        string drop_move;
        string interval;
        readonly Dictionary<string, HashSet<string>> pairs = new Dictionary<string, HashSet<string>>();
        readonly Dictionary<string, DataGridViewRow> rows = new Dictionary<string, DataGridViewRow>();
        readonly Dictionary<string, List<Candle>> candles = new Dictionary<string, List<Candle>>();
        private readonly Dictionary<string, RsiNormalCalculator> rsifast_calculators = new Dictionary<string, RsiNormalCalculator>();
        private readonly Dictionary<string, RsiNormalCalculator> rsislow_calculators = new Dictionary<string, RsiNormalCalculator>();
        private readonly Dictionary<string, RsiNormalCalculator> rsisnormal_calculators = new Dictionary<string, RsiNormalCalculator>();
        private int period = 1;
        private int period_slow = 3;
        private int period_normal = 5;
        private int fast_level = 1;
        private int slow_level = 10;
        private int main_level = 20;
        private int drop_price = 50;
       

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            udFastRsiPeriod.Value = period;
            udFastRsiPeriod.Minimum = 1;
            udFastRsiPeriod.Maximum = 100;
            udFastRsiPeriod.DecimalPlaces = 0;
            udFastRsiPeriod.ValueChanged += UdFastRsiPeriod_ValueChanged;

            numericUpDown1.Value = main_level;
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 95;
            numericUpDown1.DecimalPlaces = 0;
            numericUpDown1.ValueChanged += Level_Maksimum;

            numericUpDown2.Value = period_normal;
            numericUpDown2.Minimum = 1;
            numericUpDown2.Maximum = 100;
            numericUpDown2.DecimalPlaces = 0;
            numericUpDown2.ValueChanged += UdNormalRsiPeriod_ValueChanged;

            numericUpDown3.Value = period_slow;
            numericUpDown3.Minimum = 1;
            numericUpDown3.Maximum = 100;
            numericUpDown3.DecimalPlaces = 0;
            numericUpDown3.ValueChanged += UdSlowRsiPeriod_ValueChanged;

            numericUpDown4.Value = slow_level;
            numericUpDown4.Minimum = 1;
            numericUpDown4.Maximum = 95;
            numericUpDown4.DecimalPlaces = 0;
            numericUpDown4.ValueChanged += Level_Slow;

            numericUpDown5.Value = fast_level;
            numericUpDown5.Minimum = 1;
            numericUpDown5.Maximum = 95;
            numericUpDown5.DecimalPlaces = 0;
            numericUpDown5.ValueChanged += Level_Fast;

            numericUpDown6.Value = drop_price;
            numericUpDown6.Minimum = 0;
            numericUpDown6.Maximum = 95;
            numericUpDown6.DecimalPlaces = 0;
            numericUpDown6.ValueChanged += Drop_Price;

            cbLevelMove.Text = "-15";
            cbUpdateTime.Text = "300000";
            cbCandlePeriod.Text = "1d";

            
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

            notifyIcon1.BalloonTipTitle = "Cryptocurrency Assistant";
            notifyIcon1.BalloonTipText = "Приложение свернуто";
            notifyIcon1.Text = "Cryptocurrency Assistant";
        }

        private void Drop_Price(object sender, EventArgs e)
        {
            drop_price = (int)numericUpDown6.Value;
        }

        private void Level_Fast(object sender, EventArgs e)
        {
            fast_level = (int)numericUpDown5.Value;
        }

        private void Level_Slow(object sender, EventArgs e)
        {
            slow_level = (int)numericUpDown4.Value;
        }

        private void UdNormalRsiPeriod_ValueChanged(object sender, EventArgs e)
        {
            period_normal = (int)numericUpDown2.Value;
        }

        private void UdSlowRsiPeriod_ValueChanged(object sender, EventArgs e)
        {
            period_slow = (int)numericUpDown3.Value;
        }

        private void Level_Maksimum(object sender, EventArgs e)
        {
            main_level = (int)numericUpDown1.Value;
        }

        private void UdFastRsiPeriod_ValueChanged(object sender, EventArgs e)
        {
            period = (int)udFastRsiPeriod.Value;
        }

        public async Task Get_Pairs()
        {

            dynamic d = JsonConvert.DeserializeObject(await LoadUrlAsText("https://api.binance.com/api/v3/exchangeInfo"));
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
            candles[pair] = await LoadCandles(pair, candle_period, 213);
            rsifast_calculators[pair] = new RsiNormalCalculator(candles[pair], period);
            rsislow_calculators[pair] = new RsiNormalCalculator(candles[pair], period_slow);
            rsisnormal_calculators[pair] = new RsiNormalCalculator(candles[pair], period_normal);
            UpdateRow(pair);
        }

        public async Task LoadOneCandle(string pair)
        {
            var all = await LoadCandles(pair, candle_period, 1);
            var new_last = all.Single();
            var old_last = candles[pair].Last();
            Status($"{pair}: {old_last.сlosePrice} -> {new_last.сlosePrice}");
            candles[pair].RemoveAt(candles[pair].Count - 1);
            var previous = candles[pair].Last();
            candles[pair].Add(new_last);
            rsifast_calculators[pair].RecalculateLast(new_last, previous);
            rsislow_calculators[pair].RecalculateLast(new_last, previous);
            rsisnormal_calculators[pair].RecalculateLast(new_last, previous);
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
            row.Cells[1].Value = Math.Round(rsifast_calculators[pair].GetLast().value, 2);
            row.Cells[2].Value = Math.Round(rsislow_calculators[pair].GetLast().value, 2);
            row.Cells[3].Value = Math.Round(rsisnormal_calculators[pair].GetLast().value, 2);
            row.Cells[4].Value =
                CalculateGlobalDownPrice(
                    candles_for_pair.Select(x => x.highPrice).ToList(),
                    candles_for_pair.Select(x => x.сlosePrice).ToList());
            row.Cells[5].Value = Math.Round(
            CalculateLastlMove(
                    candles_for_pair.Skip(candles_for_pair.Count - 1).Select(x => x.сlosePrice).ToList(),
                    candles_for_pair.Skip(candles_for_pair.Count - 1).Select(x => x.highPrice).ToList(),
                    candles_for_pair.Skip(candles_for_pair.Count - 1).Select(x => x.lowPrice).ToList())
            , 2);
            row.Cells[6].Value = Math.Round(CalculateMomentum(candles_for_pair.Skip(candles_for_pair.Count - 14).Select(x => x.сlosePrice).ToList()), 2);
            row.Cells[7].Value = CalculateGlobalPower(
                rsifast_calculators[pair].GetLast().value,
                rsislow_calculators[pair].GetLast().value,
                rsisnormal_calculators[pair].GetLast().value
                );

            row.Cells[8].Value = RecommendedPpurchasePrice(
                candles[pair].Last(),
                candles_for_pair.Skip(candles_for_pair.Count - 3).Select(x => x.lowPrice).ToList()
                );
            row.Cells[9].Value = MinimumPrice(candles[pair].Last());
            row.Cells[10].Value = SimpleMA(candles_for_pair.Select(x => x.сlosePrice).ToList());
            PaintRow(row);
        }

        private object SimpleMA(List<double> closePrices)
        {
            return Math.Round((closePrices.Sum() / closePrices.Count),8);
        }

        private object MinimumPrice(Candle candle)
        {
            return candle.lowPrice;
        }

        private double CalculateGlobalPower(double rsifast_calculators, double rsislow_calculators, double rsisnormal_calculators)
        {
            return Math.Round((rsifast_calculators + rsislow_calculators + rsisnormal_calculators) / 3, 2);
        }

        private double CalculateMomentum(List<double> closePrices)
        {
            var clouse = closePrices.Last();
            double close_n = closePrices.ElementAt(0);
            var momentum = (clouse * 100) / close_n;
            return momentum;
        }

        private double RecommendedPpurchasePrice(Candle c, List<double> lowPrice)
        {
            return Math.Round(lowPrice.ElementAt(0) - (lowPrice.ElementAt(0) * ((c.highPrice / c.lowPrice) - 1)),8);
        }

        private double CalculateLastlMove(List<double> closePrices, List<double> highPrices, List<double> lowPrices)
        {
            var max = highPrices.Max();
            var min = lowPrices.Min();
            var clouse = closePrices.Last();
            var avg = (max + min) / 2;

            if (clouse == 0 || avg == 0) return 0;
            double move_last = 0;
            if (avg > clouse)
            {
                move_last = (avg/clouse-1) * 100;
                move_last = Math.Round(move_last,2);
                move_last = move_last * -1;
            }
            if (avg < clouse)
            {
                move_last = (clouse/avg-1) * 100;
                move_last = Math.Round(move_last, 2);
            }
            return move_last;
        }

        private object CalculateGlobalDownPrice(List<double> highPrices, List<double> closePrices)
        {
            var max = highPrices.Max();
            var clouse = closePrices.Last();
            double move_global = Math.Round(((max - clouse) / max) * 100);
            return move_global;
        }

        void PaintRow(DataGridViewRow r)
        {
            if (double.Parse(r.Cells[5].Value.ToString()) < double.Parse(drop_move) && checkBoxResoldMove.Checked == true)
            {
                MoveRowUp(r);
                NotifyTg(r.Cells[0].Value.ToString() + "  Resold Move : " + r.Cells[5].Value.ToString());
            }
            if (SortingAlgorithm(r))
            {
                MoveRowUp(r);
                if (r.DefaultCellStyle.BackColor != Color.LavenderBlush)
                {
                    r.DefaultCellStyle.BackColor = Color.LavenderBlush;
                }
                if (r.DefaultCellStyle.BackColor == Color.LavenderBlush && double.Parse(r.Cells[1].Value.ToString()) < fast_level
                    && checkBoxMaxresold.Checked == true
                    )
                {
                    NotifyTg(r.Cells[0].Value.ToString() + "  Maximum resold Fast RSI: " + r.Cells[1].Value.ToString());
                }
                if (r.DefaultCellStyle.BackColor == Color.LavenderBlush && double.Parse(r.Cells[1].Value.ToString()) > double.Parse(r.Cells[2].Value.ToString())
                    && checkBoxBuy.Checked == true
                    )
                {
                    NotifyTg(r.Cells[0].Value.ToString() + "  Buy price: " + r.Cells[8].Value.ToString());
                }
            }
            else if (double.Parse(r.Cells[3].Value.ToString()) > 90 && checkBoxOverbought.Checked == true)
            {
                if (r.DefaultCellStyle.BackColor != Color.Gold)
                {
                    r.DefaultCellStyle.BackColor = Color.Gold;
                }

            }
            else
            {
                if (r.DefaultCellStyle.BackColor == Color.LavenderBlush)
                {
                    MoveRowDown(r);
                }
                r.DefaultCellStyle.BackColor = Color.LightCyan;
            }
        }

        private bool SortingAlgorithm(DataGridViewRow r)
        {
            for (int i = 1; i < r.Cells.Count; i++)
            {
                if (!double.TryParse(r.Cells[i].Value.ToString(), out var d))
                {
                    return false;
                }
                if (d == 0)
                {
                    return false;
                }
            }
            return  
                     double.Parse(r.Cells[2].Value.ToString()) < slow_level && double.Parse(r.Cells[3].Value.ToString()) < main_level
                     && double.Parse(r.Cells[4].Value.ToString()) < drop_price;
        }

        private void PaintRows()
        {
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                PaintRow(r);
            }
        }

        private void MoveRowUp(DataGridViewRow r)
        {
            if (r.Index == 0) { return; }
            var upper_row = dataGridView1.Rows[r.Index - 1];
            if (upper_row.DefaultCellStyle.BackColor == Color.LavenderBlush)
            {
                return;
            }

            dataGridView1.Rows.Remove(r);
            dataGridView1.Rows.Insert(0, r);
        }

        private void MoveRowDown(DataGridViewRow r)
        {
            var last_index = dataGridView1.Rows.Count - 1;
            if (r.Index == last_index) { return; }
            dataGridView1.Rows.Remove(r);
            dataGridView1.Rows.Add(r);
        }

        int timer_tick_count = 0;
        bool timer_cancelled = true;
        private async void timer1_Tick(object sender, EventArgs e)
        {
            Status($"{Thread.CurrentThread.ManagedThreadId} load last candles for {pairs[ccy].Count} pairs");
            
            if (timer_tick_count > 0 || timer_cancelled)
            {
                return;
            }
            Interlocked.Increment(ref timer_tick_count);

            foreach (var p in pairs[ccy])
            {
                await LoadOneCandle(p);
                if (timer_cancelled)
                {
                    return;
                }
            }
            Interlocked.Decrement(ref timer_tick_count);
            timer1.Stop();
            Status($" loaded");
            timer1.Start();
        }
 

        private void Status(string str)
        {
            toolStripStatusLabel1.Text = str;
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            udFastRsiPeriod.Enabled = false;
            numericUpDown1.Enabled = false;
            numericUpDown2.Enabled = false;
            numericUpDown3.Enabled = false;
            numericUpDown4.Enabled = false;
            numericUpDown5.Enabled = false;
            numericUpDown6.Enabled = false;
            cbCandlePeriod.Enabled = false;
            cbLevelMove.Enabled = false;
            cbUpdateTime.Enabled = false;

            timer_cancelled = true;
            if (timer1.Enabled)
            {
                timer1.Stop();
            }
            Thread.Sleep(500);
            ccy = comboBox1.Text;
            dataGridView1.Rows.Clear();
            rows.Clear();
            int i = 1;
            foreach (var p in pairs[ccy].OrderBy(x => x))
            {
                Status($"load {p} ({i++} of {pairs[ccy].Count})");
                var row = dataGridView1.Rows.Add(p, "", "", "");
                rows.Add(p, dataGridView1.Rows[row]);
                await LoadAllCandles(p);
            }

            timer1.Interval = int.Parse(interval);
            timer1.Start();
            timer_cancelled = false;
        }

        static void NotifyTg(string message)
        {
            var path = @"C:\Users\Администратор\source\repos\Telegramm_Bot\bin\Debug\net5.0\Telegramm_Bot.exe";
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = $"\"{message}\"";
            p.Start();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            candle_period = cbCandlePeriod.Text;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            drop_move = cbLevelMove.Text;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            interval = cbUpdateTime.Text;
        }

        private void notifyIcon2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
            else if(FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }

        }
    }
}
