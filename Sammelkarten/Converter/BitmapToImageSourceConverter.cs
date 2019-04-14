using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sammelkarten {

    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    internal class BitmapToImageSourceConverter : IValueConverter {

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            using (var memory = new MemoryStream()) {
                var bitmap = value as Bitmap;
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

        #endregion Methods
    }

    //[ValueConversion(typeof(Card), typeof(ImageSource))]
    //internal class CardImageConverter : IValueConverter {
    //    #region Methods

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    //        if (value is Card card) {
    //            //var biImg = new BitmapImage();
    //            //biImg.CacheOption = BitmapCacheOption.OnDemand;
    //            //biImg.DownloadCompleted += BiImg_DownloadCompleted;
    //            //var ms = new MemoryStream(card);
    //            card.SetImageSource();

    //            var imgSrc = card.ImageStream as ImageSource;

    //            return imgSrc;
    //        }
    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    //        return null;
    //    }

    //    private void BiImg_DownloadCompleted(object sender, EventArgs e) {
    //        // throw new NotImplementedException();
    //    }

    //    #endregion Methods
    //}
}