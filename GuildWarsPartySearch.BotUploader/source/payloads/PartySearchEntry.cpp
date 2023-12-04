#include <cstdint>
#include <json.hpp>
#include <payloads/PartySearchEntry.h>

using json = nlohmann::json;

namespace Payloads {
    void to_json(json& j, const PartySearchEntry& p) {
        j["PartySize"] = p.PartySize;
        j["PartyMaxSize"] = p.PartyMaxSize;
        j["Npcs"] = p.Npcs;
        j["CharName"] = p.CharName;
    }
}