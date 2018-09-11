#!/bin/bash

cd src/
dotnet pack LibUsbDotNet/LibUsbDotNet.csproj -c Release
dotnet test LibUsbDotNet.LibUsb.Tests/LibUsbDotNet.LibUsb.Tests.csproj /p:RuntimeIdentifier=$RID