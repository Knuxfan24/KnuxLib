namespace KnuxLib.IO
{
    /// <summary>
    /// The base for all file-based classes in KnuxLib.
    /// </summary>
    public class FileBase
    {
        public void JsonSerialise(string filePath, object data)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public T JsonDeserialise<T>(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }
    }
}