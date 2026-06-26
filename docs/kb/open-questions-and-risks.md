---
id: KB-OPEN-QUESTIONS-RISKS
title: Offene Fragen und Risiken
tags: [questions, risks, assumptions]
status: active
updated: 2026-06-26
---

# Offene Fragen und Risiken

## Annahmen

### A-001: Docker ist Zielbetrieb

Status: bestaetigt

Die App soll in einem Linux-Docker-Container laufen und sowohl lokal auf einem PC als auch auf einem Homeserver nutzbar sein.

### A-002: Mock zuerst

Status: bestaetigt

Die erste Umsetzung arbeitet mit einem Mock-Scanner. Echte Hardwaretests folgen danach.

### A-003: Storage unter `/data`

Status: bestaetigt

Temporaere Scan- und Exportdaten koennen unter `/data/scan-documents/...` abgelegt werden.

### A-004: Kein Mehrbenutzer-MVP

Status: bestaetigt

Mehrbenutzerbetrieb und parallele Workflows werden fuer den MVP nicht umgesetzt.

### A-005: Page Editing spaeter

Status: bestaetigt

Page Editing wird erst nach funktionierendem Scannen, Layouting und Export implementiert.

## Offene Fragen

### Q-001: LAN-Zugriff und Authentifizierung

Status: offen

Soll die App im MVP nur lokal auf dem Host erreichbar sein oder direkt im LAN fuer Smartphone und andere Geraete? Bei LAN-Zugriff sollte einfache Authentifizierung frueh eingeplant werden.

### Q-002: Konkrete Testgeraete

Status: offen

Welche Scanner oder Multifunktionsdrucker stehen fuer echte Tests zur Verfuegung? Modellnamen und Netzwerkmodus sind wichtig fuer die Provider-Validierung.

### Q-003: Docker-Netzwerkmodus

Status: offen

Soll der Container im Host-Netzwerk laufen duerfen, falls Discovery per mDNS/Bonjour im Bridge-Netzwerk nicht zuverlaessig funktioniert?

### Q-004: Manuelle Scanner-IP

Status: offen

Soll der MVP neben Discovery auch direkt eine Scanner-IP oder URL akzeptieren? Das waere ein pragmatischer Fallback fuer schwierige Netzwerkumgebungen.

### Q-005: PDF-Qualitaetsstandard

Status: offen

Welche Standardbalance ist gewuenscht: kleinere PDFs oder hoehere Bildqualitaet? Architekturvorschlag ist Medium mit ca. 200 DPI und JPEG-Qualitaet 75-85.

## Risiken

### R-001: Scanner-Discovery in Docker

Status: aktiv

Automatische Discovery kann in Docker je nach Netzwerkmodus, Hostsystem und mDNS/Bonjour/Avahi-Konfiguration eingeschraenkt sein.

Moegliche Gegenmassnahmen:

- manuelle IP/URL als Fallback
- Host-Netzwerkmodus dokumentieren
- Discovery von direktem Scannerzugriff trennen

### R-002: NAPS2.Sdk-Abdeckung unter Linux-Docker

Status: aktiv

Die tatsaechliche Unterstuetzung von NAPS2.Sdk fuer die gewuenschten Netzwerkprotokolle im Linux-Docker-Zielsetup muss praktisch validiert werden.

Moegliche Gegenmassnahmen:

- Provider-Grenze strikt halten
- Mock-Provider zuerst nutzen
- echte Provider-Implementierung als separaten Spike behandeln

### R-003: Herstellerunterschiede trotz Standardprotokollen

Status: aktiv

Auch bei eSCL/AirScan koennen Hersteller und Modelle unterschiedliche Capabilities, Papierquellen, Aufloesungen oder Fehlercodes liefern.

Moegliche Gegenmassnahmen:

- Capabilities defensiv modellieren
- UI nur verfuegbare Optionen anzeigen lassen
- Testmatrix pro Geraet pflegen

### R-004: Temporaere Dateien wachsen unkontrolliert

Status: aktiv

Scans und Exporte koennen gross werden. Ohne Cleanup kann `/data` voll laufen.

Moegliche Gegenmassnahmen:

- Cleanup-Worker
- TTL fuer unfertige Dokumente
- TTL fuer Exportdateien und DownloadTokens
- spaeter optional Storage-Limit

## Verwandte Knoten

- [Projektbrief](project-brief.md)
- [Scanner-Support-Strategie](scanner-support-strategy.md)
- [MVP-Roadmap](mvp-roadmap.md)
