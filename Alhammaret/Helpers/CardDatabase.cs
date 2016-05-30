using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Storage;

namespace Alhammaret
{
    public class JsonData
    {
        public JsonSetData SOI;
        public JsonSetData BFZ;
        public JsonSetData OGW;
        public JsonSetData DTK;
        public JsonSetData ORI;
        public JsonSetData FRF;
        public JsonSetData KTK;
    }

    public class JsonSetData
    {
        public string Name;
        public JsonCardData[] Cards;
    }

    public class JsonCardData
    {
        public string Name;
        public string[] Names;
        public int CMC;
        public string[] Colors;
        public string Type;
        public string[] Supertypes;
        public string[] Types;
        public string[] Subtypes;
        public string Rarity;
        public string Text;
        public string Number;
        public string ManaCost;

        public string Power;
        public string Toughness;
        public string Loyalty;
        public int MultiverseId;
    }

    public class CardDB
    {
        public enum Color
        {
            Black = 0,
            Blue,
            Colorless,
            Generic,
            Green,
            Red,
            Scalar,
            White,
            NumColors
        }

        public enum Set
        {
            None = -1,
            ReturnToRavnica = 0,
            Gatecrash, // 2013
            DragonsMaze,
            Magic2014,
            Theros,
            BornOfTheGods, // 2014
            JourneyIntoNyx,
            Magic2015,
            KhansOfTarkir,
            FateReforged, // 2015
            DragonsOfTarkir,
            MagicOrigins,
            BattleForZendikar, // 2016
            OathOfTheGatewatch,
            ShadowsOverInnistrad,
            EldritchMoon
        }

        // Needs to be updated Q4 2016
        private const Set StandardStart = Set.DragonsOfTarkir;

        public class Mana
        {
            private static Regex manaStringParser = new Regex(@"\{([^}]+)\}");

            public int Black { get; private set; }
            public int Blue { get; private set; }
            public int Colorless { get; private set; }
            public int Green { get; private set; }
            public int Red { get; private set; }
            public int Scalar { get; private set; }
            public int White { get; private set; }
            public int Generic { get; private set; }

            public int CMC { get; private set; }

            public Mana(string manaDesc, int cmc)
            {
                this.Black = 0;
                this.Blue = 0;
                this.Colorless = 0;
                this.Green = 0;
                this.Red = 0;
                this.Scalar = 0;
                this.White = 0;
                this.Generic = 0;

                MatchCollection matches = manaStringParser.Matches(manaDesc);
                for (int i = 0; i < matches.Count; ++i)
                {
                    if (matches[i].Groups.Count == 0)
                    {
                        Debug.WriteLine("ERROR: match contained no groups: " + matches[i].Value);
                        continue;
                    }
                    else if (matches[i].Groups.Count > 2)
                    {
                        Debug.WriteLine("Warning: match contained more groups than expected");
                        for (int j = 0; j < matches[i].Groups.Count; ++j)
                        {
                            Debug.WriteLine($"Group {j} : {matches[i].Groups[j].Value}");
                        }
                    }
                    switch(matches[i].Groups[1].Value)
                    {
                        case "C":
                            this.Colorless += 1;
                            break;
                        case "B":
                            this.Black += 1;
                            break;
                        case "G":
                            this.Green += 1;
                            break;
                        case "R":
                            this.Red += 1;
                            break;
                        case "U":
                            this.Blue += 1;
                            break;
                        case "W":
                            this.White += 1;
                            break;
                        case "X":
                            this.Scalar += 1;
                            break;
                        default:
                            int result = Int32.Parse(matches[i].Groups[1].Value);
                            this.Generic += result;
                            break;
                    }
                }

                this.CMC = cmc;
            }
        }

        public class Card
        {
            public static Card Sample1()
            {
                Card c = new Card();
                c.Name = "Mighty Leap";
                c.ManaCost = new Mana("{5}", 5);
                c.CardSets = new List<Set>() { Set.MagicOrigins, Set.OathOfTheGatewatch };
                return c;
            }

            public static Card Sample2()
            {
                Card c = new Card();
                c.Name = "Tireless Tracker";
                c.ManaCost = new Mana("{2}{G}", 5);
                c.CardSets = new List<Set>() { Set.ShadowsOverInnistrad };
                return c;
            }

            public string Name { get; private set; }
            public string Text { get; private set; }
            public Mana ManaCost { get; private set; }
            public List<Set> CardSets { get; private set; }
            public List<int> Ids { get; private set; }

            private Card() { }

