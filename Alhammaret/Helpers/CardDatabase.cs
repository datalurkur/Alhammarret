using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Streams;

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
        public JsonSetData M15;
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

            public Set GetSet(int id)
            {
                if (!this.Ids.Contains(id)) { return Set.None; }
                int index = this.Ids.IndexOf(id);
                return this.CardSets[index];
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

        private Hunspell_UWP.HunspellWrapper hunspell;

        public delegate void DatabaseUpdated(float progress);
        public DatabaseUpdated OnDatabaseUpdated;

        public bool Ready { get; private set; }

        private CardDB()
        {
            this.Ready = false;
            cardByName = new Dictionary<string, Card>();
            cardById = new Dictionary<int, Card>();
        }

        public Card Get(string name)
        {
            string key = name.ToLower();
            key = key.Replace("•", "'");
            key = key.Replace("(", "");
            key = key.Trim();
            if (cardByName.ContainsKey(key))
            {
                return cardByName[key];
            }
            else
            {
                return TrySpellCorrection(key);
            }
        }

        private Card TrySpellCorrection(string misspelled)
        {
            string key = misspelled.Replace(" ", "");
            Hunspell_UWP.HunspellSuggestions s = this.hunspell.GetSuggestions(key);
            if (s.Count() == 0) { return null; }
            string suggestion = s.Get(0);
            if (cardByName.ContainsKey(suggestion))
            {
                return cardByName[suggestion];
            }
            else
            {
                return null;
            }
        }
        
        public Card Get(int id)
        {
            return cardById.ContainsKey(id) ? cardById[id] : null;
        }

        public Set GetSet(int id)
        {
            if (!cardById.ContainsKey(id)) { return Set.None; }
            return cardById[id].GetSet(id);
        }

        public List<Card> AllCards()
        {
            return cardByName.Values.ToList();
        }

        public async void Build()
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
                ProcessSet(jsonData.M15, Set.Magic2015);

                await BuildHunspellDictionary();

                Debug.WriteLine("Finished processing card data");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while building card database : " + e.Message);
            }
            this.Ready = true;
            OnDatabaseUpdated?.Invoke(1f);
        }

        private void ProcessSet(JsonSetData setData, Set set)
        {
            for (int i = 0; i < setData.Cards.Length; ++i)
            {
                if (setData.Cards[i].Rarity == "Basic Land")
                {
                    //Debug.WriteLine($"Ignoring basic land '{setData.Cards[i].Name}");
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
        }

        private async Task BuildHunspellDictionary()
        {
            StorageFolder folder = ApplicationData.Current.LocalCacheFolder;
            IStorageItem dictTest = await folder.TryGetItemAsync("hunspell_dict.txt");
            StorageFile dict;
            if (dictTest == null)
            {
                List<string> distinct = cardByName.Keys.Select(n => n.Replace(" ", "")).ToList();
                dict = await folder.CreateFileAsync("hunspell_dict.txt");
                IRandomAccessStream stream = await dict.OpenAsync(FileAccessMode.ReadWrite);
                using (IOutputStream oStream = stream.GetOutputStreamAt(0))
                {
                    using (DataWriter writer = new DataWriter(oStream))
                    {
                        writer.WriteString($"{distinct.Count}\n");
                        for (int i = 0; i < distinct.Count; ++i)
                        {
                            writer.WriteString($"{distinct[i]}\n");
                        }
                        await writer.StoreAsync();
                        await writer.FlushAsync();
                        writer.DetachStream();
                    }
                }
            }
            else
            {
                dict = dictTest as StorageFile;
            }

            IStorageItem affixTest = await folder.TryGetItemAsync("hunspell_affix.txt");
            StorageFile affix;
            if (affixTest == null)
            {
                StorageFile affixAsset = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/hunspell_affix.txt"));
                affix = await affixAsset.CopyAsync(folder);
            }
            else
            {
                affix = affixTest as StorageFile;
            }
            Debug.WriteLine($"Building hunspell from {affix.Path} and {dict.Path}");
            this.hunspell = new Hunspell_UWP.HunspellWrapper(dict.Path, affix.Path);
        }
    }
}