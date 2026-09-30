# syntax=docker/dockerfile:1

# Space Engineers dedicated-server game layer.
#
# CringeLauncher derives the game root from the entrypoint argument
# (CringeLauncher.Launcher.Initialize): ExePath = dirname(argv[0]) -> /game/DedicatedServer64,
# RootPath = its parent -> /game, so ContentPath = /game/Content. The server's writable state
# lives in /data (DOTNET_USERDEV_RUNDIR) and the inherited ENTRYPOINT/CMD are kept.
#
# Both downloads go through DepotDownloader into a build cache mounted at /cache, so a rebuild
# only fetches what changed (a second run against the same directory prints "Already have
# manifest ... Total downloaded: 0 bytes") and only re-runs when this file or a filelist changes.
#   /cache/game        Steam app 298740 depot 298741 - the game, copied to /game
#   /cache/steamclient Steam app 1007  depot 1006  - linux64/steamclient.so + libsteamwebrtc.so,
#                      copied to /usr/lib (on LD_LIBRARY_PATH) so SteamAPI_Init finds
#                      steamclient.so instead of looking in ~/.steam/sdk64
#
# What is skipped is defined by docker/depot-filelist.txt (files DepotDownloader's -filelist
# filter is include-only, hence the negative lookahead regexes there): the render/UI assets a
# null-render dedicated server never opens (Content/ShaderCache, Content/Fonts,
# Content/Particles, Content/InventoryScenes), the uploader leftovers in TempContent, the
# bundled Scenarios and QuickStarts worlds, and all CustomWorlds except "Star System" and
# "Empty World". Models, Data (incl. PlanetDataFiles/Prefabs), VoxelMaps, VisualScripts and the
# data definitions stay - the server reads them.

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
 && cp -a /cache/game/. /game/ \
 && cp -a /cache/steamclient/linux64/. /usr/lib/ \
 && rm -rf /game/.DepotDownloader/staging
