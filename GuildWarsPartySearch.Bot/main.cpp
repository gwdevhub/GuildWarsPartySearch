#ifndef __STDC__
#define __STDC__ 1
#endif

#ifndef _Static_assert
#define _Static_assert static_assert
#endif

#define _CRT_SECURE_NO_WARNINGS
#include <clocale>
#include <cmath>
#include <cstdbool>
#include <csignal>
#include <cstdio>
#include <ctime>

#define HEADQUARTER_RUNTIME_LINKING
extern "C" {
    #include <client/constants.h>
    #include <client/Headquarter.h>
    #include <common/time.h>
    #include <common/thread.h>
    #include <common/paths.h>
}

#ifdef _WIN32
# define DllExport __declspec(dllexport)
#else
# define DllExport
#endif

#include <fstream>
#include <vector>
#include <atomic>
#include <codecvt>

#ifdef array
#undef array
#endif
#include <json.hpp>
#include <easywsclient.hpp>

#include "gw-helper.c"

#include "daily_quests.h"
#include <common/uuid.h>

#define FAILED_TO_START             1
#define FAILED_TO_LOAD_GAME         2
#define FAILED_TO_LOAD_CHAR_NAME    3
#define FAILED_TO_LOAD_CONFIG       4
#define FAILED_TO_CONNECT           5

#define MapID_Ascalon_PreSearing        148
#define MapID_Kamadan                   449
#define MapID_Kamadan_Halloween         818
#define MapID_Kamadan_Wintersday        819
#define MapID_Kamadan_Canthan_New_Year  820
#define MapID_EmbarkBeach               857
#define MapID_GreatTempleOfBalthazar    148

struct BotConfiguration {
    std::string         web_socket_url = "";
    std::string         api_key = "development";
    uint32_t            map_id = 0; // stay in current map
    District            district = District::DISTRICT_CURRENT;
    uint32_t            district_number = 0;
    int32_t             connection_retries = 10;
};

struct PartySearchAdvertisement {
    uint32_t            party_id = 0;
    uint8_t             party_size = 0;
    uint8_t             hero_count = 0;
    uint8_t             search_type = 0; // 0=hunting, 1=mission, 2=quest, 3=trade, 4=guild
    uint8_t             hardmode = 0;
    uint16_t            district_number = 0;
    uint8_t             language = 0;
    uint8_t             primary = 0;
    uint8_t             secondary = 0;
    uint8_t             level = 0;
    char message[65] = { 0 };
    char sender[42] = { 0 };
    PartySearchAdvertisement() {
        memset(this, 0, sizeof(*this));
    }
};

void to_json(nlohmann::json& j, const PartySearchAdvertisement& p) {

    j = nlohmann::json{ {"i", p.party_id} };
    if (!p.party_size) {
        // Aka "remove"
        j["r"] = 1;
        return;
    }
    j["t"] = p.search_type;
    j["p"] = p.primary;
    j["s"] = p.sender;

    // The following fields can be assumed to be reasonable defaults by the server, so only need to send if they're not standard.
    if (p.party_size > 1) j["ps"] = p.party_size;
    if (p.hero_count) j["hc"] = p.hero_count;
    if (p.hardmode) j["hm"] = p.hardmode;
    if (p.language) j["dl"] = p.language;
    if (p.secondary) j["sc"] = p.secondary;
    if (p.district_number) j["dn"] = p.district_number;
    if (*p.message) j["ms"] = p.message;
}

static struct thread  bot_thread;
static std::atomic<bool> running;
static std::string last_payload;
static uint64_t last_websocket_message = 0;
static uint64_t websocket_ping_interval = 30000; // Ping every 30s

District district = District::DISTRICT_CURRENT;
int district_number = -1;
uint32_t map_id = 0;
char character_name[42] = { 0 };
char account_uuid[128] = { 0 };

struct BotPartySearches {
    char client_id_prefix[9] = { 0 };
    uint32_t map_id = 0;
    District district = (District)0;
};

