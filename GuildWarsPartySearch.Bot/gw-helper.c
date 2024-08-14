#ifdef GW_HELPER_C_INC
#error "gw-helper.c is already included"
#endif
#define GW_HELPER_C_INC

#include <float.h> // FLT_EPSILON
#include <common/time.h>
#include <common/macro.h>

#include <signal.h>
#include <stdio.h>

#include "async.h"

static int   irand(int min, int max);
static float frand(float min, float max);
static float dist2(Vec2f u, Vec2f v);
static bool  equ2f(Vec2f v1, Vec2f v2);
static Vec2f lerp2f(Vec2f a, Vec2f b, float t);

#define sec_to_ms(sec) (sec * 1000)

// Ensure uint16_t* is null terminated, adding one if needed. Returns length of uint16_t string excluding null terminator.
size_t null_terminate_uint16(uint16_t* buffer, size_t buffer_len) {
    for (size_t i = 0; i < buffer_len - 1; i++) {
        if (buffer[i] == 0)
            return i;
    }
    buffer[buffer_len - 1] = 0;
    return buffer_len - 1;
}
// Ensure wchar_t* is null terminated, adding one if needed. Returns length of wchar_t string excluding null terminator.
size_t null_terminate_wchar(wchar_t* buffer, size_t buffer_len) {
    for (size_t i = 0; i < buffer_len - 1; i++) {
        if (buffer[i] == 0)
            return i;
    }
    buffer[buffer_len - 1] = 0;
    return buffer_len - 1;
}
// Ensure char* is null terminated, adding one if needed. Returns length of char string excluding null terminator.
size_t null_terminate_char(char* buffer, size_t buffer_len) {
    LogDebug("null_terminate_char: %p, %d", buffer, buffer_len);
    for (size_t i = 0; i < buffer_len - 1; i++) {
        if (buffer[i] == 0)
            return i;
    }
    buffer[buffer_len - 1] = 0;
    return buffer_len - 1;
}

// Returns length of char string excluding null terminator.
// from_buffer MUST be a null terminated string.
static int wchar_to_char(const wchar_t* from_buffer, char* to_buffer, size_t to_buffer_length) {
    setlocale(LC_ALL, "en_US.utf8");
#pragma warning(suppress : 4996)
#if 0
    //wcstombs(pIdentifier->Description, desc.Description, MAX_DEVICE_IDENTIFIER_STRING);
    int len = WideCharToMultiByte(CP_UTF8, 0, from, -1, NULL, 0, NULL, NULL);
    if (len == 0 || len > (int)max_len)
        return -1;
    WideCharToMultiByte(CP_UTF8, 0, from, -1, to, len, NULL, NULL);
    return (int)null_terminate_char(to, max_len);
#else
    int from_buffer_len = wcslen(from_buffer);
    if (to_buffer_length < from_buffer_len * sizeof(wchar_t)) {
        LogError("wchar_to_char: Out buffer length %d is not big enough; need %d", to_buffer_length, from_buffer_len * sizeof(wchar_t));
        return -1; // char array should be twice the length of the from_buffer
    }
    LogDebug("wchar_to_char: wcstombs(%p, %p, %d)", to_buffer, from_buffer, to_buffer_length);
    int len = (int)wcstombs(to_buffer, from_buffer, to_buffer_length);
    if (len < 0) {
        LogError("wchar_to_char: wcstombs failed %d", len);
        return len;
    }
    return (int)null_terminate_char(to_buffer, to_buffer_length);
#endif
}
// Returns length of wchar_t string excluding null terminator.
static int uint16_to_wchar(const uint16_t* from_buffer, wchar_t* to_buffer, size_t to_buffer_length) {
    size_t written = 0;
    to_buffer[0] = 0;
    for (written = 0; written < to_buffer_length - 1; written++) {
        to_buffer[written] = from_buffer[written];
        if (!from_buffer[written])
            break;
    }
    return (int)null_terminate_wchar(to_buffer, to_buffer_length);
}
// Returns length of char string excluding null terminator.
static int uint16_to_char(const uint16_t* from_buffer, char* to_buffer, const size_t to_buffer_length) {
    int result = -1;
    if (!to_buffer_length)
        return result;
    size_t tmp_to_buffer_bytes = to_buffer_length * sizeof(wchar_t);
    wchar_t* tmp_to_buffer = (wchar_t*)malloc(tmp_to_buffer_bytes);
    result = uint16_to_wchar(from_buffer, tmp_to_buffer, to_buffer_length);
    if (result < 1)
        goto cleanup;
    // Result is null terminated now.
    result = wchar_to_char(tmp_to_buffer, to_buffer, to_buffer_length);
cleanup:
    free(tmp_to_buffer);
    return result;
}
static bool str_match_uint32_t(uint32_t* first, uint32_t* second) {
    size_t len1 = 0;
    while (first[len1]) len1++;
    size_t len2 = 0;
    while (first[len2]) len2++;
    if (len1 != len2)
        return false;
    for (size_t i = 0; i < len1; i++) {
        if (first[i] != second[i])
            return false;
    }
    return true;
}
static bool str_match_uint16_t(uint16_t* first, uint16_t* second) {
    size_t len1 = 0;
    while (first[len1]) len1++;
    size_t len2 = 0;
    while (first[len2]) len2++;
    if (len1 != len2)
        return false;
    for (size_t i = 0; i < len1; i++) {
        if (first[i] != second[i])
            return false;
    }
    return true;
}
static ApiAgent get_me() {
    ApiAgent res;
    res.agent_id = 0;
    int my_id = GetMyAgentId();
    if (my_id) 
        GetAgent(&res, my_id);
    return res;
}
static bool get_my_position(Vec2f* out) {
    ApiAgent me = get_me();
    if (!me.agent_id)
        return false;
    *out = me.position;
    return true;
}
static float get_distance_to_agent(ApiAgent* agent) {
    ApiAgent me = get_me();
    if (!me.agent_id)
        return 0.0f;
    return dist2(agent->position, me.position);
}

