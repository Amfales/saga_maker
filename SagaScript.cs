using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace BoxOfGods
{
    static class SagaScript
    {
        public static string Parse(string text, string folder)
        {


            string[] parts = text.Split(']');
            string mod = "";
            Random random = new Random();
            foreach (string part in parts)
            {
                if (part.Contains('['))
                {
                    mod += part.Split('[')[0];
                    string tag = part.Split('[')[1];
                    string path = folder + tag + ".txt";
                    if (File.Exists(path))
                    {
                        string[] lines = File.ReadAllLines(path);
                        mod += lines[random.Next(0, lines.Length - 1)];
                    }
                }
                else
                    mod += part;
            }
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(mod);
        }
    }
}
