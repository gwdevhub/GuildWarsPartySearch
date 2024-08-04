#ifndef __STDC__
#define __STDC__ 1
#endif

#define _CRT_SECURE_NO_WARNINGS
#include <math.h>
#include <locale.h>
#include <stdbool.h>

#define HEADQUARTER_RUNTIME_LINKING
#include <client/constants.h>
#include <client/Headquarter.h>
#include <common/time.h>
#include <common/thread.h>
#include <common/dlfunc.h>

#ifdef _WIN32
# define DllExport __declspec(dllexport)
#else
# define DllExport
#endif

#include <signal.h>
#include <stdio.h>
#include <vector>
#include <atomic>
#include <time.h>


#define FAILED_TO_START 1

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
    char                message[65];
    char                sender[41];
} PartySearchAdvertisement;

#define MapID_EmbarkBeach               857
#define MapID_Ascalon                   148
#define MapID_Kamadan                   449
#define MapID_Kamadan_Halloween         818
#define MapID_Kamadan_Wintersday        819
#define MapID_Kamadan_Canthan_New_Year  820

static struct thread  bot_thread;
static std::atomic<bool> running;
static std::atomic<bool> ready;


static int required_map_id = MapID_Kamadan;
static District required_district = DISTRICT_AMERICAN;
static uint16_t required_district_number = 0;

static PluginObject* plugin_hook;

static CallbackEntry EventType_PartySearchAdvertisement_entry;
static CallbackEntry EventType_PartySearchRemoved_entry;
static CallbackEntry EventType_PartySearchSize_entry;
static CallbackEntry EventType_PartySearchType_entry;
static CallbackEntry EventType_WorldMapEnter_entry;
static CallbackEntry EventType_WorldMapLeave_entry;

static uint16_t player_name[20];
static char player_name_utf8[40];
static char account_uuid[64];

static uint64_t last_successful_curl = 0;

static void send_bot_info();

static std::vector<PartySearchAdvertisement*> party_search_advertisements;


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

    /*if (event->PartySearchAdvertisement.message.length) {
        int written = uint16_to_char(event->PartySearchAdvertisement.message.buffer, party->message, ARRAY_SIZE(party->message));
        assert(written > 0);
    }
    else {
        *party->message = 0;
    }
    if (event->PartySearchAdvertisement.sender.length) {
        int written = uint16_to_char(event->PartySearchAdvertisement.sender.buffer, party->sender, ARRAY_SIZE(party->sender));
        assert(written > 0);
    }
    else {
        *party->sender = 0;
    }*/

    return party;
}

static void on_map_left(Event* event, void* params) {
    clear_party_search_advertisements();
}

static void on_map_entered(Event* event, void* params) {
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
            
            break;
        }
    }
}

static void exit_with_status(const char* state, int exit_code)
{
    LogCritical("%s (code=%d)", state, exit_code);
    exit(exit_code);
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

    while (running) {
        time_sleep_sec(5);
        LogInfo("Running...");
    }

cleanup:
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
DllExport void PluginUnload(PluginObject* obj)
{
    running = false;
}
DllExport bool PluginEntry(PluginObject* obj)
{
    assert(obj);
    running = true;
    thread_create(&bot_thread, main_bot, NULL);
    thread_detach(&bot_thread);
    obj->PluginUnload = &PluginUnload;
    plugin_hook = obj;
    return true;
}

DllExport void on_panic(const char *msg)
{
    exit_with_status("Error", 1);
}
int main(void) {
    printf("Hello world");
    return 0;
}
