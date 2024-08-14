﻿namespace GuildWarsPartySearch.Server.Options;

public class TextProcessorOptions
{
    public List<string> ChatFilterRegexes { get; set; } = [
        "[Μm]+[o0]+g+k+",
        "g\\S?[o0]\\S?l\\S?d\\S?a\\S?a\\S?c\\S?[0o]\\S?[Μm]",
        "[Μm]+[0o]game",
        "gvgmall",
        "g?ameblack",
        "g?amersmarket",
        "g?ameblack",
        "gw1sh[o0]p",
        "gwmmsale",
        "gw2sale",
        "gw2swap",
        "m\\S?m\\S?[o0]\\S?p\\S?i\\S?x\\S?e\\S?l",
        "mygwamm",
        "accounts?"
        ];
}