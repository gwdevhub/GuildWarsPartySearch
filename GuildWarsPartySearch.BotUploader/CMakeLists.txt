# CMakeList.txt : CMake project for GuildWarsPartySearch.BotUploader, include source and define
# project specific logic here.
#

project(GuildWarsPartySearch.BotUploader)

if(CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "You are configuring a 64bit build, this is not supported. Run cmake with `-A Win32`")
endif()

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(GuildWarsPartySearch.BotUploader_FOLDER "${CMAKE_CURRENT_LIST_DIR}/")
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_LIST_DIR}/cmake/")

add_library(GuildWarsPartySearch.BotUploader SHARED)

file(GLOB SOURCES
    "source/pch.h"
    "source/*.cpp"
    "source/*/*.cpp"
    "header/*.h*"
    "header/*/*.h")
source_group(TREE "${CMAKE_CURRENT_SOURCE_DIR}" FILES ${SOURCES})
target_sources(GuildWarsPartySearch.BotUploader PRIVATE ${SOURCES})
target_compile_features(GuildWarsPartySearch.BotUploader PUBLIC cxx_std_17)
target_compile_definitions(GuildWarsPartySearch.BotUploader PRIVATE
    $<$<CONFIG:Debug>:BUILD_TYPE_DEBUG>
    $<$<CONFIG:Release>:BUILD_TYPE_RELEASE>
)

target_precompile_headers(GuildWarsPartySearch.BotUploader PRIVATE "header/pch.h")
target_include_directories(GuildWarsPartySearch.BotUploader PUBLIC "header")

target_link_libraries(GuildWarsPartySearch.BotUploader PUBLIC
    gwca
	directx
    minhook
    winhttp)