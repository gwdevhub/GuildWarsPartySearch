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
    #include <common/dlfunc.h>
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

typedef struct {
    uint16_t            party_id;
    uint8_t             party_size;
    uint8_t             hero_count;
    uint8_t             search_type; // 0=hunting, 1=mission, 2=quest, 3=trade, 4=guild
    uint8_t             hardmode;
    uint16_t            district_number;
    uint8_t             language;
    uint8_t             primary;
    uint8_t             secondary;
    uint8_t             level;
    std::string         message;
    std::string         sender;
} PartySearchAdvertisement;

void to_json(nlohmann::json& j, const PartySearchAdvertisement& p) {
    j = nlohmann::json{
        {"party_id", p.party_id},
        {"party_size", p.party_size},
        {"hero_count", p.hero_count},
        {"search_type", p.search_type},
        {"hardmode", p.hardmode},
        {"district_number", p.district_number},
        {"language", p.language},
        {"primary", p.primary},
        {"secondary", p.secondary},
        {"level", p.level},
        {"message", p.message},
        {"sender", p.sender}
    };
}

static struct thread  bot_thread;
static std::atomic<bool> running;
static std::atomic<bool> ready;
static std::string last_payload;
static uint64_t last_websocket_message = 0;
static uint64_t websocket_ping_interval = 30000; // Ping every 30s

District district = District::DISTRICT_CURRENT;
int district_number = -1;
uint32_t map_id = 0;
char character_name[42] = { 0 };
char account_uuid[128] = { 0 };

uint32_t wanted_map_id = 0;
District wanted_district = District::DISTRICT_CURRENT;

static PluginObject* plugin_hook;
static BotConfiguration bot_configuration;

static CallbackEntry EventType_PartySearchAdvertisement_entry;
static CallbackEntry EventType_PartySearchRemoved_entry;
static CallbackEntry EventType_PartySearchSize_entry;
static CallbackEntry EventType_PartySearchType_entry;
static CallbackEntry EventType_WorldMapEnter_entry;
static CallbackEntry EventType_WorldMapLeave_entry;

static bool party_advertisements_pending = true;

static std::vector<PartySearchAdvertisement*> party_search_advertisements;

static bool connect_websocket(easywsclient::WebSocket::pointer* websocket_pt, const std::string& url, const std::string& api_key = "");
static bool disconnect_websocket(easywsclient::WebSocket::pointer* websocket_pt);

