namespace Test
{
    using ConsoleScroller;

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            using (var scroller = new Scroller(5))
            {
                for (int i = 1; i <= 10; i++)
                {
                    string text = (i % 3) switch
                    {
                        0 => $"Success: Line {i} {Symbols.Check}",
                        1 => $"Error: Line {i} {Symbols.Cross}",
                        _ => $"Info: Line {i} {Symbols.Arrow}"
                    };

                    ConsoleColor color = (i % 3) switch
                    {
                        0 => ConsoleColor.Green,
                        1 => ConsoleColor.Red,
                        _ => ConsoleColor.Cyan
                    };

                    scroller.WriteLine(text, color);
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine($"Done! {Symbols.Check}");
        }
    }
}
