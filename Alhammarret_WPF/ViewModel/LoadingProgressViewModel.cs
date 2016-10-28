using Alhammaret;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Alhammaret.ViewModel
{
    class LoadingProgressViewModel : INotifyPropertyChanged
    {
        private int loadProgress = 0;
        public int LoadProgress
        {
            get { return this.loadProgress; }
            set
            {
                this.loadProgress = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LoadProgress"));
            }
        }

        public LoadingProgressViewModel()
        { }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}