void extract_district(District district, DistrictRegion* region, DistrictLanguage* language)
{
    switch (district) {
    case DISTRICT_INTERNATIONAL:
        *region = DistrictRegion_International;
        *language = DistrictLanguage_Default;
        break;
    case DISTRICT_AMERICAN:
        *region = DistrictRegion_America;
        *language = DistrictLanguage_Default;
        break;
    case DISTRICT_EUROPE_ENGLISH:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_English;
        break;
    case DISTRICT_EUROPE_FRENCH:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_French;
        break;
    case DISTRICT_EUROPE_GERMAN:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_German;
        break;
    case DISTRICT_EUROPE_ITALIAN:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_Italian;
        break;
    case DISTRICT_EUROPE_SPANISH:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_Spanish;
        break;
    case DISTRICT_EUROPE_POLISH:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_Polish;
        break;
    case DISTRICT_EUROPE_RUSSIAN:
        *region = DistrictRegion_Europe;
        *language = DistrictLanguage_Russian;
        break;
    case DISTRICT_ASIA_KOREAN:
        *region = DistrictRegion_Korea;
        *language = DistrictLanguage_Default;
        break;
    case DISTRICT_ASIA_CHINESE:
        *region = DistrictRegion_China;
        *language = DistrictLanguage_Default;
        break;
    case DISTRICT_ASIA_JAPANESE:
        *region = DistrictRegion_Japanese;
        *language = DistrictLanguage_Default;
        break;
    default:
        *region = DistrictRegion_America;
        *language = DistrictLanguage_Default;
        break;
    }
}

DistrictRegion get_district_region(District district) {
    if (district == DISTRICT_CURRENT)
        district = GetDistrict();
    DistrictRegion region = DistrictRegion_America;
    DistrictLanguage language = DistrictLanguage_Default;
    extract_district(district, &region, &language);
    return region;
}


std::vector<BotPartySearches> map_ids_already_visited_by_other_bots;
time_t party_searches_json_updated = 0;

uint32_t wanted_map_id = 0;
District wanted_district = District::DISTRICT_CURRENT;

static PluginObject* plugin_hook;
static BotConfiguration bot_configuration;

static CallbackEntry EventType_PartySearchAdvertisement_entry;
static CallbackEntry EventType_PartySearchRemoved_entry;
static CallbackEntry EventType_PartySearchSize_entry;
static CallbackEntry EventType_PartySearchType_entry;
static CallbackEntry EventType_WorldMapEnter_entry;
static CallbackEntry EventType_PartyMembersChanged_entry;
static CallbackEntry EventType_PlayerPartySize_entry;
static CallbackEntry EventType_AgentDespawned_entry;

static bool party_advertisements_pending = true;
static bool maps_unlocked_pending = true;

static thread_mutex_t party_mutex;
static thread_mutex_t websocket_mutex;
static std::vector<std::string> pending_websocket_packets;

static std::map<uint32_t,PartySearchAdvertisement> party_search_advertisements;
static std::map<uint32_t, uint32_t> agent_party_sizes;

static std::map<uint32_t, PartySearchAdvertisement> server_parties;

struct PartySearchDistrict {
    int map_id = 0;
    District district = District::DISTRICT_CURRENT;
    int district_number = 0;
} party_search_map_info;

static void on_map_changed() {
    thread_mutex_lock(&party_mutex);
    agent_party_sizes.clear();
    server_parties.clear();
    party_search_advertisements.clear();
    maps_unlocked_pending = true;
    party_advertisements_pending = true;
    thread_mutex_unlock(&party_mutex);
}

static uint32_t get_original_map_id(uint32_t map_id);

// Position to stand that covers the most compass range for the map, if available.
static bool get_optimal_position_for_listening(float* pos) {
    switch (get_original_map_id(GetMapId())) {
    case MapID_EmbarkBeach:
        pos[0] = -570.0f;
        pos[1] = 550.0f;
        return true;
    case 194: // Kaineng
        pos[0] = 340.0f;
        pos[1] = -2250.0f;
        return true;
    }
    return false;
}

static uint32_t get_original_map_id(uint32_t map_id);

static bool connect_websocket(easywsclient::WebSocket::pointer* websocket_pt, const std::string& url, const std::string& api_key = "");
static bool disconnect_websocket(easywsclient::WebSocket::pointer* websocket_pt);

static std::string get_next_argument(int current_index) {

    int argc = 0;
    char** argv = GetCommandLineArgs(&argc);

    if (current_index <= argc) {
        return argv[current_index + 1];
    }

    return "";
}

static void exit_with_status(const char* state, int exit_code)
{
    LogCritical("%s (code=%d)", state, exit_code);
    exit(exit_code);
}

static std::string convert_uint16_to_string(const uint16_t* buffer, size_t length) {
    std::wstring wstr(buffer, buffer + length);
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> convert;
    return convert.to_bytes(wstr);
}

