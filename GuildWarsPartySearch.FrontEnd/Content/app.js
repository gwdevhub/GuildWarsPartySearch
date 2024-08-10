var map;
var mapData = {};
var loading = true;

var continent = 1;

const professions = [
    { id: 0, name: "None", alias: "Any" },
    { id: 1, name: "Warrior", alias: "W" },
    { id: 2, name: "Ranger", alias: "R" },
    { id: 3, name: "Monk", alias: "Mo" },
    { id: 4, name: "Necromancer", alias: "N" },
    { id: 5, name: "Mesmer", alias: "Me" },
    { id: 6, name: "Elementalist", alias: "E" },
    { id: 7, name: "Assassin", alias: "A" },
    { id: 8, name: "Ritualist", alias: "Rt" },
    { id: 9, name: "Paragon", alias: "P" },
    { id: 10, name: "Dervish", alias: "D" },
];

const maps = [
    { id: 0, name: "" },
    { id: 1, name: "Gladiator's Arena" },
    { id: 2, name: "DEV Test Arena (1v1)" },
    { id: 3, name: "Test Map" },
    { id: 4, name: "Warrior's Isle" },
    { id: 5, name: "Hunter's Isle" },
    { id: 6, name: "Wizard's Isle" },
    { id: 7, name: "Warrior's Isle" },
    { id: 8, name: "Hunter's Isle" },
    { id: 9, name: "Wizard's Isle" },
    { id: 10, name: "Bloodstone Fen" },
    { id: 11, name: "The Wilds" },
    { id: 12, name: "Aurora Glade" },
    { id: 13, name: "Diessa Lowlands" },
    { id: 14, name: "Gates of Kryta" },
    { id: 15, name: "D'Alessio Seaboard" },
    { id: 16, name: "Divinity Coast" },
    { id: 17, name: "Talmark Wilderness" },
    { id: 18, name: "The Black Curtain" },
    { id: 19, name: "Sanctum Cay" },
    { id: 20, name: "Droknar's Forge" },
    { id: 21, name: "The Frost Gate" },
    { id: 22, name: "Ice Caves of Sorrow" },
    { id: 23, name: "Thunderhead Keep" },
    { id: 24, name: "Iron Mines of Moladune" },
    { id: 25, name: "Borlis Pass" },
    { id: 26, name: "Talus Chute" },
    { id: 27, name: "Griffons Mouth" },
    { id: 28, name: "The Great Northern Wall" },
    { id: 29, name: "Fort Ranik" },
    { id: 30, name: "Ruins of Surmia" },
    { id: 31, name: "Xaquang Skyway" },
    { id: 32, name: "Nolani Academy" },
    { id: 33, name: "Old Ascalon" },
    { id: 34, name: "The Fissure of Woe" },
    { id: 35, name: "Ember Light Camp" },
    { id: 36, name: "Grendich Courthouse" },
    { id: 37, name: "Glint's Challenge" },
    { id: 38, name: "Augury Rock" },
    { id: 39, name: "Sardelac Sanitarium" },
    { id: 40, name: "Piken Square" },
    { id: 41, name: "Sage Lands" },
    { id: 42, name: "Mamnoon Lagoon" },
    { id: 43, name: "Silverwood" },
    { id: 44, name: "Ettin's Back" },
    { id: 45, name: "Reed Bog" },
    { id: 46, name: "The Falls" },
    { id: 47, name: "Dry Top" },
    { id: 48, name: "Tangle Root" },
    { id: 49, name: "Henge of Denravi" },
    { id: 50, name: "Senji's Corner" },
    { id: 52, name: "Burning Isle" },
    { id: 53, name: "Tears of the Fallen" },
    { id: 54, name: "Scoundrel's Rise" },
    { id: 55, name: "Lions Arch" },
    { id: 56, name: "Cursed Lands" },
    { id: 57, name: "Bergen Hot Springs" },
    { id: 58, name: "North Kryta Province" },
    { id: 59, name: "Nebo Terrace" },
    { id: 60, name: "Majesty's Rest" },
    { id: 61, name: "Twin Serpent Lakes" },
    { id: 62, name: "Watchtower Coast" },
    { id: 63, name: "Stingray Strand" },
    { id: 64, name: "Kessex Peak" },
    { id: 65, name: "D'Alessio Arena" },
    { id: 66, name: "Burning Isle" },
    { id: 68, name: "Frozen Isle" },
    { id: 69, name: "Nomads Isle" },
    { id: 70, name: "Druids Isle" },
    { id: 71, name: "Isle of the Dead" },
    { id: 72, name: "The Underworld" },
    { id: 73, name: "Riverside Province" },
    { id: 74, name: "The Hall of Heroes" },
    { id: 76, name: "Broken Tower" },
    { id: 77, name: "House zu Heltzer" },
    { id: 78, name: "The Courtyard" },
    { id: 79, name: "Unholy Temples" },
    { id: 80, name: "Burial Mounds" },
    { id: 81, name: "Ascalon City" },
    { id: 82, name: "Tomb of the Primeval Kings" },
    { id: 83, name: "The Vault" },
    { id: 84, name: "The Underworld" },
    { id: 85, name: "Ascalon Arena" },
    { id: 86, name: "Sacred Temples" },
    { id: 87, name: "Icedome" },
    { id: 88, name: "Iron Horse Mine" },
    { id: 89, name: "Anvil Rock" },
    { id: 90, name: "Lornar's Pass" },
    { id: 91, name: "Snake Dance" },
    { id: 92, name: "Tasca's Demise" },
    { id: 93, name: "Spearhead Peak" },
    { id: 94, name: "Ice Floe" },
    { id: 95, name: "Witman's Folly" },
    { id: 96, name: "Mineral Springs" },
    { id: 97, name: "Dreadnought's Drift" },
    { id: 98, name: "Frozen Forest" },
    { id: 99, name: "Traveler's Vale" },
    { id: 100, name: "Deldrimor Bowl" },
    { id: 101, name: "Regent Valley" },
    { id: 102, name: "The Breach" },
    { id: 103, name: "Ascalon Foothills" },
    { id: 104, name: "Pockmark Flats" },
    { id: 105, name: "Dragon's Gullet" },
    { id: 106, name: "Flame Temple Corridor" },
    { id: 107, name: "Eastern Frontier" },
    { id: 108, name: "The Scar" },
    { id: 109, name: "The Amnoon Oasis" },
    { id: 110, name: "Diviner's Ascent" },
    { id: 111, name: "Vulture Drifts" },
    { id: 112, name: "The Arid Sea" },
    { id: 113, name: "Prophet's Path" },
    { id: 114, name: "Salt Flats" },
    { id: 115, name: "Skyward Reach" },
    { id: 116, name: "Dunes of Despair" },
    { id: 117, name: "Thirsty River" },
    { id: 118, name: "Elona Reach" },
    { id: 119, name: "Augury Rock" },
    { id: 120, name: "The Dragon's Lair" },
    { id: 121, name: "Perdition Rock" },
    { id: 122, name: "Ring of Fire" },
    { id: 123, name: "Abaddon's Mouth" },
    { id: 124, name: "Hell's Precipice" },
    { id: 125, name: "Golden Gates" },
    { id: 127, name: "Scarred Earth" },
    { id: 128, name: "The Eternal Grove" },
    { id: 129, name: "Lutgardis Conservatory" },
    { id: 130, name: "Vasburg Armory" },
    { id: 131, name: "Serenity Temple" },
    { id: 132, name: "Ice Tooth Cave" },
    { id: 133, name: "Beacon's Perch" },
    { id: 134, name: "Yak's Bend" },
    { id: 135, name: "Frontier Gate" },
    { id: 136, name: "Beetletun" },
    { id: 137, name: "Fishermen's Haven" },
    { id: 138, name: "Temple of the Ages" },
    { id: 139, name: "Ventari's Refuge" },
    { id: 140, name: "Druid's Overlook" },
    { id: 141, name: "Maguuma Stade" },
    { id: 142, name: "Quarrel Falls" },
    { id: 143, name: "Gyala Hatchery" },
    { id: 145, name: "The Catacombs" },
    { id: 146, name: "Lakeside County" },
    { id: 147, name: "The Northlands" },
    { id: 148, name: "Ascalon City (Pre-searing)" },
    { id: 149, name: "Ascalon Academy" },
    { id: 150, name: "Ascalon Academy" },
    { id: 151, name: "Ascalon Academy" },
    { id: 152, name: "Heroes' Audience" },
    { id: 153, name: "Seeker's Passage" },
    { id: 154, name: "Destiny's Gorge" },
    { id: 155, name: "Camp Rankor" },
    { id: 156, name: "The Granite Citadel" },
    { id: 157, name: "Marhan's Grotto" },
    { id: 158, name: "Port Sledge" },
    { id: 159, name: "Copperhammer Mines" },
    { id: 160, name: "Green Hills County" },
    { id: 161, name: "Wizard's Folly" },
    { id: 162, name: "Regent Valley" },
    { id: 163, name: "The Barradin Estate" },
    { id: 164, name: "Ashford Abbey" },
    { id: 165, name: "Foible's Fair" },
    { id: 166, name: "Fort Ranik (Pre-searing)" },
    { id: 167, name: "Burning Isle" },
    { id: 168, name: "Druids Isle" },
    { id: 169, name: "Frozen Isle" },
    { id: 171, name: "Warrior's Isle" },
    { id: 172, name: "Hunter's Isle" },
    { id: 173, name: "Wizard's Isle" },
    { id: 174, name: "Nomad's Isle" },
    { id: 175, name: "Isle of the Dead" },
    { id: 176, name: "Frozen Isle" },
    { id: 177, name: "Nomad's Isle" },
    { id: 178, name: "Druid's Isle" },
    { id: 179, name: "Isle of the Dead" },
    { id: 180, name: "Fort Koga" },
    { id: 181, name: "Shiverpeak Arena" },
    { id: 182, name: "Amnoon Arena" },
    { id: 183, name: "Deldrimor Arena" },
    { id: 184, name: "The Crag" },
    { id: 185, name: "Random Arenas" },
    { id: 189, name: "Team Arenas" },
    { id: 190, name: "Sorrow's Furnace" },
    { id: 191, name: "Grenth's Footprint" },
    { id: 192, name: "Cavalon" },
    { id: 194, name: "Kaineng Center" },
    { id: 195, name: "Drazach Thicket" },
    { id: 196, name: "Jaya Bluffs" },
    { id: 197, name: "Shenzun Tunnels" },
    { id: 198, name: "Archipelagos" },
    { id: 199, name: "Maishang Hills" },
    { id: 200, name: "Mount Qinkai" },
    { id: 201, name: "Melandru's Hope" },
    { id: 202, name: "Rheas Crater" },
    { id: 203, name: "Silent Surf" },
    { id: 204, name: "Unwaking Waters" },
    { id: 205, name: "Morostav Trail" },
    { id: 206, name: "Deldrimor War Camp" },
    { id: 207, name: "Heroes' Crypt" },
    { id: 209, name: "Mourning Veil Falls" },
    { id: 210, name: "Ferndale" },
    { id: 211, name: "Pongmei Valley" },
    { id: 212, name: "Monastery Overlook" },
    { id: 213, name: "Zen Daijun" },
    { id: 214, name: "Minister Cho's Estate" },
    { id: 215, name: "Vizunah Square" },
    { id: 216, name: "Nahpui Quarter" },
    { id: 217, name: "Tahnnakai Temple" },
    { id: 218, name: "Arborstone" },
    { id: 219, name: "Boreas Seabed" },
    { id: 220, name: "Sunjiang District" },
    { id: 221, name: "Fort Aspenwood" },
    { id: 222, name: "The Eternal Grove" },
    { id: 223, name: "The Jade Quarry" },
    { id: 224, name: "Gyala Hatchery" },
    { id: 225, name: "Raisu Palace" },
    { id: 226, name: "Imperial Sanctum" },
    { id: 227, name: "Unwaking Waters" },
    { id: 228, name: "Grenz Frontier" },
    { id: 229, name: "Amatz Basin" },
    { id: 232, name: "Shadow's Passage" },
    { id: 233, name: "Raisu Palace" },
    { id: 234, name: "The Aurios Mines" },
    { id: 235, name: "Panjiang Peninsula" },
    { id: 236, name: "Kinya Province" },
    { id: 237, name: "Haiju Lagoon" },
    { id: 238, name: "Sunqua Vale" },
    { id: 239, name: "Wajjun Bazaar" },
    { id: 240, name: "Bukdek Byway" },
    { id: 241, name: "The Undercity" },
    { id: 242, name: "Shing Jea Monastery" },
    { id: 243, name: "Shing Jea Arena" },
    { id: 244, name: "Arborstone" },
    { id: 245, name: "Minister Cho's Estate" },
    { id: 246, name: "Zen Daijun" },
    { id: 247, name: "Boreas Seabed" },
    { id: 248, name: "Great Temple of Balthazar" },
    { id: 249, name: "Tsumei Village" },
    { id: 250, name: "Seitung Harbor" },
    { id: 251, name: "Ran Musu Gardens" },
    { id: 252, name: "Linnok Courtyard" },
    { id: 253, name: "Dwayna Vs Grenth" },
    { id: 254, name: "Sunjiang District" },
    { id: 257, name: "Nahpui Quarter" },
    { id: 266, name: "Urgozs Warren" },
    { id: 267, name: "Tahnnakai Temple" },
    { id: 270, name: "Altrumm Ruins" },
    { id: 273, name: "Zos Shivros Channel" },
    { id: 274, name: "Dragon's Throat" },
    { id: 275, name: "Isle of Weeping Stone" },
    { id: 276, name: "Isle of Jade" },
    { id: 277, name: "Harvest Temple" },
    { id: 278, name: "Breaker Hollow" },
    { id: 279, name: "Leviathan Pits" },
    { id: 280, name: "Isle of the Nameless" },
    { id: 281, name: "Zaishen Challenge" },
    { id: 282, name: "Zaishen Elite" },
    { id: 283, name: "Maatu Keep" },
    { id: 284, name: "Zin Ku Corridor" },
    { id: 285, name: "Monastery Overlook" },
    { id: 286, name: "Brauer Academy" },
    { id: 287, name: "Durheim Archives" },
    { id: 288, name: "Bai Paasu Reach" },
    { id: 289, name: "Seafarer's Rest" },
    { id: 290, name: "Bejunkan Pier" },
    { id: 291, name: "Vizunah Square (Local Quarter)" },
    { id: 292, name: "Vizunah Square (Foreign Quarter)" },
    { id: 293, name: "Fort Aspenwood (Luxon)" },
    { id: 294, name: "Fort Aspenwood (Kurzick)" },
    { id: 295, name: "The Jade Quarry (Luxon)" },
    { id: 296, name: "The Jade Quarry (Kurzick)" },
    { id: 297, name: "Unwaking Waters (Luxon)" },
    { id: 298, name: "Unwaking Waters (Kurzick)" },
    { id: 299, name: "Etnaran Keys" },
    { id: 301, name: "Raisu Pavilion" },
    { id: 302, name: "Kaineng Docks" },
    { id: 303, name: "The Marketplace" },
    { id: 304, name: "The Deep" },
    { id: 308, name: "Ascalon Arena" },
    { id: 309, name: "Annihilation Training" },
    { id: 310, name: "Kill Count Training" },
    { id: 311, name: "Priest Annihilation Training" },
    { id: 312, name: "Obelisk Annihilation Training" },
    { id: 313, name: "Saoshang Trail" },
    { id: 314, name: "Shiverpeak Arena" },
    { id: 315, name: "D'Alessio Arena" },
    { id: 319, name: "Amnoon Arena" },
    { id: 320, name: "Fort Koga" },
    { id: 321, name: "Heroes' Crypt" },
    { id: 322, name: "Shiverpeak Arena" },
    { id: 323, name: "Saltspray Beach (Luxon)" },
    { id: 329, name: "Saltspray Beach (Kurzick)" },
    { id: 330, name: "Heroes' Ascent" },
    { id: 331, name: "Grenz Frontier (Luxon)" },
    { id: 332, name: "Grenz Frontier (Kurzick)" },
    { id: 333, name: "The Ancestral Lands (Luxon)" },
    { id: 334, name: "The Ancestral Lands (Kurzick)" },
    { id: 335, name: "Etnaran Keys (Luxon)" },
    { id: 336, name: "Etnaran Keys (Kurzick)" },
    { id: 337, name: "Kaanai Canyon (Luxon)" },
    { id: 338, name: "Kaanai Canyon (Kurzick)" },
    { id: 339, name: "D'Alessio Arena" },
    { id: 340, name: "Amnoon Arena" },
    { id: 341, name: "Fort Koga" },
    { id: 342, name: "Heroes' Crypt" },
    { id: 343, name: "Shiverpeak Arena" },
    { id: 344, name: "The Hall of Heroes" },
    { id: 345, name: "The Courtyard" },
    { id: 346, name: "Scarred Earth" },
    { id: 347, name: "The Underworld" },
    { id: 348, name: "Tanglewood Copse" },
    { id: 349, name: "Saint Anjeka's Shrine" },
    { id: 350, name: "Eredon Terrace" },
    { id: 351, name: "Divine Path" },
    { id: 352, name: "Brawler's Pit" },
    { id: 353, name: "Petrified Arena" },
    { id: 354, name: "Seabed Arena" },
    { id: 355, name: "Isle of Weeping Stone" },
    { id: 356, name: "Isle of Jade" },
    { id: 357, name: "Imperial Isle" },
    { id: 358, name: "Isle of Meditation" },
    { id: 359, name: "Imperial Isle" },
    { id: 360, name: "Isle of Meditation" },
    { id: 361, name: "Isle of Weeping Stone" },
    { id: 362, name: "Isle of Jade" },
    { id: 363, name: "Imperial Isle" },
    { id: 364, name: "Isle of Meditation" },
    { id: 365, name: "Shing Jea Arena" },
    { id: 367, name: "Dragon Arena" },
    { id: 369, name: "Jahai Bluffs" },
    { id: 370, name: "Kamadan, Jewel of Istan" },
    { id: 371, name: "Marga Coast" },
    { id: 372, name: "Fahranur" },
    { id: 373, name: "Sunward Marches" },
    { id: 374, name: "Vortex" },
    { id: 376, name: "Camp Hojanu" },
    { id: 377, name: "Bahdok Caverns" },
    { id: 378, name: "Wehhan Terraces" },
    { id: 379, name: "Dejarin Estate" },
    { id: 380, name: "Arkjok Ward" },
    { id: 381, name: "Yohlon Haven" },
    { id: 382, name: "Gandara" },
    { id: 383, name: "The Floodplain of Mahnkelon" },
    { id: 385, name: "Lion's Arch" },
    { id: 386, name: "Turai's Procession" },
    { id: 387, name: "Sunspear Sanctuary" },
    { id: 388, name: "Aspenwood Gate (Kurzick)" },
    { id: 389, name: "Aspenwood Gate (Luxon)" },
    { id: 390, name: "Jade Flats (Kurzick)" },
    { id: 391, name: "Jade Flats (Luxon)" },
    { id: 392, name: "Yatendi Canyons" },
    { id: 393, name: "Chantry of Secrets" },
    { id: 394, name: "Garden of Seborhin" },
    { id: 395, name: "Holdings of Chokhin" },
    { id: 396, name: "Mihanu Township" },
    { id: 397, name: "Vehjin Mines" },
    { id: 398, name: "Basalt Grotto" },
    { id: 399, name: "Forum Highlands" },
    { id: 400, name: "Kaineng Center" },
    { id: 401, name: "Resplendent Makuun" },
    { id: 402, name: "Resplendent Makuun" },
    { id: 403, name: "Honur Hill" },
    { id: 404, name: "Wilderness of Bahdza" },
    { id: 405, name: "Vehtendi Valley" },
    { id: 407, name: "Yahnur Market" },
    { id: 408, name: "The Hidden City of Ahdashim" },
    { id: 414, name: "The Kodash Bazaar" },
    { id: 415, name: "Lion's Gate" },
    { id: 416, name: "The Mirror of Lyss" },
    { id: 419, name: "The Mirror of Lyss" },
    { id: 420, name: "Secure the Refuge" },
    { id: 421, name: "Venta Cemetery" },
    { id: 422, name: "Kamadan" },
    { id: 423, name: "The Tribunal" },
    { id: 424, name: "Kodonur Crossroads" },
    { id: 425, name: "Rilohn Refuge" },
    { id: 426, name: "Pogahn Passage" },
    { id: 427, name: "Moddok Crevice" },
    { id: 428, name: "Tihark Orchard" },
    { id: 429, name: "Consulate" },
    { id: 430, name: "Plains of Jarin" },
    { id: 431, name: "Sunspear Great Hall" },
    { id: 432, name: "Cliffs of Dohjok" },
    { id: 433, name: "Dzagonur Bastion" },
    { id: 434, name: "Dasha Vestibule" },
    { id: 435, name: "Grand Court of Sebelkeh" },
    { id: 436, name: "Command Post" },
    { id: 437, name: "Joko's Domain" },
    { id: 438, name: "Bone Palace" },
    { id: 439, name: "The Ruptured Heart" },
    { id: 440, name: "The Mouth of Torment" },
    { id: 441, name: "The Shattered Ravines" },
    { id: 442, name: "Lair of the Forgotten" },
    { id: 443, name: "Poisoned Outcrops" },
    { id: 444, name: "The Sulfurous Wastes" },
    { id: 445, name: "The Ebony Citadel of Mallyx" },
    { id: 446, name: "The Alkali Pan" },
    { id: 447, name: "Cliffs of Dohjok" },
    { id: 448, name: "Crystal Overlook" },
    { id: 449, name: "Kamadan" },
    { id: 450, name: "Gate of Torment" },
    { id: 451, name: "Nightfallen Garden" },
    { id: 456, name: "Churrhir Fields" },
    { id: 457, name: "Beknur Harbor" },
    { id: 458, name: "The Underworld" },
    { id: 462, name: "Heart of Abaddon" },
    { id: 463, name: "The Underworld" },
    { id: 464, name: "Nundu Bay" },
    { id: 465, name: "Nightfallen Jahai" },
    { id: 466, name: "Depths of Madness" },
    { id: 467, name: "Rollerbeetle Racing" },
    { id: 468, name: "Domain of Fear" },
    { id: 469, name: "Gate of Fear" },
    { id: 470, name: "Domain of Pain" },
    { id: 471, name: "Bloodstone Fen" },
    { id: 472, name: "Domain of Secrets" },
    { id: 473, name: "Gate of Secrets" },
    { id: 474, name: "Gate of Anguish" },
    { id: 475, name: "Oozez Pit" },
    { id: 476, name: "Jennur's Horde" },
    { id: 477, name: "Nundu Bay" },
    { id: 478, name: "Gate of Desolation" },
    { id: 479, name: "Champion's Dawn" },
    { id: 480, name: "Ruins of Morah" },
    { id: 481, name: "Fahranur" },
    { id: 482, name: "Bjora Marches" },
    { id: 483, name: "Zehlon Reach" },
    { id: 484, name: "Lahtenda Bog" },
    { id: 485, name: "Arbor Bay" },
    { id: 486, name: "Issnur Isles" },
    { id: 487, name: "Beknur Harbor" },
    { id: 488, name: "Mehtani Keys" },
    { id: 489, name: "Kodlonu Hamlet" },
    { id: 490, name: "Island of Shehkah" },
    { id: 491, name: "Jokanur Diggings" },
    { id: 492, name: "Blacktide Den" },
    { id: 493, name: "Consulate Docks" },
    { id: 494, name: "Gate of Pain" },
    { id: 495, name: "Gate of Madness" },
    { id: 496, name: "Abaddons Gate" },
    { id: 497, name: "Sunspear Arena" },
    { id: 498, name: "Ice Cliff Chasms" },
    { id: 500, name: "Bokka Amphitheatre" },
    { id: 501, name: "Riven Earth" },
    { id: 502, name: "The Astralarium" },
    { id: 503, name: "Throne Of Secrets" },
    { id: 504, name: "Churranu Island Arena" },
    { id: 505, name: "Shing Jea Monastery" },
    { id: 506, name: "Haiju Lagoon" },
    { id: 507, name: "Jaya Bluffs" },
    { id: 508, name: "Seitung Harbor" },
    { id: 509, name: "Tsumei Village" },
    { id: 510, name: "Seitung Harbor" },
    { id: 511, name: "Tsumei Village" },
    { id: 512, name: "Drakkar Lake" },
    { id: 514, name: "Minister Cho's Estate" },
    { id: 513, name: "Uncharted Isle" },
    { id: 530, name: "Isle of Wurms" },
    { id: 531, name: "Uncharted Isle" },
    { id: 532, name: "Isle of Wurms" },
    { id: 533, name: "Uncharted Isle" },
    { id: 534, name: "Isle of Wurms" },
    { id: 535, name: "Sunspear Arena" },
    { id: 537, name: "Corrupted Isle" },
    { id: 538, name: "Isle of Solitude" },
    { id: 539, name: "Corrupted Isle" },
    { id: 540, name: "Isle of Solitude" },
    { id: 541, name: "Corrupted Isle" },
    { id: 542, name: "Isle of Solitude" },
    { id: 543, name: "Sun Docks" },
    { id: 544, name: "Chahbek Village" },
    { id: 545, name: "Remains of Sahlahja" },
    { id: 546, name: "Jaga Moraine" },
    { id: 547, name: "Bombardment" },
    { id: 548, name: "Norrhart Domains" },
    { id: 549, name: "Hero Battles" },
    { id: 550, name: "Hero Battles" },
    { id: 551, name: "The Crossing" },
    { id: 552, name: "Desert Sands" },
    { id: 553, name: "Varajar Fells" },
    { id: 554, name: "Dajkah Inlet" },
    { id: 555, name: "The Shadow Nexus" },
    { id: 556, name: "Sparkfly Swamp" },
    { id: 559, name: "Gate of the Nightfallen Lands" },
    { id: 560, name: "Cathedral of Flames" },
    { id: 561, name: "Gate of Torment" },
    { id: 562, name: "Verdant Cascades" },
    { id: 567, name: "Cathedral of Flames: Level 2" },
    { id: 568, name: "Cathedral of Flames: Level 3" },
    { id: 569, name: "Magus Stones" },
    { id: 570, name: "Catacombs of Kathandrax" },
    { id: 571, name: "Catacombs of Kathandrax: Level 2" },
    { id: 572, name: "Alcazia Tangle" },
    { id: 573, name: "Rragars Menagerie" },
    { id: 574, name: "Rragars Menagerie: Level 2" },
    { id: 575, name: "Rragars Menagerie: Level 3" },
    { id: 576, name: "Ooze Pit" },
    { id: 577, name: "Slavers Exile" },
    { id: 578, name: "Oola's Lab" },
    { id: 579, name: "Oola's Lab: Level 2" },
    { id: 580, name: "Oola's Lab: Level 3" },
    { id: 581, name: "Shards of Orr" },
    { id: 582, name: "Shards of Orr: Level 2" },
    { id: 583, name: "Shards of Orr: Level 3" },
    { id: 584, name: "Arachni's Haunt" },
    { id: 585, name: "Arachni's Haunt: Level 2" },
    { id: 586, name: "Fetid River" },
    { id: 594, name: "Forgotten Shrines" },
    { id: 597, name: "The Antechamber" },
    { id: 599, name: "Vloxen Excavations" },
    { id: 605, name: "Vloxen Excavations: Level 2" },
    { id: 606, name: "Vloxen Excavations: Level 3" },
    { id: 607, name: "Heart of the Shiverpeaks" },
    { id: 608, name: "Heart of the Shiverpeaks: Level 2" },
    { id: 609, name: "Heart of the Shiverpeaks: Level 3" },
    { id: 610, name: "Bloodstone Caves" },
    { id: 613, name: "Bloodstone Caves: Level 2" },
    { id: 614, name: "Bloodstone Caves: Level 3" },
    { id: 615, name: "Bogroot Growths" },
    { id: 616, name: "Bogroot Growths: Level 2" },
    { id: 617, name: "Raven's Point" },
    { id: 618, name: "Raven's Point: Level 2" },
    { id: 619, name: "Raven's Point: Level 3" },
    { id: 620, name: "Slaver's Exile" },
    { id: 621, name: "Slaver's Exile" },
    { id: 622, name: "Slaver's Exile" },
    { id: 623, name: "Slaver's Exile" },
    { id: 624, name: "Vlox's Falls" },
    { id: 625, name: "Battledepths" },
    { id: 626, name: "Battledepths: Level 2" },
    { id: 627, name: "Battledepths: Level 3" },
    { id: 628, name: "Sepulchre of Dragrimmar" },
    { id: 629, name: "Sepulchre of Dragrimmar: Level 2" },
    { id: 630, name: "Frostmaws Burrows" },
    { id: 631, name: "Frostmaws Burrows: Level 2" },
    { id: 632, name: "Frostmaws Burrows: Level 3" },
    { id: 633, name: "Frostmaws Burrows: Level 4" },
    { id: 634, name: "Frostmaws Burrows: Level 5" },
    { id: 635, name: "Darkrime Delves" },
    { id: 636, name: "Darkrime Delves: Level 2" },
    { id: 637, name: "Darkrime Delves: Level 3" },
    { id: 638, name: "Gadd's Encampment" },
    { id: 639, name: "Umbral Grotto" },
    { id: 640, name: "Rata Sum" },
    { id: 641, name: "Tarnished Haven" },
    { id: 642, name: "Eye of the North" },
    { id: 643, name: "Sifhalla" },
    { id: 644, name: "Gunnar's Hold" },
    { id: 645, name: "Olafstead" },
    { id: 646, name: "Hall of Monuments" },
    { id: 647, name: "Dalada Uplands" },
    { id: 648, name: "Doomlore Shrine" },
    { id: 649, name: "Grothmar Wardowns" },
    { id: 650, name: "Longeye's Ledge" },
    { id: 651, name: "Sacnoth Valley" },
    { id: 652, name: "Central Transfer Chamber" },
    { id: 653, name: "Curse of the Nornbear" },
    { id: 654, name: "Blood Washes Blood" },
    { id: 655, name: "A Gate Too Far" },
    { id: 656, name: "A Gate Too Far" },
    { id: 657, name: "A Gate Too Far" },
    { id: 658, name: "Oola's Laboratory" },
    { id: 659, name: "Oola's Laboratory" },
    { id: 660, name: "Oola's Laboratory" },
    { id: 661, name: "Finding The Bloodstone" },
    { id: 662, name: "Finding The Bloodstone" },
    { id: 663, name: "Finding The Bloodstone" },
    { id: 664, name: "Genius Operated Living Enchanted Manifestation" },
    { id: 665, name: "Against the Charr" },
    { id: 666, name: "Warband of Brothers" },
    { id: 667, name: "Warband of Brothers" },
    { id: 668, name: "Warband of Brothers" },
    { id: 669, name: "Freeing the Vanguard" },
    { id: 670, name: "Destruction's Depths" },
    { id: 671, name: "Destruction's Depths" },
    { id: 672, name: "Destruction's Depths" },
    { id: 673, name: "A Time for Heroes" },
    { id: 674, name: "Steppe Practice" },
    { id: 675, name: "Boreal Station" },
    { id: 676, name: "Catacombs of Kathandrax: Level 3" },
    { id: 677, name: "Mountain Holdout" },
    { id: 679, name: "Cinematic Cave Norn Cursed" },
    { id: 680, name: "Cinematic Steppe Interrogation" },
    { id: 681, name: "Cinematic Interior Research" },
    { id: 682, name: "Cinematic Eye Vision A" },
    { id: 683, name: "Cinematic Eye Vision B" },
    { id: 684, name: "Cinematic Eye Vision C" },
    { id: 685, name: "Cinematic Eye Vision D" },
    { id: 686, name: "Polymock Coliseum" },
    { id: 687, name: "Polymock Glacier" },
    { id: 688, name: "Polymock Crossing" },
    { id: 689, name: "Cinematic Mountain Resolution" },
    { id: 690, name: "<Mountain Polar OP>" },
    { id: 691, name: "Beneath Lions Arch" },
    { id: 692, name: "Tunnels Below Cantha" },
    { id: 693, name: "Caverns Below Kamadan" },
    { id: 694, name: "Cinematic Mountain Dwarfs" },
    { id: 695, name: "The Eye of the North" },
    { id: 696, name: "The Eye of the North" },
    { id: 697, name: "The Eye of the North" },
    { id: 698, name: "Hero Tutorial" },
    { id: 699, name: "The Norn Fighting Tournament" },
    { id: 701, name: "Hundar's Resplendent Treasure Vault" },
    { id: 702, name: "Norn Brawling Championship" },
    { id: 703, name: "Kilroys Punchout Training" },
    { id: 704, name: "Fronis Irontoe's Lair" },
    { id: 705, name: "INTERIOR_WATCH_OP1_FINAL_BATTLE" },
    { id: 706, name: "The Great Norn Alemoot" },
    { id: 708, name: "(Mountain Traverse)" },
    { id: 709, name: "Destroyer Ending" },
    { id: 711, name: "Alcazia Tangle" },
    { id: 712, name: "Plains of Jarin" },
    { id: 718, name: "Plains of Jarin" },
    { id: 722, name: "Plains of Jarin" },
    { id: 723, name: "Plains of Jarin" },
    { id: 724, name: "Plains of Jarin" },
    { id: 725, name: "Plains of Jarin" },
    { id: 726, name: "Plains of Jarin" },
    { id: 727, name: "Plains of Jarin" },
    { id: 728, name: "(crash)" },
    { id: 729, name: "Special Ops Grendich Courthouse" },
    { id: 730, name: "The Tengu Accords" },
    { id: 731, name: "The Battle of Jahai" },
    { id: 732, name: "The Flight North" },
    { id: 733, name: "The Rise of the White Mantle" },
    { id: 734, name: "Finding the Bloodstone Mission" },
    { id: 760, name: "Genius Operated Living Enchanted Manifestation Mission" },
    { id: 761, name: "Against the Charr Mission" },
    { id: 762, name: "Warband of brothers Mission" },
    { id: 763, name: "Assault on the Stronghold Mission" },
    { id: 764, name: "Destructions Depths Mission" },
    { id: 765, name: "A Time for Heroes Mission" },
    { id: 766, name: "Curse of the Nornbear Mission" },
    { id: 767, name: "Blood Washes Blood Mission" },
    { id: 768, name: "A Gate Too Far Mission" },
    { id: 769, name: "The Elusive Golemancer Mission" },
    { id: 770, name: "Secret Lair of the Snowmen2" },
    { id: 782, name: "Secret Lair of the Snowmen3" },
    { id: 783, name: "Droknars Forge (cinematic)" },
    { id: 784, name: "Isle of the Nameless PvP" },
    { id: 785, name: "Temple of the Ages ROX" },
    { id: 789, name: "Wajjun Bazaar POX" },
    { id: 790, name: "Bokka Amphitheatre NOX" },
    { id: 791, name: "Secret Underground Lair" },
    { id: 792, name: "Golem Tutorial Simulation" },
    { id: 793, name: "Snowball Dominance" },
    { id: 794, name: "Zaishen Menagerie Grounds" },
    { id: 795, name: "Zaishen Menagerie (Outpost)" },
    { id: 796, name: "Codex Arena (Outpost)" },
    { id: 797, name: "The Underworld Something Wicked This Way Comes" },
    { id: 807, name: "The Underworld Don't Fear the Reapers" },
    { id: 808, name: "Lions Arch Halloween (Outpost)" },
    { id: 809, name: "Lions Arch Wintersday (Outpost)" },
    { id: 810, name: "Lions Arch Canthan New Year (Outpost)" },
    { id: 811, name: "Ascalon City Wintersday (Outpost)" },
    { id: 812, name: "Droknars Forge Halloween (Outpost)" },
    { id: 813, name: "Droknars Forge Wintersday (Outpost)" },
    { id: 814, name: "Tomb of the Primeval Kings Halloween (Outpost)" },
    { id: 815, name: "Shing Jea Monastery Dragon Festival (Outpost)" },
    { id: 816, name: "Shing Jea Monastery Canthan New Year (Outpost)" },
    { id: 817, name: "Kaineng Center Canthan New Year (Outpost)" },
    { id: 818, name: "Kamadan Jewel of Istan Halloween (Outpost)" },
    { id: 819, name: "Kamadan Jewel of Istan Wintersday (Outpost)" },
    { id: 820, name: "Kamadan Jewel of Istan Canthan New Year (Outpost)" },
    { id: 821, name: "Eyeofthe North Outpost Wintersday (Outpost)" },
    { id: 822, name: "War in Kryta Talmark Wilderness" },
    { id: 838, name: "War in Kryta Trialof Zinn" },
    { id: 839, name: "War in Kryta Divinity Coast" },
    { id: 840, name: "War in Kryta Lions Arch Keep" },
    { id: 841, name: "War in Kryta D'Alessio Seaboard" },
    { id: 842, name: "War in Kryta The Battle for Lions Arch" },
    { id: 843, name: "War in Kryta Riverside Province" },
    { id: 844, name: "War in Kryta Lions Arch" },
    { id: 845, name: "War in Kryta The Mausoleum" },
    { id: 846, name: "War in Kryta Rise" },
    { id: 847, name: "War in Kryta Shadows in the Jungle" },
    { id: 848, name: "War in Kryta A Vengeance of Blades" },
    { id: 849, name: "War in Kryta Auspicious Beginnings" },
    { id: 850, name: "Olafstead (cinematic)" },
    { id: 855, name: "The Great Snowball Fight of the Gods Operation Crush Spirits" },
    { id: 856, name: "The Great Snowball Fight of the Gods Fighting in a Winter Wonderland" },
    { id: 857, name: "Embark Beach" },
    { id: 858, name: "What Waits in Shadow" },
    { id: 861, name: "Winds of Change A Chance Encounter" },
    { id: 862, name: "Tracking The Corruption" },
    { id: 863, name: "Cantha Courier Crisis" },
    { id: 864, name: "A Treaty's a Treaty" },
    { id: 865, name: "Deadly Cargo" },
    { id: 866, name: "The Rescue Attempt" },
    { id: 867, name: "Violence in the Streets" },
    { id: 868, name: "Scarred Psyche Mission" },
    { id: 869, name: "Calling All Thugs" },
    { id: 870, name: "Finding Jinnai" },
    { id: 871, name: "Raid on Shing Jea Monastery" },
    { id: 872, name: "Raid on Kaineng Center" },
    { id: 873, name: "Ministry of Oppression" },
    { id: 874, name: "The Final Confrontation" },
    { id: 875, name: "Lakeside County 1070AE" },
    { id: 876, name: "Ashford Catacombs 1070AE" },
    { id: 877, name: "Count" },
];

