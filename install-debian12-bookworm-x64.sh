#!/bin/sh -e

sudo apt-get update
sudo apt-get install wget -y

wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

cd src
dotnet build
cd ..

sudo mkdir /opt/nodemon
sudo mv nodemon/bin/Debug/net8.0/* /opt/nodemon/
