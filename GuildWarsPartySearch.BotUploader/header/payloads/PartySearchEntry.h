#pragma once
#include <cstdint>
#include <json.hpp>

using json = nlohmann::json;

namespace Payloads {
    struct PartySearchEntry {
        int PartySize;
        int PartyMaxSize;
        int Npcs;
        std::string CharName = "";
    };

    void to_json(json& j, const PartySearchEntry& p);
}