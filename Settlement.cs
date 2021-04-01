using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxOfGods
{
    enum SettlementSize { Hamlet, Village, Small_Town, Large_Town, Small_City, Large_City };
    enum MilitaryStrength { Weak, Average, Strong };
    enum ReligiousPresence { Weak, Average, Strong };

    class Settlement
    {
        public SettlementSize size;
        public MilitaryStrength strength;
        public ReligiousPresence religiousness;

        public string name;
        public int population;
        public int military;
        public int[] guards; //guards[X] is # of guards at lvl X;
        public int clergymen;
        public int priests;
        public int WealthInGP;
        public int churches;
        
        Random rnd;

        public string Info()
        {
            string info = "Welcome to " + name + "!\n \n";
            info += "Size: " + size.ToString().Replace("_", " ") + ". Population: " + population + ".\n";
            info += "Military: " + strength + ", " + military + " guards. ";
            info += guards[1] + " Lvl1, " + guards[2] + " Lvl2, " + guards[3] + " Lvl3, "
                 + guards[4] + " Lvl4, " + guards[5] + " Lvl5 and " + guards[6] + " Lvl6.\n";
            info += "Religious presence: " + religiousness + ", " + clergymen + " clergymen where " + priests + " are priests.\n";
            info += "Wealth: " + WealthInGP + " GP."; 
            return info;
        }

        public List<List<string>> Shops()
        {
            return Tables.NotableBuisnesses(this, rnd);
        }

        public void Generate(int sizeOption, int militaryOption, int religionOption, int churches)
        {
            rnd = new Random(name.GetHashCode());
            this.churches = churches;
            
            Array sizes = Enum.GetValues(typeof(SettlementSize));
            if (sizeOption == 0)
                size = (SettlementSize)sizes.GetValue(rnd.Next(sizes.Length));
            else
                size = (SettlementSize)(sizeOption - 1);

            switch (size)
            {
                case SettlementSize.Hamlet:
                    population = rnd.Next(1, 65);
                    WealthInGP = rnd.Next((int)(200 * 0.75), (int)(200 * 1.5)); break;
                case SettlementSize.Village:
                    population = rnd.Next(66, 250);
                    WealthInGP = rnd.Next((int)(500 * 0.75), (int)(500 * 1.5)); break;
                case SettlementSize.Small_Town:
                    population = rnd.Next(251, 2000);
                    WealthInGP = rnd.Next((int)(1000 * 0.75), (int)(1000 * 1.5)); break;
                case SettlementSize.Large_Town:
                    population = rnd.Next(2001, 10000);
                    WealthInGP = rnd.Next((int)(2000 * 0.75), (int)(2000 * 1.5)); break;
                case SettlementSize.Small_City:
                    population = rnd.Next(10001, 15000);
                    WealthInGP = rnd.Next((int)(4000 * 0.75), (int)(4000 * 1.5)); break;
                case SettlementSize.Large_City:
                    population = rnd.Next(15001, 40000);
                    WealthInGP = rnd.Next((int)(8000 * 0.75), (int)(8000 * 1.5)); break;
            }

            Array miltarySizes = Enum.GetValues(typeof(MilitaryStrength));

            if (militaryOption == 0)
                strength = (MilitaryStrength)miltarySizes.GetValue(rnd.Next(miltarySizes.Length));
            else
                strength = (MilitaryStrength)(militaryOption - 1);

            double p = population / 100.0;
            switch (strength)
            {
                case MilitaryStrength.Weak: military = rnd.Next((int)(p * 2.5), (int)(p * 5)); break;
                case MilitaryStrength.Average: military = rnd.Next((int)(p * 5), (int)(p * 7)); break;
                case MilitaryStrength.Strong: military = rnd.Next((int)(p * 7), (int)(p * 15)); break;
            }

            guards = new int[7]; guards[0] = 0;
            guards[1] = military * 40 / 100;
            guards[2] = military * 22 / 100;
            guards[3] = military * 18 / 100;
            guards[4] = military * 12 / 100;
            guards[5] = military * 6 / 100;
            guards[6] = military * 2 / 100;

            Array religionLvls = Enum.GetValues(typeof(ReligiousPresence));

            if (religionOption == 0)
                religiousness = (ReligiousPresence)religionLvls.GetValue(rnd.Next(religionLvls.Length));
            else
                religiousness = (ReligiousPresence)(religionOption - 1);

            switch (religiousness)
            {
                case ReligiousPresence.Weak:
                    clergymen = rnd.Next((int)(p * 2), (int)(p * 7));
                    priests = rnd.Next((int)(clergymen * 0.01), (int)(clergymen * 0.02)); break;
                case ReligiousPresence.Average:
                    clergymen = rnd.Next((int)(p * 7), (int)(p * 17));
                    priests = rnd.Next((int)(clergymen * 0.03), (int)(clergymen * 0.05)); break;
                case ReligiousPresence.Strong:
                    clergymen = rnd.Next((int)(p * 17), (int)(p * 25));
                    priests = rnd.Next((int)(clergymen * 0.05), (int)(clergymen * 0.07)); break;
            }
        }
    }
}