var districts = [
    "Current",
    "International",
    "American",
    "Europe - English",
    "Europe - French",
    "Europe - German",
    "Europe - German",
    "Europe - Italian",
    "Europe - Spanish",
    "Europe - Polish",
    "Europe - Russian",
    "Asia - Korean",
    "Asia - Chinese",
    "Asia - Japanese"
];

var searchTypes = [
    "Hunting",
    "Mission",
    "Quest",
    "Trade",
    "Guild"
];

var markerSize = {
    "dungeon": "small",
    "arena": "medium",
    "arena_cantha": "large",
    "arena_elona": "large",
    "challenge_cantha": "large",
    "challenge_cantha_kurzick": "large",
    "challenge_cantha_luxon": "large",
    "challenge_elona": "large",
    "challenge_realm": "large",
    "city": "medium",
    "city_cantha": "large",
    "city_cantha_kurzick": "large",
    "city_cantha_luxon": "large",
    "city_elona": "large",
    "city_realm": "large",
    "guildhall": "medium",
    "gate": "medium",
    "gate_of_anguish": "large",
    "outpost": "small",
    "outpost_cantha": "medium",
    "outpost_cantha_kurzick": "medium",
    "outpost_cantha_luxon": "medium",
    "outpost_elona": "medium",
    "outpost_realm": "medium",
    "mission": "large",
    "mission_cantha": "large",
    "mission_cantha_kurzick": "large",
    "mission_cantha_luxon": "large",
    "mission_elona": "large",
    "mission_realm": "large",
    "mission_eotn": "medium",
    "zaishen": "large",
    "travel": "large",
    "vortex": "large"
};

