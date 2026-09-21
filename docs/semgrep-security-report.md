# Semgrep Security Report

Der Semgrep-Scan prüft das Repository automatisch auf bekannte Sicherheitsprobleme im Code und in der Konfiguration.

## Fazit

Der Scan wurde erfolgreich abgeschlossen und meldet **24 blockierende Findings** in **171 versionierten Dateien**. Die wichtigsten Themen sind GitHub-Actions, mögliche Shell-Injection, unsichere WebSockets, Root im Frontend-Container und eine mögliche H2C-Smuggling-Konfiguration.

## Rohe Ausgabe

```text
PS C:\Users\tobias\Documents\Bachelor_FH_st_poelten\3. Semester\Advanced Coding & Database Technologies\SIMS-ELITE> docker run --rm -v "${PWD}:/src" semgrep/semgrep semgrep --config auto --exclude="**/bin" --exclude="**/obj" --exclude="**/node_modules"


┌─────────────┐
│ Scan Status │
└─────────────┘
  Scanning 171 files tracked by git with 1074 Code rules:

  Language      Rules   Files          Origin      Rules
 ─────────────────────────────        ───────────────────
  <multilang>      72     114          Community    1074
  csharp           33      55
  ts              163      27
  json              4      18
  yaml             35      11
  dockerfile        6       5
  js              153       2
  bash              4       1
  html              1       1



┌──────────────────┐
│ 24 Code Findings │
└──────────────────┘

    .github/workflows/docker-build.yml
    ❯❱ yaml.github-actions.security.github-actions-mutable-action-tag.github-actions-mutable-action-tag
          ❰❰ Blocking ❱❱
          GitHub Actions step uses a mutable tag or branch reference. Tags and branch names can be silently
          repointed by the action owner, enabling supply-chain attacks — as seen in the trivy-action and kics-
          github-action compromises. Pin the reference to a full 40-character commit SHA instead, e.g. `uses:
          actions/checkout@8ade135a41bc03ea155e62e844d188df1ea18608`.
          Details: https://sg.run/2LgAL

           25┆ uses: actions/checkout@v4
            ⋮┆----------------------------------------
           32┆ uses: docker/login-action@v3
            ⋮┆----------------------------------------
           43┆ uses: docker/metadata-action@v5
            ⋮┆----------------------------------------
           50┆ uses: docker/build-push-action@v5
            ⋮┆----------------------------------------
           63┆ uses: docker/metadata-action@v5
            ⋮┆----------------------------------------
           70┆ uses: docker/build-push-action@v5
            ⋮┆----------------------------------------
           83┆ uses: docker/metadata-action@v5
            ⋮┆----------------------------------------
           90┆ uses: docker/build-push-action@v5
            ⋮┆----------------------------------------
          103┆ uses: docker/metadata-action@v5
            ⋮┆----------------------------------------
          110┆ uses: docker/build-push-action@v5
            ⋮┆----------------------------------------
          123┆ uses: docker/metadata-action@v5
            ⋮┆----------------------------------------
          130┆ uses: docker/build-push-action@v5
            ⋮┆----------------------------------------
          142┆ uses: actions/setup-dotnet@v4

     ❯❯❱ yaml.github-actions.security.run-shell-injection.run-shell-injection
       ❰❰ Blocking ❱❱
          Using variable interpolation `${{...}}` with `github` context data in a `run:` step could allow an
          attacker to inject their own code into the runner. This would allow them to steal secrets and code.
          `github` context data can have arbitrary user input and should be treated as untrusted. Instead, use
          an intermediate environment variable with `env:` to store the data and use the environment variable
          in the `run:` script. Be sure to use double-quotes the environment variable, like this: "$ENVVAR".
          Details: https://sg.run/pkzk

          155┆ run: |
          156┆   mkdir -p ./sbom-output
          157┆
          158┆   SERVICES=("sims-identity" "sims-aggregator" "sims-stix-ingest" "sims-incidentManager")
          159┆
          160┆   for SERVICE in "${SERVICES[@]}"; do
          161┆     # 1. Generate SBOM
          162┆     dotnet-CycloneDX "$SERVICE/$SERVICE.csproj" \
          163┆       -o ./sbom-output \
          164┆       --filename "bom-$SERVICE.json" \
             [hid 27 additional lines, adjust with --max-lines-per-finding]

    ❯❱ yaml.github-actions.security.github-actions-mutable-action-tag.github-actions-mutable-action-tag
          ❰❰ Blocking ❱❰
          GitHub Actions step uses a mutable tag or branch reference. Tags and branch names can be silently
          repointed by the action owner, enabling supply-chain attacks — as seen in the trivy-action and kics-
          github-action compromises. Pin the reference to a full 40-character commit SHA instead, e.g. `uses:
          actions/checkout@8ade135a41bc03ea155e62e844d188df1ea18608`.
          Details: https://sg.run/2LgAL

          196┆ uses: actions/setup-node@v4

   ❯❯❱ yaml.github-actions.security.run-shell-injection.run-shell-injection
          ❰❰ Blocking ❱❰
          Using variable interpolation `${{...}}` with `github` context data in a `run:` step could allow an
          attacker to inject their own code into the runner. This would allow them to steal secrets and code.
          `github` context data can have arbitrary user input and should be treated as untrusted. Instead, use
          an intermediate environment variable with `env:` to store the data and use the environment variable
          in the `run:` script. Be sure to use double-quotes the environment variable, like this: "$ENVVAR".
          Details: https://sg.run/pkzk

          249┆ run: |
          250┆   gh release upload ${{ github.ref_name }} ./sbom-output/bom-*.json --clobber

    depl-kram/sonstigeContainer/signoz/depl/casting.yaml.lock
   ❯❯❱ javascript.lang.security.detect-insecure-websocket.detect-insecure-websocket
          ❰❰ Blocking ❱❰
          Insecure WebSocket Detected. WebSocket Secure (wss) should be used for all WebSocket connections.
          Details: https://sg.run/GWyz

          149┆ server_endpoint: ws://signoz-signoz-0:4320/v1/opamp
            ⋮┆----------------------------------------
          296┆ server_endpoint: ws://signoz-signoz-0:4320/v1/opamp
            ⋮┆----------------------------------------
          351┆ - ws://signoz-signoz-0:4320

    frontend/Dockerfile
   ❯❯❱ dockerfile.security.missing-user-entrypoint.missing-user-entrypoint
          ❰❰ Blocking ❱❰
          By not specifying a USER, a program in the container may run as 'root'. This is a security hazard.
          If an attacker can control a process running as root, they may have control over the container.
          Ensure that the last USER in a Dockerfile is a USER other than 'root'.
          Details: https://sg.run/k281

           ▶▶┆ Autofix ▶ USER non-root ENTRYPOINT ["/docker-entrypoint.sh"]
           14┆ ENTRYPOINT ["/docker-entrypoint.sh"]

    frontend/nginx.conf
        ❯❱ generic.nginx.security.possible-h2c-smuggling.possible-nginx-h2c-smuggling
          ❰❰ Blocking ❱❰
          Conditions for Nginx H2C smuggling identified. H2C smuggling allows upgrading HTTP/1.1 connections
          to lesser-known HTTP/2 over cleartext (h2c) connections which can allow a bypass of reverse proxy
          access controls, and lead to long-lived, unrestricted HTTP traffic directly to back-end servers. To
          mitigate: WebSocket support required: Allow only the value websocket for HTTP/1.1 upgrade headers
          (e.g., Upgrade: websocket). WebSocket support not required: Do not forward Upgrade headers.
          Details: https://sg.run/ploZ

           22┆ proxy_http_version 1.1;
           23┆ proxy_set_header Upgrade $http_upgrade;
           24┆ proxy_set_header Connection "upgrade";
            ⋮┆----------------------------------------
           37┆ proxy_http_version 1.1;
           38┆ proxy_set_header Upgrade $http_upgrade;
           39┆ proxy_set_header Connection "upgrade";
            ⋮┆----------------------------------------
           52┆ proxy_http_version 1.1;
           53┆ proxy_set_header Upgrade $http_upgrade;
           54┆ proxy_set_header Connection "upgrade";
            ⋮┆----------------------------------------
           67┆ proxy_http_version 1.1;
           68┆ proxy_set_header Upgrade $http_upgrade;
           69┆ proxy_set_header Connection "upgrade";



┌──────────────┐
│ Scan Summary │
└──────────────┘
✅ Scan completed successfully.
 • Findings: 24 (24 blocking)
 • Rules run: 313
 • Targets scanned: 171
 • Parsed lines: ~99.9%
 • Scan was limited to files tracked by git
 • For a detailed list of skipped files and lines, run semgrep with the --verbose flag
Ran 313 rules on 171 files: 24 findings.
```
