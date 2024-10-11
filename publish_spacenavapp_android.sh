#!/bin/bash

DIR_PKG='/c/Users/Aesga/Dropbox/pkg/mobile_android'
DIR_SLN='/c/Users/Aesga/develop/dotnet/AxLib'

# API Server
# NAME='SpaceNav-net6.0'
# cd "${DIR_SLN}/FoodyApi"
# dotnet build --configuration Release
# cd "${DIR_SLN}/FoodyApi/bin/Release/net6.0"
# rm -f "../FoodyApi-net6.0.tar.xz"
# cp "${DIR_PKG}/appsettings.json" "./appsettings.json"
# cp "${DIR_PKG}/foody-api.pfx" "./foody-api.pfx"
# tar cJf "../FoodyApi-net6.0.tar.xz" *
# cp "${DIR_SLN}/FoodyApi/bin/Release/${NAME}.tar.xz" "${DIR_PKG}/${NAME}.tar.xz"

# Android
NAME='net.fablogico.spacenav'
cd "${DIR_SLN}/SpaceNavApp"
# Update config
dotnet build --configuration Release -f net7.0-android
cp "${DIR_SLN}/SpaceNavApp/bin/Release/net7.0-android/${NAME}-Signed.apk" "${DIR_PKG}/${NAME}.apk"


# dotnet build --configuration Release -f net6.0-ios
# dotnet build --configuration Release -f net6.0-maccatalyst
# dotnet build --configuration Release -f net6.0-windows10.0.19041.0


# Mongo Indexes:

# TreeIndex           (Filter + Order)
# HashIndex           (Exact match only)
# CounpoundIndex      (Multiple field BTree)
# MultiDimensionalIndex (KDB Tree)
# MultikeyIndex       (For index on array)
# TextIndex           (For text search)

# WildcardIndexes     (Index field of a object field)

# 2DSphereIndex       (Index on latitude and longitude)
# GeoHaystackIndex    (Group document by area)


## ON SERVER
# cd /home/rocky
# rm -f FoodyApi-net6.0.tar.xz
# rm -rf FoodyApi
# wget https://www.dropbox.com/s/k8lyz3tlknpuu6v/FoodyApi-net6.0.tar.xz
# mkdir FoodyApi
# cd /home/rocky/FoodyApi
# tar xf ../FoodyApi-net6.0.tar.xz
#
# cd /home/rocky/FoodyApi
# sudo ASPNETCORE_ENVIRONMENT=Development dotnet FoodyApi.dll