var leafletData = [];
var initialized = false;
var locationMap = new Map();

// Set up event delegation once
document.querySelector('.partyList').addEventListener('click', function (event) {
    if (event.target.classList.contains('mapRow')) {
        let mapId = event.target.getAttribute('data-map-id');
        let mapObj = maps.find(map => map.id.toString() === mapId);
        if (!mapObj) {
            return;
        }

        mapRowClicked(mapObj);
    }
});

async function loadLeafletData() {
    var response = await fetch("data/0.json");
    leafletData.push(await response.json());
    response = await fetch("data/1.json");
    leafletData.push(await response.json());
    response = await fetch("data/2.json");
    leafletData.push(await response.json());
    response = await fetch("data/3.json");
    leafletData.push(await response.json());
    response = await fetch("data/4.json");
    leafletData.push(await response.json());
    response = await fetch("data/5.json");
    leafletData.push(await response.json());
}

function getEntriesForMapId(searches, mapId) {
    let entries = new Map();

    searches.forEach((value, key) => {
        if (key.startsWith(mapId + '-')) {
            entries.set(key, value);
        }
    });

    return entries;
}

function aggregateSearchesByCity(searches) {
    const aggregated = {};

    searches.forEach(value => {
        const mapId = value.map_id;
        if (!aggregated[mapId]) {
            aggregated[mapId] = [];
        }

        value.parties.forEach(partySearch => {
            aggregated[mapId].push(partySearch);
        });
    });

    return aggregated;
}

