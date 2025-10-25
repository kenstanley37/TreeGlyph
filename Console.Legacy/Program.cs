namespace TreeGlyphConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var path = args.FirstOrDefault() ?? AskForFolder();
            var exclude = args.Skip(1).ToArray();

            var service = new TreeBuilderService();
            var tree = await service.GenerateTreeAsync(path, exclude);

            Console.WriteLine(tree);
            // Optionally: copy to clipboard, save to file, etc.
        }

        static string AskForFolder()
        {
            Console.Write("Enter folder path: ");
            return Console.ReadLine()?.Trim('"');
        }


    }
}
