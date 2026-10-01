ARG BASE=zznty/cringelauncher:preview
FROM ${BASE}

ARG VERSION=0.0.0

LABEL org.opencontainers.image.title="SE Launcher game content" \
      org.opencontainers.image.description="Space Engineers dedicated-server game content" \
      org.opencontainers.image.source="https://github.com/PveTeam/se-launcher" \
      org.opencontainers.image.version="${VERSION}"

COPY depot-filelist.txt depot-filelist-steamclient.txt /usr/local/share/cringe/

RUN --mount=type=cache,id=depotdownloader,target=/cache,sharing=locked \
    dotnet /usr/share/DepotDownloader/DepotDownloader.dll \
        -app 298740 -depot 298741 \
        -dir /cache/game \
        -filelist /usr/local/share/cringe/depot-filelist.txt \
        -max-downloads "$(nproc)" \
 && dotnet /usr/share/DepotDownloader/DepotDownloader.dll \
        -app 1007 -depot 1006 \
        -dir /cache/steamclient \
        -filelist /usr/local/share/cringe/depot-filelist-steamclient.txt \
        -max-downloads "$(nproc)" \
 && mkdir -p /game \
 && model-stripper strip /cache/game --out /game --copy-others --drop-lods \
 && cp -a /cache/steamclient/linux64/. /usr/lib/ \
 && rm -rf /game/.DepotDownloader/staging