static int irand(int min, int max)
{
    assert(0 < (max - min) && (max - min) < RAND_MAX);
    int range = max - min;
    return (rand() % range) + min;
}

static float frand(float min, float max)
{
    assert(0 < (max - min) && (max - min) < RAND_MAX);
    float range = max - min;
    float random = (float)rand() / RAND_MAX;
    return (random * range) + min;
}

static float dist2(Vec2f u, Vec2f v)
{
    float dx = u.x - v.x;
    float dy = u.y - v.y;
    return sqrtf((dx * dx) + (dy * dy));
}

static bool equ2f(Vec2f v1, Vec2f v2)
{
    if (v1.x != v2.x)
        return false;
    if (v1.y != v2.y)
        return false;
    return true;
}

static Vec2f lerp2f(Vec2f a, Vec2f b, float t)
{
    Vec2f pos;
    pos.x = ((1.f - t) * a.x) + (t * b.x);
    pos.y = ((1.f - t) * a.y) + (t * b.y);
    return pos;
}

#if 0
static void buy_rare_material_item(Item *item)
{
    if (!item->quote_price) return;

    TransactionInfo send_info = {0};
    TransactionInfo recv_info = {0};

    recv_info.item_count = 1;
    recv_info.item_ids[0] = item->item_id;
    recv_info.item_quants[0] = 1;

    // @TODO
    // BuyMaterials(TRANSACT_TYPE_TraderBuy, item->quote_price, &send_info, 0, &recv_info);
}
#endif


struct area_info {
    uint32_t campaign;
    uint32_t continent;
    Region region;
    RegionType region_type;
    uint32_t flags;
    uint32_t x;
    uint32_t y;
    uint32_t start_x;
    uint32_t start_y;
    uint32_t end_x;
    uint32_t end_y;
};

const area_info* get_map_info(uint32_t map_id) {
    static const struct area_info area_info_table[] = {
        #include <client/data/area_info.data>
    };
    if (map_id > ARRAY_SIZE(area_info_table))
        return nullptr;
    return &area_info_table[map_id];
}

bool is_valid_outpost(uint32_t map_id) {
    const auto map_info = get_map_info(map_id);
    if (!(map_info && map_info->start_x && map_info->start_y))
        return false;
    if ((map_info->flags & 0x5000000) == 0x5000000)
        return false; // e.g. "wrong" augury rock is map 119, no NPCs
    if ((map_info->flags & 0x80000000) == 0x80000000)
        return false; // e.g. Debug map
    switch (map_info->region_type) {
    case RegionType_City:
    case RegionType_CompetitiveMission:
    case RegionType_CooperativeMission:
    case RegionType_EliteMission:
    case RegionType_MissionOutpost:
    case RegionType_Outpost:
        break;
    default:
        return false;
    }
    return true;
}

