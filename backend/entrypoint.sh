#!/bin/sh
set -e
chmod +x ./migrations-bundle
./migrations-bundle
exec dotnet Faryma.Composer.Api.dll