static bool GetPlayerByAgentId(uint32_t agent_id, ApiPlayer* player_out) {
    auto player_count = GetPlayers(NULL, 0);

    std::vector<ApiPlayer> players(player_count);
    players.resize(GetPlayers(players.data(), player_count));
    for(auto& player : players) {
        if (player.agent_id == agent_id) {
            *player_out = player;
            return true;
        }
    }
    return false;
}

static std::map<uint32_t,PartySearchAdvertisement> collect_party_searches() {
    thread_mutex_lock(&party_mutex);
    // 1. Get a copy of actual party searches
    auto parties = party_search_advertisements;

    // 2. Append with players in range that don't have a party search, but have more than 1 member
    for (auto it : agent_party_sizes) {
        if (it.second < 2)
            continue; // Only send players with at least 2 members
        ApiPlayer player;
        if (!GetPlayerByAgentId(it.first, &player))
            continue; // Failed to get matching player

        uint16_t player_name[21] = { 0 };
        int len = GetPlayerName(player.player_id, player_name, ARRAY_SIZE(player_name));
        assert(len > 0);

        auto player_name_str = convert_uint16_to_string(player_name, len);

        bool found_party_search = false;
        for (auto& party_search : parties) {
            found_party_search = (party_search.second.sender == player_name_str);
            if (found_party_search) break;
        }
        if (found_party_search)
            continue;
        PartySearchAdvertisement party;
        party.party_id = 0xf0000 | it.first;
        party.party_size = it.second;
        party.district_number = (uint16_t)GetDistrictNumber();
        party.language = (uint8_t)GetDistrictLanguage();
        party.level = 20;
        strncpy(party.sender, player_name_str.c_str(), ARRAY_SIZE(party.sender));
        parties[party.party_id] = std::move(party);
    }
    thread_mutex_unlock(&party_mutex);
    return parties;
}

static bool is_websocket_ready(easywsclient::WebSocket::pointer ws) {
    return ws && ws->getReadyState() == easywsclient::WebSocket::OPEN;
}

static void queue_send(const std::string& payload) {
    thread_mutex_lock(&websocket_mutex);
    pending_websocket_packets.push_back(payload);
    thread_mutex_unlock(&websocket_mutex);
}

static bool send_websocket(easywsclient::WebSocket::pointer websocket, const std::string& payload) {
    if (!is_websocket_ready(websocket)) {
        return false;
    }
    LogInfo("Websocket send, %d len:", payload.size());
    fprintf(stderr, "%s\n", payload.c_str());
    last_websocket_message = time_get_ms();
    websocket->send(payload);
    return true;
}

static void send_ping(easywsclient::WebSocket::pointer websocket) {
    if (!is_websocket_ready(websocket))
        return; // No need to connect if its not ready
    //LogInfo("Sending ping");
    last_websocket_message = time_get_ms();
    websocket->sendPing();
}

static void send_player_uuid(const std::string& player_name, ApiFriend* frnd) {

    char uuid_out[128] = { 0 };
    int written = uuid_snprint(uuid_out, ARRAY_SIZE(uuid_out), (const struct uuid*)frnd->uuid);
    assert(written > 1);

    nlohmann::json j;
    j["type"] = "player_name_account_uuid";
    j["map_id"] = get_original_map_id(map_id);
    j["district"] = district;
    j["player_name"] = player_name;
    j["account_uuid"] = uuid_out;

    queue_send(j.dump());
}

// Requested from server when it wants to know the account associated with a player name
static void on_server_requested_player_account_uuid(nlohmann::json& data) {
    if (!data["player_name"].is_string()) {
        LogWarn("on_server_requested_player_account_uuid: requested player_name is missing or not a string\n%s", data.dump().c_str());
        return;
    }
    const auto player_name = data["player_name"].get<std::string>();
    if (!player_name.size()) {
        LogWarn("on_server_requested_player_account_uuid: empty player name\n%s", data.dump().c_str());
        return;
    }
    wchar_t player_name_ws[21] = { 0 };
    if (mbstowcs(player_name_ws, player_name.c_str(), ARRAY_SIZE(player_name_ws)) < 1) {
        LogWarn("on_server_requested_player_account_uuid: failed to convert to wchar_t*\n%s", data.dump().c_str());
        return;
    }
    uint16_t player_name_uint16[ARRAY_SIZE(player_name_ws)] = { 0 };
    for (size_t i = 0; i < ARRAY_SIZE(player_name_uint16) && player_name_ws[i]; i++) {
        assert(player_name_ws[i] < 0xffff);
        player_name_uint16[i] = (uint16_t)player_name_ws[i];
    }

    ApiFriend found;
    if (GetFriend(&found, player_name_uint16)) {
        send_player_uuid(player_name, &found);
        return;
    }

    // TODO: AddFriend();
    LogWarn("on_server_requested_player_account_uuid: failed find friend\n%s", data.dump().c_str());
}

