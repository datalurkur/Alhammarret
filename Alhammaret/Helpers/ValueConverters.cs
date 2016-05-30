using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Alhammaret.View
{ 
    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? p = value as bool?;
            if (p.HasValue)
            {
                return p.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? p = value as bool?;
            if (p.HasValue)
            {
                return !p.Value;
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IntGreaterThanZeroVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int? i = value as int?;
            if (i.HasValue)
            {
                return (i.Value > 0) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SetImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            CardDB.Set? s = value as CardDB.Set?;
            if (s.HasValue)
            {
                switch (s.Value)
                {
                    case CardDB.Set.ShadowsOverInnistrad:
                        return "ms-appx:///Assets/set_soi.png";
                    case CardDB.Set.OathOfTheGatewatch:
                        return "ms-appx:///Assets/set_ogw.png";
                    case CardDB.Set.BattleForZendikar:
                        return "ms-appx:///Assets/set_bfz.png";
                    case CardDB.Set.MagicOrigins:
                        return "ms-appx:///Assets/set_ori.png";
                    case CardDB.Set.DragonsOfTarkir:
                        return "ms-appx:///Assets/set_dtk.png";
                    case CardDB.Set.FateReforged:
                        return "ms-appx:///Assets/set_frf.png";
                    case CardDB.Set.KhansOfTarkir:
                        return "ms-appx:///Assets/set_ktk.png";
                    case CardDB.Set.Magic2015:
                        return "ms-appx:///Assets/set_m15.png";
                }
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}