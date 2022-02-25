using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
class PaymentPara
{
    double totalAverage = 0;
    double totalSquares = 0;
    double lastprice = 0;
    double openPrice = 0;
    double closePrice = 0;

    double average;
    double stdev;
    double up;
    double down;
    double bandWidth;
    double procup;
    double upproc;
    double procdown;
    double downproc;

    double сlosedOpen;//[JSON].[19].[1] Open
    double сlosedClouse;//[JSON].[19].[4] Clouse


    async void ChecksRara(dynamic allOrder)
    {       
         //lastprice = (Convert.ToDouble(lastPare.price));        

        //[JSON].[0].[4]
        foreach (dynamic item in allOrder)
        {
            openPrice = (Convert.ToDouble(item[1]));//[JSON].[0].[1]
            closePrice = (Convert.ToDouble(item[4]));
            totalAverage += closePrice;//итоговая цена
            totalSquares += Math.Pow(Math.Round(closePrice, 8), 2);//возводим в квадрат средние цены закрытия
        }

        double average = totalAverage / allOrder.Count;
        double stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
        double up = average + 2 * stdev;
        double down = average - 2 * stdev;
        double bandWidth = (up - down) / average;
        double procup = 1 + BollingerNewVers.Form1.InterestUp / 100;        
        double upproc = Math.Round((up * procup), 8);
        double procdown = 1 + BollingerNewVers.Form1.InterestDown / 100;
        double downproc = Math.Round((down / procdown), 8);
    }
}    