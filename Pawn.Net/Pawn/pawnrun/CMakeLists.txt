cmake_minimum_required(VERSION 3.5)
project(pawnrun)

add_executable(${PROJECT_NAME} src/main.cpp)

target_link_libraries(${PROJECT_NAME} PRIVATE amx64)

if (UNIX)
    target_link_libraries(${PROJECT_NAME} PRIVATE signalmgr)
endif (UNIX)
