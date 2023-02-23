using System.Diagnostics;
using System.Text;

namespace KnuxTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Enable shift-jis for HedgeLib# stuff.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}