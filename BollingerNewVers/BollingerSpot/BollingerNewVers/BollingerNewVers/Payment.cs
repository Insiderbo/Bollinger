using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
class DataPara
{

    string para;

    public double totalAverage = 0;
    public double totalSquares = 0;
    public double lastprice = 0;
    public double openPrice = 0;
    public double closePrice = 0;

    public double average;
    public double stdev;
    public double up;
    public double down;
    public double bandWidth;
    public double procup;
    public double upproc;
    public double procdown;
    public double downproc;

    public double сlosedOpen;//[JSON].[19].[1] Open
    public double сlosedClouse;//[JSON].[19].[4] Clouse

    public bool IsContainsInResalt;
    public bool IsContainsInMonitoring;
    public bool IsContainsInControlavg;
    
    public bool UpCheck;
    public bool DownCheck;

    public void СalculationsRara(dynamic allOrder)
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

        average = totalAverage / allOrder.Count;
        stdev = Math.Sqrt((totalSquares - Math.Pow(totalAverage, 2) / allOrder.Count) / allOrder.Count);
        up = average + 2 * stdev;
        down = average - 2 * stdev;
        bandWidth = (up - down) / average;
        procup = 1 + BollingerNewVers.Form1.InterestUp / 100;
        upproc = Math.Round((up * procup), 8);
        procdown = 1 + BollingerNewVers.Form1.InterestDown / 100;
        downproc = Math.Round((down / procdown), 8);
    }


    public string CheckingConditionsForAll(bool upCheck,bool downCheck)
    {
        if (IsContainsInResalt == false)
        {
            if (lastprice < downproc && DownCheck == true)
            {
                BollingerNewVers.Form1.resalt.Add(para);

                return "PRICE ==-> " + Math.Round(lastprice, 8).ToString() + "\n" +  "DOWN ==-> " ;

            }
            if (lastprice > upproc && UpCheck == true)
            {
                BollingerNewVers.Form1.resalt.Add(para);
                return "PRICE ==-> " + Math.Round(lastprice, 8).ToString() + "\n" + "Possibly Short ==->  " ;
            }
        }
        if (lastprice > downproc && lastprice < upproc)
        {
            BollingerNewVers.Form1.resalt.Remove(para);
        }
        return null;
    }
    public string CheckingConditionsForMe()
    {
        if (IsContainsInMonitoring == true)
        {
            if (lastprice < downproc)
            {
                return "PRICE ==-> " + Math.Round(lastprice, 8).ToString() + "\n" + "Observed Coins " + "\n" + "Possibly Long ==-> ";
            }
            if (lastprice > upproc)
            {
                return Math.Round(lastprice, 8).ToString() + "\n" + "Observed Coins " + "\n" + "Possibly Short ==->  " ;
            }
        }

        if (IsContainsInControlavg == true)
        {
            if (lastprice > down && сlosedClouse > сlosedOpen)
            {

                BollingerNewVers.Form1.controlavg.Remove(para);
                return Math.Round(lastprice, 8).ToString() + "\n" + "Perhaps a pump ==->  ";
                               
            }
        }
        return null;
    }
}