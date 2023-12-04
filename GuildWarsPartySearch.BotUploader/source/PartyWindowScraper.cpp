#include "pch.h"
#include <string>
#include <GWCA/Managers/GameThreadMgr.h>
#include <GWCA/Managers/MapMgr.h>
#include <GWCA/Managers/AgentMgr.h>
#include <GWCA/Managers/PartyMgr.h>
#include <GWCA/Managers/PlayerMgr.h>
#include <GWCA/Managers/SkillbarMgr.h>
#include <GWCA/Managers/QuestMgr.h>
#include <GWCA/Context/WorldContext.h>
#include <GWCA/GameContainers/Array.h>
#include <GWCA/GameEntities/Skill.h>
#include <GWCA/GameEntities/Quest.h>
#include <GWCA/GameEntities/Agent.h>
#include <GWCA/GameEntities/Title.h>
#include <GWCA/GameEntities/Party.h>
#include <GWCA/GameEntities/Attribute.h>
#include <GWCA/GameEntities/Player.h>
#include <GWCA/GameEntities/NPC.h>
#include <GWCA/GameEntities/Map.h>
#include <GWCA/Constants/Constants.h>
#include <WebSocketClient.h>
#include <Utils.h>
#include <payloads/PartySearch.h>
#include <payloads/PartySearchEntry.h>
#include <chrono>

using namespace std::chrono;

namespace PartySearch {
    std::chrono::steady_clock::time_point lastOperationTime = std::chrono::steady_clock::now();

    void GameThreadCallback(std::wstring uri, std::wstring apiKey, std::wstring botName) {
        steady_clock::time_point currentTime = steady_clock::now();
        duration<double> timeDiff = duration_cast<duration<double>>(currentTime - lastOperationTime);

        if (timeDiff.count() < 60) {
            return;
        }

        if (!GW::Map::GetIsMapLoaded()) {
            return;
        }

        if (!Websocket::Connected) {
            Websocket::Connect(uri.c_str(), 443, L"party-search/update", apiKey.c_str(), botName.c_str());
        }

        if (!Websocket::Connected) {
            printf("Failed to connect. Will retry later");
        }

        auto areaInfo = GW::Map::GetCurrentMapInfo();
        auto playerNameWChar = GW::PlayerMgr::GetPlayerName();
        std::wstring playerNameStr(playerNameWChar);
        auto playerName = PartySearch::Utils::WStringToString(playerNameStr);
        Payloads::PartySearch partySearch;
        partySearch.Campaign = (int)areaInfo->campaign;
        partySearch.Continent = (int)areaInfo->continent;
        partySearch.Map = (int)GW::Map::GetMapID();
        partySearch.Region = (int)areaInfo->region;
        partySearch.District = "En - 1";
        
        Payloads::PartySearchEntry entry;
        entry.CharName = playerName;
        entry.Npcs = 0;
        entry.PartyMaxSize = 8;
        entry.PartySize = 8;

        partySearch.PartySearchEntries.push_back(entry);

        json j = partySearch;
        Websocket::SendWebSocketMessage(j.dump());
        auto response = Websocket::ReceiveWebSocketMessage();
        printf("%s", response.second.c_str());
        lastOperationTime = currentTime;
    }
}