#!/usr/bin/env bash
#
# version.sh - versao AUTOMATICA do projeto, derivada do git.
#
# A cada commit (ou arvore suja) o numero muda sozinho: combinamos uma base
# semantica estavel (arquivo VERSION) com o contador de commits e o hash curto.
#   Ex.: 0.1.0+build.12.g1a2b3c4        (limpo)
#        0.1.0+build.12.g1a2b3c4-dirty  (com alteracoes nao commitadas)
#
# Fonte da verdade unica: usada pelo boot animado, pelo Makefile e (Fase 3/4)
# exposta pela API e pelo frontend via build-arg.
#
# Uso:
#   scripts/version.sh            # versao completa (--full)
#   scripts/version.sh --short    # "v0.1.0 (#12 g1a2b3c4)" para exibir
#   scripts/version.sh --commit   # so o hash curto
#   scripts/version.sh --count    # so o numero (contagem de commits)
#   scripts/version.sh --json     # {"version","commit","count"}
#   scripts/version.sh --write     # grava .version (cache p/ builds sem git) e ecoa
#
# Sem git (ex.: dentro de um container sem .git), cai para o arquivo .version.

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

BASE="$(tr -d '[:space:]' < "$ROOT_DIR/VERSION" 2>/dev/null)"
BASE="${BASE:-0.0.0}"

FULL=""; SHORT=""; COMMIT="unknown"; COUNT="0"

git_ok() {
  command -v git >/dev/null 2>&1 &&
    git -C "$ROOT_DIR" rev-parse --git-dir >/dev/null 2>&1 &&
    git -C "$ROOT_DIR" rev-parse HEAD >/dev/null 2>&1
}

if git_ok; then
  COMMIT="$(git -C "$ROOT_DIR" rev-parse --short=7 HEAD)"
  COUNT="$(git -C "$ROOT_DIR" rev-list --count HEAD)"
  DIRTY=""
  [ -n "$(git -C "$ROOT_DIR" status --porcelain 2>/dev/null)" ] && DIRTY="-dirty"
  FULL="${BASE}+build.${COUNT}.g${COMMIT}${DIRTY}"
  SHORT="v${BASE} (#${COUNT} g${COMMIT}${DIRTY})"
elif [ -f "$ROOT_DIR/.version" ]; then
  # cache gerado por --write / post-commit (para builds sem git)
  FULL="$(tr -d '[:space:]' < "$ROOT_DIR/.version")"
  SHORT="v${FULL}"
  COMMIT="$(printf '%s' "$FULL" | sed -n 's/.*\.g\([0-9a-f]\{7\}\).*/\1/p')"
  COUNT="$(printf '%s' "$FULL" | sed -n 's/.*+build\.\([0-9]\+\)\..*/\1/p')"
  COMMIT="${COMMIT:-unknown}"; COUNT="${COUNT:-0}"
else
  FULL="${BASE}"; SHORT="v${BASE}"
fi

case "${1:-}" in
  --full | "") printf '%s\n' "$FULL" ;;
  --short)     printf '%s\n' "$SHORT" ;;
  --commit)    printf '%s\n' "$COMMIT" ;;
  --count)     printf '%s\n' "$COUNT" ;;
  --json)      printf '{"version":"%s","commit":"%s","count":%s}\n' "$FULL" "$COMMIT" "${COUNT:-0}" ;;
  --write)     printf '%s\n' "$FULL" > "$ROOT_DIR/.version"; printf '%s\n' "$FULL" ;;
  -h | --help) grep -E '^#( |$)' "${BASH_SOURCE[0]}" | sed -E 's/^# ?//' ;;
  *) printf 'version.sh: opcao desconhecida: %s\n' "$1" >&2; exit 2 ;;
esac
