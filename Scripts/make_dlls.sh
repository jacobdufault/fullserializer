#!/bin/sh

# These options are configurable. You can configure them before invoking the
# script by using environment variables. For example,
#
#   dll_file="MyProj.dll" doc_file="MyProj.xml" ./Scripts/make_dlls.sh
#
unity_root=${unity_root:-"/Applications/Unity/Unity.app/Contents/Frameworks"}
source_dir=${source_dir:-"../Assets/FullSerializer/Source"}
dll_file=${dll_file:-"FullSerializer.dll"}
doc_file=${doc_file:-"FullSerializer.xml"}

all_cs_files=$(find $source_dir -name \*.cs)

echo "Compiling DLLs (dll_file: $dll_file, doc_file: $doc_file)"
$unity_root/MonoBleedingEdge/bin/mcs \
  /lib:$unity_root/Managed /reference:UnityEngine.dll \
  /nowarn:1591 \
  /target:library /debug /sdk:2 \
  /out:$dll_file /doc:$doc_file \
  $all_cs_files
