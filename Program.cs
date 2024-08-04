using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using SkyrimDescriber;

var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(openAiKey))
{
    Console.WriteLine("Need environment variable: OPENAI_API_KEY");
    return;
}

if (args.Length == 0)
{
    ShowUsage();
    return;
}

var db = new SkyrimDescriberContext();

ProcessingMode mode = ProcessingMode.Add;
if (args[0] == "--export")
{
    Export(args[1]);
    return;
}

if (args.Length < 6)
{
    ShowUsage();
    return;
}

// find the mode we're in
switch (args[1])
{
    case "add":
        mode = ProcessingMode.Add;
        break;
    case "update":
        mode = ProcessingMode.Update;
        break;
    case "upsert":
        mode = ProcessingMode.Upsert;
        break;
}

Console.ForegroundColor = ConsoleColor.Blue;

string categoryName = "";
List<string> namesToProcess = [];

// get the category and the names
switch (args[2])
{
    case "--category":
        categoryName = args[3];
        namesToProcess = LoadNames(args[5]);
        break;
    case "--names":
        categoryName = args[5];
        namesToProcess = LoadNames(args[3]);
        break;
}

// make sure the category exists
var categoryEntry = db.Locations.Find(categoryName);
if (categoryEntry == null)
{
    categoryEntry = new Location
    {
        Name = categoryName,
        Npcs = []
    };
    db.Add(categoryEntry);
}

Console.WriteLine("Processing names...");
int numberAdded = 0;
int numberUpdated = 0;
int numberSkipped = 0;
foreach (var name in namesToProcess)
{
    var theNpc = db.Npcs.Find(name);
    // check we don't have them yet
    if (mode == ProcessingMode.Add && theNpc != null)
    {
        numberSkipped++;
        continue;
    }
    else if (mode == ProcessingMode.Update && theNpc == null)
    {
        numberSkipped++;
        continue;
    }

    if (theNpc == null)
    {
        Console.WriteLine($" > Adding {name}");
        // we're adding
        theNpc = new Npc
        {
            Name = name,
            Description = GetCharacterDefinition(name),
            Location = categoryEntry
        };
        db.Npcs.Add(theNpc);
        numberAdded++;
    }
    else
    {
        Console.WriteLine($" > Updating {name}");
        // we're updating or upserting
        theNpc.Description = GetCharacterDefinition(name);
        numberUpdated++;
    }
}
db.SaveChanges();
Console.WriteLine($"Added {numberAdded} new character{(numberAdded != 1 ? "s" : "")}, updated {numberUpdated}, skipped {numberSkipped}.");
Console.ForegroundColor = ConsoleColor.White;


void Export(string folder)
{
    var directory = Directory.CreateDirectory(folder);
    var locations = db.Locations.Include(l => l.Npcs).ToList();
    var options = new JsonSerializerOptions
    {
        WriteIndented = true
    };
    foreach (var location in locations)
    {
        if (String.IsNullOrEmpty(location.Name))
        {
            continue;
        }
        // create the location folder
        var locationFolder = directory.CreateSubdirectory(location.Name.ToLower());
        foreach (var npc in location.Npcs)
        {
            var npcFolder = locationFolder.CreateSubdirectory(npc.Name.ToLower());
            var npcJson = JsonSerializer.Serialize(new
            {
                id = npc.Name,
                name = npc.Name.ToUpper(),
                description = npc.Description
            }, options);
            string npcFileName = Path.Combine(npcFolder.FullName, $"{npc.Name.ToLower()}.json");
            File.WriteAllText(npcFileName, npcJson);
        }
    }
}

string GetCharacterDefinition(string characterName)
{
    ChatClient client = new(model: "gpt-4o", openAiKey);

    ChatCompletion completion = client.CompleteChat(
        new SystemChatMessage("You are a narrator in an adult role playing game, based on Skyrim Special Edition. Your role is to concisely describe a character in the game. Use concise but engaging language, write in the third person, and act as if the user is a player in the high fantasy role playing game Skyrim."),
        $"Describe the character '{characterName}' from Skyrim Special Edition.");

    return completion.ToString();
}

List<string> LoadNames(string input)
{
    Console.WriteLine("Loading names...");
    var namesToProcess = new List<string>();
    if (!input.Contains(',') && File.Exists(input))
    {
        var lines = File.ReadAllLines(input);
        Console.WriteLine($" > Found {lines.Length} names in file.");
        namesToProcess.AddRange(lines.Select(l => l.Trim()));
    }
    else
    {
        var names = input.Split(',');
        Console.WriteLine($" > Using {names.Length} names from command line.");
        namesToProcess.AddRange(names.Select(n => n.Trim()));
    }
    return namesToProcess;
}

void ShowUsage()
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(@"Usage:
    skyrim-describer --mode add|update|upsert --category categoryName --names name1,name2|namefile
    skyrim-describer --export [folder]");
}

enum ProcessingMode
{
    Add,
    Update,
    Upsert
}
