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

#define sec_to_ms(sec) (sec * 1000)

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
    case RegionType_Arena:
    case RegionType_ZaishenBattle:
    case RegionType_HeroesAscent:
        break;
    default:
        return false;
    }

    const auto current_map = get_map_info(GetMapId());
    const auto is_pre = current_map && current_map->region == Region::Region_Presearing;

    if (is_pre != (map_info->region == Region::Region_Presearing))
        return false;
    return true;
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

static int travel_wait(int map_id, District district, uint16_t district_number)
{
    int current_map_id = GetMapId();
    District current_district = GetDistrict();
    int current_district_number = GetDistrictNumber();

    if ((current_map_id == map_id) 
        && (district == District::DISTRICT_CURRENT || current_district == district)
        && (!district_number || current_district_number == district_number)) {
        return 0;
    }
    LogInfo("travel_wait %d(%d) %d(%d) %d(%d)", map_id, current_map_id, district, current_district, district_number, current_district_number);
    if (map_id == current_map_id || true) {
        Travel(map_id, district, district_number);
    }
    else {
        // RedirectMap if we're NOT already in the same map; server will drop connection otherwise
        //Travel(map_id, district, district_number);
        RedirectMap(map_id,District::DISTRICT_CURRENT,0);
    }
    
    //Travel(map_id, district, district_number);
    return wait_map_loading(map_id, 20000);
}

const char* get_message_from_gw_error_id(int err) {
    switch (err) {
    case ASYNC_RESULT_TIMEOUT:
        return "Action timed out";
    case 37: return "District is closed. Please try another.";
    case 38: return "District is full. Please try another.";
    }
    return "Unknown err";
}