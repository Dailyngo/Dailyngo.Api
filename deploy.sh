#!/bin/bash

# Eğer 'everydaily.api' klasörü varsa, yedeğini al
if [ -d everydaily.api ]; then
    cp -r everydaily.api everydaily.api.bak
    echo "Yedekleme tamamlandı: everydaily.api.bak"
else
    echo "everydaily.api klasörü bulunamadı, yedekleme yapılmadı."
fi

# 'everydaily.api' dosyasını sil
rm -rf everydaily.api
echo "everydaily.api dosyası silindi."

# Git reposunu klonla
git clone https://ibrahimhates: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@github.com/ibrahimhates/EveryDaily.Api.git --branch=develop everydaily.api
echo "Git reposu 'develop' branch'ı ile klonlandı."

docker compose -f everydaily.api/docker-compose-services.yml down
docker builder prune -f
docker container prune -f
docker compose -f everydaily.api/docker-compose-services.yml up -d
