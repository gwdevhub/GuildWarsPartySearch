#include <WebSocketClient.h>
#include <websocket.h>
#include <iostream>
#include <Windows.h>
#include <winhttp.h>
#include <string>
#include <vector>
#include <sstream>

namespace Websocket {
    bool Connected = false;
    HINTERNET hConnectCached, hRequestCached, hSessionCached, hWebSocketCached;

    // Function to add a header to the request
    bool AddHeader(HINTERNET hRequest, const std::wstring& headerName, const std::wstring& headerValue) {
        std::wstringstream wss;
        wss << headerName << L": " << std::wstring(headerValue.begin(), headerValue.end()) << L"\r\n";
        std::wstring fullHeader = wss.str();

        return WinHttpAddRequestHeaders(hRequest, fullHeader.c_str(), -1, WINHTTP_ADDREQ_FLAG_ADD);
    }

    bool Connect(LPCWSTR target, INTERNET_PORT port, LPCWSTR path, LPCWSTR apiKey, LPCWSTR botName) {
        DWORD dwError = ERROR_SUCCESS;
        HINTERNET hSession = NULL, hConnect = NULL, hRequest = NULL, hWebSocket = NULL;

        // Create session, connection, and request handles.
        hSession = WinHttpOpen(L"Guild Wars Party Search Bot Client",
            WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
            WINHTTP_NO_PROXY_NAME,
            WINHTTP_NO_PROXY_BYPASS, 0);
        if (!hSession) {
            dwError = GetLastError();
            goto cleanup;
        }

        hConnect = WinHttpConnect(hSession, target, port, 0);
        if (!hConnect) {
            dwError = GetLastError();
            goto cleanup;
        }

        hRequest = WinHttpOpenRequest(hConnect, L"GET", path, NULL, WINHTTP_NO_REFERER,
            WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);
        if (!hRequest) {
            dwError = GetLastError();
            goto cleanup;
        }

        // Add the X-ApiKey header
        if (!AddHeader(hRequest, L"X-ApiKey", apiKey)) {
            dwError = GetLastError();
            goto cleanup;
        }

        // Add the User-Agent header
        if (!AddHeader(hRequest, L"User-Agent", botName)) {
            dwError = GetLastError();
            goto cleanup;
        }

        // Bypass SSL certificate errors (for testing only)
        DWORD dwFlags = SECURITY_FLAG_IGNORE_UNKNOWN_CA |
            SECURITY_FLAG_IGNORE_CERT_WRONG_USAGE |
            SECURITY_FLAG_IGNORE_CERT_CN_INVALID |
            SECURITY_FLAG_IGNORE_CERT_DATE_INVALID;
        if (!WinHttpSetOption(hRequest, WINHTTP_OPTION_SECURITY_FLAGS, &dwFlags, sizeof(dwFlags))) {
            dwError = GetLastError();
            goto cleanup;
        }

        // Request protocol upgrade from HTTP to WebSocket.
        if (!WinHttpSetOption(hRequest, WINHTTP_OPTION_UPGRADE_TO_WEB_SOCKET, NULL, 0)) {
            dwError = GetLastError();
            goto cleanup;
        }

        // Send request and receive response.
        if (!WinHttpSendRequest(hRequest, WINHTTP_NO_ADDITIONAL_HEADERS, 0, NULL, 0, 0, 0) ||
            !WinHttpReceiveResponse(hRequest, NULL)) {
            dwError = GetLastError();
            goto cleanup;
        }

        // Complete the WebSocket upgrade.
        hWebSocket = WinHttpWebSocketCompleteUpgrade(hRequest, NULL);
        if (!hWebSocket) {
            dwError = GetLastError();
            goto cleanup;
        }

        // The request handle is no longer needed.
        WinHttpCloseHandle(hRequest);
        hRequest = NULL;

        std::wcout << L"Successfully upgraded to WebSocket protocol\n";

        hConnectCached = hConnect;
        hRequestCached = hRequest;
        hSessionCached = hSession;
        hWebSocketCached = hWebSocket;
        Connected = true;
        return true;

    cleanup:
        if (dwError != ERROR_SUCCESS) {
            std::wcerr << L"Error: " << dwError << std::endl;
        }
        if (hRequest) WinHttpCloseHandle(hRequest);
        if (hWebSocket) WinHttpCloseHandle(hWebSocket);
        if (hConnect) WinHttpCloseHandle(hConnect);
        if (hSession) WinHttpCloseHandle(hSession);

        return dwError == ERROR_SUCCESS ? true : false;
    }

    bool SendWebSocketMessage(const std::string& message) {
        // Check if the WebSocket handle is valid
        if (hWebSocketCached == NULL) {
            return ERROR_INVALID_HANDLE;
        }

        // Send the message
        DWORD dwError = WinHttpWebSocketSend(hWebSocketCached,
            WINHTTP_WEB_SOCKET_UTF8_MESSAGE_BUFFER_TYPE,
            (PVOID)message.c_str(),
            message.size());

        Connected = (dwError == ERROR_SUCCESS);
        return Connected;
    }

    std::pair<bool, std::string> ReceiveWebSocketMessage() {
        std::string receivedMessage;
        std::vector<char> buffer(4096); // Buffer for receiving chunks of the message
        DWORD dwError = ERROR_SUCCESS;
        DWORD dwBytesTransferred = 0;
        WINHTTP_WEB_SOCKET_BUFFER_TYPE eBufferType;

        if (hWebSocketCached == NULL) {
            return { false, "" };
        }

        do {
            dwError = WinHttpWebSocketReceive(hWebSocketCached, buffer.data(), buffer.size(), &dwBytesTransferred, &eBufferType);

            if (dwError != ERROR_SUCCESS) {
                // Handle the error appropriately in your application
                Connected = false;
                return { dwError, "" };
            }

            // Append received data to the message string
            receivedMessage.append(buffer.data(), dwBytesTransferred);

        } while (eBufferType == WINHTTP_WEB_SOCKET_BINARY_FRAGMENT_BUFFER_TYPE ||
            eBufferType == WINHTTP_WEB_SOCKET_UTF8_FRAGMENT_BUFFER_TYPE);

        // Assuming the received message is UTF-8 encoded and can be directly used
        Connected = true;
        return { true, receivedMessage };
    }

    void Close() {
        Connected = false;
        if (hConnectCached) {
            WinHttpCloseHandle(hConnectCached);
        }

        if (hRequestCached) {
            WinHttpCloseHandle(hRequestCached);
        }

        if (hSessionCached) {
            WinHttpCloseHandle(hSessionCached);
        }

        if (hWebSocketCached) {
            WinHttpCloseHandle(hWebSocketCached);
        }
    }
}