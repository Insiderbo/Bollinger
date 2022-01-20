using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSI_test
{
    public class Candle
	{
		const int HighPrice = 2;
		const int LowPrice = 3;
		const int ClosePrice = 4;
		const int Timestamp = 0;


		public double highPrice;
        public double lowPrice;
        public double сlosePrice;
        public long timestamp;

		internal static Candle Create(dynamic candle)
		{
			var c = new Candle();
			c.highPrice = double.Parse(candle[HighPrice].ToString(), CultureInfo.InvariantCulture);
			c.lowPrice = double.Parse(candle[LowPrice].ToString(), CultureInfo.InvariantCulture);
			c.сlosePrice = double.Parse(candle[ClosePrice].ToString(), CultureInfo.InvariantCulture);
			c.timestamp = long.Parse(candle[Timestamp].ToString(), CultureInfo.InvariantCulture);
			return c;
		}
	}
}
