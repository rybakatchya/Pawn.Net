cmake_minimum_required(VERSION 3.5)
project(signalmgr)

add_library(${PROJECT_NAME} SHARED
        src/signalmgr.cpp
        )

target_include_directories(${PROJECT_NAME} PUBLIC ${PROJECT_SOURCE_DIR}/include)
target_include_directories(${PROJECT_NAME} PRIVATE ${PROJECT_SOURCE_DIR}/src)

target_link_libraries(${PROJECT_NAME} PRIVATE dl)