static void on_websocket_message(const std::string& message);

static void check_map_changed() {
    PartySearchDistrict d = { GetMapId(),GetDistrict(),GetDistrictNumber() };
    if (memcmp(&d, &party_search_map_info, sizeof(party_search_map_info)) != 0) {
        on_map_changed();
        party_search_map_info = d;
    }
}



static void send_party_advertisements() {
    nlohmann::json j;
    j["type"] = "client_parties";
    j["map_id"] = get_original_map_id(GetMapId());
    j["district"] = GetDistrict();

    auto searches = collect_party_searches();
    std::vector<PartySearchAdvertisement> parties_json;
    for (auto& it : searches) {
        parties_json.push_back(std::move(it.second));
    }
    j["parties"] = parties_json;
    queue_send(j.dump());
    for (auto& party : parties_json) {
        server_parties[party.party_id] = std::move(party);
    }
}

static void send_changed_party_searches() {
    if (!server_parties.size())
        return send_party_advertisements();

    auto parties = collect_party_searches();

    std::vector<PartySearchAdvertisement> to_send;

    for (auto& it : parties) {
        const auto& existing_party = it.second;
        const auto found = server_parties.find(existing_party.party_id);
        if (found != server_parties.end()) {
            auto res = memcmp(&existing_party, &found->second, sizeof(existing_party));
            if (res == 0) {
                continue; // No change, don't send
            }
        }
        to_send.push_back(existing_party);
    }

    for (auto& it : server_parties) {
        const auto& last_sent_party = it.second;
        if (!last_sent_party.party_size)
            continue; // Already flagged for removal
        const auto found = parties.find(last_sent_party.party_id);
        if (found == parties.end()) {
            // Party no longer exists, flag for removal
            auto cpy = last_sent_party;
            cpy.party_size = 0; // Aka "remove"
            to_send.push_back(cpy);
        }
    }

    if (to_send.empty())
        return; // No change
    nlohmann::json j;
    j["type"] = "updated_parties";
    j["map_id"] = get_original_map_id(GetMapId());
    j["district"] = GetDistrict();
    j["parties"] = to_send;

    queue_send(j.dump());
    for (auto& party : to_send) {
        server_parties[party.party_id] = std::move(party);
    }
}

static void collect_instance_info() {
    map_id = GetMapId();
    district = GetDistrict();
    district_number = GetDistrictNumber();
    GetCharacterName(character_name, ARRAY_SIZE(character_name));
    GetAccountUuid(account_uuid, ARRAY_SIZE(account_uuid));
    LogInfo("collect_instance_info: map %d, district %d, district_number %d, district_region %d", map_id, district, district_number, GetDistrictRegion());
}

