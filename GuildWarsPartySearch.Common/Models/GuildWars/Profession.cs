using GuildWarsPartySearch.Common.Converters;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Common.Models.GuildWars;

[JsonConverter(typeof(ProfessionJsonConverter))]
[TypeConverter(typeof(ProfessionTypeConverter))]
public sealed class Profession
{
    public static readonly Profession None = new() { Name = "None", Id = 0, Alias = "Any" };
    public static readonly Profession Warrior = new()
    {
        Name = "Warrior",
        Id = 1,
        Alias = "W",
    };
    public static readonly Profession Ranger = new()
    {
        Name = "Ranger",
        Id = 2,
        Alias = "R",
    };
    public static readonly Profession Monk = new()
    {
        Name = "Monk",
        Id = 3,
        Alias = "Mo",
    };
    public static readonly Profession Necromancer = new()
    {
        Name = "Necromancer",
        Alias = "N",
        Id = 4,
    };
    public static readonly Profession Mesmer = new()
    {
        Name = "Mesmer",
        Id = 5,
        Alias = "Me",
    };
    public static readonly Profession Elementalist = new()
    {
        Name = "Elementalist",
        Id = 6,
        Alias = "E",
    };
    public static readonly Profession Assassin = new()
    {
        Name = "Assassin",
        Id = 7,
        Alias = "A",
    };
    public static readonly Profession Ritualist = new()
    {
        Name = "Ritualist",
        Id = 8,
        Alias = "Rt",
    };
    public static readonly Profession Paragon = new()
    {
        Name = "Paragon",
        Id = 9,
        Alias = "P",
    };
    public static readonly Profession Dervish = new()
    {
        Name = "Dervish",
        Id = 10,
        Alias = "D",
    };
    public static IEnumerable<Profession> Professions = new List<Profession>
    {
        None,
        Warrior,
        Ranger,
        Monk,
        Necromancer,
        Mesmer,
        Elementalist,
        Assassin,
        Ritualist,
        Paragon,
        Dervish
    };
    public static bool TryParse(int id, out Profession profession)
    {
        profession = Professions.Where(prof => prof.Id == id).FirstOrDefault()!;
        if (profession is null)
        {
            return false;
        }

        return true;
    }
    public static bool TryParse(string name, out Profession profession)
    {
        profession = Professions.Where(prof => prof.Name == name).FirstOrDefault()!;
        if (profession is null)
        {
            return false;
        }

        return true;
    }
    public static Profession Parse(int id)
    {
        if (TryParse(id, out var profession) is false)
        {
            throw new InvalidOperationException($"Could not find a profession with id {id}");
        }

        return profession;
    }
    public static Profession Parse(string name)
    {
        if (TryParse(name, out var profession) is false)
        {
            throw new InvalidOperationException($"Could not find a profession with name {name}");
        }

        return profession;
    }

    public string? Alias { get; init; }
    public string? Name { get; init; }
    public int Id { get; set; }
    private Profession()
    {
    }

    public override string ToString()
    {
        return Name!;
    }
}
