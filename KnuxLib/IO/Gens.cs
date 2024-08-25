using KnuxLib.IO.Headers;

namespace KnuxLib.IO
{
    public class GensReader : ExtendedBinaryReader
    {
        // Constructors
        public GensReader(Stream input, bool isBigEndian = true) :
            base(input, Encoding.ASCII, isBigEndian) { }

        public GensReader(Stream input, Encoding encoding,
            bool isBigEndian = true) : base(input, encoding, isBigEndian) { }

        public uint[] ReadFooter()
        {
            uint offsetCount = ReadUInt32();
            var offsets = new uint[offsetCount];

            for (uint i = 0; i < offsetCount; ++i)
            {
                offsets[i] = (ReadUInt32() + Offset);
            }

            return offsets;
        }
    }
}