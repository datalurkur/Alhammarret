using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using System.Windows.Controls;
using System.Windows;
using Alhammaret.ViewModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Alhammaret.View
{
    public sealed partial class VariableUserControl : UserControl
    {
        public VariableUserControl()
        {
            this.InitializeComponent();
        }

        public void IncreaseVal(object sender, RoutedEventArgs e)
        {
            VariableControl control = (this.DataContext as VariableControl);
            if (control != null) { control.IncreaseVal(); }
        }

        public void DecreaseVal(object sender, RoutedEventArgs e)
        {
            VariableControl control = (this.DataContext as VariableControl);
            if (control != null) { control.DecreaseVal(); }
        }
    }
}
