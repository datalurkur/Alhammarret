using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Alhammarret
{
    public class CardCollection
    {
        public class Card
        {
            public int Foil { get; set; }
            public int Count { get; set; }
            public int Id { get; set; }

            public Card(int id)
            {
                this.Id = id;
                this.Count = 0;
                this.Foil = 0;
            }
        }

        private static CardCollection instance;
        public static CardCollection Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CardCollection();
                }
                return instance;
            }
        }

        public delegate void CollectionUpdated();
        public CollectionUpdated OnCollectionUpdated;

        private Dictionary<int, Card> cards;

        public bool Ready { get; private set; }

        public CardCollection(bool ready = false)
        {
            this.Ready = ready;
            this.cards = new Dictionary<int, Card>();
        }

        public List<Card> AllCards()
        {
            return this.cards.Values.ToList();
        }

        public int GetCount(int id)
        {
            return this.cards.ContainsKey(id) ? this.cards[id].Count : 0;
        }

        public void AddCard(int id, int count=1, int foilCount=0)
        {
            if (!this.cards.ContainsKey(id))
            {
                this.cards[id] = new Card(id);
            }
            this.cards[id].Count += count;
            this.cards[id].Foil += foilCount;
            OnCollectionUpdated?.Invoke();
        }

        public void ReduceCard(int id, int count=1, int foilCount=0)
        {
            if (this.cards.ContainsKey(id))
            {
                this.cards[id].Count -= count;
                this.cards[id].Foil -= foilCount;
                if (this.cards[id].Count <= 0)
                {
                    this.cards.Remove(id);
                }
                OnCollectionUpdated?.Invoke();
            }
        }

        public void RemoveCard(int id)
        {
            if (this.cards.ContainsKey(id))
            {
                this.cards.Remove(id);
                OnCollectionUpdated?.Invoke();
            }
        }

        public void Import()
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "Data|*.data";
            //fileDialog.InitialDirectory = Settings.Instance.GetString("ImportPath");
            fileDialog.Title = "Load Database File";
            var fileResult = fileDialog.ShowDialog();
            if (fileResult == true)
            {
                string file = fileDialog.FileName;
                // FIXME - Store this off somewhere
                //Settings.Instance.SetString("ImportPath", file);
                if (Load(file))
                {
                    this.Ready = true;
                }
            }
            else
            {
                this.Ready = true;
            }
            OnCollectionUpdated?.Invoke();
        }

        public bool DefaultImport()
        {
            /*
            string importPath = Settings.Instance.GetString("ImportPath");
            if (importPath == null) { return false; }
            bool success = Load(importPath);
            if (success)
            {
                this.Ready = true;
                OnCollectionUpdated?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
            */
            // FIXME - Once we start storing the load path somewhere, try to load it here first
            return false;
        }

        public void Export()
        {
            var fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.Filter = "Data|*.data";
            //fileDialog.InitialDirectory = Settings.Instance.GetString("ImportPath");
            fileDialog.Title = "Save Database File";
            var fileResult = fileDialog.ShowDialog();
            if (fileResult == true)
            {
                // FIXME
                //Settings.Instance.SetString("ImportPath", fileDialog.FileName);
                Save(fileDialog.FileName);
            }
        }

        public bool DefaultExport()
        {
            // FIXME
            /*
            string exportPath = Settings.Instance.GetString("ImportPath");
            if (exportPath == null) { return false; }
            Save(exportPath);
            return true;
            */
            return false;
        }

        private bool Load(string file)
        {
            try
            {
                int total = 0;
                string jsonStr = File.ReadAllText(file);
                Card[] jsonData = JsonConvert.DeserializeObject<Card[]>(jsonStr);
                for (int i = 0; i < jsonData.Length; ++i)
                {
                    Card c = jsonData[i];
                    this.cards[c.Id] = c;
                    total += c.Count;
                }
                Debug.WriteLine($"Loaded {total} cards from collection ({jsonData.Length} unique)");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while loading card collection : " + e.Message);
            }
            return false;
        }

        public void Save(string file)
        {
            Debug.WriteLine("Saving");
            Card[] jsonData = this.cards.Values.ToArray();
            string jsonStr = JsonConvert.SerializeObject(jsonData);
            
            File.WriteAllText(file, jsonStr);
            Debug.WriteLine("Done saving");
        }
    }
}
