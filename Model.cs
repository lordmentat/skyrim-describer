using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace SkyrimDescriber;

public class SkyrimDescriberContext : DbContext
{

    public DbSet<Location> Locations { get; set; }
    public DbSet<Npc> Npcs { get; set; }

    public string DbPath { get; }

    public SkyrimDescriberContext()
    {
        //var folder = Environment.SpecialFolder.LocalApplicationData;
        //var path = Environment.GetFolderPath(folder);
        //DbPath = System.IO.Path.Join(path, "skyrimdescriber.db");
        DbPath = "skyrimdescriber.db";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}

[PrimaryKey(nameof(Name))]
public class Location
{
    public required string Name { get; set; }
    public List<Npc> Npcs { get; set; } = [];
}

[PrimaryKey(nameof(Name))]
public class Npc
{
    public required string Name { get; set; }

    public required string Description { get; set; }

    [JsonIgnore]
    public Location? Location { get; set; }
}