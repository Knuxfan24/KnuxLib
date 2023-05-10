using System.Text;

namespace KnuxTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Enable shift-jis for HedgeLib# stuff.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Set the console's output to UTF8 rather than ASCII.
            Console.OutputEncoding = Encoding.UTF8;
        }
    }
}