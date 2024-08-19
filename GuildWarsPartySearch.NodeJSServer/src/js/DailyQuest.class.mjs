import {map_ids} from "./gw_constants.mjs";

export class DailyQuest {
    constructor(map_id, nearest_outpost_id = 0) {
        this.map_id = map_id;
        this.nearest_outpost_id = nearest_outpost_id || this.map_id;
    }
}

export const zaishen_bounty_cycles = [
    map_ids.Poisoned_Outcrops,               // Droajam, Mage of the Sands
    map_ids.Nahpui_Quarter_explorable,       // Royen Beastkeeper
    map_ids.Bloodstone_Caves_Level_1,        // Eldritch Ettin
    map_ids.The_Underworld,                  // Vengeful Aatxe
    map_ids.Fronis_Irontoes_Lair_mission,    // Fronis Irontoe
    map_ids.Urgozs_Warren,                   // Urgoz
    map_ids.Norrhart_Domains,                // Fenrir
    map_ids.Slavers_Exile_Level_1,           // Selvetarm
    map_ids.Gyala_Hatchery,                  // Mohby Windbeak
    map_ids.The_Underworld,                  // Charged Blackness
    map_ids.Majestys_Rest,                   // Rotscale
    map_ids.Vloxen_Excavations_Level_1,      // Zoldark the Unholy
    map_ids.Forum_Highlands,                 // Korshek the Immolated
    map_ids.Drakkar_Lake,                    // Myish, Lady of the Lake
    map_ids.Frostmaws_Burrows_Level_1,       // Frostmaw the Kinslayer
    map_ids.Unwaking_Waters,                 // Kunvie Firewing
    map_ids.Bogroot_Growths_Level_1,         // Z'him Monns
    map_ids.Domain_of_Anguish,               // The Greater Darkness
    map_ids.Oolas_Lab_Level_1,               // TPS Regulator Golem
    map_ids.Ravens_Point_Level_1,            // Plague of Destruction
    map_ids.Tomb_of_the_Primeval_Kings,      // The Darknesses
    map_ids.Jahai_Bluffs,                    // Admiral Kantoh
    map_ids.Sacnoth_Valley,                  // Borrguus Blisterbark
    map_ids.Slavers_Exile_Level_1,           // Forgewight
    map_ids.The_Undercity,                   // Baubao Wavewrath
    map_ids.Riven_Earth,                     // Joffs the Mitigator
    map_ids.Rragars_Menagerie_Level_1,       // Rragar Maneater
    map_ids.The_Undercity,                   // Chung, the Attuned
    map_ids.Domain_of_Anguish,               // Lord Jadoth
    map_ids.Drakkar_Lake,                    // Nulfastu, Earthbound
    map_ids.Sorrows_Furnace,                 // The Iron Forgeman
    map_ids.Heart_of_the_Shiverpeaks_Level_1,// Magmus
    map_ids.Sparkfly_Swamp,                  // Mobrin, Lord of the Marsh
    map_ids.Vehtendi_Valley,                 // Jarimiya the Unmerciful
    map_ids.Slavers_Exile_Level_1,           // Duncan the Black
    map_ids.Tahnnakai_Temple_explorable,     // Quansong Spiritspeak
    map_ids.Domain_of_Anguish,               // The Stygian Underlords
    map_ids.Sacnoth_Valley,                  // Fozzy Yeoryios
    map_ids.Domain_of_Anguish,               // The Black Beast of Arrgh
    map_ids.Arachnis_Haunt_Level_1,          // Arachni
    map_ids.The_Underworld,                  // The Four Horsemen
    map_ids.Sepulchre_of_Dragrimmar_Level_1, // Remnant of Antiquities
    map_ids.Morostav_Trail,                  // Arbor Earthcal
    map_ids.Ooze_Pit_mission,                // Prismatic Ooze
    map_ids.The_Fissure_of_Woe,              // Lord Khobay
    map_ids.Crystal_Overlook,                // Jedeh the Mighty
    map_ids.Archipelagos,                    // Ssuns, Blessed of Dwayna
    map_ids.Slavers_Exile_Level_1,           // Justiciar Thommis
    map_ids.Perdition_Rock,                  // Harn and Maxine Coldstone
    map_ids.Alcazia_Tangle,                  // Pywatt the Swift
    map_ids.Shards_of_Orr_Level_1,           // Fendi Nin
    map_ids.Ferndale,                        // Mungri Magicbox
    map_ids.The_Fissure_of_Woe,              // Priest of Menzies
    map_ids.Catacombs_of_Kathandrax_Level_1, // Ilsundur, Lord of Fire
    map_ids.Prophets_Path,                   // Kepkhet Marrowfeast
    map_ids.Barbarous_Shore,                 // Commander Wahli
    map_ids.The_Deep,                        // Kanaxai
    map_ids.Bogroot_Growths_Level_1,         // Khabuus
    map_ids.Dalada_Uplands,                  // Molotov Rocktai
    map_ids.Tomb_of_the_Primeval_Kings,      // The Stygian Lords
    map_ids.The_Fissure_of_Woe,              // Dragon Lich
    map_ids.Darkrime_Delves_Level_1,         // Havok Soulwai
    map_ids.Xaquang_Skyway,                  // Ghial the Bone Dancer
    map_ids.Cathedral_of_Flames_Level_1,     // Murakai, Lady of the Night
    map_ids.Slavers_Exile_Level_1,           // Rand Stormweaver
    map_ids.Kessex_Peak,                     // Verata
].map((map_id) => {
    return new DailyQuest(map_id);
});

