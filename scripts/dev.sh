#!/usr/bin/env bash
#
# dev.sh - sobe backend + frontend LOCAIS (sem Docker) com um comando.
#
# Responsabilidade: iniciar a API (.NET watch) e o front (Vite) em paralelo,
# mostrar o progresso (agora backend / agora frontend) com a animacao de
# scripts/lib/ui.sh e, quando prontos, seguir os logs de ambos.
#
# Uso:
#   scripts/dev.sh          # sobe os dois em paralelo e acompanha os logs
#   scripts/dev.sh --plain  # sem animacao (linhas simples)
#   scripts/dev.sh --demo   # so a previa da animacao (nao inicia nada)
#
# Para rodar SEPARADO, use os alvos do Makefile: `make back` e `make front`.

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
# shellcheck source=scripts/lib/ui.sh
source "$SCRIPT_DIR/lib/ui.sh"
cd "$ROOT_DIR"

# versao automatica (derivada do git; muda a cada commit)
APP_VERSION="$(bash "$SCRIPT_DIR/version.sh" --short 2>/dev/null)"

# --- flags ---
DEMO=0
for arg in "$@"; do
  case "$arg" in
    --demo)  DEMO=1 ;;
    --plain) export UI_PLAIN=1 ;;
    -h | --help) grep -E '^#( |$)' "${BASH_SOURCE[0]}" | sed -E 's/^# ?//'; exit 0 ;;
    *) ui_warn "flag desconhecida: $arg" ;;
  esac
done

# --- config (portas de dev; sobrescreviveis por env) ---
API_PROJECT="${API_PROJECT:-backend/src/OnibusExpress.Api}"
FRONTEND_DIR="${FRONTEND_DIR:-frontend}"
API_DEV_PORT="${API_DEV_PORT:-5000}"
WEB_DEV_PORT="${WEB_DEV_PORT:-5173}"

tcp_ok() { (exec 3<>"/dev/tcp/localhost/$1") >/dev/null 2>&1 && exec 3>&- 2>/dev/null; }

# --- preview / demo ---
run_demo() {
  ui_title "OniBus Express ${APP_VERSION} — preview local (demo)"
  svc_reset; svc_add backend "backend"; svc_add frontend "frontend"
  demo_probe() {
    local f=$1
    [ "$f" -ge 2  ] && svc_set backend run
    [ "$f" -ge 16 ] && { svc_set backend ok; svc_set frontend run; }
    [ "$f" -ge 30 ] && svc_set frontend ok
  }
  UI_PROBE_EVERY=1 ui_animate demo_probe 45
  ui_ok "ambiente local pronto (demo) 🚌💨"
}

if [ "$DEMO" -eq 1 ]; then run_demo; exit 0; fi

# --- descobre o que existe para rodar (degrada com elegancia) ---
HAS_BACK=0; HAS_FRONT=0
[ -d "$ROOT_DIR/$API_PROJECT" ] && HAS_BACK=1
[ -f "$ROOT_DIR/$FRONTEND_DIR/package.json" ] && HAS_FRONT=1

if [ "$HAS_BACK" -eq 0 ] && [ "$HAS_FRONT" -eq 0 ]; then
  ui_warn "backend e frontend ainda nao existem (chegam nas Fases 2-4)."
  ui_info "mostrando a previa da experiencia de boot local:"
  run_demo
  exit 0
fi

# --- prepara logs e limpeza ---
LOG_DIR="$(mktemp -d "${TMPDIR:-/tmp}/onibus-dev.XXXXXX")"
PIDS=()

stop_all() {
  ui_commit_scene
  ui_info "encerrando processos de dev..."
  local pid
  for pid in "${PIDS[@]:-}"; do
    [ -n "$pid" ] || continue
    pkill -P "$pid" >/dev/null 2>&1
    kill "$pid" >/dev/null 2>&1
  done
  rm -rf "$LOG_DIR"
  exit 0
}
trap stop_all INT TERM

ui_title "OniBus Express ${APP_VERSION} — subindo local"
svc_reset
[ "$HAS_BACK" -eq 1 ]  && svc_add backend  "backend"
[ "$HAS_FRONT" -eq 1 ] && svc_add frontend "frontend"

# --- inicia os processos (logs vao para arquivos; a tela fica para a animacao) ---
if [ "$HAS_BACK" -eq 1 ]; then
  ( cd "$ROOT_DIR/$API_PROJECT" && exec dotnet watch run ) >"$LOG_DIR/backend.log" 2>&1 &
  PIDS+=("$!"); svc_set backend run
else
  ui_warn "backend nao encontrado em $API_PROJECT — pulando"
fi

if [ "$HAS_FRONT" -eq 1 ]; then
  ( cd "$ROOT_DIR/$FRONTEND_DIR" && exec npm run dev ) >"$LOG_DIR/frontend.log" 2>&1 &
  PIDS+=("$!"); svc_set frontend run
else
  ui_warn "frontend nao encontrado em $FRONTEND_DIR — pulando"
fi

# --- aguarda as portas responderem, animando ---
dev_probe() {
  if [ "$HAS_BACK" -eq 1 ] && [ "$(svc_state backend)" != ok ]; then
    tcp_ok "$API_DEV_PORT" && svc_set backend ok
  fi
  if [ "$HAS_FRONT" -eq 1 ] && [ "$(svc_state frontend)" != ok ]; then
    tcp_ok "$WEB_DEV_PORT" && svc_set frontend ok
  fi
}
ui_animate dev_probe "${DEV_TIMEOUT:-120}"

[ "$HAS_BACK" -eq 1 ]  && ui_info "API:      ${C_BOLD}http://localhost:${API_DEV_PORT}${C_RST}"
[ "$HAS_FRONT" -eq 1 ] && ui_info "Frontend: ${C_BOLD}http://localhost:${WEB_DEV_PORT}${C_RST}"
ui_info "seguindo logs (Ctrl-C encerra tudo)"

# --- segue os logs dos processos ate o usuario encerrar ---
tail -n +1 -f "$LOG_DIR"/*.log &
PIDS+=("$!")
wait
