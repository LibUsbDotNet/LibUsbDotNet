#!/bin/bash

cd src/
dotnet pack LibUsbDotNet/LibUsbDotNet.csproj -c Release
dotnet test LibUsbDotNet.Tests/LibUsbDotNet.Tests.csproj /p:RuntimeIdentifier=$RID