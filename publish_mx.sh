#!/bin/bash

set -eu

PACK='C:/Users/Aesga/Dropbox/pkg/'

# Build Solution
dotnet build --configuration Release

NAME='AxMx-net7.0'
VERS='1.0'
cd "AxMx/bin/Release/net7.0"
rm -f "${NAME}.tar.xz"
tar cJf "../../../../${NAME}.tar.xz" *
cd "../../../../"
curl -X POST "http://pkg.axfab.net/s/${NAME}/${VERS}" \
    -H "Content-Type: application/tar+xz" \
    --data-binary "@${NAME}.tar.xz"
mv "${NAME}.tar.xz" "${PACK}/${NAME}.tar.xz"


# curl -X POST "http://pkg.axfab.net/s/VpsMx-net7.0/1.0" -H "Content-Type: application/tar+xz" --data-binary "@VpsMx-net7.0.tar.xz"
