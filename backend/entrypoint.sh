#!/bin/sh
set -e

./migrations-bundle || {
  echo "Миграции завершились с ошибкой, приложение не будет запущено"
  exit 1
}

exec dotnet Faryma.Composer.Api.dll