export const zaishen_combat_cycles = [
    map_ids.The_Jade_Quarry_mission,
    map_ids.Codex_Arena_outpost,
    map_ids.Heroes_Ascent_outpost,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.Heroes_Ascent_outpost,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.Codex_Arena_outpost,
    map_ids.Fort_Aspenwood_mission,
    map_ids.The_Jade_Quarry_mission,
    map_ids.Random_Arenas_outpost,
    map_ids.Codex_Arena_outpost,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.The_Jade_Quarry_mission,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.Heroes_Ascent_outpost,
    map_ids.Random_Arenas_outpost,
    map_ids.Fort_Aspenwood_mission,
    map_ids.The_Jade_Quarry_mission,
    map_ids.Random_Arenas_outpost,
    map_ids.Fort_Aspenwood_mission,
    map_ids.Heroes_Ascent_outpost,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.Isle_of_the_Dead_guild_hall,
    map_ids.Codex_Arena_outpost,
    map_ids.Random_Arenas_outpost,
    map_ids.Fort_Aspenwood_mission,
    map_ids.Isle_of_the_Dead_guild_hall
].map((map_id) => {
    return new DailyQuest(map_id);
});

export const zaishen_vanquish_cycles = [
    map_ids.Jaya_Bluffs,
    map_ids.Holdings_of_Chokhin,
    map_ids.Ice_Cliff_Chasms,
    map_ids.Griffons_Mouth,
    map_ids.Kinya_Province,
    map_ids.Issnur_Isles,
    map_ids.Jaga_Moraine,
    map_ids.Ice_Floe,
    map_ids.Maishang_Hills,
    map_ids.Jahai_Bluffs,
    map_ids.Riven_Earth,
    map_ids.Icedome,
    map_ids.Minister_Chos_Estate_explorable,
    map_ids.Mehtani_Keys,
    map_ids.Sacnoth_Valley,
    map_ids.Iron_Horse_Mine,
    map_ids.Morostav_Trail,
    map_ids.Plains_of_Jarin,
    map_ids.Sparkfly_Swamp,
    map_ids.Kessex_Peak,
    map_ids.Mourning_Veil_Falls,
    map_ids.The_Alkali_Pan,
    map_ids.Varajar_Fells,
    map_ids.Lornars_Pass,
    map_ids.Pongmei_Valley,
    map_ids.The_Floodplain_of_Mahnkelon,
    map_ids.Verdant_Cascades,
    map_ids.Majestys_Rest,
    map_ids.Raisu_Palace,
    map_ids.The_Hidden_City_of_Ahdashim,
    map_ids.Rheas_Crater,
    map_ids.Mamnoon_Lagoon,
    map_ids.Shadows_Passage,
    map_ids.The_Mirror_of_Lyss,
    map_ids.Saoshang_Trail,
    map_ids.Nebo_Terrace,
    map_ids.Shenzun_Tunnels,
    map_ids.The_Ruptured_Heart,
    map_ids.Salt_Flats,
    map_ids.North_Kryta_Province,
    map_ids.Silent_Surf,
    map_ids.The_Shattered_Ravines,
    map_ids.Scoundrels_Rise,
    map_ids.Old_Ascalon,
    map_ids.Sunjiang_District_explorable,
    map_ids.The_Sulfurous_Wastes,
    map_ids.Magus_Stones,
    map_ids.Perdition_Rock,
    map_ids.Sunqua_Vale,
    map_ids.Turais_Procession,
    map_ids.Norrhart_Domains,
    map_ids.Pockmark_Flats,
    map_ids.Tahnnakai_Temple_explorable,
    map_ids.Vehjin_Mines,
    map_ids.Poisoned_Outcrops,
    map_ids.Prophets_Path,
    map_ids.The_Eternal_Grove,
    map_ids.Tascas_Demise,
    map_ids.Resplendent_Makuun,
    map_ids.Reed_Bog,
    map_ids.Unwaking_Waters,
    map_ids.Stingray_Strand,
    map_ids.Sunward_Marches,
    map_ids.Regent_Valley,
    map_ids.Wajjun_Bazaar,
    map_ids.Yatendi_Canyons,
    map_ids.Twin_Serpent_Lakes,
    map_ids.Sage_Lands,
    map_ids.Xaquang_Skyway,
    map_ids.Zehlon_Reach,
    map_ids.Tangle_Root,
    map_ids.Silverwood,
    map_ids.Zen_Daijun_explorable,
    map_ids.The_Arid_Sea,
    map_ids.Nahpui_Quarter_explorable,
    map_ids.Skyward_Reach,
    map_ids.The_Scar,
    map_ids.The_Black_Curtain,
    map_ids.Panjiang_Peninsula,
    map_ids.Snake_Dance,
    map_ids.Travelers_Vale,
    map_ids.The_Breach,
    map_ids.Lahtenda_Bog,
    map_ids.Spearhead_Peak,
    map_ids.Mount_Qinkai,
    map_ids.Marga_Coast,
    map_ids.Melandrus_Hope,
    map_ids.The_Falls,
    map_ids.Jokos_Domain,
    map_ids.Vulture_Drifts,
    map_ids.Wilderness_of_Bahdza,
    map_ids.Talmark_Wilderness,
    map_ids.Vehtendi_Valley,
    map_ids.Talus_Chute,
    map_ids.Mineral_Springs,
    map_ids.Anvil_Rock,
    map_ids.Arborstone_explorable,
    map_ids.Witmans_Folly,
    map_ids.Arkjok_Ward,
    map_ids.Ascalon_Foothills,
    map_ids.Bahdok_Caverns,
    map_ids.Cursed_Lands,
    map_ids.Alcazia_Tangle,
    map_ids.Archipelagos,
    map_ids.Eastern_Frontier,
    map_ids.Dejarin_Estate,
    map_ids.Watchtower_Coast,
    map_ids.Arbor_Bay,
    map_ids.Barbarous_Shore,
    map_ids.Deldrimor_Bowl,
    map_ids.Boreas_Seabed_explorable,
    map_ids.Cliffs_of_Dohjok,
    map_ids.Diessa_Lowlands,
    map_ids.Bukdek_Byway,
    map_ids.Bjora_Marches,
    map_ids.Crystal_Overlook,
    map_ids.Diviners_Ascent,
    map_ids.Dalada_Uplands,
    map_ids.Drazach_Thicket,
    map_ids.Fahranur_The_First_City ,
    map_ids.Dragons_Gullet,
    map_ids.Ferndale,
    map_ids.Forum_Highlands,
    map_ids.Dreadnoughts_Drift,
    map_ids.Drakkar_Lake,
    map_ids.Dry_Top,
    map_ids.Tears_of_the_Fallen,
    map_ids.Gyala_Hatchery,
    map_ids.Ettins_Back,
    map_ids.Gandara_the_Moon_Fortress,
    map_ids.Grothmar_Wardowns,
    map_ids.Flame_Temple_Corridor,
    map_ids.Haiju_Lagoon,
    map_ids.Frozen_Forest,
    map_ids.Garden_of_Seborhin,
    map_ids.Grenths_Footprint
].map((map_id) => {
    return new DailyQuest(map_id);
});

