#!/bin/bash

cd src/
dotnet pack LibUsb.Common/LibUsb.Common.csproj -c Release
dotnet pack LibUsbDotNet/LibUsbDotNet.csproj -c Release
dotnet pack LibUsbDotNet.LibUsb/LibUsbDotNet.LibUsb.csproj -c Release
dotnet pack LibUsbDotNet.Windows/LibUsbDotNet.Windows.csproj -c Release
dotnet test LibUsbDotNet.LibUsb.Tests/LibUsbDotNet.LibUsb.Tests.csproj /p:RuntimeIdentifier=$RID