#include <cstdint>
#include <json.hpp>
#include <payloads/PartySearch.h>

using json = nlohmann::json;

namespace Payloads {
    void to_json(json& j, const PartySearch& p) {
        j["Continent"] = p.Continent;
        j["Campaign"] = p.Campaign;
        j["Region"] = p.Region;
        j["Map"] = p.Map;
        j["District"] = p.District;
        j["PartySearchEntries"] = p.PartySearchEntries;
    }
}