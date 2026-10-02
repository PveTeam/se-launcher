#!/bin/sh
set -eu

export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/xdg}"
install -d -m 700 "$XDG_RUNTIME_DIR"
export WLR_BACKENDS=headless \
       WLR_LIBINPUT_NO_DEVICES=1 \
       WLR_RENDERER=pixman \
       WLR_RENDERER_ALLOW_SOFTWARE=1

labwc -c /etc/labwc/rc.xml &
labwc_pid=$!
app_pid=""
term() { [ -n "$app_pid" ] && kill -TERM "$app_pid" 2>/dev/null || true; }
down() { kill -TERM "$labwc_pid" 2>/dev/null || true; }
trap term TERM INT
trap down EXIT

i=0
while [ ! -S "$XDG_RUNTIME_DIR/wayland-0" ] && [ "$i" -lt 100 ]; do
	sleep 0.1
	i=$((i + 1))
done
if [ ! -S "$XDG_RUNTIME_DIR/wayland-0" ]; then
	echo "cringe-session: labwc never created $XDG_RUNTIME_DIR/wayland-0" >&2
	exit 1
fi
WAYLAND_DISPLAY=wayland-0
export WAYLAND_DISPLAY

"$@" & # the launcher
app_pid=$!

status=0
wait "$app_pid" || status=$?
down
wait "$labwc_pid" 2>/dev/null || true
exit "$status"
