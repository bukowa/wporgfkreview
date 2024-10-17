using System.Text.Json;
using wporgfkreview.WordpressOrg;

// read url from console argument
if (args.Length == 0)
{
    Console.WriteLine("Please provide a url to scrape.");
    return;
}

var url = args[0];
var plugins = Scrape.Parse(url);
Scrape.ParseAuthors(plugins);
var json = JsonSerializer.Serialize(plugins, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("plugins.json", json);
