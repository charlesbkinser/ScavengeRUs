using ScavengeRUs.Data;
using System;
using System.Linq;

public class CodeGenerator
{
    private static readonly Random _random = new Random();
    /// <summary>
    /// A reference to the database context that will be needed to GenerateCode method
    /// </summary>
    private readonly ApplicationDbContext _context;

    private static readonly List<string> _words = new List<string> { 
        "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", 
        "Juliet", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", 
        "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "Xray", "Yankee", "Zulu", 
        "Apple", "Berry", "Cherry", "Date", "Elderberry", "Fig", "Grape", "Honeydew", 
        "Ivy", "Jackfruit", "Kiwi", "Lemon", "Mango", "Nectarine", "Orange", "Peach", 
        "Quince", "Raspberry", "Strawberry", "Tomato", "Ugli", "Vanilla", "Watermelon", 
        "Ximenia", "Yam", "Zucchini", "Azure", "Blue", "Cyan", "Denim", "Emerald", 
        "Fuchsia", "Gold", "Harlequin", "Ivory", "Jade", "Khaki", "Lavender", "Magenta", 
        "Navy", "Olive", "Purple", "Quartz", "Red", "Silver", "Teal", "Umbra", "Violet", 
        "White", "Yellow", "Zaffre"
        };


    public CodeGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates a unique access code by combining a random word from the list above with a random number between 100 and 999 while ensuring that the code is unique.
    /// by checking the database to see if the code already exists.
    /// </summary>
    /// <returns>A unique access code.</returns>
    /// <exception cref="CodeGenerationException">Thrown when an error occurs during code generation.</exception>
    public string GenerateUniqueCode()
    {
        try
        {
            string code;
            do
            {
                string word1 = _words[_random.Next(_words.Count)];
                string word2 = _words[_random.Next(_words.Count)]; // Second word
                int number = _random.Next(100, 10000); // Larger range
                code = word1 + word2 + number.ToString();
            } while (_context.AccessCodes.Any(ac => ac.Code == code));

            return code;
        }
        catch (Exception ex)
        {
            throw new CodeGenerationException("Error occurred during access code generation.", ex);
        }
    }

}

public class CodeGenerationException : Exception
{
    public CodeGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
