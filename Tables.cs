using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOfGods
{
    class Item
    {
        public string name;
        public string fullName;
        public string price;
        public string weight;
        public int quantity;
    }

    static class Tables
    {
        public static List<List<string>> NotableBuisnesses(Settlement town, Random rnd)
        {
            string workdir = Directory.GetCurrentDirectory();
            string shops = workdir + "/Tables/shops_and_items.csv";
            List<List<string>> shopTypes = new List<List<string>>();
            if (File.Exists(shops))
            {
                string[] lines = File.ReadAllLines(shops);
                string[] nameLine = lines[0].Split(',');
                string[] spLowLine = lines[1].Split(',');
                string[] spHighLine = lines[2].Split(',');
                string[] wealthPercLow = lines[3].Split(',');
                string[] wealthPercHigh = lines[4].Split(',');

                for (int i = 5; i < nameLine.Length; i++)
                {
                    List<string> buissnesses = new List<string>();
                    int spLow = int.Parse(spLowLine[i]);
                    int spHigh = int.Parse(spHighLine[i]);
                    int supportValue = rnd.Next(spLow, spHigh);
                    int nrOfBuissnesses = town.population / supportValue;
                    if (nrOfBuissnesses > 0)
                    {
                        buissnesses.Add(nameLine[i]);
                        for (int j = 1; j < nrOfBuissnesses; j++)
                            buissnesses.Add(nameLine[i] + " " + j);
                        shopTypes.Add(buissnesses);
                    }
                    //Special Cases
                    if(nameLine[i].StartsWith("Church") && town.clergymen > 0)
                    {
                        buissnesses.Add(nameLine[i]);
                        buissnesses.Add(nameLine[i]);
                        buissnesses.Add(nameLine[i] + " " + 1);
                        shopTypes.Add(buissnesses);
                    }
                }
            }
            return shopTypes;
        }

        public static int GetItemQuantity(SettlementSize s, Random r)
        {
            if (s == SettlementSize.Hamlet)
                return 1;
            if (s == SettlementSize.Village)
                return r.Next(1,5);
            if (s == SettlementSize.Small_Town)
                return r.Next(1, 7);
            if (s == SettlementSize.Large_Town)
                return r.Next(1, 9);
            if (s == SettlementSize.Small_City)
                return r.Next(1, 11);
            if (s == SettlementSize.Large_City)
                return r.Next(1, 13);

            return 0;
        }

        public static double[] GetShopTypeWealthRange(string shopType)
        {
            string workdir = Directory.GetCurrentDirectory();
            string shops = workdir + "/Tables/shops_and_items.csv";
            double[] range = new double[2];
            if (File.Exists(shops))
            {
                string[] shopLines = File.ReadAllLines(shops);
                string[] shopName = shopLines[0].Split(',');
                string[] supportValuesLow = shopLines[3].Split(',');
                string[] supportValuesHigh = shopLines[4].Split(',');
                int shopTypeNr = 0;
                for (int i = 0; i < shopName.Length; i++)
                    if (shopName[i] == shopType)
                        shopTypeNr = i;

                range[0] = double.Parse(supportValuesLow[shopTypeNr], CultureInfo.InvariantCulture);
                range[1] = double.Parse(supportValuesHigh[shopTypeNr], CultureInfo.InvariantCulture);
            }
            return range;
        }

        public static double[] GetChurchWealthRange(ReligiousPresence r)
        {
            double[] range = new double[2];
            if (r == ReligiousPresence.Weak)
                range = new double[2] { 5, 10 };
            if (r == ReligiousPresence.Average)
                range = new double[2] { 8, 16 };
            if (r == ReligiousPresence.Strong)
                range = new double[2] { 12, 20 };
            return range;
        }

        public static List<Item> GetShopItems(string shopType, string path, Settlement settlment)
        {
            string workdir = Directory.GetCurrentDirectory();
            string shops = workdir + "/Tables/shops_and_items.csv";
            List<Item> shopItems = new List<Item>();
            if (File.Exists(shops))
            {
                //Random rnd = new Random(path.GetHashCode());
                Random rnd = new Random();
                string[] shopLines = File.ReadAllLines(shops);
                string[] shopName = shopLines[0].Split(',');
                int shopTypeNr = 0;
                for (int i = 0; i < shopName.Length; i++)
                    if (shopName[i] == shopType)
                        shopTypeNr = i;
                
                Random rnd2 = new Random();
                for (int i = 5; i < shopLines.Length; i++)
                {
                    string[] itemLine = shopLines[i].Split(',');
                    string itemProbability = itemLine[shopTypeNr];
                    if (itemProbability != "")
                    {
                        string probString = itemProbability.Replace("%", "");
                        probString = probString.Replace(".00", "");
                        int prob = 0;
                        int.TryParse(probString, out prob);
                        if(prob > rnd.Next(0, 100))
                        {
                            Item item = new Item();
                            item.fullName = itemLine[0].Replace(";", ",");
                            if (item.fullName.Length > 22)
                                item.name = item.fullName.Substring(0, 20) + "...";
                            else
                                item.name = item.fullName;
                            item.price = itemLine[1].Replace(";", ",");
                            item.weight = itemLine[2].Replace(";", ",");
                            item.quantity = GetItemQuantity(settlment.size, rnd2);
                            shopItems.Add(item);
                        }
                    }
                }
            }
            return shopItems;
        }
    }
}


