using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Sammelkarten {

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageTypeEnum {

        [EnumMember(Value = "Small")]
        Small,

        [EnumMember(Value = "Normal")]
        Normal,

        [EnumMember(Value = "Large")]
        Large,

        [EnumMember(Value = "Png")]
        Png,

        [EnumMember(Value = "ArtCrop")]
        ArtCrop,

        [EnumMember(Value = "BorderCrop")]
        BorderCrop
    }
}