function aggregateSearchesByMapAndType(searches) {
    const aggregated = {};

    searches.forEach(value => {
        const mapId = value.map_id;
        if (!aggregated[mapId]) {
            aggregated[mapId] = [];
        }

        value.parties.forEach(partySearch => {
            if (!aggregated[mapId][partySearch.search_type]) {
                aggregated[mapId][partySearch.search_type] = [];
            }

            aggregated[mapId][partySearch.search_type].push(partySearch);
        });
    });

    return aggregated;
}

function updateHash() {
    var zoom = map.getZoom();
    var point = project(map.getCenter());

    history.replaceState(undefined, undefined, "?v=1&x=" + point.x + "&y=" + point.y + "&z=" + zoom + "&c=" + continent);
}

function getURLParameter(name) {
    return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [null, ''])[1].replace(/\+/g, '%20')) || null;
}

function toggleMenu() {
    document.querySelector("#menu").classList.toggle("hidden");
}

function showMenu() {
    if (document.querySelector("#menu").classList.contains("hidden")) {
        toggleMenu();
    }
}

function hideMenu() {
    if (!document.querySelector("#menu").classList.contains("hidden")) {
        toggleMenu();
    }
}

var urlCoordinates = {
    v: getURLParameter("v"),
    x: getURLParameter("x"),
    y: getURLParameter("y"),
    z: getURLParameter("z"),
    c: getURLParameter("c")
};

