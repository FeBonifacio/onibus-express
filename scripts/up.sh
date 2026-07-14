#!/usr/bin/env bash
#
# up.sh - sobe TODO o ambiente com um comando (Docker), com feedback animado.
#
# Responsabilidade: orquestrar o Docker Compose e reportar o progresso do boot
# (docker -> banco -> backend -> frontend) usando a animacao de scripts/lib/ui.sh.
#
# Uso:
#   scripts/up.sh            # sobe via Docker Compose e acompanha os logs
#   scripts/up.sh --demo     # so mostra a animacao (dados simulados), sem Docker
#   scripts/up.sh --plain    # sem animacao (saida em linhas simples / CI)
#   scripts/up.sh --no-logs  # nao segue os logs apos ficar pronto
#
# Detecta automaticamente `docker compose` (v2) ou `docker-compose` (v1/standalone).

set -uo pipefail

# --- localiza a raiz do repo e carrega a lib ---
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
# shellcheck source=scripts/lib/ui.sh
source "$SCRIPT_DIR/lib/ui.sh"
cd "$ROOT_DIR"

# versao automatica (derivada do git; muda a cada commit)
APP_VERSION="$(bash "$SCRIPT_DIR/version.sh" --short 2>/dev/null)"

# --- flags ---
DEMO=0; FOLLOW_LOGS=1
for arg in "$@"; do
  case "$arg" in
    --demo)    DEMO=1 ;;
    --plain)   export UI_PLAIN=1 ;;
    --no-logs) FOLLOW_LOGS=0 ;;
    -h | --help)
      grep -E '^#( |$)' "${BASH_SOURCE[0]}" | sed -E 's/^# ?//'; exit 0 ;;
    *) ui_warn "flag desconhecida: $arg" ;;
  esac
done

# --- endpoints de readiness (defaults; serao fixados quando o compose existir, Fase 3) ---
API_HEALTH_URL="${API_HEALTH_URL:-http://localhost:8080/health}"
WEB_URL="${WEB_URL:-http://localhost:8081}"
DB_HOST="${DB_HOST:-localhost}"; DB_PORT="${DB_PORT:-5432}"

# --- helpers de checagem ---
http_ok() { curl -fsS -o /dev/null --max-time 2 "$1" >/dev/null 2>&1; }
tcp_ok()  { (exec 3<>"/dev/tcp/$1/$2") >/dev/null 2>&1 && exec 3>&- 2>/dev/null; }

# ===========================================================================
# Modo DEMO (ou quando ainda nao ha docker-compose.yml): so a animacao
# ===========================================================================
run_demo() {
  ui_title "OniBus Express ${APP_VERSION} — preview do boot (demo)"
  svc_reset
  svc_add docker   "docker"
  svc_add db       "banco"
  svc_add backend  "backend"
  svc_add frontend "frontend"

  # Cronograma simulado (em "frames"): cada servico entra em run e depois ok.
  demo_probe() {
    local f=$1
    [ "$f" -ge 2  ] && svc_set docker run
    [ "$f" -ge 10 ] && { svc_set docker ok;  svc_set db run; }
    [ "$f" -ge 20 ] && { svc_set db ok;      svc_set backend run; }
    [ "$f" -ge 34 ] && { svc_set backend ok; svc_set frontend run; }
    [ "$f" -ge 46 ] && svc_set frontend ok
  }
  UI_PROBE_EVERY=1 ui_animate demo_probe 60
  ui_ok "ambiente pronto (demo) — bora rodar! 🚌💨"
}

# ===========================================================================
# Modo real (Docker Compose)
# ===========================================================================
COMPOSE=""
compose() { $COMPOSE "$@"; }

cleanup_boot() { ui_commit_scene; ui_warn "boot interrompido — derrubando containers..."; compose down >/dev/null 2>&1; exit 130; }

run_docker() {
  ui_title "OniBus Express ${APP_VERSION} — subindo (Docker)"
  ui_info "usando: ${C_BOLD}${COMPOSE}${C_RST}"

  svc_reset
  svc_add docker   "docker"
  svc_add db       "banco"
  svc_add backend  "backend"
  svc_add frontend "frontend"

  # Fase 1 do boot: build + subir em background. Ctrl-C aqui = down limpo.
  trap cleanup_boot INT
  svc_set docker run
  if ! compose up --build -d; then
    svc_set docker fail
    ui_err "falha ao subir os containers (veja a saida acima)"
    exit 1
  fi
  svc_set docker ok

  # Probe real: banco (TCP), backend (/health), frontend (HTTP).
  real_probe() {
    [ "$(svc_state db)" = ok ]       || { svc_set db run;       tcp_ok  "$DB_HOST" "$DB_PORT" && svc_set db ok; }
    [ "$(svc_state backend)" = ok ]  || { svc_set backend run;  http_ok "$API_HEALTH_URL"     && svc_set backend ok; }
    [ "$(svc_state frontend)" = ok ] || { svc_set frontend run; http_ok "$WEB_URL"             && svc_set frontend ok; }
  }
  ui_animate real_probe "${BOOT_TIMEOUT:-180}"
  trap - INT

  if svc_all_settled && [ "$(svc_ready_count)" -eq "${#SVC_KEY[@]}" ]; then
    ui_ok "tudo no ar 🚌💨"
    ui_info "API:      ${C_BOLD}${API_HEALTH_URL%/health}${C_RST}"
    ui_info "Frontend: ${C_BOLD}${WEB_URL}${C_RST}"
  else
    ui_warn "alguns servicos nao responderam a tempo; veja os logs abaixo"
  fi

  if [ "$FOLLOW_LOGS" -eq 1 ]; then
    ui_info "acompanhando logs (Ctrl-C encerra o acompanhamento; containers seguem no ar — use ${C_BOLD}make down${C_RST})"
    compose logs -f
  fi
}

# ===========================================================================
# Entrada
# ===========================================================================
if [ "$DEMO" -eq 1 ]; then
  run_demo
  exit 0
fi

if ! COMPOSE="$(ui_detect_compose)"; then
  ui_err "Docker Compose nao encontrado (nem 'docker compose' nem 'docker-compose')."
  ui_info "Instale o Docker Compose ou rode em modo demo: ${C_BOLD}scripts/up.sh --demo${C_RST}"
  exit 1
fi

if [ ! -f "$ROOT_DIR/docker-compose.yml" ] && [ ! -f "$ROOT_DIR/compose.yml" ]; then
  ui_warn "ainda nao existe docker-compose.yml (chega na Fase 3 — Infra/API)."
  ui_info "mostrando a previa da experiencia de boot em modo demo:"
  run_demo
  exit 0
fi

run_docker
