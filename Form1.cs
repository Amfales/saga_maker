using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoxOfGods
{
    public partial class Form1 : Form
    {
        //============================================================================================
        //                                Campaign Page
        //============================================================================================


        //---------------------------- Variables ------------------------------
        string workdir;
        List<string> levels;
        string[] levelTypes = { "Campaign", "Country", "Settlement", "Shop", "Items" };
        string[] levelTypesPlural = { "Campaigns", "Countries", "Settlements", "Shops", "Items" };
        Settlement currentSettlement;
        string currentItem;
        Random r = new Random();
        int daysPerMonth = 30;
        int daysPerHalfYear;
        int daysPerYear;
        int age;

        //============================= General Stuff ===================================
        //---------------------------- initialization -------------------------
        public Form1()
        {
            InitializeComponent();
            workdir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(workdir + "/Campaigns"))
                Directory.CreateDirectory(workdir + "/Campaigns");
            levels = new List<string>();
            levels.Add("Campaigns");
            ViewCurrentLevel();
            InitiateCultures();
            InitiateZones();
            InitiateCalendar();
            InitiateClimaticZoneDropDown();
            InitiateEncounter();
            ScanGeneratorTabs();
        }
        // ----------------------- "the path" as string -----------------------
        string LevelPath()
        {
            string path = workdir;
            foreach (string lvl in levels)
                path += "/" + lvl.Replace(" ", "_");
            return path;
        }
        //------------------ choose view depending on path depth --------------
        void ViewCurrentLevel()
        {
            if (levels.Count == 4)
                ViewSettlement();
            else if (levels.Count == 5)
                ViewShop(null, null);
            else
                VeiwSelectionControls();
        }
        // ----------------- going up the path, go back buttons ---------------
        private void GoBack_Click(object sender, EventArgs e)
        {
            SettlementShops.Controls.Clear();
            levels.RemoveAt(levels.Count - 1);
            ViewCurrentLevel();
        }

        // =============== Select Campaign/Country/Settlement View =====================
        void VeiwSelectionControls()
        {
            FillSelectionList();

            GoBack.Visible = (levels.Count > 1);
            if (GoBack.Visible)
                GoBack.Text = "Back to " + levelTypesPlural[levels.Count - 2];

            SelectLabel.Text = levelTypes[levels.Count - 1] + ":";

            TheTitleOfLevel.Text = levels[levels.Count - 1];
            TheTitleOfLevel.Visible = (levels.Count > 1);

            NamingLabel.Text = levelTypes[levels.Count - 1] + " name:";

            SelectPanel.Visible = true;
            SettlementPanel.Visible = false;
        }
        // -------- depending on what to veiw, fill the only dropdown ----------      
        void FillSelectionList()
        {
            SelectList.Items.Clear();
            foreach (string dir in Directory.GetDirectories(LevelPath()))
            {
                string name = Path.GetFileName(dir);
                SelectList.Items.Add(name.Replace("_", " "));
            }
        }
        private void CreateNew_Click(object sender, EventArgs e)
        {
            SelectPanel.Visible = false;
            if (levels.Count == 3)
                ViewSettlementCreation();
            else
                NamePanel.Visible = true;
        }
        // ------------- when choosing an existing capaign country -------------
        private void SelectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string choise = (string)SelectList.SelectedItem;
            string name = choise.Replace(" ", "_");
            levels.Add(choise);
            TheTitleOfLevel.Text = choise;
            TheTitleOfLevel.Visible = true;
            ViewCurrentLevel();
        }

        // ================ Name a new Country or Campaign view ======================= 
        private void ConfirmNameButton_Click(object sender, EventArgs e)
        {
            string input = NameInput.Text;
            string name = input.Replace(" ", "_");
            levels.Add(name);
            Directory.CreateDirectory(LevelPath());
            ViewCurrentLevel();
        }

        // =================== Settlement Create View =================================  
        void ViewSettlementCreation()
        {
            SelectPanel.Visible = false;
            NamePanel.Visible = false;
            SettlementCreatePanel.Visible = true;

            SettlementSizeOptions.SelectedIndex = 0;
            MilitaryStrengthOptions.SelectedIndex = 0;
            ReligiousPresenceOptions.SelectedIndex = 0;
            NumberOfChurcesOptions.SelectedIndex = 0;
        }

        private void SettlementCreationOK_Click(object sender, EventArgs e)
        {
            string input = SettlementNameInput.Text;
            string name = input.Replace(" ", "_");
            levels.Add(name);
            Directory.CreateDirectory(LevelPath());
            ViewSettlement();
        }

        // ========================= Settlement View ==================================  
        void ViewSettlement()
        {
            SelectPanel.Visible = false;
            NamePanel.Visible = false;
            SettlementCreatePanel.Visible = false;
            SettlementPanel.Visible = true;
            ShopPanel.Visible = false;

            if (File.Exists(LevelPath() + "/info.txt"))
            {
                ReadSettlement();
            }
            else
            {
                File.WriteAllText(LevelPath() + "/notes.txt", "Notes: ");
                GenerateSettlement();
            }
            string notes = File.ReadAllText(LevelPath() + "/notes.txt");
            SettlementNotes.Text = notes;
        }        

        private void SaveNotesButton_Click(object sender, EventArgs e)
        {
            string notes = SettlementNotes.Text;
            File.WriteAllText(LevelPath() + "/notes.txt", notes);
        }

        // ------------------------- set up ------------------------------------
        void ReadSettlement()
        {
            SettlementName.Text = levels[levels.Count - 1] + ":";
            SettlementInfo.Text = File.ReadAllText(LevelPath() + "/info.txt");
            ParseSettlement();
            DisplayShops();
        }

        void ParseSettlement()
        {
            currentSettlement = new Settlement();
            string[] infoLines = SettlementInfo.Text.Split(new[] { '\r', '\n' });
            string wealthLine = "";
            string religionWord = "";
            string sizeWord = "";
            foreach (string s in infoLines)
                if (s.StartsWith("Wealth")) wealthLine = s;
                else if (s.StartsWith("Religious")) religionWord = s.Split(' ')[2];
                else if (s.StartsWith("Size")) sizeWord = s.Split(' ')[1] + s.Split(' ')[2];
            int.TryParse(wealthLine.Split(' ')[1], out currentSettlement.WealthInGP);
            if (religionWord.StartsWith("Weak"))
                currentSettlement.religiousness = ReligiousPresence.Weak;
            if (religionWord.StartsWith("Average"))
                currentSettlement.religiousness = ReligiousPresence.Average;
            if (religionWord.StartsWith("Strong"))
                currentSettlement.religiousness = ReligiousPresence.Strong;
            if (sizeWord.StartsWith("Hamlet"))
                currentSettlement.size = SettlementSize.Hamlet;
            if (sizeWord.StartsWith("Village"))
                currentSettlement.size = SettlementSize.Village;
            if (sizeWord.StartsWith("SmallTown"))
                currentSettlement.size = SettlementSize.Small_Town;
            if (sizeWord.StartsWith("LargeTown"))
                currentSettlement.size = SettlementSize.Large_Town;
            if (sizeWord.StartsWith("SmallCity"))
                currentSettlement.size = SettlementSize.Small_City;
            if (sizeWord.StartsWith("LargeCity"))
                currentSettlement.size = SettlementSize.Large_City;
        }

        void GenerateSettlement()
        {
            Settlement settlement = new Settlement();
            settlement.name = levels[levels.Count - 1];
            SettlementName.Text = settlement.name + ":";
            settlement.Generate(
                SettlementSizeOptions.SelectedIndex,
                MilitaryStrengthOptions.SelectedIndex,
                ReligiousPresenceOptions.SelectedIndex,
                NumberOfChurcesOptions.SelectedIndex);
            string infoText = settlement.Info();
            SettlementInfo.Clear();
            foreach (string line in infoText.Split('\n'))
                SettlementInfo.AppendText(line + Environment.NewLine);
            SettlementInfo.ScrollToCaret();
            SettlementInfo.SelectionStart = 0;
            SettlementInfo.SelectionLength = 1;
            SettlementInfo.ScrollToCaret();

            File.WriteAllText(LevelPath() + "/info.txt", SettlementInfo.Text);

            List<List<string>> shops = settlement.Shops();
            
            foreach (List<string> shopType in shops)
            {
                //handle number of churches option
                if (shopType[0].Contains("Church"))
                {
                    string churchServices = shopType[0];
                    shopType.Clear();
                    shopType.Add(churchServices);
                    for (int i = 1; i <= settlement.churches; i++)
                        shopType.Add(churchServices + i.ToString());
                }

                if (shopType.Count > 1)
                {
                    string type = BadToOk(shopType[0]);
                    Directory.CreateDirectory(LevelPath() + "/" + type);
                    foreach (string shopName in shopType)
                    {
                        if (shopName != shopType[0])
                            Directory.CreateDirectory(LevelPath() + "/" + type + "/" + BadToOk(shopName));
                    }
                }
            }

            currentSettlement = settlement;
            DisplayShops();
        }
        // ------------- fill Settlement with shops ----------------------------
        void DisplayShops()
        {
            List<List<string>> shops = new List<List<string>>();
            foreach (string shopTypePath in Directory.GetDirectories(LevelPath()))
            {
                List<string> shopsOfType = new List<string>();
                string shopType = Path.GetFileName(shopTypePath);
                shopsOfType.Add(OkToBad(shopType));
                foreach (string shop in Directory.GetDirectories(LevelPath() + "/" + shopType))
                    shopsOfType.Add(OkToBad(Path.GetFileName(shop)));
                shops.Add(shopsOfType);
            }

            SettlementShops.Controls.Clear();
            int Nx = 5;
            int Ny = 14;
            int dx = SettlementShops.Width / Nx;
            int dy = SettlementShops.Height / Ny;
            int n = 0;
            for (int ny = 0; ny < Ny; ny++)
            {
                for (int nx = 0; nx < Nx; nx++)
                {
                    if (n < shops.Count)
                    {
                        ComboBox createdBox = new ComboBox();
                        createdBox.Name = "CreatedBox";
                        createdBox.Width = dx - 3;
                        createdBox.Height = dy - 3;
                        createdBox.Location = new Point(nx * dx, ny * dy);
                        createdBox.Font = new Font("Microsoft Sans Serif", 12f);
                        List<string> shopsOfOneType = shops[n];
                        for (int i = 0; i < shopsOfOneType.Count; i++)
                            createdBox.Items.Add(shopsOfOneType[i]);
                        createdBox.DropDownStyle = ComboBoxStyle.DropDownList;
                        createdBox.SelectedIndex = 0;
                        createdBox.SelectedIndexChanged += shopSelected;
                        SettlementShops.Controls.Add(createdBox);
                    }
                    n++;
                }
            }
        }
        // ------------------------------ go back ------------------------------
        private void BackToCountryButton_Click(object sender, EventArgs e)
        {
            levels.RemoveAt(levels.Count - 1);
            ViewCurrentLevel();
        }
        // ----------------------------- go down -------------------------------
        private void shopSelected(object sender, System.EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex != 0)
            {
                string shopType = box.Items[0].ToString();
                string shopName = box.SelectedItem.ToString();
                levels.Add(BadToOk(shopType));
                levels.Add(BadToOk(shopName));
                ViewShop(shopType, shopName);
            }
        }



        // =========================== Shop View =====================================          
        void ViewShop(string shopType, string shopName)
        {
            //ComboBox box = (ComboBox)sender;
            //string shopType = box.Items[0].ToString();
            //string shopName = box.SelectedItem.ToString();
            ShopPanel.Visible = true;
            SettlementPanel.Visible = false;
            ShopTitle.Text = shopName;
            if (File.Exists(LevelPath() + "/info.txt"))
            {
                ShopInfo.Text = File.ReadAllText(LevelPath() + "/info.txt");
                List<Item> items = new List<Item>();
                foreach (string s in Directory.GetFiles(LevelPath()))
                {
                    string name = Path.GetFileName(s);
                    if (name != "info.txt" && name != "data.txt" && name != "notes.txt")
                    {
                        Item item = new Item();
                        item.name = OkToBad(name.Replace(".txt", ""));
                        items.Add(item);
                    }
                }
                DisplayItems(items, false);
            }
            else
            {
                ShopInfo.Text = ShopWealth(shopType);
                File.WriteAllText(LevelPath() + "/info.txt", ShopInfo.Text);
                File.WriteAllText(LevelPath() + "/data.txt", shopType);
                File.WriteAllText(LevelPath() + "/notes.txt", "Notes: ");
                DisplayItems(Tables.GetShopItems(shopType, LevelPath(), currentSettlement), true);
            }
            string notes = File.ReadAllText(LevelPath() + "/notes.txt");
            ShopNotes.Text = notes;
        }
        
        private void SaveShop_Click(object sender, EventArgs e)
        {
            string notes = ShopNotes.Text;
            File.WriteAllText(LevelPath() + "/notes.txt", notes);
        }

        // -------------------------- set up -----------------------------------
        string ShopWealth(string shopType)
        {
            double[] r = Tables.GetShopTypeWealthRange(shopType);
            if (shopType.StartsWith("Church"))
                r = Tables.GetChurchWealthRange(currentSettlement.religiousness);
            Random rnd = new Random(LevelPath().GetHashCode());
            double wealthPersent = r[0] + (r[1] - r[0]) * rnd.NextDouble();
            int wealth = (int)(wealthPersent * currentSettlement.WealthInGP);
            return "Wealth: " + (wealth / 100).ToString() + " GP";
        }
        // ------------------------- generate Items ----------------------------
        void DisplayItems(List<Item> items, bool save)
        {
            int Nx = 5;
            int Ny = 1 + items.Count / 5;
            int dx = ShopInventory.Width / Nx;
            int dy = ShopInventory.Height / 14;
            if (Ny > 14) dx -= 4;
            int n = 0;
            ShopInventory.Controls.Clear();
            for (int ny = 0; ny < Ny; ny++)
            {
                for (int nx = 0; nx < Nx; nx++)
                {
                    if (n < items.Count)
                    {
                        Button button = new Button();
                        button.Name = "CreatedBox";
                        button.Width = dx + 3;
                        if (nx != Nx - 1) button.Width -= 6;
                        button.Height = dy - 5;
                        button.Location = new Point(nx * dx, ny * dy);
                        button.Font = new Font("Microsoft Sans Serif", 12f);
                        string name = items[n].name;
                        button.Text = name;
                        button.Click += ViewItem;
                        ShopInventory.Controls.Add(button);
                        if (save)
                            File.WriteAllText(
                                LevelPath() + "/" + BadToOk(name) + ".txt",
                                items[n].fullName.Replace("\n", "") + "\n" +
                                items[n].quantity + "\n" +
                                PriceFromRange(items[n].price) + "\n" +
                                items[n].weight);
                    }
                    n++;
                }
            }
        }

        string PriceFromRange(string priceText)
        {
            if (priceText.Contains('-'))
            {
                if (priceText.Contains("gp"))
                    priceText = TryGetPrice(priceText, " gp");
                else if (priceText.Contains("sp"))
                    priceText = TryGetPrice(priceText, " sp");
                else if (priceText.Contains("cp"))
                    priceText = TryGetPrice(priceText, " cp");
            }
            return priceText;
        }

        string TryGetPrice(string priceText, string end)
        {
            string[] parts = priceText.Split('-');
            int low = 0;
            int high = 0;
            if(parts.Length >= 2)
            {
                bool lowIsNumber = int.TryParse(parts[0].Trim(), out low);
                bool highHasNumber = int.TryParse(parts[1].Trim().Split(' ')[0], out high);
                if(lowIsNumber && highHasNumber)
                {
                    int price = r.Next(low, high);
                    priceText = price + end;
                }
            }
            return priceText;
        }

        private void CopyInventoryButton_Click(object sender, EventArgs e)
        {
            string inventory = "";
            string itemPath = "";
            foreach (Control c in ShopInventory.Controls)
            {
                Button b = (Button)c;
                itemPath = LevelPath() + "/" + BadToOk(b.Text) + ".txt";
                string[] lines = File.ReadAllLines(itemPath);
                inventory += lines[0] + "\n";
            }
            Clipboard.SetText(inventory);
        }

        private void ReplenishButton_Click(object sender, EventArgs e)
        {
            string shopType = File.ReadAllText(LevelPath() + "/data.txt");
            List<Item> newItems = Tables.GetShopItems(shopType, LevelPath(), currentSettlement);            
            string itemPath = "";
            foreach (Control c in ShopInventory.Controls)
            {
                Button b = (Button)c;
                itemPath = LevelPath() + "/" + BadToOk(b.Text) + ".txt";
                if (r.Next(1,100) < 20)
                {
                    Item replacement = newItems[r.Next(0, newItems.Count - 1)];
                    string replacementPath = LevelPath() + "/" + BadToOk(replacement.name) + ".txt";
                    if (!File.Exists(replacementPath))
                    {
                        File.WriteAllText(
                            replacementPath,
                            replacement.fullName.Replace("\n", "") + "\n" +
                            replacement.quantity + "\n" +
                            PriceFromRange(replacement.price) + "\n" +
                            replacement.weight);
                        File.Delete(itemPath);
                    }
                }
            }
            ViewShop(shopType,ShopTitle.Text);
        }

        // ---------------------------- go back --------------------------------
        private void BackToSettlement_Click(object sender, EventArgs e)
        {
            levels.RemoveAt(levels.Count - 1);
            levels.RemoveAt(levels.Count - 1);
            ViewSettlement();
        }
        // ============================ item panel ==================================
        private void ViewItem(object sender, System.EventArgs e)
        {
            Button button = (Button)sender;
            currentItem = LevelPath() + "/" + BadToOk(button.Text) + ".txt";
            updateItemPanel();
        }

        private void updateItemPanel()
        {
            string[] lines = File.ReadAllLines(currentItem);
            ItemPanel.BringToFront();
            ItemPanelName.Text = lines[0];
            if (lines.Length > 1)
                ItemPanelStock.Text = lines[1] + " in stock";
            if (lines.Length > 2)
                ItemPanelPrice.Text = "price: " + lines[2];
            if (lines.Length > 3)
                ItemPanelWeight.Text = "weight: " + lines[3];
        }

        private void buyItemButton_Click(object sender, EventArgs e)
        {
            modifyItemQuantity(-1);
            updateItemPanel();
        }

        private void modifyItemQuantity(int delta)
        {
            string[] lines = File.ReadAllLines(currentItem);
            int quantity = int.Parse(lines[1]);
            quantity += delta;
            if (quantity < 0)
                quantity = 0;
            lines[1] = quantity.ToString();
            File.WriteAllLines(currentItem, lines);
        }

        private void sellItemButton_Click(object sender, EventArgs e)
        {
            modifyItemQuantity(1);
            updateItemPanel();
        }

        private void restockButton_Click(object sender, EventArgs e)
        {
            string[] lines = File.ReadAllLines(currentItem);
            lines[1] = Tables.GetItemQuantity(currentSettlement.size, new Random()).ToString();
            File.WriteAllLines(currentItem, lines);
            updateItemPanel();
        }

        private void CloseItemPanel_Click(object sender, EventArgs e)
        {
            ItemPanel.SendToBack();
        }

        // ==================================================================================
        //                              DM Page
        // ==================================================================================
        // ========================== Settlement Name =======================================
        private void SettlementNameButton_Click(object sender, EventArgs e)
        {
            string culture = SettlementCultureDropDown.SelectedItem.ToString();

            if (SettlementCultureDropDown.SelectedIndex == 0)
            {
                NPCSettlementTextbox.Text = "Select Culture..";
            }
            else
            {
                string pathA = workdir + "/Tables/Cultures/" + culture + "/SettlementNames/A.txt";
                string pathB = workdir + "/Tables/Cultures/" + culture + "/SettlementNames/B.txt";

                if(File.Exists(pathA) && File.Exists(pathB))
                {
                    string[] lines_A = File.ReadAllLines(pathA);
                    int n_A = r.Next(0, lines_A.Length);

                    string[] lines_B = File.ReadAllLines(pathB);
                    int n_B = r.Next(0, lines_B.Length);

                    string file_C_path = workdir + "/Tables/Cultures/" + culture + "/SettlementNames/C.txt";
                    if (File.Exists(file_C_path))
                    {
                        string[] lines_C = File.ReadAllLines(workdir + "/Tables/Cultures/" + culture + "/SettlementNames/C.txt");
                        int n_C = r.Next(0, lines_C.Length);
                        NPCSettlementTextbox.Text = lines_A[n_A] + lines_B[n_B] + lines_C[n_C];
                    }
                    else
                    {
                        NPCSettlementTextbox.Text = lines_A[n_A] + lines_B[n_B];
                    }
                }
                else
                    NPCSettlementTextbox.Text = "No names yet";

            }
        }

        // ============================ Name =================================================
        private void GetNameButton_Click(object sender, EventArgs e)
        {
            NPCNameTextbox.Text = "";
            string culture = NameCultureDropDown.SelectedItem.ToString();
            if (NameCultureDropDown.SelectedIndex == 0)
            {
                NPCNameTextbox.Text = "Select Culture..";
            }
            else
            {
                string namesPath = workdir + "/Tables/Cultures/" + culture + "/PeopleNames/";
                string femalePath = namesPath + "Female_Names.txt";
                string malePath = namesPath + "Male_Names.txt";
                string generatorPath = namesPath + "generator.txt";

                if (File.Exists(generatorPath))
                {
                    NPCNameTextbox.Text =
                        SagaScript.Parse(File.ReadAllText(generatorPath), namesPath);
                }
                else if(File.Exists(femalePath) && File.Exists(malePath))
                {
                    string[] lines_NameFemale = File.ReadAllLines(femalePath);
                    int n_Females = r.Next(0, lines_NameFemale.Length);

                    string[] lines_NameMale = File.ReadAllLines(malePath);
                    int n_Male = r.Next(0, lines_NameMale.Length);

                    string lastNamesPath = workdir + "/Tables/Cultures/" + culture + "/PeopleNames/Last_Names.txt";
                    if (File.Exists(lastNamesPath))
                    {
                        string[] lines_LastName = File.ReadAllLines(lastNamesPath);
                        int n_LastsFemale = r.Next(0, lines_LastName.Length);
                        int n_LastsMale = r.Next(0, lines_LastName.Length);

                        NPCNameTextbox.Text += lines_NameFemale[n_Females] + " " + lines_LastName[n_LastsFemale];
                        NPCNameTextbox.Text += Environment.NewLine + Environment.NewLine;
                        NPCNameTextbox.Text += lines_NameMale[n_Females] + " " + lines_LastName[n_LastsMale];                        
                    }
                    else
                    {
                        NPCNameTextbox.Text = lines_NameFemale[n_Females];
                        NPCNameTextbox.Text += Environment.NewLine + Environment.NewLine;
                        NPCNameTextbox.Text += lines_NameMale[n_Male];
                    }
                }
                else
                {
                    NPCNameTextbox.Text = "No names yet";
                }
            }
        }


        // ================================= Get Person =====================================

        private void GetPersonButton_Click(object sender, EventArgs e)
        {
            RegenerateAppearanceButton_Click(null, null);
            RegeneratePeronalityButton_Click(null, null);
            RegenerateBackstoryButton_Click(null, null);
        }

        private void RegenerateAppearanceButton_Click(object sender, EventArgs e)
        {
            PersonOutputPanel.BringToFront();

            string vowels = "aeiou";

            string a = "A [Gender] with [HairColor] [LengthofHair] [StyleofHair] hair, that [HairFall] their [ShapeofFace] and [FaceFeature] face. Their [EyePlacement], [EyeDescription], [EyeColor] eyes, [ActionofEyes] look at you. They sport {a} [FacialHairDescription] [FacialHairType], that smells like [FacialHairSmell]. {A} [ScarDescription] scar starts [ScarPlacement1] and ends [ScarPlacement2], it was acquired by [ScarAccident]. A birthmark resembling a [MarkDescription1] [Birthmark] rests [Placement1]. {A} tattoo of {a} [MarkDescription2] [Tattoo] rests [Placement2]. {A} [MarkDescription3], [TribalColor], [TribalMark] tribal design [Adverb] covers [TribalPlacement]. They are [Height] and [Weight] among the people of their culture. At {1-70} years old, they have {a} [SpeakingVoice] and [PhysicalTrait].";

            string gender = "";
            if (MaleCheck.Checked)
                gender = "man";
            else if (FemaleCheck.Checked)
                gender = "woman";
            else
                gender = RndString(new string[] { "man", "woman" });

            a = a.Replace("[Gender]", gender);

            string lengthofHair = RndString(File.ReadAllLines(workdir + "/Tables/Person/LengthOfHair.txt").ToArray());
            if (lengthofHair.StartsWith("bald") || lengthofHair.StartsWith("near shaved"))
            {
                a = a.Replace(" [HairColor]", "");
                a = a.Replace(" [StyleofHair]", "");
                a = a.Replace(" that [HairFall]", "");
                a = a.Replace("[LengthofHair]", lengthofHair);
            }
            else
            {
                a = a.Replace("[HairColor]", RndString(File.ReadAllLines(workdir + "/Tables/Person/HairColor.txt").ToArray()) + ",");
                a = a.Replace("[StyleofHair]", RndString(File.ReadAllLines(workdir + "/Tables/Person/StyleOfHair.txt").ToArray()));
                a = a.Replace("[LengthofHair]", lengthofHair + ",");
                a = a.Replace("[HairFall]", RndString(File.ReadAllLines(workdir + "/Tables/Person/HairFall.txt").ToArray()));
            }
            a = a.Replace("[ShapeofFace]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ShapeOfFace.txt").ToArray()));
            a = a.Replace("[FaceFeature]", RndString(File.ReadAllLines(workdir + "/Tables/Person/FaceFeature.txt").ToArray()));
            a = a.Replace("[EyePlacement]", RndString(File.ReadAllLines(workdir + "/Tables/Person/EyePlacement.txt").ToArray()));
            a = a.Replace("[EyeDescription]", RndString(File.ReadAllLines(workdir + "/Tables/Person/EyeDescription.txt").ToArray()));
            a = a.Replace("[ActionofEyes]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ActionOfEyes.txt").ToArray()));
            string eyeColor = RndString(File.ReadAllLines(workdir + "/Tables/Person/EyeColor.txt").ToArray());
            a = a.Replace("[EyeColor]", eyeColor);

            if (((!YoungCheck.Checked && gender != "woman") && (r.Next(0, 100) < 20 && !NoFacialHairCheck.Checked)) || ForceFacialHairCheck.Checked)
            {
                string facialHairDescription = RndString(File.ReadAllLines(workdir + "/Tables/Person/FacialHairDescription.txt").ToArray());
                if (vowels.Contains(facialHairDescription[0].ToString()))
                    a = a.Replace("They sport {a}", "They sport an");
                else
                    a = a.Replace("They sport {a}", "They sport a");

                a = a.Replace("[FacialHairDescription]", facialHairDescription);
                a = a.Replace("[FacialHairSmell]", RndString(File.ReadAllLines(workdir + "/Tables/Person/FacialHairSmell.txt").ToArray()));
                a = a.Replace("[FacialHairType]", RndString(File.ReadAllLines(workdir + "/Tables/Person/FacialHairType.txt").ToArray()));
            }
            else
                a = a.Replace("They sport {a} [FacialHairDescription] [FacialHairType], that smells like [FacialHairSmell]. ", "");

            a = a.Replace("[HairColor]", RndString(File.ReadAllLines(workdir + "/Tables/Person/HairColor.txt").ToArray()));
            a = a.Replace("[HairFall]", RndString(File.ReadAllLines(workdir + "/Tables/Person/HairFall.txt").ToArray()));
            a = a.Replace("[Height]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Height.txt").ToArray()));
            a = a.Replace("[LengthOfHair]", RndString(File.ReadAllLines(workdir + "/Tables/Person/LengthOfHair.txt").ToArray()));
            a = a.Replace("[ShapeOfFace]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ShapeOfFace.txt").ToArray()));
            a = a.Replace("[StyleOfHair]", RndString(File.ReadAllLines(workdir + "/Tables/Person/StyleOfHair.txt").ToArray()));
            a = a.Replace("[Weight]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Weight.txt").ToArray()));

            if (ForceScarCheck.Checked || (r.Next(0, 100) < 15 && !NoScarCheck.Checked))
            {
                string scarDescription = RndString(File.ReadAllLines(workdir + "/Tables/Person/ScarDescription.txt").ToArray());
                if (vowels.Contains(scarDescription[0].ToString()))
                    a = a.Replace("{A} [ScarDescription]", "An [ScarDescription]");
                else
                    a = a.Replace("{A} [ScarDescription]", "A [ScarDescription]");

                a = a.Replace("[ScarAccident]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ScarAccident.txt").ToArray()));
                a = a.Replace("[ScarDescription]", scarDescription);
                a = a.Replace("[ScarPlacement1]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ScarPlacement.txt").ToArray()));
                a = a.Replace("[ScarPlacement2]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ScarPlacement.txt").ToArray()));
            }
            else
            {
                a = a.Replace(" {A} [ScarDescription] scar starts [ScarPlacement1] and ends [ScarPlacement2], it was acquired by [ScarAccident].", "");
            }

            if (ForceBirthMarkCheck.Checked || (r.Next(0, 100) < 15 && !NoBirthMarkCheck.Checked))
            {
                string markDescription = RndString(File.ReadAllLines(workdir + "/Tables/Person/MarkDescription.txt").ToArray());

                a = a.Replace("[MarkDescription1]", markDescription);
                a = a.Replace("[Placement1]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Placement.txt").ToArray()));
                a = a.Replace("[Birthmark]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Birthmark.txt").ToArray()));
            }
            else
            {
                a = a.Replace(" A birthmark resembling a [MarkDescription1] [Birthmark] rests [Placement1].", "");
            }

            if (ForceTattooCheck.Checked || (r.Next(0, 100) < 15 && !NoTattooCheck.Checked))
            {
                string tattoDescription = RndString(File.ReadAllLines(workdir + "/Tables/Person/Tattoo.txt").ToArray());
                if (vowels.Contains(tattoDescription[0].ToString()))
                    a = a.Replace("{A} tattoo of", "An tattoo of");
                else
                    a = a.Replace("{A} tattoo of", "A tattoo of");

                string markDescription = RndString(File.ReadAllLines(workdir + "/Tables/Person/markDescription.txt").ToArray());
                if (vowels.Contains(markDescription[0].ToString()))
                    a = a.Replace("{a} [MarkDescription2]", "an [MarkDescription2]");
                else
                    a = a.Replace("{a} [MarkDescription2]", "a [MarkDescription2]");

                a = a.Replace("[Tattoo]", tattoDescription);
                a = a.Replace("[MarkDescription2]", markDescription);
                a = a.Replace("[Placement2]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Placement.txt").ToArray()));
            }
            else
            {
                a = a.Replace(" {A} tattoo of {a} [MarkDescription2] [Tattoo] rests [Placement2].", "");
            }


            if (ForceTribalCheck.Checked || (r.Next(0, 100) < 15 && !NoTribalCheck.Checked))
            {

                string markDescription3 = RndString(File.ReadAllLines(workdir + "/Tables/Person/markDescription.txt").ToArray());
                if (vowels.Contains(markDescription3[0].ToString()))
                    a = a.Replace("{A} [MarkDescription3]", "An [MarkDescription3]");
                else
                    a = a.Replace("{A} [MarkDescription3]", "A [MarkDescription3]");

                a = a.Replace("[MarkDescription3]", markDescription3);
                a = a.Replace("[TribalMark]", RndString(File.ReadAllLines(workdir + "/Tables/Person/TribalMark.txt").ToArray()));
                a = a.Replace("[TribalColor]", eyeColor);
                a = a.Replace("[TribalPlacement]", RndString(File.ReadAllLines(workdir + "/Tables/Person/TribalPlacement.txt").ToArray()));
                a = a.Replace("[Adverb]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adverb.txt").ToArray()));
            }
            else
            {
                a = a.Replace(" {A} [MarkDescription3], [TribalColor], [TribalMark] tribal design [Adverb] covers [TribalPlacement].", "");
            }

            string speakingVoice = RndString(File.ReadAllLines(workdir + "/Tables/Person/SpeakingVoice.txt").ToArray());
            if (vowels.Contains(speakingVoice[0].ToString()))
                a = a.Replace("{a} [SpeakingVoice]", "an [SpeakingVoice]");
            else
                a = a.Replace("{a} [SpeakingVoice]", "a [SpeakingVoice]");

            a = a.Replace("[SpeakingVoice]", speakingVoice);
            a = a.Replace("[PhysicalTrait]", RndString(File.ReadAllLines(workdir + "/Tables/Person/PhysicalTrait.txt").ToArray()));

            SetAge();
            a = a.Replace("At {1-70} years old", "At " + age + " years old");

            Appearance.Text = a;
            if (BackStory.Text.Length > 20)
            {
                string[] words = BackStory.Text.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "turned" && words[i - 1] == "they" && words[i - 2] == "when")
                    {
                        int lifeEventAge = r.Next(1, age);
                        words[i + 1] = lifeEventAge + ".";
                    }
                    if (words[i] == "at" && words[i - 1] == "age" && words[i - 2] == "old")
                    {
                        int deathAge = r.Next(age, 101);
                        words[i + 1] = deathAge.ToString();
                    }
                }
                BackStory.Text = string.Join(" ", words);
            }
        }

        void SetAge()
        {
            if (YoungCheck.Checked)
            {
                age = r.Next(8, 21);
            }
            else if (MiddleAgeCheck.Checked)
            {
                age = r.Next(21, 51);
            }
            else if (OldCheck.Checked)
            {
                age = r.Next(51, 101);
            }
            else
            {
                age = r.Next(8, 101);
            }
        }

        private void RegeneratePeronalityButton_Click(object sender, EventArgs e)
        {
            PersonOutputPanel.BringToFront();

            string a = "They are [Adjective1], [Adjective2], [Adjective3], and maybe a little too [Adjective4]. They are motivated by [Motives1], and [Motives2]. They also [PersonalityTrait]." 
                        + Environment.NewLine + Environment.NewLine +
                        "They dislike people who are [Adjective5], [Adjective6], and [Adjective7], and despise those motivated by [Motives3]."
                        + Environment.NewLine + Environment.NewLine + 
                        "Regardless, most people tend to [ActionofOthers] while [SecretActionofOthers]. At their worst, they display signs of [BehaviorTrait].";

            a = a.Replace("[Adjective1]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));
            a = a.Replace("[Adjective2]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));
            a = a.Replace("[Adjective3]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));
            a = a.Replace("[Adjective4]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));
            a = a.Replace("[Adjective5]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));
            a = a.Replace("[Adjective6]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));
            a = a.Replace("[Adjective7]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray()));

            a = a.Replace("[Motives1]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Motives.txt").ToArray()));
            a = a.Replace("[Motives2]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Motives.txt").ToArray()));
            a = a.Replace("[Motives3]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Motives.txt").ToArray()));


            a = a.Replace("[PersonalityTrait]", RndString(File.ReadAllLines(workdir + "/Tables/Person/PersonalityTrait.txt").ToArray()));


            a = a.Replace("[ActionofOthers]", RndString(File.ReadAllLines(workdir + "/Tables/Person/ActionofOthers.txt").ToArray()));
            a = a.Replace("[SecretActionofOthers]", RndString(File.ReadAllLines(workdir + "/Tables/Person/SecretActionofOthers.txt").ToArray()));
            a = a.Replace("[BehaviorTrait]", RndString(File.ReadAllLines(workdir + "/Tables/Person/BehaviorTrait.txt").ToArray()));

            Peronality.Text = a;

        }

        private void RegenerateBackstoryButton_Click(object sender, EventArgs e)
        {
            PersonOutputPanel.BringToFront();

            string vowels = "aeiou";

            string a = "They were born in a [WhereTheyWereBorn] where they lived [HowTheyLived] alongside {a} [Adjective] [TypeofFamily]. [Until] when they [LifeChange] (if tragic event: [TragicEvent])(if somebody: [Somebody]), when they turned {1-70}." + 
                       Environment.NewLine + Environment.NewLine + 
                       "Since then they have [LifeAction], [Transition], [Conclusion]. Currently they are [CurrentAction], while hoping to [Attempt]. In the meantime they work as {a} [Career] where they [CareerDescription]." +
                       Environment.NewLine + Environment.NewLine + 
                       "Everyone has their secrets, and they are no excpetion, [SecretStrength] [Secret1], and [Secret2]. They will die of old age at {50-95} in the [Season].";

            a = a.Replace("[WhereTheyWereBorn]", RndString(File.ReadAllLines(workdir + "/Tables/Person/WhereTheyWereBorn.txt").ToArray()));
            a = a.Replace("[HowTheyLived]", RndString(File.ReadAllLines(workdir + "/Tables/Person/HowTheyLived.txt").ToArray()));


            string adjective = RndString(File.ReadAllLines(workdir + "/Tables/Person/Adjective.txt").ToArray());
            if (vowels.Contains(adjective[0].ToString()))
                a = a.Replace("{a} [Adjective]", "an [Adjective]");
            else
                a = a.Replace("{a} [Adjective]", "a [Adjective]");
            a = a.Replace("[Adjective]", adjective);

            a = a.Replace("[TypeofFamily]", RndString(File.ReadAllLines(workdir + "/Tables/Person/TypeofFamily.txt").ToArray()));
            a = a.Replace("[Until]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Until.txt").ToArray()));
            string lifeChange = RndString(File.ReadAllLines(workdir + "/Tables/Person/LifeChange.txt").ToArray());
            a = a.Replace("[LifeChange]", lifeChange);
            if (lifeChange.Contains("(tragic event)"))
            {
                a = a.Replace("(tragic event)", "");
                a = a.Replace("(if tragic event: [TragicEvent])", RndString(File.ReadAllLines(workdir + "/Tables/Person/TragicEvent.txt").ToArray()));
            }
            else
            {
                a = a.Replace("(if tragic event: [TragicEvent])", "");
            }

            if (lifeChange.Contains("somebody"))
            {
                a = a.Replace("(if somebody: [Somebody])", RndString(File.ReadAllLines(workdir + "/Tables/Person/Somebody.txt").ToArray()));
                a = a.Replace("somebody", "");
            }
            else
            {
                a = a.Replace("(if somebody: [Somebody])", "");
            }

            if (age == 0)
                SetAge();

            a = a.Replace("{1-70}", r.Next(1, age).ToString());

            a = a.Replace("[LifeAction]", RndString(File.ReadAllLines(workdir + "/Tables/Person/LifeAction.txt").ToArray()));
            a = a.Replace("[Transition]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Transition.txt").ToArray()));
            a = a.Replace("[Conclusion]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Conclusion.txt").ToArray()));
            a = a.Replace("[CurrentAction]", RndString(File.ReadAllLines(workdir + "/Tables/Person/CurrentAction.txt").ToArray()));
            a = a.Replace("[Attempt]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Attempt.txt").ToArray()));
            a = a.Replace("[Attempt]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Attempt.txt").ToArray()));

            string career = RndString(File.ReadAllLines(workdir + "/Tables/Person/Career.txt").ToArray());
            if (vowels.Contains(career[0].ToString()))
                a = a.Replace("{a} [Career]", "an [Career]");
            else
                a = a.Replace("{a} [Career]", "a [Career]");
            a = a.Replace("[Career]", career);

            a = a.Replace("[CareerDescription]", RndString(File.ReadAllLines(workdir + "/Tables/Person/CareerDescription.txt").ToArray()));
            a = a.Replace("[SecretStrength]", RndString(File.ReadAllLines(workdir + "/Tables/Person/SecretStrength.txt").ToArray()));
            a = a.Replace("[Secret1]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Secret.txt").ToArray()));
            a = a.Replace("[Secret2]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Secret.txt").ToArray()));
            a = a.Replace("[Season]", RndString(File.ReadAllLines(workdir + "/Tables/Person/Season.txt").ToArray()));
            int minOldAge = age;
            if (age < 50)
                minOldAge = 50;
            a = a.Replace("{50-95}", r.Next(minOldAge, 101).ToString());

            BackStory.Text = a;
        }

        string RndString(string[] l)
        {
            int i = r.Next(0, l.Length);
            return l[i];
        }

        private void MaleCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (MaleCheck.Checked) FemaleCheck.Checked = false;
        }
        private void FemaleCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (FemaleCheck.Checked) MaleCheck.Checked = false;
        }

        private void ForceFacialHairCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceFacialHairCheck.Checked) NoFacialHairCheck.Checked = false;
        }
        private void NoFacialHairCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (NoFacialHairCheck.Checked) ForceFacialHairCheck.Checked = false;
        }

        private void ForceScarCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceScarCheck.Checked) NoScarCheck.Checked = false;
        }
        private void NoScarCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (NoScarCheck.Checked) ForceScarCheck.Checked = false;
        }

        private void ForceBirthMarkCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceBirthMarkCheck.Checked) NoBirthMarkCheck.Checked = false;
        }
        private void NoBirthMarkCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (NoBirthMarkCheck.Checked) ForceBirthMarkCheck.Checked = false;
        }

        private void YoungCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (YoungCheck.Checked)
            {
                MiddleAgeCheck.Checked = false;
                OldCheck.Checked = false;
            }
        }
        private void MiddleAgeCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (MiddleAgeCheck.Checked)
            {
                YoungCheck.Checked = false;
                OldCheck.Checked = false;
            }
        }
        private void OldCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (OldCheck.Checked)
            {
                YoungCheck.Checked = false;
                MiddleAgeCheck.Checked = false;
            }
        }

        private void ForceTattooCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceTattooCheck.Checked) NoTattooCheck.Checked = false;
        }
        private void NoTattooCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (NoTattooCheck.Checked) ForceTattooCheck.Checked = false;
        }

        private void ForceTribalCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceTribalCheck.Checked) NoTribalCheck.Checked = false;
        }
        private void NoTribalCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (NoTribalCheck.Checked) ForceTribalCheck.Checked = false;
        }

        private void RandomizePersonButton_Click(object sender, EventArgs e)
        {
            int genderChoice = r.Next(0, 2);
            if (genderChoice == 0) MaleCheck.Checked = true;
            else if (genderChoice == 1) FemaleCheck.Checked = true;

            int hairChoice = r.Next(0, 2);
            if (hairChoice == 0) ForceFacialHairCheck.Checked = true;
            else if (hairChoice == 1) NoFacialHairCheck.Checked = true;

            int scarChoice = r.Next(0, 2);
            if (scarChoice == 0) ForceScarCheck.Checked = true;
            else if (scarChoice == 1) NoScarCheck.Checked = true;

            int birthmarkChoice = r.Next(0, 2);
            if (birthmarkChoice == 0) ForceBirthMarkCheck.Checked = true;
            else if (birthmarkChoice == 1) NoBirthMarkCheck.Checked = true;

            int ageChoice = r.Next(0, 3);
            if (ageChoice == 0) YoungCheck.Checked = true;
            else if (ageChoice == 1) MiddleAgeCheck.Checked = true;
            else if (ageChoice == 2) OldCheck.Checked = true;

            int tattoChoice = r.Next(0, 2);
            if (tattoChoice == 0) ForceTattooCheck.Checked = true;
            else if (tattoChoice == 1) NoTattooCheck.Checked = true;

            int tribalMarkChoice = r.Next(0, 2);
            if (tribalMarkChoice == 0) ForceTribalCheck.Checked = true;
            else if (tribalMarkChoice == 1) NoTribalCheck.Checked = true;
        }

        // =============================== Tavern Page ===================================
        string Generator(string text, string table)
        {
            string[] words = text.Split(' ');
            string vowels = "aeiou";
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].StartsWith("["))
                {
                    string[] stuff = words[i].Split('[')[1].Split(']');
                    string keyword = stuff[0];
                    string filePath = workdir + table + "/" + keyword + ".txt";
                    words[i] = RndString(File.ReadAllLines(filePath).ToArray());
                    if (stuff.Length > 1) words[i] += stuff[1];
                }
            }
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "{a}")
                {
                    if (vowels.Contains(words[i + 1][0].ToString()))
                        words[i] = "an";
                    else
                        words[i] = "a";
                }
            }
            return string.Join(" ", words);
        }

        private void TavernDescriptionButton_Click(object sender, EventArgs e)
        {
            string text = "Welcome to the [TavernNameFront] [TavernNameEnd], {a} [Adjective] <Stories> [TavernNameType] made of [Adjective] [MadeOf]. It has {a} [Reputation], and is known for its [KnownForDescription] [KnownForObject].";
            TavernDescription.Text = Generator(text, "/Tables/" + "Tavern");
            string[] story = { "single-story", "two-story", "three-story" };
            int stories = r.Next(0, 3);
            TavernDescription.Text = TavernDescription.Text.Replace("<Stories>", story[stories]);

            TavernDescription.Text += Environment.NewLine + Environment.NewLine;

            stories += 1;
            int totalRooms = 0;
            for(int s = 0; s < stories; s++)
                totalRooms += r.Next(1, 16);
            int rentedRooms = r.Next(0, totalRooms + 1);
            int capacity = r.Next(0, 101);
            if(r.Next(0,100)<20)
                capacity += r.Next(1, 101);
            int availableRooms = r.Next(0, totalRooms);
            string finish = "Tonight the tavern hall is at " + capacity + "% capacity. " + rentedRooms + " of the tavern's " + totalRooms + " rooms are rented.";
            TavernDescription.Text += finish;

            int nbrOfPoorRooms = 0;
            int nbrOfCommonRooms = 0;
            int nbrOfGoodRooms = 0;
            int nbrOfSmallSuites = 0;
            int nbrOfAverageSuites = 0;
            int nbrOfLuxuriousSuites = 0;

            for(int i = 0; i < totalRooms - rentedRooms; i++)
            {
                int p = r.Next(0, 100);
                if (p < 40)
                    nbrOfPoorRooms++;
                else if (p < 60)
                    nbrOfCommonRooms++;
                else if (p < 75)
                    nbrOfGoodRooms++;
                else if (p < 87)
                    nbrOfSmallSuites++;
                else if (p < 95)
                    nbrOfAverageSuites++;
                else
                    nbrOfLuxuriousSuites++;
            }

            string rooms = "";
            if (nbrOfPoorRooms > 0)
                rooms += nbrOfPoorRooms + " " + "Poor Room" + s(nbrOfPoorRooms) + " available for " + priceRange(20) + " copper per night" + Environment.NewLine;
            if (nbrOfCommonRooms > 0)
                rooms += nbrOfCommonRooms + " " + "Common Room" + s(nbrOfCommonRooms) + " available for " + priceRange(50) + " copper per night" + Environment.NewLine;
            if (nbrOfGoodRooms > 0)
                rooms += nbrOfGoodRooms + " " + "Good Room" + s(nbrOfCommonRooms) + " available for " + priceRange(100) + " copper per night" + Environment.NewLine;
            if (nbrOfSmallSuites > 0)
                rooms += nbrOfSmallSuites + " " + "Small Suite" + s(nbrOfCommonRooms) + " available for " + priceRange(400) + " copper per night" + Environment.NewLine;
            if (nbrOfAverageSuites > 0)
                rooms += nbrOfAverageSuites + " " + "Average Suite" + s(nbrOfCommonRooms) + " available for " + priceRange(1600) + " copper per night" + Environment.NewLine;
            if (nbrOfLuxuriousSuites > 0)
                rooms += nbrOfLuxuriousSuites + " " + "Luxurious Suite" + s(nbrOfCommonRooms) + " available for " + priceRange(32000) + " copper per night" + Environment.NewLine;

            TavernDescription.Text += Environment.NewLine + Environment.NewLine + rooms;
        }

        string s(int nr)
        {
            if (nr > 1)
                return "s";
            else
                return "";
        }

        string priceRange(int price)
        {
            return (price * r.Next(50, 150) / 100).ToString();
        }

        private void TavernEventButton_Click(object sender, EventArgs e)
        {
            TavernEvent.Text = "Tonight in the Tavern there is: ";
            string filePath = workdir + "/Tables/Tavern/TavernEvent.txt";
            TavernEvent.Text += Environment.NewLine + Environment.NewLine;
            TavernEvent.Text += RndString(File.ReadAllLines(filePath).ToArray());

        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            string tavernName = "The Prancing Pony";
            if(TavernDescription.Text.Length > 1)
            {
                string[] words = TavernDescription.Text.Split(' ');
                tavernName = "The " + words[3] + " " + words[4].Replace(",","");
            }
            MenuTitle.Text = tavernName;

            Meal1.Text = "";
            string filePath = workdir + "/Tables/Tavern/TavernMenu.txt";

            Meal1.Text += RndString(File.ReadAllLines(filePath).ToArray());
            Meal1.Text += " for " + r.Next(1, 51) + " copper,";
            Meal1.Text += Environment.NewLine;

            Meal1.Text += RndString(File.ReadAllLines(filePath).ToArray());
            Meal1.Text += " for " + r.Next(1, 51) + " copper,";
            Meal1.Text += Environment.NewLine;

            Meal1.Text += RndString(File.ReadAllLines(filePath).ToArray());
            Meal1.Text += " for " + r.Next(1, 51) + " copper";
            Meal1.Text += Environment.NewLine +  "~" + Environment.NewLine ;

            filePath = workdir + "/Tables/Tavern/DrinksServed.txt";
            Meal1.Text += RndString(File.ReadAllLines(filePath).ToArray());
            Meal1.Text += " for " + r.Next(1, 51) + " copper,";
            Meal1.Text += Environment.NewLine;

            Meal1.Text += RndString(File.ReadAllLines(filePath).ToArray());
            Meal1.Text += " for " + r.Next(1, 51) + " copper,";
            Meal1.Text += Environment.NewLine;
        }
        
        private void NewTavernButton_Click(object sender, EventArgs e)
        {
            TavernDescriptionButton_Click(null, null);
            TavernEventButton_Click(null, null);
            MenuButton_Click(null, null);
        }

        // =============================== Misc =============================================

        private void NewSong_Click(object sender, EventArgs e)
        {
            string culture = SongCultureDropDown.SelectedItem.ToString();
            if (SongCultureDropDown.SelectedIndex == 0)
                Song.Text = "Select Culture..";
            else
            {
                string file = workdir + "/Tables/Cultures/" + culture + "/Songs.txt";
                if (File.Exists(file))
                    Song.Text = RndString(File.ReadAllLines(file).ToArray());
                else
                    Song.Text = "This Culture has no songs yet";
            }
        }
               
        private void NewRumorButton_Click(object sender, EventArgs e)
        {
            string culture = RumorCultureDropDown.SelectedItem.ToString();
            if (RumorCultureDropDown.SelectedIndex == 0)
                Rumor.Text = "Select Culture..";
            else
            {
                string file = workdir + "/Tables/Cultures/" + culture + "/Rumors.txt";
                if (File.Exists(file))
                    Rumor.Text = RndString(File.ReadAllLines(file).ToArray());
                else
                    Rumor.Text = "This Culture has no rumors yet";
            }
        }

        private void ActivityButton_Click(object sender, EventArgs e)
        {
            Activity.Text = RndString(File.ReadAllLines(workdir + "/Tables/Misc/Activities.txt").ToArray());
        }

        private void DownTimeEventButton_Click(object sender, EventArgs e)
        {
            DowntimeEvent.Text = DownTimeEvent();
        }

        string DownTimeEvent()
        {
            string[] events = { "In the midst of downtime someone dares you to: [Dares]",
                                "When trying to speak to a peasant, they say to you: [Talk]",
                                "While in downtime, the following event occurs: [Event]",
                                "While visiting a new town the following Circus Act is performing: [CircusActs] and costs CIRCUS_COST per ticket.",
                                "While visiting a new town the players learn the following local legend: [legend]",
                                "While visiting a new town the players learn that everyone is talking about how: [When], [FirstName] [SurName], the local[Local], [Action] [Temperment] [Subject] [Why]",
                                "While visiting a new town a merchant approaches the players and tries to sell them a: [Item] for MERCHANT_COST gold",
                                "While visiting a new town the players hear the following rumor: [Person] [Occurance] [Resolution] Then again I heard that from [Teller]"
                               };
            int l = events.Length;
            string a = events[r.Next(0, l)];
            a = a.Replace("CIRCUS_COST", r.Next(1, 50).ToString());
            a = a.Replace("MERCHANT_COST", r.Next(1, 200).ToString());
            return Generator(a, "/Tables/" + "Misc");
        }

        // ================================= encounter ======================================

        private void ForageZoneDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string zone = (string)cb.SelectedItem;
            string path = workdir + "/Tables/Zones/" + zone + "/foraging.txt";
            ForageTerrainDropDown.Items.Clear();
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach(string line in lines)
                {
                    if (line.Contains(":"))
                    {
                        ForageTerrainDropDown.Items.Add(line.Split(':')[0]);
                    }
                }
                if (ForageTerrainDropDown.Items.Count > 1)
                    ForageLabel.Text = "";
            }
            else if (ForageZoneDropDown.SelectedIndex != 0)
                ForageLabel.Text = "No foraging here yet";
        }
                
        private void EncounterZoneDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string zone = (string)cb.SelectedItem;
            string path = workdir + "/Tables/Zones/" + zone + "/encounter.txt";
            EncounterTerrainDropDown.Items.Clear();
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (line.Contains(":"))
                    {
                        EncounterTerrainDropDown.Items.Add(line.Split(':')[0]);
                    }
                }
                if(EncounterTerrainDropDown.Items.Count > 1)
                    EncounterOutput.Text = "";
            }
            else if(EncounterZoneDropDown.SelectedIndex != 0)
                EncounterOutput.Text = "No encounters set up here";
        }

        private void ForageTerrainDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Forage();
        }
        
        private void ForageButton_Click(object sender, EventArgs e)
        {
            Forage();
        }

        void Forage()
        {
            string zone = (string)ForageZoneDropDown.SelectedItem;
            string terrain = (string)ForageTerrainDropDown.SelectedItem;
            string path = workdir + "/Tables/Zones/" + zone + "/foraging.txt";
            string items = "";
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (line.Contains(":"))
                    {
                        if (line.Split(':')[0] == terrain)
                            items = line.Split(':')[1];
                    }
                }
                string[] itemList = items.Split(',');
                ForageLabel.Text = itemList[r.Next(0, itemList.Length)];
            }
            else
                ForageLabel.Text = "No foraging here yet";
        }
               
        private void ForageSpecialButton_Click(object sender, EventArgs e)
        {
            string terrainSelect = (string) ForageSpecialDropDown.SelectedItem;
            if (terrainSelect != null)
            {
                string path = workdir + "/Tables/Zones/Special_Resources.txt";
                int rarness = r.Next(0, 100);
                string rarity = "";
                if (rarness < 60)
                    rarity = "Common";
                else if (rarness < 60 + 25)
                    rarity = "Uncommon";
                else if (rarness < 60 + 25 + 10)
                    rarity = "Rare";
                else // if (rarness < 60 + 25 + 10 + 5)
                    rarity = "Very Rare";

                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    if (line.Contains(":"))
                    {
                        string terrainAndRarity = line.Split(':')[0];
                        if(terrainAndRarity.Contains(terrainSelect) && terrainAndRarity.Contains(rarity))
                        {
                            string[] items = line.Split(':')[1].Split(',');
                            ForageLabel.Text = items[r.Next(0,items.Length)];
                        }
                    }
                }
            }
            else
                ForageLabel.Text = "Select Terrain";

        }

        // ================================ custom stuff ====================================

        void ScanGeneratorTabs()
        {
            string[] tabs = Directory.GetDirectories(workdir + "/Custom");
            foreach(string t in tabs)
            {
                string tab = Path.GetFileName(t);
                TabPage tp = new TabPage(tab);
                tp.Name = tab;
                string[] generators = Directory.GetDirectories(workdir + "/Custom/" + tab);
                int n = 0;
                foreach (string g in generators)
                {
                    n++;
                    tp.BackColor = Color.White;
                    Panel p = new Panel();                    
                    string generator = Path.GetFileName(g);
                    p.Name = generator;
                    if (generators.Length == 1)
                        p.Size = new Size(1104, 726);
                    else
                    {
                        int w = 550;
                        int h = 340;
                        p.Size = new Size(w, h);
                        if (n == 1)
                            p.Location = new Point(0, 0);
                        if (n == 2)
                            p.Location = new Point(w + 2, 0);
                        if (n == 3)
                            p.Location = new Point(0, h + 2);
                        if (n == 4)
                            p.Location = new Point(w + 2, h + 2);
                    }

                    Button b = new Button();
                    b.Text = generator;
                    b.Width = p.Width - (generators.Length == 1 ? 8 : 4);
                    b.Height = 60;
                    
                    b.Click += GeneratorButtonClick;

                    p.Controls.Add(b);

                    //Label label = new Label();
                    TextBox label = new TextBox();
                    label.Multiline = true;
                    label.ScrollBars = ScrollBars.Vertical;
                    label.Width = b.Width;
                    label.Height = p.Height - b.Height - 4;
                    label.Location = new Point(0, b.Height);
                    label.Text = SingleWordGenerator(g);

                    p.Controls.Add(label);
                    p.BackColor = Color.White;
                    tp.Controls.Add(p);
                }
                tabControl1.TabPages.Add(tp);
            }
        }

        string SingleWordGenerator(string path)
        {
            string[] files = Directory.GetFiles(path);
            string s = "";
            for(int i = 0; i < files.Length; i++)
            {
                if (i > 0) s += Environment.NewLine;
                s += RndString(File.ReadAllLines(files[i]).ToArray());
            }
            return s;
        }

        void GeneratorButtonClick(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            Panel p = (Panel)b.Parent;
            TabPage tab = (TabPage)p.Parent;
            //Label l = (Label)b.Parent.GetChildAtPoint(new Point(0, b.Height));
            TextBox l = (TextBox)b.Parent.GetChildAtPoint(new Point(0, b.Height));
            string path = workdir + "/Custom/" + tab.Name + "/" + p.Name;
            if (File.Exists(path + "/generator.txt"))
                l.Text = Generator(File.ReadAllText(path + "/generator.txt"), "/Custom/" + tab.Name + "/" + p.Name);
            else
                l.Text = SingleWordGenerator(path);
        }

        // ============================= Random encounters ====================================

        void InitiateClimaticZoneDropDown()
        {
            string[] zones = { "Desert", "Tropical savanna", "Steppe", "Equatorial", "Monsoon", "Warm and rainy", "Warm with dry summer", "Warm with dry winter", "Cool and rainy", "Cool with dry winter", "Tundra/polar" };

            string[] latitudTable = {
                "Desert 0–30°",
                "Tropical Savanna 0–20°",
                "Steppes 20–50°",
                "Equatorial 0–20°",
                "Monsoon 0–20°",
                "Warm and Rainy 20–50°",
                "Warm with Dry Summer 20–50°",
                "Warm with Dry Winter 20–40°",
                "Cool and Rainy 40–70°",
                "Cool with Dry Winter 50–70°",
                "Tundra 70°+",
                "Polar 70°+" };

            ClimaticZoneDropDown.Items.Clear();
            ClimaticZoneDropDown.Items.Add("Climat Zone... ");
            ClimaticZoneDropDown.Items.AddRange(latitudTable);
            ClimaticZoneDropDown.SelectedIndex = 0;
        }

        void InitiateCalendar()
        {
            string[] latitudes = { "0-10°", "11-20°", "21-30°", "31-40°", "41-50°", "51-60°", "61-70°", "70-90°" };

            latitudeDropDown.Items.Clear();
            latitudeDropDown.Items.Add("Lat. ");
            latitudeDropDown.Items.AddRange(latitudes);
            latitudeDropDown.SelectedIndex = 0;
            

            if (EncounterCultureDropDown.SelectedIndex != 0)
            {
                string culture = EncounterCultureDropDown.SelectedItem.ToString();
                string calendarPath = workdir + "/Tables/Cultures/" + culture + "/calendar.txt";
                if (File.Exists(calendarPath))
                {
                    string[] lines = File.ReadAllLines(calendarPath);
                    List<string> months = new List<string>();
                    daysPerMonth = 30;
                    foreach (string line in lines)
                    {
                        if (line.Contains("DAYS_PER_MONTH"))
                        {
                            string[] parts = line.Split(':');
                            daysPerMonth = int.Parse(parts[1]);
                        }
                        else
                            months.Add(line);
                    }

                    List<string> days = new List<string>();
                    for (int i = 1; i < daysPerMonth + 1; i++) days.Add(i.ToString());

                    daysPerYear = (months.ToArray().Length * daysPerMonth);
                    daysPerHalfYear = (months.ToArray().Length * daysPerMonth) / 2;

                    MonthDropDown.Items.Clear();
                    MonthDropDown.Items.Add("Month..");
                    MonthDropDown.Items.AddRange(months.ToArray());
                    MonthDropDown.SelectedIndex = 0;

                    DaysDropDown.Items.Clear();
                    DaysDropDown.Items.Add("Day..");
                    DaysDropDown.Items.AddRange(days.ToArray());
                    DaysDropDown.SelectedIndex = 0;
                }
                else
                    EncounterOutput.Text = "Bad Culture";
            }
        }

        void InitiateEncounter()
        {
            ForceEncounterDropDown.Items.Clear();
            ForceEncounterDropDown.Items.Add("Force Creature");
            ForceEncounterDropDown.Items.Add("Force Downtime");
            ForceEncounterDropDown.Items.Add("Force Feywild");
            ForceEncounterDropDown.Items.Add("Force Patrol");
            ForceEncounterDropDown.Items.Add("Force Roadside");
            ForceEncounterDropDown.Items.Add("Force Settlement");
            ForceEncounterDropDown.Items.Add("Force Special");
            ForceEncounterDropDown.Items.Add("Force Wilderness");
            ForceEncounterDropDown.Items.Add("Force Zone Specific");
            ForceEncounterDropDown.Items.Add("Non-Forced Random");
            ForceEncounterDropDown.Items.Add("Force Random");
            ForceEncounterDropDown.SelectedIndex = 0;
        }

        private void EncounterCultureDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitiateCalendar();
        }

        private void GetWeatherButton_Click(object sender, EventArgs e)
        {
            if (latitudeDropDown.SelectedIndex == 0)
                EncounterOutput.Text = "Choose Latitude...";
            else if (MonthDropDown.Items.Count < 2)
                EncounterOutput.Text = "Choose Cuture!";
            else if (MonthDropDown.SelectedIndex == 0)
                EncounterOutput.Text = "Choose Month!";
            else if (DaysDropDown.SelectedIndex == 0)
                EncounterOutput.Text = "Choose Day!";
            else if (ClimaticZoneDropDown.SelectedIndex == 0)
                EncounterOutput.Text = "Choose Climate Zone!";
            else
            {
                double[] summer_solstice_sunrises = { 6.0, 5.5, 5.0, 4.5, 4.0, 3.0, 0.5, 0.0 };
                double[] summer_solstice_sunsets = { 18.0, 18.5, 19.0, 19.5, 20.0, 21.0, 23.5, 24.0 };
                double[] winter_solstice_sunrises = { 6.0, 6.5, 7.0, 7.5, 8.0, 9.0, 11.5, 13 };
                double[] winter_solstice_sunsets = { 18.0, 17.5, 17.0, 16.5, 16.0, 15.0, 12.5, 13 };

                int lat = latitudeDropDown.SelectedIndex - 1;
                double summer_solstice_sunrise = summer_solstice_sunrises[lat];
                double summer_solstice_sunset = summer_solstice_sunsets[lat];
                double winter_solstice_sunrise = winter_solstice_sunrises[lat];
                double winter_solstice_sunset = winter_solstice_sunsets[lat];

                int dayOfYear = (MonthDropDown.SelectedIndex - 1) * daysPerMonth + DaysDropDown.SelectedIndex;
                int dayFromSummerSolstice = Math.Abs(dayOfYear - daysPerHalfYear);
                double winteryness = ((double)dayFromSummerSolstice) / daysPerHalfYear;

                string sunriseEnding = "am";
                string sunsetEnding = "am";

                double sunriseMilitary = Lerp(summer_solstice_sunrise, winter_solstice_sunrise, winteryness);
                int sunrise = (int)sunriseMilitary;
                int sunriseMinutes = (int)((sunriseMilitary - sunrise) * 60);

                if (sunrise == 12)
                    sunriseEnding = "pm";
                if (sunriseMilitary >= 13)
                {
                    sunrise -= 12;
                    sunriseEnding = "pm";
                }

                double sunsetMilitary = Lerp(summer_solstice_sunset, winter_solstice_sunset, winteryness);
                int sunset = (int)sunsetMilitary;
                int sunsetMinutes = (int)((sunsetMilitary - sunset) * 60);

                if (sunset == 12)
                    sunsetEnding = "pm";
                if (sunsetMilitary >= 13)
                {
                    sunset -= 12;
                    sunsetEnding = "pm";
                }

                string sunriseZero = "";
                if (sunriseMinutes < 10)
                    sunriseZero = "0";

                string sunsetZero = "";
                if (sunsetMinutes < 10)
                    sunsetZero = "0";

                int chanceOfRain = 0;
                double m = 0.0833; //month fraction

                double fractionOfYear = dayOfYear / (double)daysPerYear;

                double Jan = 1.0; double Feb = 2.0; double Mar = 3.0; double Apr = 4.0;
                double May = 5.0; double Jun = 6.0; double Jul = 7.0; double Aug = 8.0;
                double Sep = 9.0; double Oct = 10.0; double Nov = 11.0; double Dec = 12.0;

                string zone = ClimaticZoneDropDown.SelectedItem.ToString();
                if (zone.StartsWith("Desert"))
                    chanceOfRain = 5;
                else if (zone.StartsWith("Tropical Savanna"))
                {
                    if (Apr * m < fractionOfYear && fractionOfYear < Sep * m) chanceOfRain = 85;
                    else if (Oct * m < fractionOfYear || fractionOfYear < Mar * m) chanceOfRain = 10;
                    else chanceOfRain = 35;
                }
                else if (zone.StartsWith("Steppe"))
                {
                    if (Jun * m < fractionOfYear && fractionOfYear < Sep * m) chanceOfRain = 5;
                    else chanceOfRain = 20;
                }
                else if (zone.StartsWith("Equatorial"))
                {
                    if (Feb * m < fractionOfYear && fractionOfYear < May * m) chanceOfRain = 50;
                    else if (May * m < fractionOfYear && fractionOfYear < Dec * m) chanceOfRain = 40;
                    else chanceOfRain = 30;
                }
                else if (zone.StartsWith("Monsoon"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Oct * m) chanceOfRain = 90;
                    else chanceOfRain = 15;
                }
                else if (zone.StartsWith("Warm and Rainy")) chanceOfRain = 40;
                else if (zone.StartsWith("Warm with Dry Summer"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Aug * m) chanceOfRain = 10;
                    else chanceOfRain = 30;
                }
                else if (zone.StartsWith("Warm with Dry Winter"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Aug * m) chanceOfRain = 45;
                    else chanceOfRain = 15;
                }
                else if (zone.StartsWith("Cool and Rainy")) chanceOfRain = 35;
                else if (zone.StartsWith("Cool with Dry Winter"))
                {
                    if (Jun * m < fractionOfYear && fractionOfYear < Sep * m) chanceOfRain = 35;
                    else if (Oct * m < fractionOfYear || fractionOfYear < Apr * m) chanceOfRain = 10;
                    else chanceOfRain = 20;
                }
                else if (zone.StartsWith("Polar") || zone.StartsWith("Tundra")) chanceOfRain = 10;

                bool isRain = r.Next(0, 100) < chanceOfRain;
                string rain = "no rain";
                if (isRain) rain = "rain";
                string precipitation = "-";

                string cloudType = "Not a cloud on the sky";
                int d3 = r.Next(1,4);
                if (d3 == 1) cloudType = "Not a cloud in the sky";
                if (d3 == 2) cloudType = "A few clouds";
                if (d3 == 3) cloudType = "Mostly cloudy";

                int d6 = r.Next(1, 7);
                if (isRain)
                {
                    if (d6 == 1) { precipitation = "Light mist/few flakes"; cloudType = "A few clouds"; }
                    if (d6 == 2) { precipitation = "Drizzle/dusting"; cloudType = "Mostly cloudy"; }
                    if (d6 == 3) { precipitation = "Steady rainfall/flurries"; cloudType = "Gray, slightly overcast"; }
                    if (d6 == 4) { precipitation = "Strong rainfall/moderate snowfall"; cloudType = "Gray, highly overcast"; }
                    if (d6 == 5) { precipitation = " Pounding rain/heavy snowfall"; cloudType = "Dark storm clouds"; }
                    if (d6 == 6) { precipitation = " Downpour/blizzard"; cloudType = "Dark storm clouds"; }
                }

                string windType = "";
                string windSpeed = "";
                string windDescription = "";

                int windspeedNumber = 0;
                if (d6 == 1) { windspeedNumber = r.Next(1, 5); }
                if (d6 == 2) { windspeedNumber = r.Next(1, 7); }
                if (d6 == 3) { windspeedNumber = r.Next(1, 5) + r.Next(1, 5); }
                if (d6 == 4) { windspeedNumber = r.Next(1, 7) + r.Next(1, 7); }
                if (d6 == 5) { windspeedNumber = r.Next(1, 9) + r.Next(1, 9); }
                if (d6 == 6) { windspeedNumber = r.Next(1, 11) + r.Next(1, 11); }

                if (windspeedNumber == 1) { windType = "Calm"; windSpeed = "Less than 1"; windDescription = "Smoke rises vertically"; }
                else if (windspeedNumber <= 3) { windType = "Light air"; windSpeed = "1-3"; windDescription = "Wind direction shown by smoke but not wind vanes"; }
                else if (windspeedNumber <= 5) { windType = "Light breeze"; windSpeed = "4-7"; windDescription = "Wind felt on face, leaves rustle, and ordinary vanes move"; }
                else if (windspeedNumber <= 7) { windType = "Gentle breeze"; windSpeed = "8-12"; windDescription = "Leaves and small twigs sway and banners flap"; }
                else if (windspeedNumber <= 9) { windType = "Moderate breeze "; windSpeed = "13-18"; windDescription = "Small branches move, and dust and small branches are raised"; }
                else if (windspeedNumber <= 11) { windType = "Fresh breeze"; windSpeed = "19-24"; windDescription = "Small trees sway and small waves form on inland waters"; }
                else if (windspeedNumber <= 13) { windType = "Strong breeze"; windSpeed = "25-31"; windDescription = "Large branches move"; }
                else if (windspeedNumber <= 15) { windType = "Moderate gale (or near gale)"; windSpeed = "32-38"; windDescription = "Whole trees sway and walking against wind is an inconvenience"; }
                else if (windspeedNumber <= 17) { windType = "Fresh gale (or gale)"; windSpeed = "39-46"; windDescription = "Twigs break off trees and general progress is impeded"; }
                else if (windspeedNumber == 18) { windType = "Strong gale"; windSpeed = "47-54"; windDescription = "Slight structural damage occurs"; }
                else if (windspeedNumber == 19) { windType = "Whole gale (or storm)"; windSpeed = "55-63"; windDescription = "Trees are uprooted and considerable structural damage occurs"; }
                else if (windspeedNumber == 20)
                {
                    int d10 = r.Next(1, 11);
                    if (d10 < 9) {  windType = "Storm (or violent storm)"; windSpeed = "64-72"; windDescription = "Widespread damage occurs"; }
                    else {          windType = "Hurricane"; windSpeed = "73-136"; windDescription = "Widespread devastation occurs"; }
                }

                int d8 = r.Next(1, 9);
                string windDirection = "";
                if (d8 == 1) windDirection = "North";
                if (d8 == 2) windDirection = "Northeast";
                if (d8 == 3) windDirection = "East";
                if (d8 == 4) windDirection = "Southeast";
                if (d8 == 5) windDirection = "South";
                if (d8 == 6) windDirection = "Southwest";
                if (d8 == 7) windDirection = "West";
                if (d8 == 8) windDirection = "Northwest";

                Sunrise.Text = sunrise + ":" + sunriseZero + sunriseMinutes + " " + sunriseEnding;
                Sunset.Text = sunset + ":" + sunsetZero + sunsetMinutes + " " + sunsetEnding;
                WindBox1.Text = windType + ", " + windSpeed + " MHP " + windDirection;
                WindBox2.Text = windDescription;
                CoudCoverage.Text = cloudType;
                Percipitation.Text = precipitation;

                int d100 = r.Next(1, 101);

                int temp = 0;

                if (zone.StartsWith("Desert"))
                {
                    if (Nov * m < fractionOfYear || fractionOfYear < Feb * m)
                    {
                        if (d100 <= 5) temp = p(55);
                        else if (d100 <= 95) temp = p(55);
                        else temp = p(55);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(55);
                        else if (d100 <= 95) temp = r.Next(70, 91);
                        else temp = p(110);
                    }
                }
                else if (zone.StartsWith("Tropical Savanna"))
                {
                    if (d100 <= 5) temp = p(75);
                    else if (d100 <= 95) temp = r.Next(90, 106);
                    else temp = p(115);
                }
                else if (zone.StartsWith("Steppe"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Aug * m)
                    {
                        if (d100 <= 5) temp = p(70);
                        else if (d100 <= 95) temp = r.Next(85, 95);
                        else temp = p(110);
                    }
                    else if (
                        (Aug * m < fractionOfYear && fractionOfYear < Nov * m) ||
                        (Feb * m < fractionOfYear && fractionOfYear < May * m))
                    {
                        if (d100 <= 5) temp = p(50);
                        else if (d100 <= 95) temp = r.Next(60, 71);
                        else temp = p(80);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(35);
                        else if (d100 <= 95) temp = r.Next(40, 46);
                        else temp = p(50);
                    }
                }
                else if (zone.StartsWith("Equatorial"))
                {
                    if (d100 <= 5) temp = p(60);
                    else if (d100 <= 95) temp = r.Next(70, 85);
                    else temp = p(100);
                }
                else if (zone.StartsWith("Monsoon"))
                {
                    if (d100 <= 5) temp = p(70);
                    else if (d100 <= 50) temp = r.Next(85, 100);
                    else if (d100 <= 95) temp = r.Next(100, 110);
                    else temp = p(120);
                }
                else if (zone.StartsWith("Warm and Rainy"))
                {

                    if (May * m < fractionOfYear && fractionOfYear < Sep * m)
                    {
                        if (d100 <= 5) temp = p(60);
                        else if (d100 <= 50) temp = r.Next(65, 71);
                        else if (d100 <= 95) temp = r.Next(70, 76);
                        else temp = p(85);
                    }
                    else if (
                        (Sep * m < fractionOfYear && fractionOfYear < Oct * m) ||
                        (Feb * m < fractionOfYear && fractionOfYear < May * m))
                    {
                        if (d100 <= 5) temp = p(40);
                        else if (d100 <= 50) temp = 50;
                        else if (d100 <= 95) temp = 60;
                        else temp = p(65);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(10);
                        else if (d100 <= 50) temp = r.Next(25, 33);
                        else if (d100 <= 95) temp = r.Next(33, 45);
                        else temp = p(50);
                    }
                }
                else if (zone.StartsWith("Warm with Dry Summer"))
                {
                    if (Feb * m < fractionOfYear && fractionOfYear < Sep * m)
                    {
                        if (d100 <= 5) temp = p(60);
                        else if (d100 <= 50) temp = r.Next(65, 71);
                        else if (d100 <= 95) temp = r.Next(70, 86);
                        else temp = p(95);
                    }
                    else if (
                        (Sep * m < fractionOfYear && fractionOfYear < Nov * m) ||
                        (Feb * m < fractionOfYear && fractionOfYear < Apr * m))
                    {
                        if (d100 <= 5) temp = p(50);
                        else if (d100 <= 50) temp = 60;
                        else if (d100 <= 95) temp = 65;
                        else temp = p(70);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(10);
                        else if (d100 <= 50) temp = r.Next(20, 33);
                        else if (d100 <= 95) temp = r.Next(35, 51);
                        else temp = p(60);
                    }
                }
                else if (zone.StartsWith("Warm with Dry Winter"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Jul * m)
                    {
                        if (d100 <= 5) temp = p(70);
                        else if (d100 <= 95) temp = r.Next(85, 91);
                        else temp = p(110);
                    }
                    else if (
                        (Jul * m < fractionOfYear && fractionOfYear < Oct * m) ||
                        (Feb * m < fractionOfYear && fractionOfYear < May * m))
                    {
                        if (d100 <= 5) temp = p(50);
                        else if (d100 <= 95) temp = r.Next(60, 66);
                        else temp = p(70);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(32);
                        else if (d100 <= 95) temp = r.Next(35, 46);
                        else temp = p(50);
                    }

                }
                else if (zone.StartsWith("Cool and Rainy"))
                {
                    if (Jun * m < fractionOfYear && fractionOfYear < Sep * m)
                    {
                        if (d100 <= 5) temp = p(60);
                        else if (d100 <= 50) temp = r.Next(65, 71);
                        else if (d100 <= 95) temp = r.Next(70, 76);
                        else temp = p(85);
                    }
                    else if (
                        (Sep * m < fractionOfYear && fractionOfYear < Oct * m) ||
                        (Feb * m < fractionOfYear && fractionOfYear < Jun * m))
                    {
                        if (d100 <= 5) temp = p(35);
                        else if (d100 <= 50) temp = r.Next(40, 51);
                        else if (d100 <= 95) temp = r.Next(50, 61);
                        else temp = p(65);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(5);
                        else if (d100 <= 50) temp = r.Next(15, 26);
                        else if (d100 <= 95) temp = r.Next(25, 33);
                        else temp = p(40);
                    }
                }
                else if (zone.StartsWith("Cool with Dry Winter"))
                {
                    if (Jun * m < fractionOfYear && fractionOfYear < Sep * m)
                    {
                        if (d100 <= 5) temp = p(60);
                        else if (d100 <= 50) temp = r.Next(65, 71);
                        else if (d100 <= 95) temp = r.Next(70, 76);
                        else temp = p(85);
                    }
                    else if (
                        (Sep * m < fractionOfYear && fractionOfYear < Oct * m) ||
                        (Feb * m < fractionOfYear && fractionOfYear < Jun * m))
                    {
                        if (d100 <= 5) temp = p(35);
                        else if (d100 <= 50) temp = r.Next(40, 51);
                        else if (d100 <= 95) temp = r.Next(50, 61);
                        else temp = p(65);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(5);
                        else if (d100 <= 50) temp = r.Next(15, 26);
                        else if (d100 <= 95) temp = r.Next(25, 33);
                        else temp = p(40);
                    }
                }
                else if (zone.StartsWith("Polar"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Jul * m)
                    {
                        if (d100 <= 5) temp = p(32);
                        else if (d100 <= 50) temp = r.Next(35, 41);
                        else if (d100 <= 95) temp = r.Next(40, 51);
                        else temp = p(65);
                    }
                    else if (
                        (Jul * m < fractionOfYear && fractionOfYear < Nov * m) ||
                        (Jan * m < fractionOfYear && fractionOfYear < May * m))
                    {
                        if (d100 <= 5) temp = p(10);
                        else if (d100 <= 50) temp = r.Next(15, 21);
                        else if (d100 <= 95) temp = r.Next(25, 33);
                        else temp = p(35);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(-15);
                        else if (d100 <= 50) temp = r.Next(-5, 16);
                        else if (d100 <= 95) temp = r.Next(15, 33);
                        else temp = p(35);
                    }
                }
                else if (zone.StartsWith("Tundra"))
                {
                    if (May * m < fractionOfYear && fractionOfYear < Jul * m)
                    {
                        if (d100 <= 5) temp = p(32);
                        else if (d100 <= 50) temp = r.Next(35, 41);
                        else if (d100 <= 95) temp = r.Next(40, 51);
                        else temp = p(65);
                    }
                    else if (
                        (Jul * m < fractionOfYear && fractionOfYear < Nov * m) ||
                        (Jan * m < fractionOfYear && fractionOfYear < May * m))
                    {
                        if (d100 <= 5) temp = p(25);
                        else if (d100 <= 50) temp = 30;
                        else if (d100 <= 95) temp = 32;
                        else temp = p(40);
                    }
                    else
                    {
                        if (d100 <= 5) temp = p(-35);
                        else if (d100 <= 50) temp = r.Next(-25, 1);
                        else if (d100 <= 95) temp = r.Next(0, 31);
                        else temp = p(32);
                    }
                }

                Temp.Text = temp + "°F" + Environment.NewLine + (temp - r.Next(5, 30)) + "°F";

                double fractionOfMonth = ((double)DaysDropDown.SelectedIndex - 1) / daysPerMonth;
                int moon = (int) (fractionOfMonth * 12.0) + 1;
                if (moon == 1)
                {
                    Moon1.BringToFront();
                    MoonName.Text = "New" + Environment.NewLine + "Moon";
                }
                else if (moon == 2)
                {
                    Moon2.BringToFront();
                    MoonName.Text = "Young" + Environment.NewLine + "Moon";
                }
                else if (moon == 3)
                {
                    Moon3.BringToFront();
                    MoonName.Text = "Young" + Environment.NewLine + "Moon";
                }
                else if (moon == 4)
                {
                    Moon4.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Quarter";
                }
                else if (moon == 5)
                {
                    Moon5.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Cresent";
                }
                else if (moon == 6)
                {
                    Moon6.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Gibbous";
                }
                else if (moon == 7)
                {
                    Moon7.BringToFront();
                    MoonName.Text = "Full" + Environment.NewLine + "Moon";
                }
                else if (moon == 8)
                {
                    Moon8.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Gibbous";
                }
                else if (moon == 9)
                {
                    Moon9.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Gibbous";
                }
                else if (moon == 10)
                {
                    Moon10.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Quarter";
                }
                else if (moon == 11)
                {
                    Moon11.BringToFront();
                    MoonName.Text = "Waxing" + Environment.NewLine + "Cresent";
                }
                else if (moon == 12)
                {
                    Moon12.BringToFront();
                    MoonName.Text = "Old" + Environment.NewLine + "Moon";
                }
            }
        }

        //function for slightly scrambling numbers
        int p(int numberToScramble)
        {
            return numberToScramble + r.Next(-4, 13);
        }

        double Lerp(double firstFloat, double secondFloat, double by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }        

        void GetEncounterButton_Click(object sender, EventArgs e)
        {
            if (EncounterAreaDropDown.SelectedItem == null)
                EncounterOutput.Text = "Please select area";
            else if (EncounterTravelSpeedDropDown.SelectedItem == null)
                EncounterOutput.Text = "Please select speed";
            else if (EncounterZoneDropDown.SelectedItem == null)
                EncounterOutput.Text = "Please select zone";
            else
            {
                string speed = EncounterTravelSpeedDropDown.SelectedItem.ToString();
                string area = EncounterAreaDropDown.SelectedItem.ToString();

                int minutes = 1;
                int encouterMaxTime = 241;
                if (speed == "Fast")
                    encouterMaxTime = 181;
                else if (speed == "Slow")
                    encouterMaxTime = 481;

                if (area == "Roadside")
                    minutes = r.Next(1, encouterMaxTime);
                if (area == "Settlement")
                    minutes = r.Next(1, 1441);
                if (area == "Wilderness")
                    minutes = r.Next(1, encouterMaxTime);

                int hours = minutes / 60;
                minutes -= hours * 60;
                string a = "";
                if (hours > 0) a += hours + " hours and ";
                if (area == "Settlement")
                    a += minutes + " minutes into your stay the following occurs: ";
                else
                    a += minutes + " minutes into this leg of the journey following occurs: ";
                EncounterOutput.Text = a;
                EncounterOutput.Text += Environment.NewLine + Environment.NewLine;

                bool meeting = false;

                if (speed == "Fast")
                {
                    if (area == "Roadside")
                        meeting = r.Next(1, 100) <= 35;
                    if (area == "Wilderness")
                        meeting = r.Next(1, 100) <= 25;
                    if (area == "Settlement")
                        meeting = r.Next(1, 100) <= 25;
                }
                if (speed == "Normal")
                {
                    if (area == "Roadside")
                        meeting = r.Next(1, 100) <= 20;
                    if (area == "Wilderness")
                        meeting = r.Next(1, 100) <= 15;
                    if (area == "Settlement")
                        meeting = r.Next(1, 100) <= 25;
                }
                if (speed == "Slow")
                {
                    if (area == "Roadside")
                        meeting = r.Next(1, 100) <= 10;
                    if (area == "Wilderness")
                        meeting = r.Next(1, 100) <= 5;
                    if (area == "Settlement")
                        meeting = r.Next(1, 100) <= 25;
                }

                string forced = ForceEncounterDropDown.SelectedItem.ToString();

                if (forced == "Force Creature")
                    EncounterOutput.Text += CreatureEncouter();
                else if (forced == "Force Downtime")
                    EncounterOutput.Text += DownTimeEvent();
                else if (forced == "Force Feywild")
                    EncounterOutput.Text += FeywildEncounter();
                else if (forced == "Force Patrol")
                    PatrolEnconter();
                else if (forced == "Force Roadside")
                    EncounterOutput.Text += RoadsideEncounter();
                else if (forced == "Force Settlement")
                    EncounterOutput.Text += SettlementEncounter();
                else if (forced == "Force Special")
                    EncounterOutput.Text += SpecialEncounter();
                else if (forced == "Force Wilderness")
                    EncounterOutput.Text += WildernessEncounter();
                else if (forced == "Force Zone Specific")
                    EncounterOutput.Text += RegionSpecificEncounter();
                else if (forced == "Force Random" || meeting)
                {
                    int p = r.Next(1, 101);
                    if (area == "Roadside")
                    {
                        if (p <= 25)
                            EncounterOutput.Text += RoadsideEncounter();
                        else if (p <= 45)
                            PatrolEnconter();
                        else if (p <= 60)
                            EncounterOutput.Text += CreatureEncouter();
                        else if (p <= 75)
                            EncounterOutput.Text += SpecialEncounter();
                        else if (p <= 90)
                            EncounterOutput.Text += RegionSpecificEncounter();
                        else if (p <= 96)
                            EncounterOutput.Text += FeywildEncounter();
                        else if (p <= 99)
                            EncounterOutput.Text += "Wild Hunt Appearance";
                        else
                            EncounterOutput.Text += "Ley Line Emergence";
                    }
                    if (area == "Wilderness")
                    {
                        if (p <= 25)
                            EncounterOutput.Text += WildernessEncounter();
                        else if (p <= 45)
                            EncounterOutput.Text += CreatureEncouter();
                        else if (p <= 62)
                            EncounterOutput.Text += SpecialEncounter();
                        else if (p <= 79)
                            EncounterOutput.Text += RegionSpecificEncounter();
                        else if (p <= 96)
                            EncounterOutput.Text += FeywildEncounter();
                        else if (p <= 99)
                            EncounterOutput.Text += "Wild Hunt Appearance";
                        else
                            EncounterOutput.Text += "Ley Line Emergence";
                    }
                    if (area == "Settlement")
                    {
                        if (p <= 35)
                            EncounterOutput.Text += SettlementEncounter();
                        else if (p <= 70)
                            EncounterOutput.Text += DownTimeEvent();
                        else if (p <= 80)
                            EncounterOutput.Text += SpecialEncounter();
                        else if (p <= 86)
                            EncounterOutput.Text += RegionSpecificEncounter();
                        else if (p <= 91)
                            EncounterOutput.Text += CreatureEncouter();
                        else if (p <= 96)
                            EncounterOutput.Text += FeywildEncounter();
                        else if (p <= 99)
                            EncounterOutput.Text += "Wild Hunt Appearance";
                        else
                            EncounterOutput.Text += "Ley Line Emergence";
                    }
                }
                else
                {
                    if (EncounterAreaDropDown.SelectedItem.ToString() == "Settlement")
                        EncounterOutput.Text = "No encounter occurs today";
                    else
                        EncounterOutput.Text = "No encounter occurs during this leg of your journey";
                }
            }
        }

        string SpecialEncounter()
        {
            string zone = EncounterZoneDropDown.SelectedItem.ToString();
            string path = workdir + "/Tables/Zones/" + zone + "/Special_Encounters.txt";
            if (!File.Exists(path))
                return "No Special Encounters set up for this Zone";
            else
                return RandomNonBlankTextLine(path);
        }

        string RegionSpecificEncounter()
        {
            string zone = EncounterZoneDropDown.SelectedItem.ToString();
            string path = workdir + "/Tables/Zones/" + zone + "/Region_Specific_Encounters.txt";
            if (!File.Exists(path))
                return "No Specific Encounters set up for this Zone";
            else
                return RandomNonBlankTextLine(path);
        }

        string WildernessEncounter()
        {
            string path = workdir + "/Tables/Zones/Wilderness_Encounters.txt";
            return RandomNonBlankTextLine(path);
        }

        string SettlementEncounter()
        {
            string path = workdir + "/Tables/Zones/Settlement_Encounters.txt";
            return RandomNonBlankTextLine(path);
        }

        string RandomNonBlankTextLine(string path)
        {
            string[] lines = File.ReadAllLines(path);
            List<string> lineList = new List<string>();
            foreach (string line in lines)
                if (line.Length > 2) lineList.Add(line);
            return lineList[r.Next(0, lineList.Count)];
        }

        string EncounterDetails(string feyOrCreature)
        {
            int n = r.Next(1, 101);
            if (n <= 33)
                feyOrCreature = "You discover the lair of " + feyOrCreature;
            else if (n <= 66)
                feyOrCreature = "You discover the tracks of " + feyOrCreature;
            else
            {
                feyOrCreature = "You encounter " + feyOrCreature + ", ";
                string[] stuff =
                {
                    "they are defending its territory from __",
                    "they are running past you",
                    "they are running from __",
                    "they are protecting __",
                    "they are sneaking up on the party",
                    "they are stalking its prey, a __",
                    "they are attacking __",
                    "they are playing",
                    "they are eating",
                    "they are sleeping",
                    "they are caught in a trap",
                    "they are in a fight to the death with __",
                    "they are hiding",
                    "they are wandering about aimlessly",
                    "they are looking for something",
                    "they are lying down wounded",
                    "they are lying down resting",
                    "they are interacting with it terrain",
                    "they are toying with its prey, a __"
                };

                string description = stuff[r.Next(0, stuff.Length)];

                if (description.Contains("__"))
                {
                    bool creatureNotFey = true;
                    if (r.Next(1, 101) < 15) creatureNotFey = false;
                    int j = r.Next(1, 101);
                    if (j <= 50)
                        description = description.Replace("__", "same type of Creature / Fey");
                    else if (j <= 75)
                        description = description.Replace("__", FeyWildAndCreature(creatureNotFey));
                    else
                        description = description.Replace("__", HumanInteractingWithCreatureOrFay());
                }

                feyOrCreature += description;
            }
            return feyOrCreature;
        }

        string HumanInteractingWithCreatureOrFay()
        {
            string[] stuff =
            {
                "1D6 Random People",
                "1D6 Hunters",
                "1D6 Yuherix Union Soldiers",
                "1D6 Bandits",
                "Merchant Caravan consisting of 1D20 people",
                "Bardic College Member and their team of 1D6 people",
                "1D6 Ranger’s Lodge Patrol",
                "1D6 Arcane Order Patrol",
                "1D6 Society of Sisters Patrol",
                "Druidic Circle member",
                "Witch woman",
                "1D6 Conclave Patrol",
                "1D6 Sinister Smile Patrol",
                "1D6 Local Guard Patrol",
                "1D6 Streangan Slavers",
                "1D6 Vastavicland Raiders",
                "Noble and their Entourage of 1D10 people",
                "1D6 escaped slaves",
                "1D6 priests",
                "1D6 cultists"
            };

            string description = stuff[r.Next(0, stuff.Length)];
            int N = 1;
            if (description.Contains("1D6"))
            {
                N = r.Next(1, 7);
                description = description.Replace("1D6", N.ToString());
            }
            if (description.Contains("1D10"))
            {
                N = r.Next(1, 11);
                description = description.Replace("1D10", N.ToString());
            }
            if (description.Contains("1D20"))
            {
                N = r.Next(1, 21);
                description = description.Replace("1D20", N.ToString());
            }
            
            int[] humansLevels = thingsLevels2(N);
            Array.Sort(humansLevels);
            string people2LevelsText = levelTxt(humansLevels);

            return description + Environment.NewLine + Environment.NewLine + people2LevelsText;
        }

        string FeywildEncounter()
        {
            string fey = FeyWildAndCreature(true);
            return EncounterDetails(fey);
        }

        void PatrolEnconter()
        {
            EncounterOutput.Text += "Patrol of some kind passes by:";
            int p2 = r.Next(1, 101);
            if (p2 <= 25)
                EncounterOutput.Text += PatrolEncounter("Arresting");
            else if (p2 <= 50)
                EncounterOutput.Text += PatrolEncounter("Confronting");
            else if (p2 <= 75)
                EncounterOutput.Text += PatrolEncounter("Escorting");
            else
                EncounterOutput.Text += PatrolEncounter("Peaceful");
        }

        string CreatureEncouter()
        {
            string creature = FeyWildAndCreature(false);
            return EncounterDetails(creature);
        }

        string FeyWildAndCreature(bool isFey)
        {
            string zone = EncounterZoneDropDown.SelectedItem.ToString();
            string terrain = EncounterTerrainDropDown.SelectedItem.ToString();
            string[] creatureLines = File.ReadAllLines(workdir + "/Tables/Zones/" + zone + "/encounter.txt");
            string[] feyLines = File.ReadAllLines(workdir + "/Tables/Zones/Feywild.txt");
            string[] lines = creatureLines;
            if (isFey) lines = feyLines;

            int n = 0;
            string creature = "";
            double cr;
            foreach (string line in lines)
            {
                if (line.StartsWith(terrain))
                {
                    string[] encounters = line.Split(':')[1].Split(',');
                    string encounter = encounters[r.Next(0, encounters.Length - 1)];
                    string dice = encounter.Split(' ')[1];
                    if (dice.Contains("D"))
                    {
                        int numberOfDices = int.Parse(dice.Split('D')[0]);
                        int typeOfDice = int.Parse(dice.Split('D')[1]);
                        n = numberOfDices * r.Next(1, typeOfDice + 1);
                    }
                    else
                        n = int.Parse(dice);
                    List<string> creatureWords = new List<string>();
                    creatureWords.AddRange(encounter.Split(' '));
                    creatureWords.RemoveAt(0);
                    creatureWords.RemoveAt(0);
                    creature = string.Join(" ", creatureWords);
                    string crText = encounter.Split(new string[] { "CR" },
                        StringSplitOptions.RemoveEmptyEntries)[1];
                    crText = crText.Replace(" ", "");
                    if (crText == "½") cr = 1.0 / 2.0;
                    else if (crText == "⅓") cr = 1.0 / 3.0;
                    else if (crText == "¼") cr = 1.0 / 4.0; 
                    else if (crText == "⅙") cr = 1.0 / 6.0;
                    else if (crText == "1/2") cr = 1.0 / 2.0;
                    else if (crText == "1/4") cr = 1.0 / 4.0;
                    else if (crText == "1/3") cr = 1.0 / 3.0;
                    else if (crText == "1/6") cr = 1.0 / 6.0;
                    else if (crText == "1/8") cr = 1.0 / 8.0;
                    else cr = double.Parse(crText);
                }
            }
            return n + " " + creature;
        }

        string RoadsideEncounter()
        {
            string[] lines = File.ReadAllLines(workdir + "/Tables/Zones/Roadside_Encounters.txt");
            List<string> encounters = new List<string>();
            foreach (string line in lines)
                if (line.Length > 1) encounters.Add(line);
            return encounters[r.Next(0, encounters.Count)];
        }

        int[] thingsLevels1(int N)
        {
            int[] levels = new int[N];
            for (int i = 0; i < levels.Length; i++)
            {
                int d100 = r.Next(1, 101);
                if (d100 <= 40) levels[i] = 2;
                else if (d100 <= 65) levels[i] = 3;
                else if (d100 <= 85) levels[i] = 4;
                else if (d100 <= 95) levels[i] = 5;
                else levels[i] = 6;
            }
            return levels;
        }

        int[] thingsLevels2(int N)
        {
            int[] levels = new int[N];
            for (int i = 0; i < levels.Length; i++)
            {
                int d100 = r.Next(1, 101);
                if (d100 <= 40) levels[i] = 2;
                else if (d100 <= 65) levels[i] = 3;
                else if (d100 <= 85) levels[i] = 4;
                else if (d100 <= 95) levels[i] = 5;
                else levels[i] = 6;
            }
            return levels;
        }

        string PatrolEncounter(string ecounterType)
        {
            string zone = EncounterZoneDropDown.SelectedItem.ToString();
            string[] lines = File.ReadAllLines(workdir + "/Tables/Zones/" + zone + "/patrols.txt");
            string people = "";
            string people2 = "";
            foreach(string line in lines)
            {
                if (line.StartsWith("Patrol"))
                {
                    string[] peoples = line.Split(':')[1].Split(',');
                    people = peoples[r.Next(0, peoples.Length)];
                }
                if (ecounterType != "Peaceful")
                {
                    if (line.StartsWith(ecounterType))
                    {
                        string[] peoples2 = line.Split(':')[1].Split(',');
                        people2 = peoples2[r.Next(0, peoples2.Length)];
                    }
                }
            }
            if (people.Length < 1) return "No Patrol set up for zone " + zone;
            if (ecounterType != "Peaceful")
                if (people2.Length < 1) return "No " + ecounterType + " encounters set up for zone " + zone;


            int d12 = r.Next(1, 13);
            int[] patrolLevels = thingsLevels1(d12);

            int d20 = r.Next(1, 21);
            int[] people2Levels = thingsLevels2(d20);
            if (ecounterType == "Peaceful")
            {
                people2Levels = new int[d20];
            }

            Array.Sort(patrolLevels);
            Array.Sort(people2Levels);

            string patrolLevelText = levelTxt(patrolLevels);
            string people2LevelsText = levelTxt(people2Levels);


            string verb = "";
            if (ecounterType == "Arresting") verb = "arresting ";
            if (ecounterType == "Confronting") verb = "confronting ";
            if (ecounterType == "Escorting") verb = "escorting ";
            if (ecounterType == "Peaceful")
                return " " + patrolLevels.Length + " members of" + people + " are peacefully patroling the road." +
                    Environment.NewLine + Environment.NewLine +
                    people + " levels: " + Environment.NewLine + patrolLevelText;
            else
                return " " + patrolLevels.Length + " members of" + people + " are " 
                    + verb + people2Levels.Length + " members of" + people2 + 
                    Environment.NewLine + Environment.NewLine +
                    people + " levels: " + Environment.NewLine + patrolLevelText + " " 
                    + Environment.NewLine + Environment.NewLine +
                    people2 + " levels: " + Environment.NewLine + people2LevelsText + " ";
        }

        string levelTxt(int[] levels)
        {
            List<List<int>> lvls = new List<List<int>>();
            foreach (int i in levels)
            {
                if (lvls.Count == 0)
                {
                    List<int> lvl = new List<int>();
                    lvl.Add(i);
                    lvls.Add(lvl);
                }
                else
                {
                    List<int> lastLvl = lvls[lvls.Count - 1];
                    if (i != lastLvl[lastLvl.Count - 1])
                    {
                        List<int> lvl = new List<int>();
                        lvl.Add(i);
                        lvls.Add(lvl);
                    }
                    else lastLvl.Add(i);
                }
            }
            string levelText = "";
            foreach (List<int> lvl in lvls)
                levelText += lvl.Count + " lv." + lvl[0] + ", ";
            return levelText;
        }        

        private void GetLeyLine_Click(object sender, EventArgs e)
        {
            LeyLineBox.Text = LeyLineText();
        }

        string LeyLineText()
        {
            LeyLine firstLeyLine = new LeyLine(r.Next());
            LeyLine secondLeyLine = null;
            if (r.Next(1, 101) <= 5)
                secondLeyLine = new LeyLine(r.Next());
            string hexesLong = "";
            if(firstLeyLine.lengthUnit == "hexes") hexesLong = " long";
            string text =
                "A " + firstLeyLine.length + " " + firstLeyLine.lengthUnit + hexesLong +
                " Ley Line emerges and remains for " +
                firstLeyLine.time + " " + firstLeyLine.timeUnit + ", " +
                "it has an aura of " + r.Next(5, 501) + " feet." +
                Environment.NewLine + Environment.NewLine +
                "The players are " + firstLeyLine.distanceToCenterOrConvergence + " "
                + firstLeyLine.lengthUnit + " " + firstLeyLine.playerPositionOrConvergencePoint
                + " of the center of the Ley Line.";

            if (firstLeyLine.willBreak)
            {
                text +=
                Environment.NewLine + Environment.NewLine +
                "When this line breaks, it will transform everything ";
                if (!firstLeyLine.amarythArea.Contains("directly underneath"))
                    text += "in a ";
                text += firstLeyLine.amarythArea + " into " + firstLeyLine.amarythColor + ".";
            }

            string passiveEffectsPath = workdir + "/Tables/LeyLines/Passive_Effect.txt";
            string triggeredEffectsPath = workdir + "/Tables/LeyLines/Triggered_Effect.txt";
            PassiveLeyLineEffectBox.Text = RandomNonBlankTextLine(passiveEffectsPath);
            TriggeredLeyLineEffectBox.Text = RandomNonBlankTextLine(triggeredEffectsPath);
            
            if (secondLeyLine != null)
            {
                hexesLong = "";
                if (secondLeyLine.lengthUnit == "hexes") hexesLong = " long";
                text += Environment.NewLine + Environment.NewLine +
                "Another " + secondLeyLine.length + " " + secondLeyLine.lengthUnit + hexesLong +
                " Ley Line emerges and remains for " +
                secondLeyLine.time + " " + secondLeyLine.timeUnit + ", " +
                "it has an aura of " + r.Next(5, 501) + " feet." +
                Environment.NewLine + Environment.NewLine +
                "It converges " + secondLeyLine.distanceToCenterOrConvergence + " "
                + secondLeyLine.lengthUnit + " " + secondLeyLine.playerPositionOrConvergencePoint
                + " of the center of the original Ley Line.";
            }
            return text;
        }


        private void NewPassiveEffectButton_Click(object sender, EventArgs e)
        {
            string passiveEffectsPath = workdir + "/Tables/LeyLines/Passive_Effect.txt";
            PassiveLeyLineEffectBox.Text = RandomNonBlankTextLine(passiveEffectsPath);
        }

        private void NewTriggeredEffectButton_Click(object sender, EventArgs e)
        {
            string triggeredEffectsPath = workdir + "/Tables/LeyLines/Triggered_Effect.txt";
            TriggeredLeyLineEffectBox.Text = RandomNonBlankTextLine(triggeredEffectsPath);
        }

        // ============================== helper functions ==================================
        string BadToOk(string bad)
        {
            return bad.Replace(" ", "_").Replace(":","COLONCHARACTER").Replace("/", "SLASH_CHARACTER");
        }
        string OkToBad(string ok)
        {
            return ok.Replace("COLONCHARACTER", ":").Replace("SLASH_CHARACTER", "/").Replace("_", " ");
        }
        
        void InitiateCultures()
        {
            string[] culturePaths = Directory.GetDirectories(workdir + "/Tables/Cultures/");
            List<string> cultures = new List<string>();
            foreach (string c in culturePaths)
                cultures.Add(Path.GetFileName(c));

            InitiateCultureDropDown(SettlementCultureDropDown, cultures);
            InitiateCultureDropDown(NameCultureDropDown, cultures);
            InitiateCultureDropDown(SongCultureDropDown, cultures);
            InitiateCultureDropDown(RumorCultureDropDown, cultures);
            InitiateCultureDropDown(EncounterCultureDropDown, cultures);
        }

        void InitiateCultureDropDown(ComboBox cb, List<string> cultures)
        {
            cb.Items.Clear();
            cb.Items.Add("Choose Culture");
            cb.Items.AddRange(cultures.ToArray());
            cb.SelectedIndex = 0;
        }

        void InitiateZones()
        {
            string[] zonesPaths = Directory.GetDirectories(workdir + "/Tables/Zones/");
            List<string> zones = new List<string>();
            foreach (string z in zonesPaths)
                zones.Add(Path.GetFileName(z));

            ForageZoneDropDown.Items.Clear();
            ForageZoneDropDown.Items.Add("Choose Zone");
            ForageZoneDropDown.Items.AddRange(zones.ToArray());
            ForageZoneDropDown.SelectedIndex = 0;

            EncounterZoneDropDown.Items.Clear();
            EncounterZoneDropDown.Items.Add("Choose Zone");
            EncounterZoneDropDown.Items.AddRange(zones.ToArray());
            EncounterZoneDropDown.SelectedIndex = 0;

            InintiateForageSpecial();
        }

        void InintiateForageSpecial()
        {
            string path = workdir + "/Tables/Zones/Special_Resources.txt";
            string[] lines = File.ReadAllLines(path);
            foreach(string line in lines)
            {
                if (line.Contains(":"))
                {
                    string terrainAndRarity = line.Split(':')[0];
                    if (terrainAndRarity.Contains("Common")){
                        string terrain = terrainAndRarity.Split(' ')[0];
                        ForageSpecialDropDown.Items.Add(terrain);
                    }
                }
            }
        }

        private void SetDetailsButton_Click(object sender, EventArgs e)
        {
            PersonInputPanel.BringToFront();
        }

        private void ShowNPCNameGen_Click(object sender, EventArgs e)
        {
            PersonNameOutputPanel.BringToFront();
        }
    }

    class LeyLine
    {
        public int length;
        public string lengthUnit;
        public int time;
        public string timeUnit;
        public string playerPositionOrConvergencePoint;
        public int distanceToCenterOrConvergence;
        public bool willBreak;
        public string amarythArea;
        public string amarythColor;

        public LeyLine(int seed)
        {
            Random r = new Random(seed);
            int i = r.Next(1, 101);
            length = 0;
            lengthUnit = "";
            if (i <= 45)
            {
                lengthUnit = "feet";
                int j = r.Next(1, 7);
                if (j == 1) length = 1;
                if (j == 2) length = 10;
                if (j == 3) length = 25;
                if (j == 4) length = 50;
                if (j == 5) length = 75;
                if (j == 6) length = 100;
            }
            else if (i <= 90)
            {
                lengthUnit = "feet";
                int j = r.Next(1, 6);
                if (j == 1) length = 100;
                if (j == 2) length = 500;
                if (j == 3) length = 1000;
                if (j == 4) length = 2500;
                if (j == 5) length = 5000;
            }
            else
            {
                lengthUnit = "hexes";
                length = r.Next(1, 7);
            }

            i = r.Next(1, 101);
            time = 0;
            timeUnit = "";
            if (i <= 95)
            {
                int j = r.Next(1, 7);
                if (j == 1) { time = 1; timeUnit = "round"; };
                if (j == 2) { time = 1; timeUnit = "minute"; };
                if (j == 3) { time = 5; timeUnit = "minutes"; };
                if (j == 4) { time = 10; timeUnit = "minutes"; };
                if (j == 5) { time = 1; timeUnit = "hour"; };
                if (j == 6) { time = 2; timeUnit = "hours"; };
            }
            else if (i <= 99)
            {
                int j = r.Next(1, 7);
                if (j == 1) { time = 3; timeUnit = "hours"; };
                if (j == 2) { time = 6; timeUnit = "hours"; };
                if (j == 3) { time = 12; timeUnit = "hours"; };
                if (j == 4) { time = 24; timeUnit = "hours"; };
                if (j == 5) { time = 2; timeUnit = "days"; };
                if (j == 6) { time = 3; timeUnit = "days"; };
                if (j == 7) { time = 4; timeUnit = "days"; };
                if (j == 8) { time = 5; timeUnit = "days"; };
                if (j == 9) { time = 6; timeUnit = "days"; };
                if (j == 10) { time = 7; timeUnit = "days"; };
            }
            else
            {
                int j = r.Next(1, 7);
                if (j == 1) { time = 1; timeUnit = "week"; };
                if (j == 2) { time = 2; timeUnit = "weeks"; };
                if (j == 3) { time = 3; timeUnit = "weeks"; };
                if (j == 4) { time = 4; timeUnit = "weeks"; };
                if (j == 5) { time = 5; timeUnit = "weeks"; };
                if (j == 6) { time = 6; timeUnit = "weeks"; };
                if (j == 7) { time = 7; timeUnit = "weeks"; };
                if (j == 8) { time = 8; timeUnit = "weeks"; };
                if (j == 9) { time = 8; timeUnit = "weeks"; };
                if (j == 10) { time = 10; timeUnit = "weeks"; };
            }

            i = r.Next(1, 5);
            if (i == 1) playerPositionOrConvergencePoint = "north";
            if (i == 2) playerPositionOrConvergencePoint = "east";
            if (i == 3) playerPositionOrConvergencePoint = "south";
            if (i == 4) playerPositionOrConvergencePoint = "west";
            
            distanceToCenterOrConvergence = r.Next(0, (length / 2) + 1);
            
            i = r.Next(1, 21);
            if (i == 1) amarythArea = "5 foot radius at the Ley Line's center";
            if (i == 2) amarythArea = "10 foot radius at the Ley Line's center";
            if (i == 3) amarythArea = "25 foot radius at the Ley Line's center";
            if (i == 4) amarythArea = "50 foot radius at the Ley Line's center";
            if (i == 5) amarythArea = "75 foot radius at the Ley Line's center";
            if (i == 6) amarythArea = "100 foot radius at the Ley Line's center";
            if (i == 7) amarythArea = "5 foot radius at the Ley Line's entrance point";
            if (i == 8) amarythArea = "10 foot radius at the Ley Line's entrance point";
            if (i == 9) amarythArea = "25 foot radius at the Ley Line's entrance point";
            if (i == 10) amarythArea = "50 foot radius at the Ley Line's entrance point";
            if (i == 11) amarythArea = "75 foot radius at the Ley Line's entrance point";
            if (i == 12) amarythArea = "100 foot radius at the Ley Line's entrance point";
            if (i == 13) amarythArea = "5 foot radius at the Ley Line's exit point";
            if (i == 14) amarythArea = "10 foot radius at the Ley Line's exit point";
            if (i == 15) amarythArea = "25 foot radius at the Ley Line's exit point";
            if (i == 16) amarythArea = "50 foot radius at the Ley Line's exit point";
            if (i == 17) amarythArea = "75 foot radius at the Ley Line's exit point";
            if (i == 18) amarythArea = "100 foot radius at the Ley Line's exit point";
            if (i == 19) amarythArea = "directly beneath entirety of the Ley Line";
            if (i == 20) amarythArea = "5 foot radius around entirety of the Ley Line";

            i = r.Next(1, 8);
            if (i == 1) amarythColor = "Black Amaryth";
            if (i == 2) amarythColor = "Blue Amaryth";
            if (i == 3) amarythColor = "Green Amaryth";
            if (i == 4) amarythColor = "Gold Amaryth";
            if (i == 5) amarythColor = "Red Amaryth";
            if (i == 6) amarythColor = "Violet Amaryth";
            if (i == 7) amarythColor = "White Amaryth";

            if (r.Next(1, 101) < 5)
                willBreak = true;
        }
        
    }
}