export const zaishen_mission_cycles = [
    map_ids.Augury_Rock_mission,
    map_ids.Grand_Court_of_Sebelkeh,
    map_ids.Ice_Caves_of_Sorrow,
    map_ids.Raisu_Palace_outpost_mission,
    map_ids.Gate_of_Desolation,
    map_ids.Thirsty_River,
    map_ids.Blacktide_Den,
    map_ids.Against_the_Charr_mission,
    map_ids.Abaddons_Mouth,
    map_ids.Nundu_Bay,
    map_ids.Divinity_Coast,
    map_ids.Zen_Daijun_outpost_mission,
    map_ids.Pogahn_Passage,
    map_ids.Tahnnakai_Temple_outpost_mission,
    map_ids.The_Great_Northern_Wall,
    map_ids.Dasha_Vestibule,
    map_ids.The_Wilds,
    map_ids.Unwaking_Waters_mission,
    map_ids.Chahbek_Village,
    map_ids.Aurora_Glade,
    map_ids.A_Time_for_Heroes_mission,
    map_ids.Consulate_Docks,
    map_ids.Ring_of_Fire,
    map_ids.Nahpui_Quarter_outpost_mission,
    map_ids.The_Dragons_Lair,
    map_ids.Dzagonur_Bastion,
    map_ids.DAlessio_Seaboard,
    map_ids.Assault_on_the_Stronghold_mission,
    map_ids.The_Eternal_Grove_outpost_mission,
    map_ids.Sanctum_Cay,
    map_ids.Rilohn_Refuge,
    map_ids.Warband_of_brothers_mission,
    map_ids.Borlis_Pass,
    map_ids.Imperial_Sanctum_outpost_mission,
    map_ids.Moddok_Crevice,
    map_ids.Nolani_Academy,
    map_ids.Destructions_Depths_mission,
    map_ids.Venta_Cemetery,
    map_ids.Fort_Ranik,
    map_ids.A_Gate_Too_Far_mission,
    map_ids.Minister_Chos_Estate_outpost_mission,
    map_ids.Thunderhead_Keep,
    map_ids.Tihark_Orchard,
    map_ids.Finding_the_Bloodstone_mission,
    map_ids.Dunes_of_Despair,
    map_ids.Vizunah_Square_mission,
    map_ids.Jokanur_Diggings,
    map_ids.Iron_Mines_of_Moladune,
    map_ids.Kodonur_Crossroads,
    map_ids.Genius_Operated_Living_Enchanted_Manifestation_mission,
    map_ids.Arborstone_outpost_mission,
    map_ids.Gates_of_Kryta,
    map_ids.Gate_of_Madness,
    map_ids.The_Elusive_Golemancer_mission,
    map_ids.Riverside_Province,
    map_ids.Boreas_Seabed_outpost_mission,
    map_ids.Ruins_of_Morah,
    map_ids.Hells_Precipice,
    map_ids.Ruins_of_Surmia,
    map_ids.Curse_of_the_Nornbear_mission,
    map_ids.Sunjiang_District_outpost_mission,
    map_ids.Elona_Reach,
    map_ids.Gate_of_Pain,
    map_ids.Blood_Washes_Blood_mission,
    map_ids.Bloodstone_Fen,
    map_ids.Jennurs_Horde,
    map_ids.Gyala_Hatchery_outpost_mission,
    map_ids.Abaddons_Gate,
    map_ids.The_Frost_Gate
].map((map_id) => {
    return new DailyQuest(map_id);
});

