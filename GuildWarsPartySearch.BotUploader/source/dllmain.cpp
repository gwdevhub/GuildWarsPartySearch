// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <GWCA/GWCA.h>
#include <GWCA/Utilities/Scanner.h>
#include <GWCA/Utilities/Hook.h>
#include <GWCA/Utilities/Hooker.h>
#include <GWCA/Managers/MemoryMgr.h>
#include <GWCA/Managers/GameThreadMgr.h>
#include <mutex>
#include <WebSocketClient.h>
#include <payloads/PartySearch.h>
#include <fstream>
#include <filesystem>
#include <string>
#include <PartyWindowScraper.h>

volatile bool initialized;
volatile WNDPROC oldWndProc;
std::mutex startupMutex;
HMODULE dllmodule;
HANDLE serverThread;
static FILE* stdout_proxy;
static FILE* stderr_proxy;
std::wstring uri, apiKey, botName;
GW::HookEntry gameThreadCallback;

void Terminate()
{
#ifdef BUILD_TYPE_DEBUG
    if (stdout_proxy)
        fclose(stdout_proxy);
    if (stderr_proxy)
        fclose(stderr_proxy);
    FreeConsole();
#endif

    GW::GameThread::RemoveGameThreadCallback(&gameThreadCallback);
    FreeLibraryAndExitThread(dllmodule, EXIT_SUCCESS);
}


static DWORD WINAPI Init(LPVOID)
{
#ifdef BUILD_TYPE_DEBUG
    AllocConsole();
    SetConsoleTitleA("Guild Wars Party Search Console");
    freopen_s(&stdout_proxy, "CONOUT$", "w", stdout);
    freopen_s(&stderr_proxy, "CONOUT$", "w", stderr);
#endif

    char dllpath[MAX_PATH]{};
    GetModuleFileName(dllmodule, dllpath, MAX_PATH); //ask for name and path of this dll
    auto dll_path = std::filesystem::path(dllpath).parent_path();
    auto config = dll_path / "config.txt";
    printf("Reading %s", config.string().c_str());
    std::wifstream file(config);
    if (!file.is_open()) {
        printf("Failed to open config");
        return 0;
    }

    std::wstring line;
    // Set the locale of the file stream to the environment's default
    file.imbue(std::locale());

    // Read the first line for URI
    if (std::getline(file, line)) {
        uri = line;
    }

    // Read the second line for API Key
    if (std::getline(file, line)) {
        apiKey = line;
    }

    // Read the third line for Bot Name
    if (std::getline(file, line)) {
        botName = line;
    }

    file.close();

    printf("Init: Setting up scanner\n");
    GW::Scanner::Initialize();
    printf("Init: Setting up hook base\n");
    GW::HookBase::Initialize();
    printf("Init: Setting up GWCA\n");
    if (!GW::Initialize()) {
        printf("Init: Failed to set up GWCA\n");
        return 0;
    }

    printf("Init: Enabling hooks\n");
    GW::HookBase::EnableHooks();

    printf("Registering game thread callback");
    GW::GameThread::RegisterGameThreadCallback(&gameThreadCallback, [=](GW::HookStatus* status) {
        PartySearch::GameThreadCallback(uri, apiKey, botName);
        });

    
    printf("Init: Returning success\n");
    return 0;
}


// DLL entry point, dont do things in this thread unless you know what you are doing.
BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    DisableThreadLibraryCalls(hModule);

    if (ul_reason_for_call == DLL_PROCESS_ATTACH) {
        dllmodule = hModule;
        HANDLE handle = CreateThread(0, 0, Init, hModule, 0, 0);
        CloseHandle(handle);
    }

    return TRUE;
}