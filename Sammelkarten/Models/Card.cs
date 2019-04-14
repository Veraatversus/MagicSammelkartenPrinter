using Newtonsoft.Json;
using NPOI.Util;
using Sammelkarten;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scryfall.API.Models {

    public partial class Card : ViewModelBase, IEquatable<Card> {

        #region Properties

        [JsonIgnore]
        public static double CardHeight { get; set; } = Units.ToPoints(3175000);

        // private Stream _imageStream;
        [JsonIgnore]
        public static double CardWidth { get; set; } = Units.ToPoints(2279015);

        [JsonIgnore]
        public static int CardHeightPixel { get; set; } = 680;

        [JsonIgnore]
        public static int CardWidthPixel { get; set; } = 488;

        //[JsonIgnore]
        //public BitmapImage ImageStream { get; private set; }

        [JsonIgnore]
        public IEnumerable<string> PrintImages {
            get {
                if (ImageUris != null) {
                    switch (SearchViewModel.ImageFormat) {
                        case ImageTypeEnum.Small:
                            return new[] { ImageUris.Small };

                        case ImageTypeEnum.Normal:
                            return new[] { ImageUris.Normal };

                        case ImageTypeEnum.Large:
                            return new[] { ImageUris.Large };

                        case ImageTypeEnum.Png:
                            return new[] { ImageUris.Png };

                        case ImageTypeEnum.ArtCrop:
                            return new[] { ImageUris.ArtCrop };

                        case ImageTypeEnum.BorderCrop:
                            return new[] { ImageUris.BorderCrop };

                        default:
                            return new[] { ImageUris.Normal };
                    }
                }
                else {
                    switch (SearchViewModel.ImageFormat) {
                        case ImageTypeEnum.Small:
                            return CardFaces.Where(face => face.ImageUris?.Small != null).Select(face => face.ImageUris.Small);

                        case ImageTypeEnum.Normal:
                            return CardFaces.Where(face => face.ImageUris?.Normal != null).Select(face => face.ImageUris.Normal);

                        case ImageTypeEnum.Large:
                            return CardFaces.Where(face => face.ImageUris?.Large != null).Select(face => face.ImageUris.Large);

                        case ImageTypeEnum.Png:
                            return CardFaces.Where(face => face.ImageUris?.Png != null).Select(face => face.ImageUris.Png);

                        case ImageTypeEnum.ArtCrop:
                            return CardFaces.Where(face => face.ImageUris?.ArtCrop != null).Select(face => face.ImageUris.ArtCrop);

                        case ImageTypeEnum.BorderCrop:
                            return CardFaces.Where(face => face.ImageUris?.BorderCrop != null).Select(face => face.ImageUris.BorderCrop);

                        default:
                            return CardFaces.Where(face => face.ImageUris?.Normal != null).Select(face => face.ImageUris.Normal);
                    }
                }
            }
        }

        [JsonIgnore]
        public string ShowImage {
            get {
                if (ImageUris != null) {
                    return ImageUris.Small;
                }
                else {
                    return CardFaces.FirstOrDefault()?.ImageUris?.Small;
                }
            }
        }

        [JsonProperty]
        public bool IsInCollection { get; set; }

        [JsonProperty]
        public int Count {
            get => _count;
            set {
                CardCollection.Current.CardCountChanged(this, value);
                _count = value;
                RaisePropertyChanged();
            }
            //if (value != _count) {
            //    _count = value;
            //    if (IsInCollection && _count <= 0) {
            //        CardCollection.Current.CardsToPrint.Remove(this);
            //    }
            //    else if (!IsInCollection && _count >= 1) {
            //        var indexPrint = CardCollection.Current.CardsToPrint.IndexOf(this);
            //        if (indexPrint >= 0) {
            //            var indexSearch = CardCollection.Current.CurrentSearch.Data.IndexOf(this);
            //            if (indexSearch >= 0) {
            //                var card = CardCollection.Current.CardsToPrint[indexPrint];

            //                CardCollection.Current.CurrentSearch.Data[indexSearch] = card;
            //            }
            //        }
            //        else {
            //            CardCollection.Current.CardsToPrint.Add(this);
            //        }
            //    }

            //}
        }

        #endregion Properties

        #region Methods

        public bool Equals(Card other) {
            return Id == other.Id;
        }

        #endregion Methods

        #region Fields

        private int _count;

        #endregion Fields
    }
}