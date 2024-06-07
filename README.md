# nodemon
Packet node monitor

## Install .NET on Debian 12 x86-64
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update && sudo apt install -y aspnetcore-runtime-8.0

```

arduino
screen /dev/serial/by-id/usb-1a86_USB_Serial-if00-port0 9600

modems
/dev/serial/by-path/pci-0000:00:14.0-usb-0:3:1.0
/dev/serial/by-path/pci-0000:00:14.0-usb-0:4:1.0

radios
/dev/serial/by-path/pci-0000:00:14.0-usb-0:1.2:1.0-port0
/dev/serial/by-path/pci-0000:00:14.0-usb-0:1.3:1.0-port0

```