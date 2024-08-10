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