function GetZaishenBountyIdx(_unix) {
    if(_unix > 2147483647)
        _unix /= 1000;
    return Math.floor((_unix - 1244736000) / 86400 % zaishen_bounty_cycles.length);
}

function GetZaishenCombatIdx(_unix) {
    if(_unix > 2147483647)
        _unix /= 1000;
    return Math.floor((_unix - 1256227200) / 86400 % zaishen_combat_cycles.length);
}

function GetZaishenMissionIdx(_unix) {
    if(_unix > 2147483647)
        _unix /= 1000;
    return Math.floor((_unix - 1299168000) / 86400 % zaishen_mission_cycles.length);
}

function GetZaishenVanquishIdx(_unix) {
    if(_unix > 2147483647)
        _unix /= 1000;
    return Math.floor((_unix - 1299168000) / 86400 % zaishen_vanquish_cycles.length);
}

export function GetZaishenBounty(_unix = Date.now()) {
    const idx = GetZaishenBountyIdx(_unix);
    return zaishen_bounty_cycles[idx];
}
export function GetZaishenMission(_unix = Date.now()) {
    const idx = GetZaishenMissionIdx(_unix);
    return zaishen_mission_cycles[idx];
}
export function GetZaishenCombat(_unix = Date.now()) {
    const idx = GetZaishenCombatIdx(_unix);
    return zaishen_combat_cycles[idx];
}
export function GetZaishenVanquish(_unix = Date.now()) {
    const idx = GetZaishenVanquishIdx(_unix);
    return zaishen_vanquish_cycles[idx];
}