#!/bin/sh

# This script will install a dummy USB device, which we can use in our tests.
# Ubunto doesn't ship with the dummy_hcd kernel module, so we need to build it
# from source.

# Get the source for the Linux kernel
echo "deb-src http://archive.ubuntu.com/ubuntu/ trusty main restricted" | sudo tee --append /etc/apt/sources.list > /dev/null
sudo apt-get update -qq
apt-get source -qq linux-image-$(uname -r) 

# Get dependencies required to build and load the library
sudo apt-get install -qq -y libusb-1.0-0-dev usbutils linux-headers-$(uname -r) dkms dpkg-dev

# Prepare the driver source folder
cp linux-*/drivers/usb/gadget/udc/dummy_hcd.c ci/dummy_hcd-0.1/
sudo cp -r ci/dummy_hcd-0.1 /usr/src/

# Build the kernel module
sudo dkms add -m dummy_hcd -v 0.1
sudo dkms build -m dummy_hcd -v 0.1
sudo dkms install -m dummy_hcd -v 0.1
cat /var/lib/dkms/dummy_hcd/0.1/build/make.log

# Install the root controller
sudo modprobe dummy_hcd

# Create a fake USB mass storage device
dd bs=1M count=64 if=/dev/zero of=$TRAVIS_BUILD_DIR/ci/disk.img
sudo modprobe g_mass_storage file=$TRAVIS_BUILD_DIR/ci/disk.img stall=0

# Wait for the device to come online
echo "Waiting for the mass storage device to come online"
until [ $(lsusb | wc -l) -eq "2" ]; do
  echo '.'
  sleep 1s
done

# Install dependencies
sudo apt-get update
sudo apt-get install -y debootstrap qemu-user-static schroot

# Bootstrap an amd64 environment
sudo qemu-debootstrap --arch=$TARGET_ARCH $RELEASE $RID
echo "[$RID]
description=Ubuntu $RELEASE ($TARGET_ARCH)
directory=$(pwd)/$RID
root-users=$(whoami)
users=$(whoami)
type=directory" | sudo tee /etc/schroot/chroot.d/$RID

# Install the required dependencies
schroot -c $RID -u root -d / -- VERSION=$VERSION; RID=$RID; echo Running on version $VERSION with rid $RID; ci/install-dependencies.sh