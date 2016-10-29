using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;
using System.Windows;

namespace Alhammarret.Converters
{ 
    public class BooleanVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? p = value as bool?;
            if (p.HasValue)
            {
                return p.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? p = value as bool?;
            if (p.HasValue)
            {
                return !p.Value;
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntGreaterThanZeroVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? i = value as int?;
            if (i.HasValue)
            {
                return (i.Value > 0) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SetImageConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CardDB.Set? s = value as CardDB.Set?;
            if (s.HasValue)
            {
                switch (s.Value)
                {
                    case CardDB.Set.EldritchMoon:
                        return "ms-appx:///Assets/set_emn.png";
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

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}