#!/usr/bin/env bash
#
# ui.sh - biblioteca de apresentacao/orquestracao compartilhada (OniBus Express).
#
# Responsabilidade unica (SRP): tudo que e "como mostrar" fica aqui -
# cores, deteccao do Docker Compose, feedback de estagio e a animacao do onibus.
# Os scripts up.sh / dev.sh cuidam do "o que fazer" e usam estas funcoes (DRY).
#
# Uso:  source "scripts/lib/ui.sh"
#
# Convencoes:
#   - Nao usa `set -e` (o chamador decide); funcoes retornam status previsivel.
#   - Degrada com elegancia: sem TTY, NO_COLOR ou UI_PLAIN=1 -> saida em linhas simples.

# ---------------------------------------------------------------------------
# Capacidades do terminal
# ---------------------------------------------------------------------------

# ui_tty: verdadeiro quando podemos repintar uma regiao (stdout e um terminal
# interativo e o modo simples nao foi forcado).
ui_tty() { [ -t 1 ] && [ "${UI_PLAIN:-0}" != "1" ]; }

# Detecta suporte a UTF-8 para escolher glifos (braille/blocos) x ASCII.
case "${LC_ALL:-${LC_CTYPE:-${LANG:-}}}" in
  *UTF-8* | *utf8* | *UTF8*) UI_UTF8=1 ;;
  *) UI_UTF8=0 ;;
esac

# Cores: ativas apenas em TTY e sem NO_COLOR.
if ui_tty && [ -z "${NO_COLOR:-}" ]; then
  C_RST=$'\033[0m'; C_DIM=$'\033[2m'; C_BOLD=$'\033[1m'
  C_RED=$'\033[31m'; C_GRN=$'\033[32m'; C_YEL=$'\033[33m'
  C_BLU=$'\033[34m'; C_CYN=$'\033[36m'
else
  C_RST=''; C_DIM=''; C_BOLD=''
  C_RED=''; C_GRN=''; C_YEL=''; C_BLU=''; C_CYN=''
fi

# ---------------------------------------------------------------------------
# Mensagens
# ---------------------------------------------------------------------------
ui_info()  { printf '%s\n' "${C_CYN}·${C_RST} $*"; }
ui_ok()    { printf '%s\n' "${C_GRN}✓${C_RST} $*"; }
ui_warn()  { printf '%s\n' "${C_YEL}!${C_RST} $*"; }
ui_err()   { printf '%s\n' "${C_RED}✗${C_RST} $*" >&2; }
ui_title() { printf '\n%s\n' "${C_BOLD}${C_CYN}🚌 $*${C_RST}"; }

# ui_stage: marco do boot ("agora docker / agora backend / agora frontend").
ui_stage() { printf '%s\n' "${C_BOLD}${C_BLU}▶${C_RST} agora: ${C_BOLD}$*${C_RST}"; }

# ---------------------------------------------------------------------------
# Deteccao do Docker Compose (v2 plugin x v1/standalone) — portabilidade
# ---------------------------------------------------------------------------
# Ecoa o comando utilizavel ("docker compose" ou "docker-compose").
# Retorna 1 (silencioso) se nenhum estiver disponivel.
ui_detect_compose() {
  if docker compose version >/dev/null 2>&1; then
    printf 'docker compose'
  elif command -v docker-compose >/dev/null 2>&1; then
    printf 'docker-compose'
  else
    return 1
  fi
}

# ---------------------------------------------------------------------------
# Modelo de servicos (arrays paralelos indexados)
#   SVC_KEY[i]   -> chave interna (docker, db, backend, frontend)
#   SVC_LABEL[i] -> rotulo exibido
#   SVC_STATE[i] -> pending | run | ok | fail
# ---------------------------------------------------------------------------
SVC_KEY=(); SVC_LABEL=(); SVC_STATE=()

svc_reset() { SVC_KEY=(); SVC_LABEL=(); SVC_STATE=(); }

svc_add() { # svc_add <key> <label>
  SVC_KEY+=("$1"); SVC_LABEL+=("$2"); SVC_STATE+=("pending")
}

