FROM archlinux:base

ARG VERSION=0.0.0

LABEL org.opencontainers.image.title="SE Launcher (dedicated server)" \
      org.opencontainers.image.description="Space Engineers dedicated server via the Cringe launcher" \
      org.opencontainers.image.source="https://github.com/PveTeam/se-launcher" \
      org.opencontainers.image.version="${VERSION}"

RUN pacman -Syu --noconfirm --needed \
      labwc \
      wayvnc \
      harfbuzz \
      fontconfig \
      freetype2 \
      hunspell \
      vulkan-icd-loader \
      vulkan-swrast \
      ttf-dejavu \
 && rm -rf /var/cache/pacman/pkg/* /var/lib/pacman/sync/*

COPY CringeLauncher.AppDir/ /

COPY docker/labwc/rc.xml /etc/labwc/rc.xml
COPY docker/session.sh /usr/local/bin/cringe-session

ENV DOTNET_ROOT=/usr/share/dotnet \
    LD_LIBRARY_PATH=/usr/lib \
    DOTNET_USERDEV_RUNDIR=/data \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/usr/share/dotnet

WORKDIR /data
VOLUME ["/data"]

ENTRYPOINT ["/usr/local/bin/cringe-session"]
CMD ["/usr/share/CringeLauncher/CringeBootstrap", "/game/DedicatedServer64/SpaceEngineers.exe", "--dedicated"]
