#!/bin/sh

# Exit if any (simple) command fails.
#set -e

# Inspired from https://github.com/JonathanPorta/ci-build

project="FullSerializer"
version=$(cat VERSION.md)

log_dir=$(pwd)/Logs
output_dir=$(pwd)/Output
project_path=$(pwd)
unity=/Applications/Unity/Unity.app/Contents/MacOS/Unity

mkdir $log_dir
mkdir $output_dir

# Modify the directory structure so that we have a clean Unity package export.
mv *.md* Assets/FullSerializer/
mv Assets/FullSerializer/Testing Assets-FullSerializer-Testing
mv Assets/FullSerializer/Testing.meta Assets-FullSerializer-Testing.meta

#### Run tests
#echo "Attempting to run tests for $project"
#$unity \
#  -batchmode \
#  -nographics \
#  -silent-crashes \
#  -logFile $log_dir/unity_tests.log \
#  -projectPath $project_path \
#  -runEditorTests \
#  -editorTestsResultsFile="$log_dir/unity_test_results.xml"

#### Build DLL export.
echo "Attempting to export $project DLLs"
mv Assets/FullSerializer/Source Assets-FullSerializer-Source
mv Assets/FullSerializer/Source.meta Assets-FullSerializer-Source.meta

dll_file="Assets/FullSerializer/FullSerializer.dll" \
  doc_file="Assets/FullSerializer/FullSerializer.xml" \
  source_dir="Assets-FullSerializer-Source/" ./Scripts/make_dlls.sh
$unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $log_dir/unity_export_package_dll.log \
  -projectPath $project_path \
  -exportPackage "Assets/FullSerializer" "$output_dir/FullSerializer-DLLs.unitypackage" \
  -quit

rm Assets/FullSerializer/FullSerializer.dll
rm Assets/FullSerializer/FullSerializer.dll.meta
rm Assets/FullSerializer/FullSerializer.dll.mdb
rm Assets/FullSerializer/FullSerializer.dll.mdb.meta
rm Assets/FullSerializer/FullSerializer.xml
rm Assets/FullSerializer/FullSerializer.xml.meta

mv Assets-FullSerializer-Source Assets/FullSerializer/Source
mv Assets-FullSerializer-Source.meta Assets/FullSerializer/Source.meta

#### Build Source export.
echo "Attempting to export $project Source"
$unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $log_dir/unity_export_package_source.log \
  -projectPath $project_path \
  -exportPackage "Assets/FullSerializer" "$output_dir/FullSerializer-Source.unitypackage" \
  -quit

# Restore directory structure.
mv Assets/FullSerializer/*.md* .
mv Assets-FullSerializer-Testing Assets/FullSerializer/Testing
mv Assets-FullSerializer-Testing.meta Assets/FullSerializer/Testing.meta

exit

# -executeMethod UnityTest.Batch.RunUnitTests \
#  -executeMethod FullSerializer.fsContinuousIntegration.ExportPackages \

echo "Attempting to build $project for Windows"
$unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildWindowsPlayer "$(pwd)/Build/windows/$project.exe" \
  -quit

echo "Attempting to build $project for OS X"
$unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildOSXUniversalPlayer "$(pwd)/Build/osx/$project.app" \
  -quit

echo "Attempting to build $project for Linux"
$unity \
  -batchmode \
  -nographics \
  -silent-crashes 
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildLinuxUniversalPlayer "$(pwd)/Build/linux/$project" \
  -quit

echo 'Logs from build'
cat $(pwd)/unity.log
