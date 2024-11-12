namespace Test
{
    public static class Symbols
    {
        // Common ASCII alternatives
        public const string Check = "\u221A";      // √ (square root symbol - widely supported)
        public const string Cross = "\u00D7";      // × (multiplication symbol - widely supported)
        public const string Arrow = "->";          // ASCII fallback
        public const string Star = "*";            // ASCII fallback

        // More ornate options if supported
        public const string FancyCheck = "\u2713"; // ✓
        public const string FancyArrow = "\u2192"; // →
        public const string FancyStar = "\u2605";  // ★
    }
}
