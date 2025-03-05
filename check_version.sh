!/bin/bash

PROJECT_DIR="/root/everydaily.api"

# Remote commit ID'sini almak için
remote_commit=$(git ls-remote https://ibrahimhates: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@github.com/ibrahimhates/EveryDaily.Api.git develop | awk '{print $1}')

# Local commit ID'sini almak için
local_commit=$(git -C "$PROJECT_DIR" rev-parse HEAD)

# Commit ID'lerini karşılaştırma
if [ "$remote_commit" != "$local_commit" ]; then
  echo "Commit ID'leri farklı, script çalıştırılıyor..."

  # Deploy script'ini çalıştır
  ./deploy.sh
else
  echo "Commit ID'leri aynı."
fi