static std::string get_next_argument(int current_index) {
    if (current_index <= GetArgc()) {
        return GetArgv()[current_index + 1];
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

static void clear_party_search_advertisements() {
    for (auto party : party_search_advertisements) {
        delete party;
    }

    party_search_advertisements.clear();
}

static PartySearchAdvertisement* get_party_search_advertisement(uint32_t party_search_id) {
    for (const auto party : party_search_advertisements) {
        if (party->party_id == party_search_id) {
            return party;
        }
    }

    return nullptr;
}

static bool remove_party_search_advertisement(uint32_t party_search_id) {
    auto party = get_party_search_advertisement(party_search_id);
    if (party) {
        auto it = std::find(party_search_advertisements.begin(), party_search_advertisements.end(), party);
        if (it != party_search_advertisements.end()) {
            party_search_advertisements.erase(it);
            delete party;
            return true;
        }
    }

    return false;
}

static PartySearchAdvertisement* create_party_search_advertisement(Event* event) {
    assert(event && event->PartySearchAdvertisement.party_id);

    PartySearchAdvertisement* party = get_party_search_advertisement(event->PartySearchAdvertisement.party_id);
    if (!party) {
        party = new PartySearchAdvertisement();
        party_search_advertisements.push_back(party);
    }

    party->party_id = event->PartySearchAdvertisement.party_id;
    party->party_size = event->PartySearchAdvertisement.party_size;
    party->hero_count = event->PartySearchAdvertisement.hero_count;
    party->search_type = event->PartySearchAdvertisement.search_type;
    party->hardmode = event->PartySearchAdvertisement.hardmode;
    party->district_number = event->PartySearchAdvertisement.district_number;
    party->language = event->PartySearchAdvertisement.language;
    party->primary = event->PartySearchAdvertisement.primary;
    party->secondary = event->PartySearchAdvertisement.secondary;
    party->level = event->PartySearchAdvertisement.level;

    if (event->PartySearchAdvertisement.message.length) {
        party->message = convert_uint16_to_string(event->PartySearchAdvertisement.message.buffer, event->PartySearchAdvertisement.message.length);
    }
    else {
        party->message.clear();
    }

    if (event->PartySearchAdvertisement.sender.length) {
        party->sender = convert_uint16_to_string(event->PartySearchAdvertisement.sender.buffer, event->PartySearchAdvertisement.sender.length);
    }
    else {
        party->sender.clear();
    }

    return party;
}

static bool is_websocket_ready(easywsclient::WebSocket::pointer ws) {
    return ws && ws->getReadyState() == easywsclient::WebSocket::OPEN;
}

static bool send_websocket(easywsclient::WebSocket::pointer websocket, const std::string& payload) {
    if (!is_websocket_ready(websocket)) {
        return false;
    }
    LogInfo("Websocket send:");
    printf("%s\n", payload.c_str());
    last_websocket_message = time_get_ms();
    websocket->send(payload);
    return true;
}

static void send_ping(easywsclient::WebSocket::pointer websocket) {
    if (!is_websocket_ready(websocket))
        return; // No need to connect if its not ready
    LogInfo("Sending ping");
    last_websocket_message = time_get_ms();
    websocket->sendPing();
}

static int send_party_advertisements(easywsclient::WebSocket::pointer websocket) {
    assert(is_websocket_ready(websocket));
    std::vector<PartySearchAdvertisement> ads;
    for (const auto& ad : party_search_advertisements) {
        if (!ad) {
            continue;
        }

        ads.push_back(*ad);
    }
    assert(map_id != 0);
    nlohmann::json j;
    j["map_id"] = map_id;
    j["district"] = district;
    j["parties"] = ads;

    const auto payload = j.dump();
    return send_websocket(websocket, payload);
}

static void collect_instance_info() {
    map_id = GetMapId();
    district = GetDistrict();
    district_number = GetDistrictNumber();
    *character_name = 0;
    assert(GetCharacterName(character_name, ARRAY_SIZE(character_name)) > 0);
    *account_uuid = 0;
    assert(GetAccountUuid(account_uuid, ARRAY_SIZE(account_uuid)) > 0);
}

static uint32_t calculate_map_to_visit(uint32_t map_id_requested, District* district_out) {
    // @TODO: Figure out if a district for a map is available; this should already be given because we can see it on the world map
    if (map_id_requested && IsMapUnlocked(map_id_requested)) {
        // The map given is accessible
        if (district_out && !*district_out)
            *district_out = District::DISTRICT_CURRENT;
        return map_id_requested;
    }
    uint32_t fallback_map_ids[] = {
        857, // Embark
        148, // Gtob
    };
    for (auto fallback_id : fallback_map_ids) {
        if (IsMapUnlocked(fallback_id))
            return fallback_id;
    }
    return 0;
}


static void on_map_left(Event* event, void* params) {
    clear_party_search_advertisements();
}

static void on_map_entered(Event* event, void* params) {
    collect_instance_info();
    party_advertisements_pending = true;
    ready = true;
}

static void add_party_search_advertisement(Event* event, void* params) {
    assert(event && event->type == EventType_PartySearchAdvertisement && event->PartySearchAdvertisement.party_id);

    create_party_search_advertisement(event);
    party_advertisements_pending = true;
}

static void update_party_search_advertisement(Event* event, void* params) {
    assert(event && event->PartySearchAdvertisement.party_id);

    PartySearchAdvertisement* party = get_party_search_advertisement(event->PartySearchAdvertisement.party_id);
    if (party) {
        switch (event->type) {
        case EventType_PartySearchSize:
            party->party_size = event->PartySearchAdvertisement.party_size;
            party->hero_count = event->PartySearchAdvertisement.hero_count;
            break;
        case EventType_PartySearchType:
            party->search_type = event->PartySearchAdvertisement.search_type;
            party->hardmode = event->PartySearchAdvertisement.hardmode;
            break;
        case EventType_PartySearchRemoved:
            remove_party_search_advertisement(party->party_id);
            break;
        }
    }
    party_advertisements_pending = true;
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
    try {
        for (i = 0; i < GetArgc(); i++) {
            arg = GetArgv()[i];
            if (arg == "-websocket-url") {
                bot_configuration.web_socket_url = get_next_argument(i);
                i++;
            }
            else if (arg == "-travel-mapid") {
                bot_configuration.map_id = stoi(get_next_argument(i));
                i++;
            }
            else if (arg == "-district") {
                bot_configuration.district = static_cast<District>(stoi(get_next_argument(i)));
                i++;
            }
            else if (arg == "-district-number") {
                bot_configuration.district_number = stoi(get_next_argument(i));
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
    switch(map_id) {
    case MapID_Kamadan_Halloween:
    case MapID_Kamadan_Wintersday:
    case MapID_Kamadan_Canthan_New_Year:
        return MapID_Kamadan;
        // TODO: Other outposts that have festival versions
    }
    return map_id;
}

static bool in_correct_outpost() {
    if (!GetIsIngame())
        return false;
    if (!map_id)
        return false;

    uint32_t check_map_id = get_original_map_id(map_id);

    return ((!wanted_map_id || check_map_id == wanted_map_id) &&
        (wanted_district == District::DISTRICT_CURRENT || district == wanted_district) &&
        (!bot_configuration.district_number || district_number == bot_configuration.district_number));
}

static void ensure_correct_outpost() {
    wanted_map_id = calculate_map_to_visit(bot_configuration.map_id, &wanted_district);
    if (!wanted_map_id)
        exit_with_status("calculate_map_to_visit failed, ensure this character has gtob unlocked", 1);
    if (in_correct_outpost())
        return;
    LogInfo("Zoning into outpost");
    int res = 0;
    size_t retries = 4;
    for (size_t i = 0; i < retries && !in_correct_outpost(); i++) {
        LogInfo("Travel attempt %d of %d", i + 1, retries);
        res = travel_wait(wanted_map_id, wanted_district, bot_configuration.district_number);
        if (res == 38) {
            retries = 50; // District full, more retries
        }
    }
    if (!in_correct_outpost()) {
        exit_with_status("Couldn't travel to outpost", 1);
    }
    LogInfo("I should be in outpost %d %d %d", GetMapId(), GetDistrict(), GetDistrictNumber());
}

static void on_websocket_message(const std::string& message) {
    LogInfo("Websocket recv:");
    printf("%s\n", message.c_str());
    last_websocket_message = time_get_ms();
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
    assert(snprintf(user_agent, ARRAY_SIZE(user_agent), "%s-%d-%d", uuid_less_hyphens, map_id, static_cast<uint32_t>(district)) > 0);

    std::map<std::string, std::string> extra_headers = {
        {"User-Agent",user_agent },
        {"X-Api-Key",api_key }
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
    CallbackEntry_Init(&EventType_WorldMapLeave_entry, on_map_left, NULL);
    RegisterEvent(EventType_WorldMapLeave, &EventType_WorldMapLeave_entry);

    CallbackEntry_Init(&EventType_WorldMapEnter_entry, on_map_entered, NULL);
    RegisterEvent(EventType_WorldMapEnter, &EventType_WorldMapEnter_entry);

    CallbackEntry_Init(&EventType_PartySearchAdvertisement_entry, add_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchAdvertisement, &EventType_PartySearchAdvertisement_entry);

    CallbackEntry_Init(&EventType_PartySearchRemoved_entry, update_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchRemoved, &EventType_PartySearchRemoved_entry);

    CallbackEntry_Init(&EventType_PartySearchSize_entry, update_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchSize, &EventType_PartySearchSize_entry);

    CallbackEntry_Init(&EventType_PartySearchType_entry, update_party_search_advertisement, NULL);
    RegisterEvent(EventType_PartySearchType, &EventType_PartySearchType_entry);
 
    load_configuration();

    wait_until_ingame();

    easywsclient::WebSocket::pointer sending_websocket = NULL;

    while (running) {
        wait_until_ingame();
        ensure_correct_outpost();
        if (!is_websocket_ready(sending_websocket)) {
            // Websocket not currently active, reset vars ready to send bits on sucessful connect later
            party_advertisements_pending = true;
            last_websocket_message = time_get_ms() - websocket_ping_interval;
        }
        if (!connect_websocket(&sending_websocket, bot_configuration.web_socket_url, bot_configuration.api_key)) {
            // Connection to server failed, continue the loop until we connect
            continue;
        }
        if (party_advertisements_pending) {
            send_party_advertisements(sending_websocket);
            party_advertisements_pending = false;
        }
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
    UnRegisterEvent(&EventType_WorldMapLeave_entry);

    clear_party_search_advertisements();

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
