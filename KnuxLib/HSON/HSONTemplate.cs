using Newtonsoft.Json;

namespace KnuxLib.HSON
{
    public class HSONTemplate
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public HSONTemplate() { }
        public HSONTemplate(string filepath) => Data = JsonConvert.DeserializeObject<FormatData>(File.ReadAllText(filepath));

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The version of the HSON Template Table.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "version")]
            public int Version { get; set; } = 1;

            /// <summary>
            /// The format identifier for this HSON Template Table.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "format")]
            public string Format { get; set; } = "gedit_v3";

            /// <summary>
            /// The object enums this HSON Template Table has.
            /// </summary>
            [JsonProperty(Order = 3, PropertyName = "enums")]
            public Dictionary<string, HSONEnum> Enums { get; set; } = new();

            /// <summary>
            /// The object structures this HSON Template Table has.
            /// </summary>
            [JsonProperty(Order = 4, PropertyName = "structs")]
            public Dictionary<string, HSONStruct> Structs { get; set; } = new();

            /// <summary>
            /// The objects this HSON Template Table has.
            /// </summary>
            [JsonProperty(Order = 5, PropertyName = "objects")]
            public Dictionary<string, HSONObject> Objects { get; set; } = new();
        }

        public class HSONEnum
        {
            /// <summary>
            /// This enum's data type.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "type")]
            public string Type { get; set; } = "int8";

            /// <summary>
            /// The values this enum can have.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "values")]
            public Dictionary<string, HSONEnumValue> Values { get; set; } = new();
        }
        public class HSONEnumValue
        {
            /// <summary>
            /// The value this enum corresponds to.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "value")]
            public int Value { get; set; }

            /// <summary>
            /// The descriptions for this enum, by language then description.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "descriptions")]
            public Dictionary<string, string> Descriptions { get; set; } = new();
        }

        public class HSONStruct
        {
            /// <summary>
            /// The name of the struct that this struct is a child of (if applicable).
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "parent")]
            public string? ParentStruct { get; set; }

            // The object parameters this struct can have.
            [JsonProperty(Order = 2, PropertyName = "fields")]
            public HSONStructField[]? Fields { get; set; }
        }
        public class HSONStructField
        {
            /// <summary>
            /// The name of this object parameter.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "name")]
            public string Name { get; set; } = "";

            /// <summary>
            /// The data type of this object parameter.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "type")]
            public string Type { get; set; } = "";

            /// <summary>
            /// How many bytes this parameter's data should be aligned to in the SET file.
            /// </summary>
            [JsonProperty(Order = 3, PropertyName = "alignment")]
            public int Alignment { get; set; }

            /// <summary>
            /// The lowest value this parameter's data can have.
            /// </summary>
            [JsonProperty(Order = 4, PropertyName = "min_range")]
            public object? MinimumRange { get; set; }

            /// <summary>
            /// The highest value this parameter's data can have.
            /// </summary>
            [JsonProperty(Order = 5, PropertyName = "max_range")]
            public object? MaximumRange { get; set; }

            /// <summary>
            /// How much each addition or subtraction of this parameter's data should be.
            /// </summary>
            [JsonProperty(Order = 6, PropertyName = "step")]
            public object? Step { get; set; }

            /// <summary>
            /// The descriptions for this parameter, by language then description.
            /// </summary>
            [JsonProperty(Order = 7, PropertyName = "descriptions")]
            public Dictionary<string, string> Descriptions { get; set; } = new();

            public override string ToString() => Name;
        }

        public class HSONObject
        {
            /// <summary>
            /// The struct that represents this object.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "struct")]
            public string ObjectStruct { get; set; } = "";

            /// <summary>
            /// The category this object should be placed in.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "category")]
            public string Category { get; set; } = "";

            public override string ToString() => ObjectStruct;
        }

        // Actual data presented to the end user.
        public FormatData Data = new();
    }
}
