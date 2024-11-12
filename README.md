![consolescroller](https://github.com/jchristn/consolescroller/blob/main/assets/icon.ico)

# ConsoleScroller

[![NuGet Version](https://img.shields.io/nuget/v/ConsoleScroller.svg?style=flat)](https://www.nuget.org/packages/ConsoleScroller/) [![NuGet](https://img.shields.io/nuget/dt/ConsoleScroller.svg)](https://www.nuget.org/packages/ConsoleScroller)    

Need a simple way to constrain long output of a console app to a specific scrolling number of lines? Yep, it does that.

For best results, ensure nothing else is using `Console` while you have and are using a `Scroller` object.

## New in v1.0.0

- Initial release

## Help or Feedback

Need help or have feedback? Please file an issue here!

## Simple Example

```csharp
using ConsoleScroller;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using (var scroller = new Scroller(5))
{
  for (int i = 1; i <= 10; i++)
      Console.WriteLine("Line " + i);
}

Console.WriteLine($"Done!");
```

## Version History

Please refer to CHANGELOG.md.
