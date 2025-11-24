#!/usr/bin/env bash
set -euo pipefail

OPEN3D_ROOT=${OPEN3D_ROOT:-/opt/open3d}
BUILD_DIR=${BUILD_DIR:-build}

mkdir -p "$BUILD_DIR"
cd "$BUILD_DIR"

cmake .. \
  -DOPEN3D_ROOT="$OPEN3D_ROOT" \
  -DCMAKE_BUILD_TYPE=Release

cmake --build . --config Release
