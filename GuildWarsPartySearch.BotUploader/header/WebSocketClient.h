#include <iostream>
#include <winhttp.h>
#pragma once

namespace Websocket {
    extern bool Connected;
    bool Connect(LPCWSTR target, INTERNET_PORT port, LPCWSTR path, LPCWSTR apiKey, LPCWSTR botName);
    bool SendWebSocketMessage(const std::string& message);
    std::pair<bool, std::string> ReceiveWebSocketMessage();
    void Close();
}