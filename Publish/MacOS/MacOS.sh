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

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
RID="${1:-osx-arm64}"

case "$RID" in
    osx-arm64)
        APP_TAR_NAME2="macOS_arm64"
        ;;
    osx-x64)
        APP_TAR_NAME2="macOS_x86_64"
        ;;
    *)
        echo "Unsupported macOS RID: $RID"
        exit 1
        ;;
esac

PUBLISH_OUTPUT_SUBDIRECTORY="publishMac-$RID"
cd "$ROOT_DIR"
dotnet publish "$ROOT_DIR/AmplitudeSoundboard.csproj" -r "$RID" -c Release -p:SelfContained=True -o "bin/Release/net10.0/$PUBLISH_OUTPUT_SUBDIRECTORY"

APP_NAME="Amplitude Soundboard.app"
APP_OUTPUT_PATH="$SCRIPT_DIR/Output"
APP_TAR_NAME1="Amplitude_Soundboard_"
PUBLISH_OUTPUT_DIRECTORY="$ROOT_DIR/bin/Release/net10.0/$PUBLISH_OUTPUT_SUBDIRECTORY/."
INFO_PLIST="$SCRIPT_DIR/Info.plist"
ICON_FILE="$SCRIPT_DIR/Icon.icns"
ICON_FILE_NAME="Icon.icns"

if [ -d "$APP_OUTPUT_PATH" ]
then
    rm -rf "$APP_OUTPUT_PATH"
fi

mkdir "$APP_OUTPUT_PATH"
mkdir "$APP_OUTPUT_PATH/$APP_NAME"

mkdir "$APP_OUTPUT_PATH/$APP_NAME/Contents"
mkdir "$APP_OUTPUT_PATH/$APP_NAME/Contents/MacOS"
mkdir "$APP_OUTPUT_PATH/$APP_NAME/Contents/Resources"

VERSION="$(tr -d '\r\n' < "$ROOT_DIR/Publish/version.txt")"

cp "$INFO_PLIST" "$APP_OUTPUT_PATH/$APP_NAME/Contents/InfoTEMP.plist"

# Set version
sed 's/{VERSION}/'"$VERSION"'/g' "$APP_OUTPUT_PATH/$APP_NAME/Contents/InfoTEMP.plist" > "$APP_OUTPUT_PATH/$APP_NAME/Contents/Info.plist"
rm "$APP_OUTPUT_PATH/$APP_NAME/Contents/InfoTEMP.plist"

unzip -o "$ICON_FILE".zip -d "$SCRIPT_DIR"

cp "$ICON_FILE" "$APP_OUTPUT_PATH/$APP_NAME/Contents/Resources/$ICON_FILE_NAME"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_OUTPUT_PATH/$APP_NAME/Contents/MacOS"
chmod +x "$APP_OUTPUT_PATH/$APP_NAME/Contents/MacOS/amplitude_soundboard"
cd "$APP_OUTPUT_PATH"
tar -czvf "$APP_TAR_NAME1$APP_TAR_NAME2.tar.gz" "$APP_NAME/"
mv "$APP_TAR_NAME1$APP_TAR_NAME2.tar.gz" "$ROOT_DIR/Publish/$APP_TAR_NAME1$APP_TAR_NAME2.tar.gz"
