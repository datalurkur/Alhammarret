using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Alhammaret.View
{
    public sealed partial class CardManaBlock : UserControl
    {
        private static readonly string[] manaCircleResources = new string[]
        {
            "ms-appx:///Assets/mana_black.png",
            "ms-appx:///Assets/mana_blue.png",
            "ms-appx:///Assets/mana_colorless.png",
            "ms-appx:///Assets/mana_background.png",
            "ms-appx:///Assets/mana_green.png",
            "ms-appx:///Assets/mana_red.png",
            "ms-appx:///Assets/mana_background.png",
            "ms-appx:///Assets/mana_white.png"
        };
        private static BitmapImage[] manaCircle = new BitmapImage[(int)CardDB.Color.NumColors];
        private static BitmapImage GetManaCircle(CardDB.Color color)
        {
            int index = (int)color;
            if (manaCircle[index] == null)
            {
                manaCircle[index] = new BitmapImage(new Uri(manaCircleResources[index]));
            }
            return manaCircle[index];
        }

        public CardManaBlock()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            PopulateManaBlock();
        }

        private void PopulateManaBlock()
        {
            CardDB.Mana mana = this.DataContext as CardDB.Mana;
            if (mana == null) { return; }

            this.ManaContainer.Children.Clear();
            if (mana.Generic > 0)
            {
                AddMana(CardDB.Color.Generic, mana.Generic.ToString());
            }
            AddMultipleMana(mana.Scalar, CardDB.Color.Scalar, "X");
            AddMultipleMana(mana.Colorless, CardDB.Color.Colorless, "X");
            AddMultipleMana(mana.Black, CardDB.Color.Black, null);
            AddMultipleMana(mana.Blue, CardDB.Color.Blue, null);
            AddMultipleMana(mana.Green, CardDB.Color.Green, null);
            AddMultipleMana(mana.Red, CardDB.Color.Red, null);
            AddMultipleMana(mana.White, CardDB.Color.White, null);
        }

        private void AddMultipleMana(int count, CardDB.Color color, string overlay)
        {
            for (int i = 0; i < count; ++i) { AddMana(color, null); }
        }

        private void AddMana(CardDB.Color color, string overlay)
        {
            Image image = new Image();
            image.Width = 20;
            image.Height = 20;
            image.Source = GetManaCircle(color);

            if (overlay != null)
            {
                Grid grid = new Grid();
                grid.Children.Add(image);

                TextBlock text = new TextBlock();
                text.Text = overlay;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                grid.Children.Add(text);

                this.ManaContainer.Children.Add(grid);
            }
            else
            {
                this.ManaContainer.Children.Add(image);
            }
        }
    }
}
