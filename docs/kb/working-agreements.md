---
id: KB-WORKING-AGREEMENTS
title: Arbeitsweise
tags: [process, documentation, engineering]
status: active
updated: 2026-06-26
---

# Arbeitsweise

## Projektmodus

Vor Implementierungen werden relevante Entscheidungen in dieser Knowledge-Base festgehalten. Die Knowledge-Base ist kein Ersatz fuer Code, sondern eine dauerhafte Orientierung fuer Scope, Reihenfolge und technische Leitplanken.

## Aktuelle Arbeitsregel

Auf Nutzerwunsch wird aktuell noch nichts an der App-Funktionalitaet umgesetzt. Diese Phase dient nur dem Aufbau der Knowledge-Base.

## Dokumentationsregeln

- Neue Entscheidungen in [architecture-decisions.md](architecture-decisions.md) dokumentieren.
- Roadmap-Aenderungen in [mvp-roadmap.md](mvp-roadmap.md) nachziehen.
- Hardware- und Protokollerkenntnisse in [scanner-support-strategy.md](scanner-support-strategy.md) sammeln.
- Offene Fragen und Risiken in [open-questions-and-risks.md](open-questions-and-risks.md) pflegen.
- Veraltete Punkte mit Status markieren statt kommentarlos entfernen.

## Engineering-Leitplanken

- Kleine, validierbare Schritte.
- Erst Mock-Workflow, dann echte Hardware.
- Keine Businesslogik in Razor Components.
- Scannerzugriff nur ueber `IScannerProvider`.
- Temporäre Dateien nie ueber clientgelieferte Pfade adressieren.
- `/data` als Storage-Wurzel konfigurierbar halten.
- Docker-Betrieb von Anfang an als Zielumgebung respektieren.

## Repo-Hygiene

- Build-Artefakte wie `bin/` und `obj/` gehoeren nicht ins Repository.
- OS- und IDE-Artefakte wie `.DS_Store` und `.idea/` gehoeren nicht ins Repository.
- Template-Beispielcode wird entfernt oder auf minimale technische Platzhalter reduziert, bevor fachliche Funktionalitaet entsteht.
- Die App soll nach Bereinigungen weiterhin kompilieren.
- NuGet-Versionen werden zentral in `Directory.Packages.props` gepflegt.
- Gemeinsame .NET-Build-Defaults werden zentral in `Directory.Build.props` gepflegt.
- Haeufig verwendete projektinterne Namespaces duerfen pro Projekt in `GlobalUsings.cs` gebuendelt werden.
- Neue Codebereiche sollen die Projektgrenzen `Domain`, `Application`, `Infrastructure` und `Web` respektieren.
- Die massgebliche Solution-Datei ist `SimpleScan.slnx`.

## UI-Leitplanken fuer spaeter

- Die erste Ansicht ist die nutzbare Scan-Oberflaeche, keine Landingpage.
- Desktop-Layout: Preview als Hauptflaeche, Scanner/Settings/Status als seitlicher Arbeitsbereich, Seitenliste unten.
- Mobile-Layout: einfache, robuste Bedienung vor Drag-and-Drop-Komplexitaet.
- Page Editing wird spaeter als eigener Modus umgesetzt.

## Verwandte Knoten

- [Projektbrief](project-brief.md)
- [Architekturentscheidungen](architecture-decisions.md)