svc_index() { # svc_index <key> -> ecoa o indice ou vazio
  local i
  for i in "${!SVC_KEY[@]}"; do
    [ "${SVC_KEY[$i]}" = "$1" ] && { printf '%s' "$i"; return 0; }
  done
  return 1
}

svc_set() { # svc_set <key> <state>
  local i; i=$(svc_index "$1") || return 0
  SVC_STATE[$i]="$2"
}

svc_state() { local i; i=$(svc_index "$1") || return 1; printf '%s' "${SVC_STATE[$i]}"; }

# Concluido quando nenhum servico esta pending/run (todos ok ou fail).
svc_all_settled() {
  local s
  for s in "${SVC_STATE[@]}"; do
    case "$s" in pending | run) return 1 ;; esac
  done
  return 0
}

svc_ready_count() {
  local s n=0
  for s in "${SVC_STATE[@]}"; do [ "$s" = "ok" ] && n=$((n + 1)); done
  printf '%s' "$n"
}

# ---------------------------------------------------------------------------
# Peças da animacao: onibus fixo + estrada rolando
# ---------------------------------------------------------------------------
UI_SCENE_HEIGHT=8   # nº de linhas repintadas (mantenha em sincronia com ui_scene)
UI_DREW=0

# Peças fixas do onibus (larguras casadas: 32 = corpo interno).
UI_BUS_BAR="________________________________"  # 32 chars
UI_BUS_AXLE="========================"          # 24 chars (entre as rodas)

