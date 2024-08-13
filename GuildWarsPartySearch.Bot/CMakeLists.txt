cmake_minimum_required(VERSION 3.5)

set(CMAKE_CXX_FLAGS_DEBUG "" CACHE STRING "CMAKE_CXX_FLAGS_DEBUG" FORCE)
set(CMAKE_POSITION_INDEPENDENT_CODE ON CACHE BOOL "CMAKE_POSITION_INDEPENDENT_CODE" FORCE)
set(BUILD_SHARED_LIBS OFF CACHE BOOL "BUILD_SHARED_LIBS" FORCE)
set(CMAKE_BUILD_TYPE Debug CACHE STRING "CMAKE_BUILD_TYPE" FORCE)

set(BUILD_TESTING_STATIC OFF CACHE BOOL "BUILD_TESTING_STATIC" FORCE)
set(BUILD_TESTING_SHARED OFF CACHE BOOL "BUILD_TESTING_SHARED" FORCE)
set(BUILD_TESTING OFF CACHE BOOL "BUILD_TESTING" FORCE)

project(
  GuildWarsPartySearch.Bot
  VERSION 1.0
)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED YES)

if (MSVC)
  # Add this line to allow keyword macros
  add_definitions(-D_ALLOW_KEYWORD_MACROS)

  # Ensure consistent runtime libraries
  set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MDd /ZI")
  set(CMAKE_C_FLAGS_DEBUG "${CMAKE_C_FLAGS_DEBUG} /MDd /ZI")
  set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} /NODEFAULTLIB:MSVCRT")
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /NODEFAULTLIB:MSVCRT")
  set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} /NODEFAULTLIB:MSVCRT")
endif()

add_subdirectory(${CMAKE_CURRENT_SOURCE_DIR}/Dependencies/Headquarter)

# Look for required libraries
add_library(
  GuildWarsPartySearch.Bot

  SHARED

  main.cpp
  Dependencies/easywsclient/easywsclient.cpp
)

target_link_libraries(GuildWarsPartySearch.Bot PRIVATE common client mbedtls mbedx509 mbedcrypto)

set_target_properties(common
    PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
)

set_target_properties(portal
    PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
)
set_target_properties(client
    PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
)
set_target_properties(GuildWarsPartySearch.Bot
    PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/"
)

target_compile_definitions(GuildWarsPartySearch.Bot PRIVATE HEADQUARTER_BUILD_EXPORTS)

if (MSVC)
    target_compile_options(GuildWarsPartySearch.Bot PRIVATE /wd4267 /wd4996)
endif()

target_include_directories(
  GuildWarsPartySearch.Bot

  PUBLIC
  ${CMAKE_CURRENT_SOURCE_DIR}/Dependencies/Headquarter/include
  ${CMAKE_CURRENT_SOURCE_DIR}/Dependencies/easywsclient
  ${CMAKE_CURRENT_SOURCE_DIR}/Dependencies/json
)

file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/Debug/logs)