// Client callback
static void on_agent_despawned(Event* event, void* params) {
    thread_mutex_lock(&party_mutex);
    assert(event && event->type == EventType_AgentDespawned && event->AgentDespawned.agent_id);
    check_map_changed();
    const auto found = agent_party_sizes.find(event->AgentDespawned.agent_id);
    if (found != agent_party_sizes.end()) {
        agent_party_sizes.erase(found);
        party_advertisements_pending = true;
    }
    thread_mutex_unlock(&party_mutex);
}
static void on_player_party_size(Event* event, void* params) {
    thread_mutex_lock(&party_mutex);
    {
        assert(event && event->type == EventType_PlayerPartySize);
        check_map_changed();
        if (!event->PlayerPartySize.player_id)
            goto leave; // Player id empty?
        ApiAgent agent;
        if (!GetAgentOfPlayer(&agent, event->PlayerPartySize.player_id))
            goto leave;
        check_map_changed();
        if (event->PlayerPartySize.size < 1) {
            const auto found = agent_party_sizes.find(agent.agent_id);
            if (found != agent_party_sizes.end()) {
                agent_party_sizes.erase(found);
                party_advertisements_pending = true;
            }
            goto leave;
        }
        agent_party_sizes[agent.agent_id] = event->PlayerPartySize.size;
        party_advertisements_pending = true;
    }
leave:
    thread_mutex_unlock(&party_mutex);
}
static void on_map_entered(Event* event, void* params) {
    collect_instance_info();
    check_map_changed();
}
static void on_create_party_search_advertisement(Event* event, void* params) {
    thread_mutex_lock(&party_mutex);
    assert(event && event->type == EventType_PartySearchAdvertisement && event->PartySearchAdvertisement.party_id);
    check_map_changed();
    if (!party_search_advertisements.contains(event->PartySearchAdvertisement.party_id)) {
        party_search_advertisements[event->PartySearchAdvertisement.party_id] = PartySearchAdvertisement();
    }
    auto& party = party_search_advertisements[event->PartySearchAdvertisement.party_id];

    party.party_id = event->PartySearchAdvertisement.party_id;
    party.party_size = event->PartySearchAdvertisement.party_size;
    party.hero_count = event->PartySearchAdvertisement.hero_count;
    party.search_type = event->PartySearchAdvertisement.search_type;
    party.hardmode = event->PartySearchAdvertisement.hardmode;
    party.district_number = event->PartySearchAdvertisement.district_number;
    party.language = event->PartySearchAdvertisement.language;
    party.primary = event->PartySearchAdvertisement.primary;
    party.secondary = event->PartySearchAdvertisement.secondary;
    party.level = event->PartySearchAdvertisement.level;

    *party.message = 0;
    if (event->PartySearchAdvertisement.message.length) {
        const auto message = convert_uint16_to_string(event->PartySearchAdvertisement.message.buffer, event->PartySearchAdvertisement.message.length);
        strncpy(party.message, message.c_str(), ARRAY_SIZE(party.message));
    }

    *party.sender = 0;
    if (event->PartySearchAdvertisement.sender.length) {
        const auto sender = convert_uint16_to_string(event->PartySearchAdvertisement.sender.buffer, event->PartySearchAdvertisement.sender.length);
        strncpy(party.sender, sender.c_str(), ARRAY_SIZE(party.sender));
    }

    party_advertisements_pending = true;
    thread_mutex_unlock(&party_mutex);
    
}
static void on_update_party_search_advertisement(Event* event, void* params) {
    thread_mutex_lock(&party_mutex);
    assert(event && event->PartySearchAdvertisement.party_id);
    const auto found = party_search_advertisements.find(event->PartySearchAdvertisement.party_id);
    if (found == party_search_advertisements.end())
        goto leave;
    switch (event->type) {
    case EventType_PartySearchSize:
        found->second.party_size = event->PartySearchAdvertisement.party_size;
        found->second.hero_count = event->PartySearchAdvertisement.hero_count;
        break;
    case EventType_PartySearchType:
        found->second.search_type = event->PartySearchAdvertisement.search_type;
        found->second.hardmode = event->PartySearchAdvertisement.hardmode;
        break;
    case EventType_PartySearchRemoved:
        party_search_advertisements.erase(found);
        break;
    }
    party_advertisements_pending = true;
leave:
    thread_mutex_unlock(&party_mutex);
}

static void wait_until_ingame() {
    //LogInfo("Waiting for game load");
    const int max_tries = 10;
    for (auto i = 0; i < 10000; i+=50) {
        if (GetIsIngame()) {
            return;
        }
        time_sleep_ms(50);
    }
    
    exit_with_status("Failed to get in-game", FAILED_TO_LOAD_GAME);
}

