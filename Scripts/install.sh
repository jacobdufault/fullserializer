#!/bin/sh

# Inspired from https://github.com/JonathanPorta/ci-build

# This link changes from time to time. I haven't found a reliable hosted
# installer package for doing regular installs like this. You will probably need
# to grab a current link from: http://unity3d.com/get-unity/download/archive

URL=$"http://netstorage.unity3d.com/unity/cc9cbbcc37b4/MacEditorInstaller/Unity-5.3.1f1.pkg"
echo "Downloading from $URL"
curl -o Unity.pkg $URL

echo "Installing Unity.pkg"
sudo installer -dumplog -package Unity.pkg -target /
