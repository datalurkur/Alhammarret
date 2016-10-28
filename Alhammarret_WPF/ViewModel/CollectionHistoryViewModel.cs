using Alhammaret;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Alhammaret.ViewModel
{
    class CollectionTransaction
    {
        public int CardId;
        public int Count;
        public int FoilCount;

        public bool Executed;

        public CollectionTransaction(int id, int count, int foil)
        {
            this.CardId = id;
            this.Count = count;
            this.FoilCount = foil;

            this.Executed = false;
        }

        public void Execute()
        {
            if (this.Executed) { return; }
            CardCollection.Instance.AddCard(this.CardId, this.Count, this.FoilCount);
            this.Executed = true;
        }

        public void Rollback()
        {
            if (!this.Executed) { return; }
            CardCollection.Instance.ReduceCard(this.CardId, this.Count, this.FoilCount);
            this.Executed = false;
        }
    }

    class CollectionHistoryViewModel : INotifyPropertyChanged
    {
        public CollectionHistoryViewModel()
        { }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}