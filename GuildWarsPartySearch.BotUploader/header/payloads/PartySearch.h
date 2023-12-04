#pragma once
#include <cstdint>
#include <json.hpp>
#include <payloads/PartySearchEntry.h>

using json = nlohmann::json;

namespace Payloads {
    struct PartySearch{
        int Continent;
        int Campaign;
        int Region;
        int Map;
        std::string District = "";
        std::vector<PartySearchEntry> PartySearchEntries;
    };

    void to_json(json& j, const PartySearch& p);
}