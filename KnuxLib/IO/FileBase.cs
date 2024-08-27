namespace KnuxLib.IO
{
    /// <summary>
    /// The base for all file-based classes in KnuxLib, used to simplify JSON parsing.
    /// </summary>
    public class FileBase
    {
        public static void JsonSerialise(string filePath, object data) => File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));

        public static T JsonDeserialise<T>(string filePath) => JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
    }
}