uint32_t get_nearest_outpost_id(uint32_t map_id) {
    if (is_valid_outpost(map_id)) {
        return map_id;
    }
    // TODO: calc!
    return 0;
}

static int get_rare_material_trader_id(int map_id)
{
    switch(map_id) {
    case 4:
    case 5:
    case 6:
    case 52:
    case 176:
    case 177:
    case 178:
    case 179:
        return 205;
    case 275:
    case 276:
    case 359:
    case 360:
    case 529:
    case 530:
    case 537:
    case 538:
        return 192;
    case 109:
        return 1997;
    case 193:
        return 3621;
    case 194:
    case 250:
    case 857:
        return 3282;
    case 242:
        return 3281;
    case 376:
        return 5388;
    case 398:
    case 433:
        return 5667;
    case 414:
        return 5668;
    case 424:
        return 5387;
    case 438:
        return 5613;
    case 49:
        return 2038;
    case 491:
    case 818:
        return 4723;
    case 449:
        return 4729;
    case 492:
        return 4722;
    case 638:
        return 6760;
    case 640:
        return 6759;
    case 641:
        return 6060;
    case 642:
        return 6045;
    case 643:
        return 6386;
    case 644:
        return 6385;
    case 652:
        return 6228;
    case 77:
        return 3410;
    case 81:
        return 2083;
    default:
        return 0;
    }
}

static int wait_map_loading(int map_id, msec_t timeout_ms);
static int travel_wait(int map_id, District district, uint16_t district_number)
{
    if ((GetMapId() == map_id) && (GetDistrict() == district)
        && (GetDistrictNumber() == district_number)) {
        return 0;
    }
    Travel(map_id, district, district_number);
    return wait_map_loading(map_id, 20000);
}

static int random_travel(int map_id)
{
    District districts[] = {
        DISTRICT_ASIA_KOREAN,
        DISTRICT_ASIA_CHINESE,
        DISTRICT_ASIA_JAPANESE
    };

    int r = irand(0, ARRAY_SIZE(districts));
    District district = districts[r];
    Travel(map_id, district, 0);
    return wait_map_loading(map_id, 20000);
}

static int travel_gh_safe(void)
{
    // @Enhancement: Theoricly, they could add a gh, but yeah...
    static int gh_map_ids[] = {
        4, 5, 6, 52, 176, 177, 178, 179, 275, 276, 359, 360, 529, 530, 537, 538
    };
    int map_id = GetMapId();
    for (int i = 0; i < ARRAY_SIZE(gh_map_ids); i++) {
        if (map_id == gh_map_ids[i])
            return true;
    }
    uint32_t guild_id = GetMyGuildId();
    TravelHall(guild_id);
    return wait_map_loading(0, 20000);
}

static ArrayApiPlayer get_all_players(void)
{
    ArrayApiPlayer players = {0};
    size_t count = GetPlayers(NULL, 0);
    if (!count) return players;
    array_reserve(&players, count);
    players.size = GetPlayers(players.data, players.capacity);
    return players;
}

static bool get_player_with_name(const wchar_t *name, ApiPlayer *player)
{
    size_t length;
    uint16_t buffer[20];
    ArrayApiPlayer players = get_all_players();

    ApiPlayer *it;
    bool match = true;
    array_foreach(it, &players) {
        match = true;
        length = GetPlayerName(it->player_id, buffer, ARRAY_SIZE(buffer));
        for (size_t i = 0; name[i] && i < length && match; i++) {
            match = name[i] == buffer[i];
        }
        if (match) {
            if (player) *player = *it;
            array_reset(&players);
            return true;
        }
    }

    array_reset(&players);
    return false;
}

static int wait_map_loading(int map_id, msec_t timeout_ms)
{
    AsyncState state;
    async_wait_map_loading(&state, timeout_ms);

    unsigned int current_ms = 0;
    while (!async_check(&state)) {
        time_sleep_ms(16);
    }
    if (state.result != ASYNC_RESULT_OK) {
        return state.result;
    }
    // @Cleanup:
    // We shouldn't have that here.
    current_ms = 0;
    while (!GetMyAgentId()) {
        time_sleep_ms(current_ms += 16);
        if (current_ms > (int)timeout_ms) {
            return ASYNC_RESULT_TIMEOUT; // Timeout
        }
    }

    return GetMapId() == map_id ? ASYNC_RESULT_OK : ASYNC_RESULT_WRONG_VALUE;
}
