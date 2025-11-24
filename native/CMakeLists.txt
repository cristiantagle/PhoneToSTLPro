cmake_minimum_required(VERSION 3.15)
project(PhoneToSTLProNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(MSVC)
  # Match Open3D static runtime (/MT) to avoid CRT mismatch.
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

# Expected dependency roots (adjust to your environment)
# set(OPEN3D_ROOT "C:/libs/open3d")
# set(LIBIGL_ROOT "C:/libs/libigl")

find_package(Open3D QUIET PATHS ${OPEN3D_ROOT})
find_package(Eigen3 QUIET)

add_library(phonetsl SHARED
  src/reconstruct.cpp
)

target_include_directories(phonetsl
  PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
    ${OPEN3D_INCLUDE_DIRS}
    ${EIGEN3_INCLUDE_DIR}
)

target_link_libraries(phonetsl
  PUBLIC
    Open3D::Open3D
)

target_compile_definitions(phonetsl PRIVATE PHONETSL_EXPORTS)

if(Open3D_FOUND)
  target_compile_definitions(phonetsl PRIVATE HAVE_OPEN3D=1)
endif()