async function buildPartyList() {
    const response = await fetch('/status/map-activity');
    const data = await response.json();
    let aggregatedPartySearches = aggregateSearchesByCity(locationMap);
    let container = document.querySelector(".partyList");
    container.innerHTML = "";
    for (const mapId in aggregatedPartySearches) {
        if (aggregatedPartySearches[mapId.toString()].length === 0) {
            continue;
        }
        const activity = data.find(a => a.mapId.toString() === mapId.toString());
        if (!activity) {
            continue;
        }

        const mapObj = maps.find(map => map.id.toString() === mapId);
        let outerDiv = `<div><div class="mapRow" data-map-id="${mapId}")>${mapObj.name} - ${aggregatedPartySearches[mapId.toString()].length}</div>`;

        outerDiv += `</div>`;
        container.innerHTML += outerDiv;
    }
}

function navigateToMap(mapObj) {
    leafletData.forEach(continentData => {
        if (!continentData.regions) {
            return;
        }

        continentData.regions.forEach(region => {
            if (!region.locations) {
                return;
            }

            region.locations.forEach(location => {
                if (location.mapId &&
                    location.mapId === mapObj.id) {
                    var url = new URL(window.location.href);
                    url.searchParams.set("x", location.coordinates[0]);
                    url.searchParams.set("y", location.coordinates[1]);
                    url.searchParams.set("c", continentData.id);
                    url.searchParams.set("z", "4");
                    window.location = url;
                }
            });
        });
    });
}

