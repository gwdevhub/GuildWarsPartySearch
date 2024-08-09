#ifndef __STDC__
#define __STDC__ 1
#endif

#ifndef _Static_assert
#define _Static_assert static_assert
#endif

#define _CRT_SECURE_NO_WARNINGS
#include <math.h>
#include <locale.h>
#include <stdbool.h>

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
#include <signal.h>
#include <stdio.h>
#include <vector>
#include <atomic>
#include <time.h>
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

struct BotConfiguration {
    std::string         web_socket_url = "ws://217.160.162.89/party-search/update";
    uint32_t            map_id = 857; // Embark beach
    District            district = District::DISTRICT_AMERICAN;
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

District district = District::DISTRICT_CURRENT;
int district_number = -1;
uint32_t map_id = 0;
char character_name[42] = { 0 };

static PluginObject* plugin_hook;
static BotConfiguration bot_configuration;

static CallbackEntry EventType_PartySearchAdvertisement_entry;
static CallbackEntry EventType_PartySearchRemoved_entry;
static CallbackEntry EventType_PartySearchSize_entry;
static CallbackEntry EventType_PartySearchType_entry;
static CallbackEntry EventType_WorldMapEnter_entry;
static CallbackEntry EventType_WorldMapLeave_entry;

static easywsclient::WebSocket::pointer ws;

static std::vector<PartySearchAdvertisement*> party_search_advertisements;

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
    PartySearchAdvertisement* party;
    for (auto party : party_search_advertisements) {
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

static std::string get_json_payload() {
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
    return j.dump();
}

static void on_map_left(Event* event, void* params) {
    clear_party_search_advertisements();
}

static void on_map_entered(Event* event, void* params) {
    map_id = GetMapId();
    district = GetDistrict();
    district_number = GetDistrictNumber();
    *character_name = 0;
    GetCharacterName(character_name, sizeof(character_name)  / sizeof(*character_name));
    ready = true;
}

static void add_party_search_advertisement(Event* event, void* params) {
    assert(event && event->type == EventType_PartySearchAdvertisement && event->PartySearchAdvertisement.party_id);

    create_party_search_advertisement(event);
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
    try {
        for (int i = 0; i < GetArgc(); i++) {
            const std::string arg = GetArgv()[i];
            if (arg == "-websocket-url") {
                bot_configuration.web_socket_url = get_next_argument(i);
                i++;
            }
            else if (arg == "-mapid") {
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
        }
    }
    catch (std::exception) {
        exit_with_status("Failed to load configuration", FAILED_TO_LOAD_CONFIG);
    }

    if (bot_configuration.web_socket_url == "") {
        exit_with_status("No websocket url specified. Use -websocket-url", FAILED_TO_LOAD_CONFIG);
    }

    if (bot_configuration.map_id == -1) {
        exit_with_status("No map specified. Use -mapid", FAILED_TO_LOAD_CONFIG);
    }
}

static bool in_correct_outpost() {
    if (!GetIsIngame())
        return false;
    if (!bot_configuration.map_id)
        return true;
    if (!map_id)
        return false;

    return ((!bot_configuration.map_id || map_id == bot_configuration.map_id) &&
        (bot_configuration.district == District::DISTRICT_CURRENT || district == bot_configuration.district) &&
        (!bot_configuration.district_number || district_number == bot_configuration.district_number));
}

static void ensure_correct_outpost() {
    if (in_correct_outpost())
        return;
    LogInfo("Zoning into outpost");
    int res = 0;
    size_t retries = 4;
    assert(bot_configuration.map_id);
    for (size_t i = 0; i < retries && !in_correct_outpost(); i++) {
        LogInfo("Travel attempt %d of %d", i + 1, retries);
        res = travel_wait(bot_configuration.map_id, bot_configuration.district, bot_configuration.district_number);
        if (res == 38) {
            retries = 50; // District full, more retries
        }
    }
    if (!in_correct_outpost()) {
        exit_with_status("Couldn't travel to outpost", 1);
    }
    LogInfo("I should be in outpost %d %d %d", GetMapId(), GetDistrict(), GetDistrictNumber());
}

static void send_info() {
    const auto payload = get_json_payload();
    if (payload == last_payload) {
        return;
    }

    LogInfo(payload.c_str());
    ws->send(payload);
    ws->poll();
    if (ws->getReadyState() != easywsclient::WebSocket::OPEN) {
        LogInfo("Disconnected while attempting to send payload");
        return;
    }

    // Pretty lazy way to detect changes
    last_payload = payload;
    time_sleep_sec(5);
}

static void disconnect_websocket() {
    if (!ws) return;
    ws->close();
    // Wait for websocket to close
    for (auto j = 0; j < 5000; j+=50) {
        ws->poll();
        if (ws->getReadyState() == easywsclient::WebSocket::CLOSED)
            break;
        time_sleep_ms(50);
    }
    ws = NULL;
}
static void connect_websocket() {
    if (ws && ws->getReadyState() == easywsclient::WebSocket::OPEN)
        return;

    assert(*character_name && map_id);
    char user_agent[255];
    assert(snprintf(user_agent, sizeof(user_agent) / sizeof(*user_agent), "%s-%d-%d", character_name, map_id, static_cast<uint32_t>(district)) > 0);

    const auto connect_retries = 10;

    for (auto i = 0; i < connect_retries; i++) {
        disconnect_websocket();

        LogInfo("Attempting to connect. Try %d/%d", i + 1, connect_retries);
        ws = easywsclient::WebSocket::from_url(bot_configuration.web_socket_url, user_agent);
        if (!ws)
            continue;
        // Wait for websocket to open
        for (auto j = 0; j < 5000; j+=50) {
            ws->poll();
            if (ws->getReadyState() == easywsclient::WebSocket::OPEN)
                break;
            time_sleep_ms(50);
        }
        break;
    }
    if (ws && ws->getReadyState() == easywsclient::WebSocket::OPEN)
        return;
    
    exit_with_status("Timed out while attempting to connect", FAILED_TO_CONNECT);
}

static int main_bot(void* param)
{
    CallbackEntry_Init(&EventType_WorldMapLeave_entry, on_map_left, NULL);
    RegisterEvent(EventType_WorldMapLeave, &EventType_WorldMapLeave_entry);

    on_map_entered(NULL, NULL);
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
    
    wait_until_ingame();

    load_configuration();

    while (running) {
        wait_until_ingame();
        ensure_correct_outpost();
        connect_websocket();
        send_info();
        time_sleep_sec(1);
    }

cleanup:
    UnRegisterEvent(&EventType_PartySearchAdvertisement_entry);
    UnRegisterEvent(&EventType_PartySearchRemoved_entry);
    UnRegisterEvent(&EventType_PartySearchSize_entry);
    UnRegisterEvent(&EventType_PartySearchType_entry);
    UnRegisterEvent(&EventType_WorldMapEnter_entry);
    UnRegisterEvent(&EventType_WorldMapLeave_entry);

    clear_party_search_advertisements();
    ws->close();
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
