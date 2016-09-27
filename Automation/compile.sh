ROOT_DIR=$(pwd)
WORK_DIR="Output"
SOURCE_DIR="$ROOT_DIR/Assets/FullSerializer"
UNITY_DLL_FOLDER="$ROOT_DIR/Automation"

# Output DLL names.
OUTPUT_NAME=FullSerializer

mkdir "$WORK_DIR"
cd "$WORK_DIR"

# Fetch sources to use.
ALL_SOURCES=$(find $SOURCE_DIR -name \*.cs | grep -v 'Editor/')

# Common compilation settings.
COMPILER="mcs
  /lib:$UNITY_DLL_FOLDER
  /reference:UnityEngine.dll
  /reference:UnityEngine.UI.dll
  /nowarn:1591
  /target:library /debug /sdk:2"

# Compile runtime and editor DLLs.
$COMPILER \
  /out:"$OUTPUT_NAME.dll" /doc:"$OUTPUT_NAME.xml" \
  $ALL_SOURCES

zip -r "$ROOT_DIR/FullSerializer-DLLs.zip" $(find . -type f)

cd $ROOT_DIR/Assets
zip -r "$ROOT_DIR/FullSerializer-Source.zip" $(find FullSerializer -name \*.cs)

cd $ROOT_DIR
rm -rf "$WORK_DIR"