function mapRowClicked(mapObj) {
    window.location.hash = mapObj.name;
    navigateToMap(mapObj);
    showPartyWindow();
    buildPartyWindow();
}

function hidePartyWindowRows(rowType) {
    const tbody = document.querySelector("#popupWindowTable tbody");
    const existingRows = tbody.querySelectorAll(`.${rowType}`);
    existingRows.forEach(row => row.remove());
    const pivotRow = document.getElementById(`${rowType}`);
    if (!pivotRow.classList.contains('hidden')) {
        pivotRow.classList.add('hidden');
        pivotRow.classList.add('collapsed');
    }
}

function populatePartyWindowRows(rowType, partySearchesArray) {
    const tbody = document.querySelector("#popupWindowTable tbody");
    const existingRows = tbody.querySelectorAll(`.${rowType}`);
    existingRows.forEach(row => row.remove());
    const pivotRow = document.getElementById(`${rowType}`);
    if (pivotRow.classList.contains('hidden')) {
        pivotRow.classList.remove('hidden');
        pivotRow.classList.remove('collapsed');
    }
    let desiredIndex = Array.from(tbody.children).indexOf(pivotRow) + 1;
    for (var partySearchId in partySearchesArray) {
        let partySearch = partySearchesArray[partySearchId];
        let newRow = document.createElement('tr');
        newRow.className = rowType + " row small centered";

        let leaderCell = document.createElement('td');
        leaderCell.textContent = partySearch.sender || 'N/A';
        newRow.appendChild(leaderCell);

        let regionCell = document.createElement('td');
        regionCell.textContent = districts[partySearch.district] || 'N/A';
        newRow.appendChild(regionCell);

        let sizeCell = document.createElement('td');
        sizeCell.textContent = partySearch.party_size || 'N/A';
        newRow.appendChild(sizeCell);

        let districtCell = document.createElement('td');
        districtCell.textContent = partySearch.district_number || 'N/A';
        newRow.appendChild(districtCell);

        let descriptionCell = document.createElement('td');
        descriptionCell.textContent = partySearch.message || 'N/A';
        newRow.appendChild(descriptionCell);

        tbody.insertBefore(newRow, tbody.children[desiredIndex]);
        desiredIndex++;
    }
}

