
# Usefull things
http://localhost:5066/scalar/



# To Do:
[]nur im Def Scalar anzeigen Lassen!


# Für Uns Docs (Kann man später weglöschen!)

dotnet new webapi -n sims -f net10.0 --use-program-main -controllers
dotnet sln sims.slnx add sims/sims.csproj


# Start

Siehe ./depl-kram ordner, hier befinden sich unterschieldiche docker compose datein welche für unterschiedliche zwekce vorhanden sind. 
nach cd zum ordner, kann man mittel compose den contaienr stack starten.
1. ein compose für die lokale run entwicklung, wo nur die dbs angelegt werden und ports gemapped werden.
2. ein compose wo die images direkt on up gebaut werden für ein locales testen builden
3. ein compose für die produktivumgebung, wo die images direkt vom Github reg. gezogen werden


## Frontend per CLI für dev Starten

```bash
cd ./frontend
npm install
npm run dev
```