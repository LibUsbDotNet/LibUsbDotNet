#!/bin/bash

sudo apt-get install -y wget
wget -q https://packages.microsoft.com/config/ubuntu/$VERSION/packages-microsoft-prod.deb
echo 'deb http://security.ubuntu.com/ubuntu xenial-security main' | sudo tee -a /etc/apt/sources.list
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install -qq -y apt-transport-https libusb-1.0-0-dev
sudo apt-get update -qq
sudo apt-get install -qq -y libicu55 
sudo apt-get install -qq -y dotnet-sdk-2.1