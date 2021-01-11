using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EDITgui
{
   public static class ViewAspects
   {
        public static double pointSize = 2.5;
   
        public static SolidColorBrush cyan = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FFFF"));
        public static SolidColorBrush yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3FF00"));
        public static SolidColorBrush silver = System.Windows.Media.Brushes.Silver; 
        public static SolidColorBrush magenta = System.Windows.Media.Brushes.Magenta; 

        public static SolidColorBrush greenMarker = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
        public static SolidColorBrush redMarker = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));


        public static T GetCopy<T>(this T element) where T : System.Windows.UIElement
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.Windows.Markup.XamlWriter.Save(element, ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)System.Windows.Markup.XamlReader.Load(ms);
            }
        }


    }
}