async function buildPartyWindow() {
    let mapName = window.location.hash;
    if (!mapName) {
        return;
    }

    mapName = decodeURIComponent(mapName.substring(1));
    const map = maps.find(m => m.name === mapName);
    if (!map) {
        return;
    }

    await waitForInitialized();
    const aggregatedPartySearches = aggregateSearchesByMapAndType(locationMap);
    if (!aggregatedPartySearches[map.id.toString()]) {
        return;
    }

    let popupWindowTitle = document.querySelector("#popupWindowTitle");
    popupWindowTitle.textContent = `Party Search - ${map.name}`;
    if (aggregatedPartySearches[map.id.toString()] &&
        aggregatedPartySearches[map.id.toString()][0] &&
        aggregatedPartySearches[map.id.toString()][0].length > 0) {
        populatePartyWindowRows("huntingRow", aggregatedPartySearches[map.id.toString()][0]);
    }
    else {
        hidePartyWindowRows("huntingRow");
    }

    if (aggregatedPartySearches[map.id.toString()] &&
        aggregatedPartySearches[map.id.toString()][1] &&
        aggregatedPartySearches[map.id.toString()][1].length > 0) {
        populatePartyWindowRows("missionRow", aggregatedPartySearches[map.id.toString()][1]);
    }
    else {
        hidePartyWindowRows("missionRow");
    }

    if (aggregatedPartySearches[map.id.toString()] &&
        aggregatedPartySearches[map.id.toString()][2] &&
        aggregatedPartySearches[map.id.toString()][2].length > 0) {
        populatePartyWindowRows("questRow", aggregatedPartySearches[map.id.toString()][2]);
    }
    else {
        hidePartyWindowRows("questRow");
    }

    if (aggregatedPartySearches[map.id.toString()] &&
        aggregatedPartySearches[map.id.toString()][3] &&
        aggregatedPartySearches[map.id.toString()][3].length > 0) {
        populatePartyWindowRows("tradeRow", aggregatedPartySearches[map.id.toString()][3]);
    }
    else {
        hidePartyWindowRows("tradeRow");
    }

    if (aggregatedPartySearches[map.id.toString()] &&
        aggregatedPartySearches[map.id.toString()][4] &&
        aggregatedPartySearches[map.id.toString()][4].length > 0) {
        populatePartyWindowRows("guildRow", aggregatedPartySearches[map.id.toString()][4]);
    }
    else {
        hidePartyWindowRows("guildRow");
    }

}

function mapClicked(map) {
    if (map.mapId) {
        let partySearches = getEntriesForMapId(locationMap, map.mapId.toString());
        let found = false;
        partySearches.forEach(partySearch => {
            if (partySearch.parties.length > 0) {
                found = true;
            }
        });

        if (found) {
            const mapObj = maps.find(m => m.id.toString() === map.mapId.toString());
            if (!mapObj) {
                return;
            }

            window.location.hash = mapObj.name;
            buildPartyWindow();
            showPartyWindow();
        }
    }
}

function unproject(coord) {
    return map.unproject(coord, map.getMaxZoom());
}

function project(coord) {
    return map.project(coord, map.getMaxZoom());
}

function loadMap(mapIndex) {
    hideMenu();
    continent = mapIndex;
    document.querySelector("#menu").classList.add("hidden");

    let maplinks = document.querySelectorAll(".mapLink");
    for (let m = 0; m < maplinks.length; m++) {
        maplinks[m].classList.remove("selected");
    }

    let mapLink = document.querySelector(".mapLink[data-id='" + mapIndex + "']");

    if (mapLink !== null) {
        mapLink.classList.add("selected");
    }

    if (map !== undefined) {
        map.off();
        map.remove();
    }

    fetch("data/" + mapIndex + ".json?v=20200516001")
        .then((response) => {
            return response.json();
        })
        .then((data) => {
            mapData = data;

            document.title = "Guild Wars Party Search [" + mapData.name + "]";

            map = L.map("mapdiv", {
                minZoom: 0,
                maxZoom: 4,
                crs: L.CRS.Simple,
                attributionControl: false
            }).on('click', function (e) {
                hidePartyWindow();
            }).on('zoomend', function () {
                zoomEnd();
            }).on('moveend zoomend', function () {
                if (loading) {
                    loading = false;
                } else {
                    updateHash();
                }
            }).on('click focus movestart', function () {
                hideMenu();
            });



            var mapbounds = new L.LatLngBounds(unproject([0, 0]), unproject(mapData.dims));
            map.setMaxBounds(mapbounds);
            if (urlCoordinates.x !== null && urlCoordinates.y !== null && urlCoordinates.z !== null) {
                map.setView(unproject([urlCoordinates.x, urlCoordinates.y]), urlCoordinates.z);
                urlCoordinates.x, urlCoordinates.y, urlCoordinates.z = null;
            } else if (data.center !== undefined) {
                map.setView(unproject(data.center), 4);
            } else {
                map.setView(unproject([mapData.dims[0] / 2, mapData.dims[1] / 2]), 4);
            }

            map.addLayer(L.tileLayer("tiles/" + mapData.id + "/{z}/{x}/{y}.jpg", { minZoom: 0, maxZoom: 4, continuousWorld: true, bounds: mapbounds }));

            for (let r = 0; r < mapData.regions.length; r++) {

                let region = mapData.regions[r];

                if (region.areas !== undefined) {
                    for (let a = 0; a < region.areas.length; a++) {
                        areaLabel(region.areas[a]).addTo(map);
                    }
                }

                if (region.locations !== undefined) {
                    for (let o = 0; o < region.locations.length; o++) {
                        locationMarker(region.locations[o]).addTo(map);
                    }
                }
            }

            zoomEnd();
            updateMarkers();
        });
}