            public Card(JsonCardData data, Set set)
            {
                this.Name = data.Name;
                this.Text = data.Text;
                this.CardSets = new List<Set> { set };
                this.Ids = new List<int> { data.MultiverseId };
                if (data.ManaCost != null)
                {
                    this.ManaCost = new Mana(data.ManaCost, data.CMC);
                }
                else if (data.CMC > 0)
                {
                    Debug.WriteLine("ERROR: CMC does not match mana cost string");
                }
            }

            public void AddSet(Set set, int id)
            {
                if (this.CardSets.Contains(set)) { return; }
                this.CardSets.Add(set);
                this.Ids.Add(id);
            }
        }
 
        private static CardDB instance;
        public static CardDB Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CardDB();
                }
                return instance;
            }
        }

        private Dictionary<string, Card> cardByName;
        private Dictionary<int, Card> cardById;

        public delegate void DatabaseUpdated();
        public DatabaseUpdated OnDatabaseUpdated;

        public CardDB()
        {
            this.Ready = false;
            cardByName = new Dictionary<string, Card>();
            cardById = new Dictionary<int, Card>();
            Build();
        }

        public Card Get(string name)
        {
            string key = name.ToLower();
            key = key.Replace("•", "'");
            if (cardByName.ContainsKey(key))
            {
                return cardByName[key];
            }
            else
            {
                return GetPermute(key);
            }
        }

        private Card GetPermute(string key)
        {
            Card ret;
            ret = Permute(key, 'i', "j");
            if (ret != null) { return ret; }
            ret = Permute(key, 'l', "j");
            if (ret != null) { return ret; }
            ret = Permute(key, 'i', "z");
            if (ret != null) { return ret; }
            return null;
        }

        private Card Permute(string key, char src, string dst)
        {
            if (!key.Contains(src)) { return null; }
            int offset = 0;
            int idx;
            while ((idx = key.Substring(offset).IndexOf(src)) != -1)
            {
                int adj = idx + offset;
                string sub = key.Substring(0, adj) + dst + key.Substring(adj + 1, key.Length - adj - 1);
                Debug.WriteLine($"Attempting substitution '{sub}'");
                if (cardByName.ContainsKey(sub)) { return cardByName[sub]; }
                offset = adj + 1;
            }
            return null;
        }
        
        public Card Get(int id)
        {
            return cardById.ContainsKey(id) ? cardById[id] : null;
        }

        public List<Card> AllCards()
        {
            return cardByName.Values.ToList();
        }

        public bool Ready { get; private set; }

        private async void Build()
        {
            try
            {
                StorageFile jsonFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/AllSets.txt"));
                string jsonStr = await FileIO.ReadTextAsync(jsonFile);
                JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(jsonStr);
                ProcessSet(jsonData.BFZ, Set.BattleForZendikar);
                ProcessSet(jsonData.DTK, Set.DragonsOfTarkir);
                ProcessSet(jsonData.OGW, Set.OathOfTheGatewatch);
                ProcessSet(jsonData.ORI, Set.MagicOrigins);
                ProcessSet(jsonData.SOI, Set.ShadowsOverInnistrad);
                ProcessSet(jsonData.FRF, Set.FateReforged);
                ProcessSet(jsonData.KTK, Set.KhansOfTarkir);
                Debug.WriteLine("Finished processing card data");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while building card database : " + e.Message);
            }
            this.Ready = true;
            OnDatabaseUpdated?.Invoke();
        }

        private void ProcessSet(JsonSetData setData, Set set)
        {
            for (int i = 0; i < setData.Cards.Length; ++i)
            {
                if (setData.Cards[i].Rarity == "Basic Land")
                {
                    Debug.WriteLine($"Ignoring basic land '{setData.Cards[i].Name}");
                    continue;
                }
                string nameKey = setData.Cards[i].Name.ToLower();
                if (cardByName.ContainsKey(nameKey))
                {
                    cardByName[nameKey].AddSet(set, setData.Cards[i].MultiverseId);
                }
                else
                {
                    cardByName[nameKey] = new Card(setData.Cards[i], set);
                }

                if (cardById.ContainsKey(setData.Cards[i].MultiverseId))
                {
                    Debug.WriteLine($"ERROR: Multiple cards found with same multiverse id {setData.Cards[i].MultiverseId}");
                }
                else
                {
                    cardById[setData.Cards[i].MultiverseId] = cardByName[nameKey];
                }
            }
            OnDatabaseUpdated?.Invoke();
        }
    }
}