static void load_configuration() {
    int i = 0;
    std::string arg;

    int argc = 0;
    char** argv = GetCommandLineArgs(&argc);
    try {
        for (i = 0; i < argc; i++) {
            arg = argv[i];
            if (arg == "-websocket-url") {
                bot_configuration.web_socket_url = get_next_argument(i);
                i++;
            }
            else if (arg == "-connection-retries") {
                bot_configuration.connection_retries = stoi(get_next_argument(i));
                i++;
            }
            else if (arg == "-api-key") {
                bot_configuration.api_key = get_next_argument(i);
                i++;
            }
        }
    }
    catch (std::exception) {
        printf("Failed to process arg:%s\n", arg.c_str());
        exit_with_status("Failed to load configuration", FAILED_TO_LOAD_CONFIG);
    }

    if (bot_configuration.web_socket_url == "") {
        exit_with_status("No websocket url specified. Use -websocket-url", FAILED_TO_LOAD_CONFIG);
    }

    if (bot_configuration.map_id == -1) {
        exit_with_status("No map specified. Use -mapid", FAILED_TO_LOAD_CONFIG);
    }

    wanted_map_id = bot_configuration.map_id;
    wanted_district = bot_configuration.district;
}
// If the given map id isn't the normal one (i.e. festival, return the original. Useful for comparisons)
static uint32_t get_original_map_id(uint32_t map_id) {

    using namespace GW::Constants;
    switch((GW::Constants::MapID)map_id) {
    case MapID::Kamadan_Jewel_of_Istan_Halloween_outpost:
    case MapID::Kamadan_Jewel_of_Istan_Wintersday_outpost:
    case MapID::Kamadan_Jewel_of_Istan_Canthan_New_Year_outpost:
        return (uint32_t)MapID::Kamadan_Jewel_of_Istan_outpost;

    case MapID::Lions_Arch_Halloween_outpost:
    case MapID::Lions_Arch_Wintersday_outpost:
    case MapID::Lions_Arch_Canthan_New_Year_outpost:
        return (uint32_t)MapID::Lions_Arch_outpost;

    case MapID::Ascalon_City_Wintersday_outpost:
        return (uint32_t)MapID::Ascalon_City_outpost;

    case MapID::Droknars_Forge_Halloween_outpost:
    case MapID::Droknars_Forge_Wintersday_outpost:
        return (uint32_t)MapID::Droknars_Forge_outpost;

    case MapID::Tomb_of_the_Primeval_Kings_Halloween_outpost:
        return (uint32_t)MapID::Tomb_of_the_Primeval_Kings;

    case MapID::Shing_Jea_Monastery_Dragon_Festival_outpost:
    case MapID::Shing_Jea_Monastery_Canthan_New_Year_outpost:
        return (uint32_t)MapID::Shing_Jea_Monastery_outpost;

    case MapID::Kaineng_Center_Canthan_New_Year_outpost:
        return (uint32_t)MapID::Kaineng_Center_outpost;

    case MapID::Eye_of_the_North_outpost_Wintersday_outpost:
        return (uint32_t)MapID::Eye_of_the_North_outpost;
    }
    return map_id;
}

static bool in_correct_outpost() {
    if (!GetIsIngame())
        return false;

    uint32_t check_map_id = get_original_map_id(GetMapId());

    return ((!wanted_map_id || check_map_id == wanted_map_id) &&
        (wanted_district == District::DISTRICT_CURRENT || GetDistrict() == wanted_district) &&
        (!bot_configuration.district_number || GetDistrictNumber() == bot_configuration.district_number));
}

static void ensure_correct_outpost() {
    if (!wanted_map_id)
        exit_with_status("No wanted_map_id in ensure_correct_outpost", 1);
    if (in_correct_outpost())
        return;
    if (!IsMapUnlocked(wanted_map_id)) {
        LogError("map %d not unlocked", wanted_map_id);
        exit_with_status("Map not unlocked",1);
    }
    LogInfo("Zoning into outpost");
    int res = 0;
    size_t retries = 4;
    for (size_t i = 0; i < retries && !in_correct_outpost(); i++) {
        LogInfo("Travel attempt %d of %d", i + 1, retries);
        res = travel_wait(wanted_map_id, wanted_district, bot_configuration.district_number);
        if (res == 38) {
            retries = 50; // District full, more retries
        }
        if (res != ASYNC_RESULT_OK) {
            LogInfo("Travel error %d (%d)", get_message_from_gw_error_id(res), res);
        }
    }
    collect_instance_info();
    if (!in_correct_outpost()) {
        exit_with_status("Couldn't travel to outpost", 1);
    }
    float pos[2];
    if (get_optimal_position_for_listening(pos)) {
        MoveToCoord(pos[0], pos[1]);
    }
    LogInfo("I should be in outpost %d %d %d", GetMapId(), GetDistrict(), GetDistrictNumber());
}

static void send_maps_unlocked() {
    std::vector<uint32_t> maps_unlocked;
    maps_unlocked.resize(32,0);
    const auto len = GetMapsUnlocked(maps_unlocked.data(), maps_unlocked.capacity());
    maps_unlocked.resize(len);

    nlohmann::json j;
    j["type"] = "client_unlocked_maps";
    j["unlocked_maps"] = maps_unlocked;

    queue_send(j.dump());
}