# Spinner (braille em UTF-8, ASCII caso contrario).
if [ "$UI_UTF8" = "1" ]; then UI_SPIN='⣾⣽⣻⢿⡿⣟⣯⣷'; else UI_SPIN='|/-\'; fi
ui_spin() { local f=$1 n=${#UI_SPIN}; printf '%s' "${UI_SPIN:$((f % n)):1}"; }

# Estrada rolando: janela deslizante sobre um padrao repetido.
ui_road() { # ui_road <width> <offset>
  local width=$1 offset=$2 base full start
  if [ "$UI_UTF8" = "1" ]; then base='═·═─═·═╌═·═▪═·═─═'; else base='=-=.=-=_=-=*=-=.='; fi
  full="${base}${base}${base}${base}${base}"
  start=$(( offset % ${#base} ))
  printf '%s' "${full:start:width}"
}

# Barra de progresso.
ui_bar() { # ui_bar <ready> <total> <width>
  local ready=$1 total=$2 width=${3:-14} filled i out=''
  if [ "$total" -gt 0 ]; then filled=$(( ready * width / total )); else filled=0; fi
  local ch_on ch_off
  if [ "$UI_UTF8" = "1" ]; then ch_on='█'; ch_off='░'; else ch_on='#'; ch_off='-'; fi
  for ((i = 0; i < width; i++)); do
    if [ "$i" -lt "$filled" ]; then out+="$ch_on"; else out+="$ch_off"; fi
  done
  printf '%s' "$out"
}

# Onibus em ASCII (4 linhas). As rodas alternam para dar sensacao de rolagem.
ui_bus_lines() { # ui_bus_lines <frame> -> imprime 4 linhas
  local frame=$1 wl wr
  if [ $((frame % 2)) -eq 0 ]; then wl='(O)'; wr='(O)'; else wl='(o)'; wr='(o)'; fi
  local left=" ${C_CYN}OniBus Express${C_RST}"      # 15 col visiveis
  local right="${C_DIM}[o][o][o]${C_RST} "          # 10 col visiveis
  printf '%s\n' "  ${C_YEL}.${UI_BUS_BAR}.${C_RST}"
  printf '%s\n' "  ${C_YEL}|${C_RST}${left}       ${right}${C_YEL}|${C_RST}"   # 15+7+10=32
  printf '%s\n' "  ${C_YEL}|${UI_BUS_BAR}|${C_RST}"
  printf '%s\n' "     ${C_BOLD}${wl}${C_RST}${UI_BUS_AXLE}${C_BOLD}${wr}${C_RST}"
}

# Linha de status por servico: "docker ✓  db ✓  api ⣾  web ·".
ui_status_line() { # ui_status_line <frame>
  local frame=$1 spin i glyph out=''
  spin=$(ui_spin "$frame")
  for i in "${!SVC_KEY[@]}"; do
    case "${SVC_STATE[$i]}" in
      ok)   glyph="${C_GRN}✓${C_RST}" ;;
      run)  glyph="${C_YEL}${spin}${C_RST}" ;;
      fail) glyph="${C_RED}✗${C_RST}" ;;
      *)    glyph="${C_DIM}·${C_RST}" ;;
    esac
    out+="${SVC_LABEL[$i]} ${glyph}   "
  done
  printf '%s' "$out"
}

# Desenha a cena inteira, repintando a regiao fixa (apenas em TTY).
ui_scene() { # ui_scene <frame>
  ui_tty || return 0
  local frame=$1 total ready pct road bar
  total=${#SVC_KEY[@]}; ready=$(svc_ready_count)
  if [ "$total" -gt 0 ]; then pct=$(( ready * 100 / total )); else pct=0; fi
  road=$(ui_road 40 "$frame"); bar=$(ui_bar "$ready" "$total" 14)

  # Se ja desenhamos antes, sobe para o topo da regiao.
  [ "$UI_DREW" -eq 1 ] && printf '\033[%dA' "$UI_SCENE_HEIGHT"

  {
    printf '\033[2K%s\n' "  ${C_DIM}Linha OniBus Express${C_RST}"
    printf '\033[2K%s\n' ""
    # 5 linhas do onibus
    while IFS= read -r l; do printf '\033[2K%s\n' "$l"; done < <(ui_bus_lines "$frame")
    printf '\033[2K%s\n' "  ${C_DIM}${road}${C_RST}"
    printf '\033[2K%s\n' ""
    printf '\033[2K%s\n' "  ${C_GRN}${bar}${C_RST} ${C_BOLD}${pct}%${C_RST}   $(ui_status_line "$frame")"
  }
  UI_DREW=1
}

# Encerra a regiao animada, "fixando" o que estiver acima (log persistente).
# Use antes de imprimir uma linha que deve permanecer na tela.
ui_commit_scene() {
  if ui_tty && [ "$UI_DREW" -eq 1 ]; then
    printf '\033[%dA\033[J' "$UI_SCENE_HEIGHT"  # sobe e limpa da regiao ate o fim
    UI_DREW=0
  fi
}

# ---------------------------------------------------------------------------
# Loop de animacao dirigido por uma funcao de "probe"
# ---------------------------------------------------------------------------
# ui_animate <probe_fn> [timeout_seg]
#   <probe_fn> e chamada periodicamente e deve atualizar SVC_STATE (via svc_set).
#   Emite um log persistente sempre que um servico fica pronto/falha, mostrando
#   "agora: <servico>" — o feedback pedido. Sem TTY, so imprime esses marcos.
ui_animate() {
  local probe=$1 timeout=${2:-180}
  local frame=0 start now elapsed i
  local -a prev=("${SVC_STATE[@]}")
  start=$(date +%s)

  # probe a cada ~0.5s; repinta a cada ~0.08s (so em TTY). Configuravel.
  local probe_every=${UI_PROBE_EVERY:-6}

  while :; do
    if [ $((frame % probe_every)) -eq 0 ]; then "$probe" "$frame"; fi

    # Detecta transicoes e registra marcos persistentes.
    for i in "${!SVC_KEY[@]}"; do
      if [ "${SVC_STATE[$i]}" != "${prev[$i]}" ]; then
        case "${SVC_STATE[$i]}" in
          run)  ui_commit_scene; ui_stage "${SVC_LABEL[$i]}" ;;
          ok)   ui_commit_scene; ui_ok "${SVC_LABEL[$i]} pronto" ;;
          fail) ui_commit_scene; ui_err "${SVC_LABEL[$i]} falhou" ;;
        esac
        prev[$i]="${SVC_STATE[$i]}"
      fi
    done

    ui_scene "$frame"

    svc_all_settled && break
    now=$(date +%s); elapsed=$((now - start))
    if [ "$elapsed" -ge "$timeout" ]; then
      ui_commit_scene; ui_warn "tempo limite (${timeout}s) atingido aguardando servicos"
      break
    fi
    sleep "${UI_TICK:-0.08}"
    frame=$((frame + 1))
  done

  # Fixa a cena final na tela.
  ui_scene "$frame"
  ui_commit_scene
}
