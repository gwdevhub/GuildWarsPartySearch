#ifndef __STDC__
#define __STDC__ 1
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

#define FAILED_TO_START             1
#define FAILED_TO_LOAD_GAME         2
#define FAILED_TO_LOAD_CHAR_NAME    3
#define FAILED_TO_LOAD_CONFIG       4
#define FAILED_TO_CONNECT           5

typedef struct {
    std::string         web_socket_url;
    uint32_t            map_id;
    District            district;
    uint32_t            district_number;
    int32_t             connection_retries;
} BotConfiguration;

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

static PluginObject* plugin_hook;
static BotConfiguration bot_configuration;

static CallbackEntry EventType_PartySearchAdvertisement_entry;
static CallbackEntry EventType_PartySearchRemoved_entry;
static CallbackEntry EventType_PartySearchSize_entry;
static CallbackEntry EventType_PartySearchType_entry;
static CallbackEntry EventType_WorldMapEnter_entry;
static CallbackEntry EventType_WorldMapLeave_entry;

static std::string character_name;
static int map_id;
static District district;
static int district_number;

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

    nlohmann::json j;
    j["map_id"] = bot_configuration.map_id;
    j["district"] = bot_configuration.district;
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
    LogInfo("Waiting for game load");
    const int max_tries = 10;
    for (auto i = 0; i < max_tries; i++) {
        if (GetIsIngame()) {
            return;
        }

        time_sleep_sec(1);
    }
    
    exit_with_status("Failed to get in-game", FAILED_TO_LOAD_GAME);
}

static void load_character_name() {
    const auto max_tries = 10;
    character_name.resize(256, 0);
    for (auto i = 0; i < max_tries; i++)
    {
        GetCharacterName(character_name.data(), 256);
        if (character_name != "") {
            return;
        }

        time_sleep_sec(1);
    }

    exit_with_status("Failed to load character name", FAILED_TO_LOAD_CHAR_NAME);
}

static void load_map_info() {
    map_id = GetMapId();
    district = GetDistrict();
    district_number = GetDistrictNumber();
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

static bool proc_state() {
    if (!GetIsIngame()) {
        return false;
    }

    if (map_id != bot_configuration.map_id ||
        district != bot_configuration.district ||
        district_number != bot_configuration.district_number) {
        LogError("Not in correct location. In %d %d %d. Required %d %d %d", map_id, district, district_number, bot_configuration.map_id, bot_configuration.district, bot_configuration.district_number);
        Travel(bot_configuration.map_id, bot_configuration.district, bot_configuration.district_number);
        time_sleep_sec(10);
        return false;
    }

    return true;
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

static void connect_websocket() {
    const auto max_tries = 5;
    for (auto i = 0; i < bot_configuration.connection_retries || bot_configuration.connection_retries == 0; i++) {
        if (ws) {
            ws->close();
        }

        LogInfo("Attempting to connect. Try %d/%d", i, bot_configuration.connection_retries);
        try {
            const auto user_agent = std::format("{}-{}-{}", character_name.c_str(), bot_configuration.map_id, static_cast<uint32_t>(bot_configuration.district));
            ws = easywsclient::WebSocket::from_url(bot_configuration.web_socket_url, user_agent);
            
        }
        catch (std::exception) {
        }

        if (!ws) {
            LogError("Failed to connect");
            continue;
        }

        for (auto j = 0; j < max_tries; j++) {
            if (ws->getReadyState() == easywsclient::WebSocket::OPEN) {
                return;
            }

            ws->poll();
            time_sleep_sec(1);
        }
    }
    
    exit_with_status("Timed out while attempting to connect", FAILED_TO_CONNECT);
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
    
    wait_until_ingame();

    load_character_name();
    
    load_map_info();

    load_configuration();

    connect_websocket();

    while (running) {
        if (ws->getReadyState() == easywsclient::WebSocket::CLOSED) {
            connect_websocket();
        }

        if (!proc_state()) {
            continue;
        }

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