// Server has send a websocket message asking this bot to travel
static void on_server_requested_travel(nlohmann::json& data) {
    if (!data["map_id"].is_number()) {
        LogWarn("on_server_requesting_travel: requested map_id is missing or not a number\n%s", data.dump().c_str());
        return;
    }
    const auto requested_map_id = data["map_id"].get<uint32_t>();
    if (!is_valid_outpost(requested_map_id)) {
        LogWarn("on_server_requesting_travel: requested map_id %d is not a valid outpost", requested_map_id);
        return;
    }
    if (!IsMapUnlocked(requested_map_id)) {
        LogWarn("on_server_requesting_travel: requested map_id %d is not unlocked", requested_map_id);
        return;
    }
    if (!data["district"].is_number()) {
        LogWarn("on_server_requesting_travel: requested district is missing or not a number\n%s", data.dump().c_str());
        return;
    }
    const auto requested_district = data["district"].get<uint32_t>();
    if (requested_district > District::DISTRICT_ASIA_JAPANESE) {
        LogWarn("on_server_requesting_travel: requested map_id %d is not a valid district", requested_district);
        return;
    }

    wanted_map_id = requested_map_id;
    wanted_district = (District)requested_district;
}

static void on_websocket_message(const std::string& message) {
    LogInfo("Websocket recv, %d len",message.size());
    fprintf(stderr, "%s\n", message.c_str());
    last_websocket_message = time_get_ms();
    nlohmann::json j = nlohmann::json::parse(message);
    if (j == nlohmann::json::value_t::discarded)
        return;
    if (j["type"].is_string()) {
        const auto type = j["type"].get<std::string>();
        if (type == "server_requested_travel") {
            on_server_requested_travel(j);
        }
        else if (type == "server_requested_player_account_uuid") {
            on_server_requested_player_account_uuid(j);
        }
    }
    // Unhandled request
}

static bool disconnect_websocket(easywsclient::WebSocket::pointer* websocket_pt) {
    if (!websocket_pt)
        return false;
    auto websocket = *websocket_pt;
    if (!websocket)
        return true;

    websocket->close();
    // Wait for websocket to close
    for (auto j = 0; j < 5000; j+=50) {
        websocket->poll();
        if (websocket->getReadyState() == easywsclient::WebSocket::CLOSED)
            break;
        time_sleep_ms(50);
    }
    *websocket_pt = NULL;
    return true;
}
static bool connect_websocket(easywsclient::WebSocket::pointer* websocket_pt, const std::string& url, const std::string& api_key) {
    assert(websocket_pt && url.size() && *account_uuid && map_id);
    if (is_websocket_ready(*websocket_pt))
        return true;
    char user_agent[255];
    char uuid_less_hyphens[ARRAY_SIZE(account_uuid)] = { 0 };
    size_t added = 0;
    for (size_t i = 0; i < ARRAY_SIZE(account_uuid); i++) {
        if (account_uuid[i] != '-')
            uuid_less_hyphens[added++] = account_uuid[i];
    }
    assert(snprintf(user_agent, ARRAY_SIZE(user_agent), "%s-%d-%d", uuid_less_hyphens, 0, 0) > 0);

    std::map<std::string, std::string> extra_headers = {
        {"User-Agent",user_agent },
        {"X-Api-Key",api_key },
        {"X-Account-Uuid",account_uuid},
        {"X-Bot-Version","1"}
    };

    const auto connect_retries = 10;

    for (auto i = 0; i < connect_retries; i++) {
        if(i > 0)
            LogInfo("Attempting to connect to %s. Try %d/%d", url.c_str(), i + 1, connect_retries);
        auto websocket = easywsclient::WebSocket::from_url(url, extra_headers);
        // Wait for websocket to open
        for (auto j = 0; websocket && j < 5000; j+=50) {
            websocket->poll();
            if (is_websocket_ready(websocket)) {
                *websocket_pt = websocket;
                on_map_changed(); // Triggers fresh send of data
                return true;
            }
            time_sleep_ms(50);
        }
        disconnect_websocket(&websocket);
        // Sleep before retry
        time_sleep_sec(5);
    }
    return false;
}

