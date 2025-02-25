#!/bin/bash

echo 'Copyright 2020-2025 dan0v

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
'

ORIGIN="$(pwd)"
cd "../.."

# RUN IN POWERSHELL
dotnet publish -r win-x64 -c Release -p:SelfContained=True -p:IncludeAllContentForSelfExtract=True -p:PublishSingleFile=True -o bin/Release/net8.0/publishWin
cd "$ORIGIN"

APP_NAME="Amplitude Soundboard"
APP_OUTPUT_PATH="Build"
PUBLISH_OUTPUT_DIRECTORY="../../bin/Release/net8.0/publishWin/."
APP_TAR_NAME1="Amplitude_Soundboard_"
APP_TAR_NAME2="win_x86_64"

if [ -d "$APP_OUTPUT_PATH" ]
then
    rm -rf "$APP_OUTPUT_PATH"
fi

mkdir "$APP_OUTPUT_PATH"
mkdir "$APP_OUTPUT_PATH/$APP_NAME"

cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_OUTPUT_PATH/$APP_NAME/"
cp "../../NOTICE.txt" "$APP_OUTPUT_PATH/NOTICE.txt"
cp "../../LICENSE.txt" "$APP_OUTPUT_PATH/LICENSE.txt"

#VERSION=$(cat ../version.txt | sed 's/ *$//g' | sed 's/\r//' | sed ':a;N;$!ba;s/\n//g')
VERSION=""

cd "$APP_OUTPUT_PATH"
zip -r "$APP_TAR_NAME1$VERSION$APP_TAR_NAME2.zip" "$APP_NAME/" "LICENSE.txt" "NOTICE.txt"
mv "$APP_TAR_NAME1$VERSION$APP_TAR_NAME2.zip" ../../"$APP_TAR_NAME1$VERSION$APP_TAR_NAME2.zip"