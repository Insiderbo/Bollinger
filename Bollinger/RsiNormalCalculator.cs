using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSI_test
{
    class RsiNormalCalculator
	{

		public readonly List<Rsi> rsi_list_normal = new List<Rsi>();
		private double averageGain;
		private double averageLoss;
		private double prev_avg_gain;
		private double prev_avg_loss;
        public bool NoCandles;
        private readonly int period_normal;

		public RsiNormalCalculator(List<Candle> candles, int period_normal)
		{
			this.period_normal = period_normal;
			NoCandles = true;
			CalculateAll(candles);
		}

		private void CalculateAll(List<Candle> candles)
		{
			rsi_list_normal.Clear();
            if (period_normal + 1 >= candles.Count)
            {
				NoCandles = true;
				return;
            }

			double gainSum = 0;
			double lossSum = 0;
			for (int i = 1; i < period_normal; i++)
			{
				double thisChange = candles[i].сlosePrice - candles[i - 1].сlosePrice;
				if (thisChange > 0)
				{
					gainSum += thisChange;
				}
				else
				{
					lossSum += (-1) * thisChange;
				}
			}

			averageGain = gainSum / period_normal;
			averageLoss = lossSum / period_normal;

			for (int i = period_normal + 1; i < candles.Count; i++)
			{
				rsi_list_normal.Add(CalcuateRsi(candles[i], candles[i - 1]));
			}
			NoCandles = false;
		}

		private Rsi CalcuateRsi(Candle last, Candle prev)
		{
			prev_avg_gain = averageGain;
			prev_avg_loss = averageLoss;
			double thisChange = last.сlosePrice - prev.сlosePrice;
			if (thisChange > 0)
			{
				averageGain = (averageGain * (period_normal - 1) + thisChange) / period_normal;
				averageLoss = (averageLoss * (period_normal - 1)) / period_normal;
			}
			else
			{
				averageGain = (averageGain * (period_normal - 1)) / period_normal;
				averageLoss = (averageLoss * (period_normal - 1) + (-1) * thisChange) / period_normal;
			}
			double rs = averageGain / averageLoss;
			var rsi = new Rsi();
			rsi.value = 100 - (100 / (1 + rs));
			return rsi;
		}

		public void RecalculateLast(Candle last, Candle previous)
		{
            if (NoCandles)
            {
				return;
            }
			averageGain = prev_avg_gain;
			averageLoss = prev_avg_loss;
			rsi_list_normal.RemoveAt(rsi_list_normal.Count - 1);
			rsi_list_normal.Add(CalcuateRsi(last, previous));
		}

		public Rsi GetLast() => NoCandles ? new Rsi { value = 0} : rsi_list_normal.Last();
	}
}

