# Uhr

Nachbau der Windows-11-Uhr-App für Ubuntu. Die App bietet einen Wecker, einen Timer und einen Pomodoro-Timer, jeweils in einem eigenen Tab. Ist eine Zeit erreicht oder abgelaufen, spielt die App einen Systemsound ab und zeigt eine Systembenachrichtigung an.

Umgesetzt mit C#/.NET 10 und Avalonia nach dem MVVM-Prinzip.

## Voraussetzungen

- Ubuntu (x86_64)
- `paplay` für den Sound (unter Ubuntu vorinstalliert); Benachrichtigungen gehen per D-Bus an den Benachrichtigungsdienst des Desktops
- Zum Bauen: .NET 10 SDK (`sudo apt install dotnet-sdk-10.0`)

## Starten

Aus dem Quellcode:

```
dotnet run --project Uhr
```

Als eigenständige Programmdatei (ohne installiertes .NET lauffähig):

```
dotnet publish Uhr/Uhr.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Die fertige Datei liegt danach unter `Uhr/bin/Release/net10.0/linux-x64/publish/Uhr`.

## Nutzung

Jeder Tab hat oben einen Bereich zum Anlegen eines neuen Eintrags. Nach „Hinzufügen“ erscheint der Eintrag als Kachel und kann dort gestartet, angehalten oder gelöscht werden.

### Wecker

- Name und Uhrzeit (24 Stunden) eingeben und hinzufügen.
- Mit dem Schalter auf der Kachel wird der Wecker ein- oder ausgeschaltet. Die Kachel zeigt, wann er das nächste Mal klingelt.
- Nach dem Klingeln schaltet sich der Wecker ab und lässt sich mit dem Schalter wieder aktivieren.

### Timer

- Name und Dauer (Stunden, Minuten, Sekunden) eingeben und hinzufügen.
- ▶ startet den Timer, ⏸ hält ihn an, ↻ setzt ihn auf die volle Dauer zurück.
- Der Ring auf der Kachel zeigt die verbleibende Zeit an.

### Pomodoro

- Name, Fokuszeit, Pausenlänge (jeweils in Minuten) und Anzahl der Zyklen eingeben und hinzufügen.
- Nach dem Start wechseln sich Fokuszeit und Pause ab, bis alle Zyklen durchlaufen sind.
- Sound und Benachrichtigung kommen bei jedem Wechsel und am Ende aller Zyklen.
- In der Benachrichtigung am Ende der Fokuszeit lässt sich die Pause um 3 Minuten verschieben. Die Fokuszeit läuft dann 3 Minuten weiter, danach kommt die Benachrichtigung erneut. Das geht beliebig oft.
- Die Kachel zeigt die aktuelle Phase und den Zyklus an, z. B. „Fokus · Zyklus 2 von 4“.

## Gespeicherte Daten

Alle angelegten Wecker, Timer und Pomodoros werden in `~/.config/Uhr/settings.json` gespeichert und beim nächsten Start wieder geladen. Laufende Timer werden nicht gespeichert.
