namespace KnuxTools
{
    public class Helpers
    {
        /// <summary>
        /// Colours the console for a string.
        /// </summary>
        /// <param name="message">The message to print in red.</param>
        /// <param name="isLine">Whether the message should be a WriteLine or a standard Write.</param>
        /// <param name="color">The colour to use, defaulting to red.</param>
        public static void ColourConsole(string message, bool isLine = true, ConsoleColor color = ConsoleColor.Red)
        {
            // Colour the console.
            Console.ForegroundColor = color;

            // Write the message depending on line type.
            if (isLine)
                Console.WriteLine(message);
            else
                Console.Write(message);

            // Return the console colour to grey.
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Asks the user to select from a valid list of formats.
        /// </summary>
        /// <param name="noticeLine">The message to print to the user.</param>
        /// <param name="formats">The valid formats, with the boolean being used to determine if the text should be coloured red or not.</param>
        /// <param name="userInput">The type to print when asking the user for input.</param>
        public static void VersionChecker(string noticeLine, Dictionary<string, bool> formats, string userInput = "Format Type")
        {
            // Check if we don't already have a version specified.
            if (string.IsNullOrEmpty(Program.Version))
            {
                // Inform the user of the need for a version value.
                Console.WriteLine(noticeLine);

                // Loop through each format in the dictionary and print it.
                foreach (KeyValuePair<string, bool> format in formats)
                {
                    // Colour the text red if this key pair is true.
                    if (format.Value)
                        ColourConsole($"    {format.Key}");

                    // Just write the text normally is this key pair is false.
                    else
                        Console.WriteLine($"    {format.Key}");
                }

                // Ask for the user's input.
                Console.Write($"\n{userInput}: ");

                // Wait for the user's input.
                Program.Version = Console.ReadLine().ToLower();

                // Sanity check the input, inform the user if its still empty or null. If not, add a line break.
                if (string.IsNullOrEmpty(Program.Version))
                {
                    Console.WriteLine($"\nNo {userInput.ToLower()} specified! Aborting...\nPress any key to continue.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }
    
        /// <summary>
        /// Asks the user to select from a valid list of extensions.
        /// </summary>
        /// <param name="extensions">The valid extensions, with the key being the extension and the value being a description.</param>
        public static void ExtensionChecker(Dictionary<string, string> extensions)
        {
            // Check if we don't already have an extension specified.
            if (string.IsNullOrEmpty(Program.Extension))
            {
                // Inform the user of the need for an extension value.
                Console.WriteLine("This file has multiple file extension options, please select the extension to save with:");

                // Set up a value to hold the index of the extensions. 
                int extensionIndex = 1;

                // Loop through each extension.
                foreach (KeyValuePair<string, string> extension in extensions)
                {
                    // Print our current extension index, the extension and its description.
                    Console.WriteLine($"{extensionIndex}. {extension.Key} ({extension.Value})");

                    // Increment the extension index.
                    extensionIndex++;
                }

                // Wait for the user to input a character, then parse it into extension index.
                bool validKeyPress = int.TryParse(Console.ReadKey().KeyChar.ToString(), out extensionIndex);

                // Decrement extension index, as we count from 1 rather than 0 for this.
                extensionIndex--;

                // Check that the character was valid and that our extension dictionary actually has this index.
                // If so, then set the extension to the key at this index.
                if (validKeyPress && extensionIndex < extensions.Count)
                    Program.Extension = extensions.ElementAt(extensionIndex).Key;

                // Sanity check the input, inform the user and abort if its still null or empty.
                if (string.IsNullOrEmpty(Program.Extension))
                {
                    Console.WriteLine("\nNo format extension specified! Aborting...\nPress any key to continue.");
                    Console.ReadKey();
                    return;
                }

                // Add a line break.
                Console.WriteLine();
            }
        }
    
        /// <summary>
        /// Tells the user that the format identifier given is invalid.
        /// </summary>
        /// <param name="fileType">The type of file the user was trying to process.</param>
        public static void InvalidFormatVersion(string fileType)
        {
            Console.WriteLine($"Format identifer '{Program.Version}' is not valid for any currently supported {fileType} types.\nPress any key to continue.");
            Console.ReadKey();
        }
    }
}
