
#include "gw_constants.h"

namespace DailyQuests {
    constexpr size_t ZAISHEN_BOUNTY_COUNT = 66;
    constexpr size_t ZAISHEN_MISSION_COUNT = 69;
    constexpr size_t ZAISHEN_COMBAT_COUNT = 28;
    constexpr size_t ZAISHEN_VANQUISH_COUNT = 136;
    constexpr int SECONDSINAWEEK = 604800;

    struct DailyQuest {
        uint32_t map_id = 0;
        uint32_t nearest_outpost_id = 0;
        DailyQuest(GW::Constants::MapID _map_id, GW::Constants::MapID _nearest_outpost_id = (GW::Constants::MapID)0) {
            map_id = (uint32_t)_map_id;
            nearest_outpost_id = (uint32_t)_nearest_outpost_id;
            if (nearest_outpost_id == 0) {
                nearest_outpost_id = get_nearest_outpost_id(map_id);
            }
        }
    };
    DailyQuest zaishen_bounty_cycles[] = {
    { GW::Constants::MapID::Poisoned_Outcrops },               // Droajam, Mage of the Sands
    { GW::Constants::MapID::Nahpui_Quarter_explorable },       // Royen Beastkeeper
    { GW::Constants::MapID::Bloodstone_Caves_Level_1 },        // Eldritch Ettin
    { GW::Constants::MapID::The_Underworld },                  // Vengeful Aatxe
    { GW::Constants::MapID::Fronis_Irontoes_Lair_mission },    // Fronis Irontoe
    { GW::Constants::MapID::Urgozs_Warren },                   // Urgoz
    { GW::Constants::MapID::Norrhart_Domains },                // Fenrir
    { GW::Constants::MapID::Slavers_Exile_Level_1 },           // Selvetarm
    { GW::Constants::MapID::Gyala_Hatchery },                  // Mohby Windbeak
    { GW::Constants::MapID::The_Underworld },                  // Charged Blackness
    { GW::Constants::MapID::Majestys_Rest },                   // Rotscale
    { GW::Constants::MapID::Vloxen_Excavations_Level_1 },      // Zoldark the Unholy
    { GW::Constants::MapID::Forum_Highlands },                 // Korshek the Immolated
    { GW::Constants::MapID::Drakkar_Lake },                    // Myish, Lady of the Lake
    { GW::Constants::MapID::Frostmaws_Burrows_Level_1 },       // Frostmaw the Kinslayer
    { GW::Constants::MapID::Unwaking_Waters },                 // Kunvie Firewing
    { GW::Constants::MapID::Bogroot_Growths_Level_1 },         // Z'him Monns
    { GW::Constants::MapID::Domain_of_Anguish },               // The Greater Darkness
    { GW::Constants::MapID::Oolas_Lab_Level_1 },               // TPS Regulator Golem
    { GW::Constants::MapID::Ravens_Point_Level_1 },            // Plague of Destruction
    { GW::Constants::MapID::Tomb_of_the_Primeval_Kings },      // The Darknesses
    { GW::Constants::MapID::Jahai_Bluffs },                    // Admiral Kantoh
    { GW::Constants::MapID::Sacnoth_Valley },                  // Borrguus Blisterbark
    { GW::Constants::MapID::Slavers_Exile_Level_1 },           // Forgewight
    { GW::Constants::MapID::The_Undercity },                   // Baubao Wavewrath
    { GW::Constants::MapID::Riven_Earth },                     // Joffs the Mitigator
    { GW::Constants::MapID::Rragars_Menagerie_Level_1 },       // Rragar Maneater
    { GW::Constants::MapID::The_Undercity },                   // Chung, the Attuned
    { GW::Constants::MapID::Domain_of_Anguish },               // Lord Jadoth
    { GW::Constants::MapID::Drakkar_Lake },                    // Nulfastu, Earthbound
    { GW::Constants::MapID::Sorrows_Furnace },                 // The Iron Forgeman
    { GW::Constants::MapID::Heart_of_the_Shiverpeaks_Level_1 },// Magmus
    { GW::Constants::MapID::Sparkfly_Swamp },                  // Mobrin, Lord of the Marsh
    { GW::Constants::MapID::Vehtendi_Valley },                 // Jarimiya the Unmerciful
    { GW::Constants::MapID::Slavers_Exile_Level_1 },           // Duncan the Black
    { GW::Constants::MapID::Tahnnakai_Temple_explorable },     // Quansong Spiritspeak
    { GW::Constants::MapID::Domain_of_Anguish },               // The Stygian Underlords
    { GW::Constants::MapID::Sacnoth_Valley },                  // Fozzy Yeoryios
    { GW::Constants::MapID::Domain_of_Anguish },               // The Black Beast of Arrgh
    { GW::Constants::MapID::Arachnis_Haunt_Level_1 },          // Arachni
    { GW::Constants::MapID::The_Underworld },                  // The Four Horsemen
    { GW::Constants::MapID::Sepulchre_of_Dragrimmar_Level_1 }, // Remnant of Antiquities
    { GW::Constants::MapID::Morostav_Trail },                  // Arbor Earthcal
    { GW::Constants::MapID::Ooze_Pit_mission },                // Prismatic Ooze
    { GW::Constants::MapID::The_Fissure_of_Woe },              // Lord Khobay
    { GW::Constants::MapID::Crystal_Overlook },                // Jedeh the Mighty
    { GW::Constants::MapID::Archipelagos },                    // Ssuns, Blessed of Dwayna
    { GW::Constants::MapID::Slavers_Exile_Level_1 },           // Justiciar Thommis
    { GW::Constants::MapID::Perdition_Rock },                  // Harn and Maxine Coldstone
    { GW::Constants::MapID::Alcazia_Tangle },                  // Pywatt the Swift
    { GW::Constants::MapID::Shards_of_Orr_Level_1 },           // Fendi Nin
    { GW::Constants::MapID::Ferndale },                        // Mungri Magicbox
    { GW::Constants::MapID::The_Fissure_of_Woe },              // Priest of Menzies
    { GW::Constants::MapID::Catacombs_of_Kathandrax_Level_1 }, // Ilsundur, Lord of Fire
    { GW::Constants::MapID::Prophets_Path },                   // Kepkhet Marrowfeast
    { GW::Constants::MapID::Barbarous_Shore },                 // Commander Wahli
    { GW::Constants::MapID::The_Deep },                        // Kanaxai
    { GW::Constants::MapID::Bogroot_Growths_Level_1 },         // Khabuus
    { GW::Constants::MapID::Dalada_Uplands },                  // Molotov Rocktai
    { GW::Constants::MapID::Tomb_of_the_Primeval_Kings },      // The Stygian Lords
    { GW::Constants::MapID::The_Fissure_of_Woe },              // Dragon Lich
    { GW::Constants::MapID::Darkrime_Delves_Level_1 },         // Havok Soulwai
    { GW::Constants::MapID::Xaquang_Skyway },                  // Ghial the Bone Dancer
    { GW::Constants::MapID::Cathedral_of_Flames_Level_1 },     // Murakai, Lady of the Night
    { GW::Constants::MapID::Slavers_Exile_Level_1 },           // Rand Stormweaver
    { GW::Constants::MapID::Kessex_Peak },                     // Verata
    };
    static_assert(ARRAY_SIZE(zaishen_bounty_cycles) == ZAISHEN_BOUNTY_COUNT);

