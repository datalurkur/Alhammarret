using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Newtonsoft.Json;

namespace Alhammaret
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

        public async void Import()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".data");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                bool success = await Load(file);
                if (success)
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

        public async void Export()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("JSON data", new List<string>() { ".data" });
            picker.SuggestedFileName = "alhammarret";
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await Save(file);
            }
        }

        private async Task<bool> Load(StorageFile file)
        {
            try
            {
                int total = 0;
                string jsonStr = await FileIO.ReadTextAsync(file);
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

        public async Task Save(StorageFile file)
        {
            Debug.WriteLine("Saving");
            Card[] jsonData = this.cards.Values.ToArray();
            string jsonStr = JsonConvert.SerializeObject(jsonData);
            
            await FileIO.WriteTextAsync(file, jsonStr);
            Debug.WriteLine("Done saving");
        }
    }
}
