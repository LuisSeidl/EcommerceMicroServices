using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore;
using BigBuyAPI;
using Microsoft.EntityFrameworkCore;
using EFCore.Entities;
using System.Diagnostics.Eventing.Reader;
using UpdateDatabase;


class Program
{
    public static void Main(string[] args)
    {
        UpdateFunctions functions = new UpdateFunctions();

        functions.getCompetitorUrlsFromCSV("C:\\Users\\luiss\\OneDrive\\Desktop\\Small Code\\KickFlip\\scrape ebay urls.csv");

        return;
    }

}