function zoomEnd() {

    let zoom = map.getZoom();
    let locationLabels = document.querySelectorAll(".marker_location .label");
    let areaLabels = document.querySelectorAll(".marker_area .label");
    let icons = document.querySelectorAll(".icon");


    for (let l = 0; l < locationLabels.length; l++) {
        locationLabels[l].classList.remove("hidden");
    }

    for (let l = 0; l < areaLabels.length; l++) {
        areaLabels[l].classList.remove("hidden");
    }

    for (let i = 0; i < icons.length; i++) {
        icons[i].classList.remove("hidden");
        icons[i].classList.remove("scaled_70");
        icons[i].classList.remove("scaled_40");
    }

    if (zoom === 3) {
        for (let l = 0; l < locationLabels.length; l++) {
            locationLabels[l].classList.add("hidden");
        }

        for (let i = 0; i < icons.length; i++) {
            icons[i].classList.add("scaled_70");
        }
    } else if (zoom === 2) {

        for (let l = 0; l < locationLabels.length; l++) {
            locationLabels[l].classList.add("hidden");
        }

        for (let l = 0; l < areaLabels.length; l++) {
            areaLabels[l].classList.add("hidden");
        }

        for (let i = 0; i < icons.length; i++) {
            icons[i].classList.add("scaled_40");
        }
    } else if (zoom < 2) {
        for (let l = 0; l < locationLabels.length; l++) {
            locationLabels[l].classList.add("hidden");
        }

        for (let l = 0; l < areaLabels.length; l++) {
            areaLabels[l].classList.add("hidden");
        }

        for (let i = 0; i < icons.length; i++) {
            icons[i].classList.add("hidden");
        }
    }
}

function areaLabel(data) {

    let wiki = data.name.replace(/ /g, "_");
    if (data.wiki !== undefined) {
        wiki = data.wiki;
    }

    return L.marker(unproject(data.coordinates), {
        icon: L.divIcon({
            iconSize: null,
            className: "marker marker_area seethru unclickable",
            html: "<div class='holder'><span class='label'>" + data.name + "</span></div>"
        }),
        options: {
            wiki: wiki
        }
    });
}

function locationMarker(data) {

    let wiki = data.name.replace(/ /g, "_");
    if (data.wiki !== undefined) {
        wiki = data.wiki;
    }

    let label = data.name;
    if (data.label !== undefined) {
        label = data.label;
    }

    let divId = data.mapId ? " id='" + data.mapId + "'" : "";

    return L.marker(unproject(data.coordinates), {
        icon: L.divIcon({
            iconSize: null,
            className: "marker marker_location " + markerSize[data.type] + " seethru",
            html: "<div" + divId + " class='holder'><img class='icon' src='resources/icons/" + data.type + ".png'/><div class='label'>" + label + "</div></div>"
        }),
        options: {
            wiki: wiki,
            mapId: data.mapId
        }
    }).on('click', function (e) {

        if (map.getZoom() > 1) {
            mapClicked(e.target.options.options);
        }
    });
}

if (urlCoordinates.c !== null) {
    continent = urlCoordinates.c;
}

async function updateMarkers() {
    const response = await fetch('/status/map-activity');
    const data = await response.json();

    let allInnerDivs = document.querySelectorAll(".marker.marker_location .holder[id]");
    allInnerDivs.forEach(function (innerDiv) {
        let parentMarker = innerDiv.closest('.marker.marker_location');
        if (parentMarker) {
            parentMarker.classList.add('seethru');
            parentMarker.classList.add('unclickable');
        }
    });

    data.forEach(function (activity) {
        let matchingInnerDivs = Array.prototype.filter.call(allInnerDivs, function (innerDiv) {
            return innerDiv.id && innerDiv.id === activity.mapId.toString();
        });
        matchingInnerDivs.forEach(function (innerDiv) {
            let parentMarker = innerDiv.closest('.marker.marker_location');
            if (parentMarker) {
                parentMarker.classList.remove('seethru');
                parentMarker.classList.remove('unclickable');
            }
        });
    });
}

function connectToLiveFeed() {
    const wsUrl = (window.location.protocol === 'https:' ? 'wss://' : 'ws://') + window.location.host + '/party-search/live-feed';
    let socket;
    let retryDelay = 1000;

    function openWebSocket() {
        socket = new WebSocket(wsUrl);

        socket.onopen = function (event) {
            console.log('WebSocket connection established:', event);
            retryDelay = 1000;
            showMenu();
        };

        socket.onmessage = function (event) {
            console.log('WebSocket message received:', event);
            if (event.data === 'pong') {
                console.log('Pong received from server.');
            } else {
                let obj = JSON.parse(event.data);
                console.log(obj);
                obj.Searches.forEach(searchEntry => {
                    let combinedKey = `${searchEntry.map_id}-${searchEntry.district}`;
                    locationMap.set(combinedKey, searchEntry);
                });

                initialized = true;
                updateMarkers();
                buildPartyList();
                buildPartyWindow();
            }
        };

        socket.onerror = function (event) {
            console.error('WebSocket error:', event);
        };

        socket.onclose = function (event) {
            console.log('WebSocket connection closed:', event);
            if (!event.wasClean) {
                console.log(`WebSocket closed unexpectedly. Reconnecting in ${retryDelay / 1000} seconds...`);
                setTimeout(openWebSocket, retryDelay);
                retryDelay = Math.min(retryDelay * 2, 30000); // Exponential backoff with a max delay of 30 seconds
            }
        };
    }

    openWebSocket();
}

async function waitForInitialized() {
    while (!initialized) {
        await new Promise(resolve => setTimeout(resolve, 50));
    }
}

function togglePartyWindow(){
    document.querySelector("#popupWindow").classList.toggle("hidden");
}

function hidePartyWindow(){
    if (!document.querySelector("#popupWindow").classList.contains("hidden")) {
        togglePartyWindow();
        window.location.hash = "";
    }
}

function showPartyWindow(){
    if (document.querySelector("#popupWindow").classList.contains("hidden")) {
        togglePartyWindow();
    }
}

function checkNavigation() {
    let mapName = window.location.hash;
    if (!mapName) {
        return;
    }

    mapName = decodeURIComponent(mapName.substring(1));
    const map = maps.find(m => m.name === mapName);
    if (!map) {
        return;
    }

    buildPartyWindow();
    showPartyWindow();
}

checkNavigation();
loadLeafletData();
loadMap(continent);
connectToLiveFeed();