static int main_bot(void* param)
{
    thread_mutex_init(&party_mutex);
    thread_mutex_init(&websocket_mutex);

    CallbackEntry_Init(&EventType_WorldMapEnter_entry, on_map_entered, NULL);
    RegisterEvent(EventType_WorldMapEnter, &EventType_WorldMapEnter_entry);

    CallbackEntry_Init(&EventType_PartySearchAdvertisement_entry, on_create_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchAdvertisement, &EventType_PartySearchAdvertisement_entry);

    CallbackEntry_Init(&EventType_PartySearchRemoved_entry, on_update_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchRemoved, &EventType_PartySearchRemoved_entry);

    CallbackEntry_Init(&EventType_PartySearchSize_entry, on_update_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchSize, &EventType_PartySearchSize_entry);

    CallbackEntry_Init(&EventType_PartySearchType_entry, on_update_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchType, &EventType_PartySearchType_entry);

    CallbackEntry_Init(&EventType_PlayerPartySize_entry, on_player_party_size, NULL);
    RegisterEvent(EventType_PlayerPartySize, &EventType_PlayerPartySize_entry);

    CallbackEntry_Init(&EventType_AgentDespawned_entry, on_agent_despawned, NULL);
    RegisterEvent(EventType_AgentDespawned, &EventType_AgentDespawned_entry);


    load_configuration();

    wait_until_ingame();

    easywsclient::WebSocket::pointer sending_websocket = NULL;

    msec_t last_calculated_map_check = 0;

    // Theres a HQ bug where it knows our map, but not district until you travel.
    // Fix this by deliberately travelling somewhere else.
    /*uint32_t tmp_map_id = 0;
    uint32_t current_map_id = GetMapId();
    for (size_t i = 1; i < 877; i++) {
        if (current_map_id == i)
            continue;
        if (!is_valid_outpost(i))
            continue;
        if (!IsMapUnlocked(i))
            continue;
        tmp_map_id = i;
        break;
    }
    if (!tmp_map_id) {
        exit_with_status("Failed to find another map ID to travel to; make sure you have at least 2 outposts unlocked", 1);
    }
    assert(travel_wait(tmp_map_id, District::DISTRICT_CURRENT, 0) == 0);*/

    while (running) {
        wait_until_ingame();
        if (!*account_uuid)
            collect_instance_info();
        
        if (!wanted_map_id)
            wanted_map_id = map_id;
        if (!is_websocket_ready(sending_websocket)) {
            // Websocket not currently active, reset vars ready to send bits on sucessful connect later
            party_advertisements_pending = true;
            last_websocket_message = time_get_ms() - websocket_ping_interval;
            maps_unlocked_pending = true;
        }
        if (!connect_websocket(&sending_websocket, bot_configuration.web_socket_url, bot_configuration.api_key)) {
            
            // Connection to server failed, continue the loop until we connect
            continue;
        }

        ensure_correct_outpost();
        if (party_advertisements_pending) {
            party_advertisements_pending = false;
            send_changed_party_searches();
        }
        if (maps_unlocked_pending) {
            maps_unlocked_pending = false;
            send_maps_unlocked();
        }
        thread_mutex_lock(&websocket_mutex);
        while (pending_websocket_packets.size()) {
            const auto& payload = pending_websocket_packets.begin();
            if (!send_websocket(sending_websocket, *payload))
                break;
            pending_websocket_packets.erase(payload);
        }
        thread_mutex_unlock(&websocket_mutex);
        if (sending_websocket) {
            if (time_get_ms() - last_websocket_message > websocket_ping_interval) {
                send_ping(sending_websocket);
            }
            sending_websocket->dispatch(on_websocket_message);
            sending_websocket->poll();
        }

        time_sleep_ms(50);
    }
    disconnect_websocket(&sending_websocket);

    UnRegisterEvent(&EventType_PartySearchAdvertisement_entry);
    UnRegisterEvent(&EventType_PartySearchRemoved_entry);
    UnRegisterEvent(&EventType_PartySearchSize_entry);
    UnRegisterEvent(&EventType_PartySearchType_entry);
    UnRegisterEvent(&EventType_WorldMapEnter_entry);
    UnRegisterEvent(&EventType_PlayerPartySize_entry);
    UnRegisterEvent(&EventType_AgentDespawned_entry);

    thread_mutex_destroy(&party_mutex);
    thread_mutex_destroy(&websocket_mutex);

    raise(SIGTERM);
    return 0;
}
extern "C" DllExport void PluginUnload(PluginObject * obj)
{
    running = false;
}
extern "C" DllExport bool PluginEntry(PluginObject * obj)
{
    assert(obj);
    running = true;
    thread_create(&bot_thread, main_bot, NULL);
    thread_detach(&bot_thread);
    obj->PluginUnload = &PluginUnload;
    plugin_hook = obj;
    return true;
}

extern "C" DllExport void on_panic(const char* msg)
{
    exit_with_status("Error", 1);
}
int main(void) {
    printf("Hello world");
    return 0;
}