    DailyQuest zaishen_combat_cycles[] = {
        {GW::Constants::MapID::The_Jade_Quarry_mission},
        {GW::Constants::MapID::Codex_Arena_outpost},
        {GW::Constants::MapID::Heroes_Ascent_outpost},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::Heroes_Ascent_outpost},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::Codex_Arena_outpost},
        {GW::Constants::MapID::Fort_Aspenwood_mission},
        {GW::Constants::MapID::The_Jade_Quarry_mission},
        {GW::Constants::MapID::Random_Arenas_outpost},
        {GW::Constants::MapID::Codex_Arena_outpost},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::The_Jade_Quarry_mission},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::Heroes_Ascent_outpost},
        {GW::Constants::MapID::Random_Arenas_outpost},
        {GW::Constants::MapID::Fort_Aspenwood_mission},
        {GW::Constants::MapID::The_Jade_Quarry_mission},
        {GW::Constants::MapID::Random_Arenas_outpost},
        {GW::Constants::MapID::Fort_Aspenwood_mission},
        {GW::Constants::MapID::Heroes_Ascent_outpost},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall},
        {GW::Constants::MapID::Codex_Arena_outpost},
        {GW::Constants::MapID::Random_Arenas_outpost},
        {GW::Constants::MapID::Fort_Aspenwood_mission},
        {GW::Constants::MapID::Isle_of_the_Dead_guild_hall}
    };
    static_assert(ARRAY_SIZE(zaishen_combat_cycles) == ZAISHEN_COMBAT_COUNT);

    DailyQuest zaishen_vanquish_cycles[] = {
            {GW::Constants::MapID::Jaya_Bluffs},
            {GW::Constants::MapID::Holdings_of_Chokhin},
            {GW::Constants::MapID::Ice_Cliff_Chasms},
            {GW::Constants::MapID::Griffons_Mouth},
            {GW::Constants::MapID::Kinya_Province},
            {GW::Constants::MapID::Issnur_Isles},
            {GW::Constants::MapID::Jaga_Moraine},
            {GW::Constants::MapID::Ice_Floe},
            {GW::Constants::MapID::Maishang_Hills},
            {GW::Constants::MapID::Jahai_Bluffs},
            {GW::Constants::MapID::Riven_Earth},
            {GW::Constants::MapID::Icedome},
            {GW::Constants::MapID::Minister_Chos_Estate_explorable},
            {GW::Constants::MapID::Mehtani_Keys},
            {GW::Constants::MapID::Sacnoth_Valley},
            {GW::Constants::MapID::Iron_Horse_Mine},
            {GW::Constants::MapID::Morostav_Trail},
            {GW::Constants::MapID::Plains_of_Jarin},
            {GW::Constants::MapID::Sparkfly_Swamp},
            {GW::Constants::MapID::Kessex_Peak},
            {GW::Constants::MapID::Mourning_Veil_Falls},
            {GW::Constants::MapID::The_Alkali_Pan},
            {GW::Constants::MapID::Varajar_Fells},
            {GW::Constants::MapID::Lornars_Pass},
            {GW::Constants::MapID::Pongmei_Valley},
            {GW::Constants::MapID::The_Floodplain_of_Mahnkelon},
            {GW::Constants::MapID::Verdant_Cascades},
            {GW::Constants::MapID::Majestys_Rest},
            {GW::Constants::MapID::Raisu_Palace},
            {GW::Constants::MapID::The_Hidden_City_of_Ahdashim},
            {GW::Constants::MapID::Rheas_Crater},
            {GW::Constants::MapID::Mamnoon_Lagoon},
            {GW::Constants::MapID::Shadows_Passage},
            {GW::Constants::MapID::The_Mirror_of_Lyss},
            {GW::Constants::MapID::Saoshang_Trail},
            {GW::Constants::MapID::Nebo_Terrace},
            {GW::Constants::MapID::Shenzun_Tunnels},
            {GW::Constants::MapID::The_Ruptured_Heart},
            {GW::Constants::MapID::Salt_Flats},
            {GW::Constants::MapID::North_Kryta_Province},
            {GW::Constants::MapID::Silent_Surf},
            {GW::Constants::MapID::The_Shattered_Ravines},
            {GW::Constants::MapID::Scoundrels_Rise},
            {GW::Constants::MapID::Old_Ascalon},
            {GW::Constants::MapID::Sunjiang_District_explorable},
            {GW::Constants::MapID::The_Sulfurous_Wastes},
            {GW::Constants::MapID::Magus_Stones},
            {GW::Constants::MapID::Perdition_Rock},
            {GW::Constants::MapID::Sunqua_Vale},
            {GW::Constants::MapID::Turais_Procession},
            {GW::Constants::MapID::Norrhart_Domains},
            {GW::Constants::MapID::Pockmark_Flats},
            {GW::Constants::MapID::Tahnnakai_Temple_explorable},
            {GW::Constants::MapID::Vehjin_Mines},
            {GW::Constants::MapID::Poisoned_Outcrops},
            {GW::Constants::MapID::Prophets_Path},
            {GW::Constants::MapID::The_Eternal_Grove},
            {GW::Constants::MapID::Tascas_Demise},
            {GW::Constants::MapID::Resplendent_Makuun},
            {GW::Constants::MapID::Reed_Bog},
            {GW::Constants::MapID::Unwaking_Waters},
            {GW::Constants::MapID::Stingray_Strand},
            {GW::Constants::MapID::Sunward_Marches},
            {GW::Constants::MapID::Regent_Valley},
            {GW::Constants::MapID::Wajjun_Bazaar},
            {GW::Constants::MapID::Yatendi_Canyons},
            {GW::Constants::MapID::Twin_Serpent_Lakes},
            {GW::Constants::MapID::Sage_Lands},
            {GW::Constants::MapID::Xaquang_Skyway},
            {GW::Constants::MapID::Zehlon_Reach},
            {GW::Constants::MapID::Tangle_Root},
            {GW::Constants::MapID::Silverwood},
            {GW::Constants::MapID::Zen_Daijun_explorable},
            {GW::Constants::MapID::The_Arid_Sea},
            {GW::Constants::MapID::Nahpui_Quarter_explorable},
            {GW::Constants::MapID::Skyward_Reach},
            {GW::Constants::MapID::The_Scar},
            {GW::Constants::MapID::The_Black_Curtain},
            {GW::Constants::MapID::Panjiang_Peninsula},
            {GW::Constants::MapID::Snake_Dance},
            {GW::Constants::MapID::Travelers_Vale},
            {GW::Constants::MapID::The_Breach},
            {GW::Constants::MapID::Lahtenda_Bog},
            {GW::Constants::MapID::Spearhead_Peak},
            {GW::Constants::MapID::Mount_Qinkai},
            {GW::Constants::MapID::Marga_Coast},
            {GW::Constants::MapID::Melandrus_Hope},
            {GW::Constants::MapID::The_Falls},
            {GW::Constants::MapID::Jokos_Domain},
            {GW::Constants::MapID::Vulture_Drifts},
            {GW::Constants::MapID::Wilderness_of_Bahdza},
            {GW::Constants::MapID::Talmark_Wilderness},
            {GW::Constants::MapID::Vehtendi_Valley},
            {GW::Constants::MapID::Talus_Chute},
            {GW::Constants::MapID::Mineral_Springs},
            {GW::Constants::MapID::Anvil_Rock},
            {GW::Constants::MapID::Arborstone_explorable},
            {GW::Constants::MapID::Witmans_Folly},
            {GW::Constants::MapID::Arkjok_Ward},
            {GW::Constants::MapID::Ascalon_Foothills},
            {GW::Constants::MapID::Bahdok_Caverns},
            {GW::Constants::MapID::Cursed_Lands},
            {GW::Constants::MapID::Alcazia_Tangle},
            {GW::Constants::MapID::Archipelagos},
            {GW::Constants::MapID::Eastern_Frontier},
            {GW::Constants::MapID::Dejarin_Estate},
            {GW::Constants::MapID::Watchtower_Coast},
            {GW::Constants::MapID::Arbor_Bay},
            {GW::Constants::MapID::Barbarous_Shore},
            {GW::Constants::MapID::Deldrimor_Bowl},
            {GW::Constants::MapID::Boreas_Seabed_explorable},
            {GW::Constants::MapID::Cliffs_of_Dohjok},
            {GW::Constants::MapID::Diessa_Lowlands},
            {GW::Constants::MapID::Bukdek_Byway},
            {GW::Constants::MapID::Bjora_Marches},
            {GW::Constants::MapID::Crystal_Overlook},
            {GW::Constants::MapID::Diviners_Ascent},
            {GW::Constants::MapID::Dalada_Uplands},
            {GW::Constants::MapID::Drazach_Thicket},
            {GW::Constants::MapID::Fahranur_The_First_City },
            {GW::Constants::MapID::Dragons_Gullet},
            {GW::Constants::MapID::Ferndale},
            {GW::Constants::MapID::Forum_Highlands},
            {GW::Constants::MapID::Dreadnoughts_Drift},
            {GW::Constants::MapID::Drakkar_Lake},
            {GW::Constants::MapID::Dry_Top},
            {GW::Constants::MapID::Tears_of_the_Fallen},
            {GW::Constants::MapID::Gyala_Hatchery},
            {GW::Constants::MapID::Ettins_Back},
            {GW::Constants::MapID::Gandara_the_Moon_Fortress},
            {GW::Constants::MapID::Grothmar_Wardowns},
            {GW::Constants::MapID::Flame_Temple_Corridor},
            {GW::Constants::MapID::Haiju_Lagoon},
            {GW::Constants::MapID::Frozen_Forest},
            {GW::Constants::MapID::Garden_of_Seborhin},
            {GW::Constants::MapID::Grenths_Footprint}
    };
    static_assert(ARRAY_SIZE(zaishen_vanquish_cycles) == ZAISHEN_VANQUISH_COUNT);

    DailyQuest zaishen_mission_cycles[] = {
      GW::Constants::MapID::Augury_Rock_mission,
      GW::Constants::MapID::Grand_Court_of_Sebelkeh,
      GW::Constants::MapID::Ice_Caves_of_Sorrow,
      GW::Constants::MapID::Raisu_Palace_outpost_mission,
      GW::Constants::MapID::Gate_of_Desolation,
      GW::Constants::MapID::Thirsty_River,
      GW::Constants::MapID::Blacktide_Den,
      GW::Constants::MapID::Against_the_Charr_mission,
      GW::Constants::MapID::Abaddons_Mouth,
      GW::Constants::MapID::Nundu_Bay,
      GW::Constants::MapID::Divinity_Coast,
      GW::Constants::MapID::Zen_Daijun_outpost_mission,
      GW::Constants::MapID::Pogahn_Passage,
      GW::Constants::MapID::Tahnnakai_Temple_outpost_mission,
      GW::Constants::MapID::The_Great_Northern_Wall,
      GW::Constants::MapID::Dasha_Vestibule,
      GW::Constants::MapID::The_Wilds,
      GW::Constants::MapID::Unwaking_Waters_mission,
      GW::Constants::MapID::Chahbek_Village,
      GW::Constants::MapID::Aurora_Glade,
      GW::Constants::MapID::A_Time_for_Heroes_mission,
      GW::Constants::MapID::Consulate_Docks,
      GW::Constants::MapID::Ring_of_Fire,
      GW::Constants::MapID::Nahpui_Quarter_outpost_mission,
      GW::Constants::MapID::The_Dragons_Lair,
      GW::Constants::MapID::Dzagonur_Bastion,
      GW::Constants::MapID::DAlessio_Seaboard,
      GW::Constants::MapID::Assault_on_the_Stronghold_mission,
      GW::Constants::MapID::The_Eternal_Grove_outpost_mission,
      GW::Constants::MapID::Sanctum_Cay,
      GW::Constants::MapID::Rilohn_Refuge,
      GW::Constants::MapID::Warband_of_brothers_mission,
      GW::Constants::MapID::Borlis_Pass,
      GW::Constants::MapID::Imperial_Sanctum_outpost_mission,
      GW::Constants::MapID::Moddok_Crevice,
      GW::Constants::MapID::Nolani_Academy,
      GW::Constants::MapID::Destructions_Depths_mission,
      GW::Constants::MapID::Venta_Cemetery,
      GW::Constants::MapID::Fort_Ranik,
      GW::Constants::MapID::A_Gate_Too_Far_mission,
      GW::Constants::MapID::Minister_Chos_Estate_outpost_mission,
      GW::Constants::MapID::Thunderhead_Keep,
      GW::Constants::MapID::Tihark_Orchard,
      GW::Constants::MapID::Finding_the_Bloodstone_mission,
      GW::Constants::MapID::Dunes_of_Despair,
      GW::Constants::MapID::Vizunah_Square_mission,
      GW::Constants::MapID::Jokanur_Diggings,
      GW::Constants::MapID::Iron_Mines_of_Moladune,
      GW::Constants::MapID::Kodonur_Crossroads,
      GW::Constants::MapID::Genius_Operated_Living_Enchanted_Manifestation_mission,
      GW::Constants::MapID::Arborstone_outpost_mission,
      GW::Constants::MapID::Gates_of_Kryta,
      GW::Constants::MapID::Gate_of_Madness,
      GW::Constants::MapID::The_Elusive_Golemancer_mission,
      GW::Constants::MapID::Riverside_Province,
      GW::Constants::MapID::Boreas_Seabed_outpost_mission,
      GW::Constants::MapID::Ruins_of_Morah,
      GW::Constants::MapID::Hells_Precipice,
      GW::Constants::MapID::Ruins_of_Surmia,
      GW::Constants::MapID::Curse_of_the_Nornbear_mission,
      GW::Constants::MapID::Sunjiang_District_outpost_mission,
      GW::Constants::MapID::Elona_Reach,
      GW::Constants::MapID::Gate_of_Pain,
      GW::Constants::MapID::Blood_Washes_Blood_mission,
      GW::Constants::MapID::Bloodstone_Fen,
      GW::Constants::MapID::Jennurs_Horde,
      GW::Constants::MapID::Gyala_Hatchery_outpost_mission,
      GW::Constants::MapID::Abaddons_Gate,
      GW::Constants::MapID::The_Frost_Gate
    };
    static_assert(ARRAY_SIZE(zaishen_mission_cycles) == ZAISHEN_MISSION_COUNT);


    uint32_t GetZaishenBountyIdx(const uint64_t _unix)
    {
        return static_cast<uint32_t>((_unix - 1244736000) / 86400 % ZAISHEN_BOUNTY_COUNT);
    }

    uint32_t GetZaishenCombatIdx(const uint64_t _unix)
    {
        return static_cast<uint32_t>((_unix - 1256227200) / 86400 % ZAISHEN_COMBAT_COUNT);
    }

    uint32_t GetZaishenMissionIdx(const uint64_t _unix)
    {
        return static_cast<uint32_t>((_unix - 1299168000) / 86400 % ZAISHEN_MISSION_COUNT);
    }

    uint32_t GetZaishenVanquishIdx(const uint64_t _unix)
    {
        return static_cast<uint32_t>((_unix - 1299168000) / 86400 % ZAISHEN_VANQUISH_COUNT);
    }

    time_t GetNextEventTime(const time_t cycle_start_time, const time_t current_time, const int event_index, const int event_count, const int interval_in_seconds)
    {
        const auto cycle_duration = interval_in_seconds * event_count;
        const auto cycles_since_start = (current_time - cycle_start_time) / cycle_duration;
        const auto current_cycle_start_time = cycle_start_time + (cycles_since_start * cycle_duration);
        const auto time_in_currentCycle = event_index * interval_in_seconds;
        auto next_event_time = current_cycle_start_time + time_in_currentCycle;
        if (next_event_time < current_time) {
            // The event started in the past. We need to check if the event is ongoing or has already finished
            if (next_event_time + interval_in_seconds < current_time) {
                // The event ended already so we offset the start time with one cycle
                next_event_time += cycle_duration;
            }
            else {
                // The event is ongoing so we set the start time as the current time
                next_event_time = current_time;
            }

        }

        return next_event_time;
    }

    DailyQuest* GetZaishenBounty(const uint64_t _unix)
    {
        auto idx = GetZaishenBountyIdx(_unix);
        return &zaishen_bounty_cycles[idx];
    }

    DailyQuest* GetZaishenMission(const uint64_t _unix)
    {
        auto idx = GetZaishenMissionIdx(_unix);
        return &zaishen_mission_cycles[idx];
    }

    DailyQuest* GetZaishenCombat(const uint64_t _unix)
    {
        auto idx = GetZaishenCombatIdx(_unix);
        return &zaishen_combat_cycles[idx];
    }

    DailyQuest* GetZaishenVanquish(const uint64_t _unix)
    {
        auto idx = GetZaishenVanquishIdx(_unix);
        return &zaishen_vanquish_cycles[idx];
    }
}
