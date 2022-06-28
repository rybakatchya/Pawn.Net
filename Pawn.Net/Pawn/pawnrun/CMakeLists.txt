cmake_minimum_required(VERSION 3.21)
project(pawnrun)

add_executable(${PROJECT_NAME} src/main.cpp)

target_link_libraries(${PROJECT_NAME} PRIVATE amx64)
