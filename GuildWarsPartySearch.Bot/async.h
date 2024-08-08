#ifdef ASYNC_H
#error "async.h is already included"
#endif
#define ASYNC_H

#define async_completed(s) ((s).complete)

#define async_waitfor(s, f) _async_waitfor(s, f, &async_completed(s))

const int ASYNC_RESULT_OK = 0;
const int ASYNC_RESULT_TIMEOUT = -1;
const int ASYNC_RESULT_REQUEST_FAILED = -2;
const int ASYNC_RESULT_WRONG_VALUE = -3; // e.g. quote for different item or wrong map loaded
const int ASYNC_RESULT_CANCELLED = -4;

const msec_t ASYNC_TRAVEL_TIMEOUT_MS = 20000;

typedef void AsyncStateReset(void* state);
typedef bool AsyncStateCheck(void* state);

typedef struct AsyncState {
    msec_t          started_at;
    int             result;
    bool            complete;
    msec_t          timeout_ms;
    CallbackEntry   callback_1;
    CallbackEntry   callback_2;
    void*        extra_info_1;
    void*        extra_info_2;
} AsyncState;
void async_reset(AsyncState* state)
{
    
    if (state->callback_1.registered) {
        assert(UnRegisterEvent(&state->callback_1));
        assert(!state->callback_1.registered);
    }
    if (state->callback_2.registered) {
        assert(UnRegisterEvent(&state->callback_2));
        assert(!state->callback_2.registered);
    }
    
}
bool async_check(AsyncState* state) {
    
    if (!state->complete && state->timeout_ms) {
        msec_t now = time_get_ms();
        msec_t elapsed = now - state->started_at;
        if (elapsed > state->timeout_ms) {
            
            async_reset(state);
            
            state->result = ASYNC_RESULT_TIMEOUT;
            state->complete = true;
        }
    }
    return state->complete;
}
void async_cancel(AsyncState* state) {
    async_reset(state);
    state->result = ASYNC_RESULT_CANCELLED;
    state->complete = true;
}
static void async_cb_map_enter(Event* event, void * param)
{
    assert(event->type == EventType_WorldMapEnter);
    assert(param != NULL);
    AsyncState* state = (AsyncState*)param;
    async_reset(state);
    state->result = ASYNC_RESULT_OK;
    state->complete = true;
    
}

static void async_cb_cant_travel(Event* event, void * param)
{
    assert(event->type == EventType_WorldCantTravel);
    assert(param != NULL);
    AsyncState* state = (AsyncState*)param;
    async_reset(state);
    state->result = event->WorldCantTravel.value;
    state->complete = true;
    
}

void async_wait_map_loading(AsyncState* state, msec_t timeout_ms)
{
    memset(state, 0, sizeof(*state));
    state->started_at = time_get_ms();
    state->timeout_ms = timeout_ms;
    CallbackEntry_Init(&state->callback_1, async_cb_map_enter, state);
    CallbackEntry_Init(&state->callback_2, async_cb_cant_travel, state);

    assert(RegisterEvent(EventType_WorldMapEnter, &state->callback_1));
    assert(RegisterEvent(EventType_WorldCantTravel, &state->callback_2));
}

void async_redirect_map(AsyncState* state,
    uint32_t map_id, uint32_t type, District district, int district_number, msec_t timeout_ms)
{
    async_wait_map_loading(state, timeout_ms);

    RedirectMap(map_id, type, district, district_number);
}

void async_travel(AsyncState* state,
    uint32_t map_id, District district, uint16_t district_number, msec_t timeout_ms)
{
    async_wait_map_loading(state, timeout_ms);

    Travel(map_id, district, district_number);
}
typedef struct QuoteDetails {
    int             quote_item_id;
    uint32_t price;
} QuoteDetails;
static void async_cb_quote_received(Event* event, void* param)
{
    assert(event->type == EventType_ItemQuotePrice);
    assert(param);
    AsyncState* state = (AsyncState*)param;
    int32_t requested_item_id = (int32_t)state->extra_info_1;
    if (requested_item_id != event->ItemQuotePrice.item_id)
        return; // Different item, are you running multiple quotes?
    async_reset(state);
    *(uint32_t*)state->extra_info_2 = event->ItemQuotePrice.quote_price;
    
    state->result = ASYNC_RESULT_OK;
    state->complete = true;
}

void async_get_quote(AsyncState* state, uint32_t item_id, uint32_t* price, msec_t timeout_ms)
{
    
    memset(state, 0, sizeof(*state));
    state->started_at = time_get_ms();
    state->timeout_ms = timeout_ms;
    state->extra_info_1 = (void*)item_id;
    state->extra_info_2 = (void*)price;
    CallbackEntry_Init(&state->callback_1, async_cb_quote_received, state);
    
    RegisterEvent(EventType_ItemQuotePrice, &state->callback_1);
    
    if (!RequestItemQuote(item_id)) {
        // Request failed e.g. invalid item id
        async_reset(state);
        *(uint32_t*)state->extra_info_2 = 0;
        state->result = ASYNC_RESULT_REQUEST_FAILED;
        state->complete = true;
    }
    
}

static void async_cb_dialog_opened(Event* event, void * param)
{
    assert(event->type == EventType_DialogOpen);
    assert(param != NULL);

    AsyncState* state = (AsyncState*)param;
    async_reset(state);
    state->result = ASYNC_RESULT_OK;
    state->complete = true;
}

void async_go_to_npc(AsyncState *state, uint32_t agent_id, msec_t timeout_ms)
{
    memset(state, 0, sizeof(*state));
    state->started_at = time_get_ms();
    state->timeout_ms = timeout_ms;

    CallbackEntry_Init(&state->callback_1, async_cb_dialog_opened, state);
    RegisterEvent(EventType_DialogOpen, &state->callback_1);

    // @Robustness:
    // Might be interesting to do extra check here, to ensure we don't get stuck.
    InteractAgent(agent_id);
}

typedef struct AsyncState_MoveTo {
    bool            completed;

    Vec2f           dest;
    float           dist_to_reach;
} AsyncState_MoveTo;

void async_init_MoveTo(AsyncState_MoveTo *state, Vec2f dest, float dist_to_reach)
{
    state->completed = false;
    state->dest = dest;
    state->dist_to_reach = dist_to_reach;

    MoveToPoint(dest);
}

void async_reset_MoveTo(AsyncState_MoveTo *state)
{
}

// @Enhancement: @Robustness:
// We don't have to redefine dist2
static float MoveTo_dist2(Vec2f u, Vec2f v)
{
    float dx = u.x - v.x;
    float dy = u.y - v.y;
    return sqrtf((dx * dx) + (dy * dy));
}

void async_update_MoveTo(AsyncState_MoveTo *state)
{
    AgentId agent_id = GetMyAgentId();
    if (agent_id) {
        Vec2f pos = GetAgentPos(agent_id);
        float dist = MoveTo_dist2(pos, state->dest);
        if (dist <= state->dist_to_reach) {
            async_reset_MoveTo(state);
            state->completed = true;
        }
    }
}
