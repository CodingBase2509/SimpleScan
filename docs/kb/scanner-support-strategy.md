---
id: KB-SCANNER-SUPPORT
title: Scanner-Support-Strategie
tags: [scanner, printer, protocols, docker, linux]
status: active
updated: 2026-06-26
---

# Scanner-Support-Strategie

## Ziel

SimpleScan soll langfristig moeglichst viele gaengige Scanner, Multifunktionsdrucker und Hersteller unterstuetzen. Der MVP darf dieses Ziel nicht durch herstellerspezifische Kopplung verbauen.

## Grundsatz

Die App spricht nie direkt aus UI-Code mit einem Scanner. Alle Scannerzugriffe laufen ueber `IScannerProvider`.

Dadurch koennen mehrere Provider koexistieren:

- Mock-Provider fuer Entwicklung und Tests
- Netzwerk-Scanner-Provider fuer eSCL/AirScan
- spaeter optional SANE-basierter Provider
- spaeter optional herstellerspezifische Fallbacks, falls wirklich noetig

## Priorisierte Protokollrichtung

Fuer Linux-Docker und breite Netzwerkgeraete-Unterstuetzung sind diese Richtungen relevant:

1. eSCL / AirScan fuer moderne Netzwerk-Scanner und Multifunktionsgeraete
2. SANE/AirScan auf Linux als moegliche Integrationsschicht
3. IPP spaeter fuer Druckfunktionen
4. TWAIN/WIA nicht als MVP-Ziel, da sie stark desktop- und Windows-nah sind

Diese Liste ist eine Arbeitsannahme und muss vor echter Hardwareintegration mit aktueller Dokumentation und konkreten Testgeraeten validiert werden.

## Herstellerziel

Die App soll nicht "Canon-App", "Epson-App" oder "HP-App" werden, sondern Protokolle nutzen, die viele Marken abdecken.

Relevante Marken fuer spaetere Tests:

- Brother
- Canon
- Epson
- HP
- Kyocera
- Lexmark
- Ricoh
- Samsung / HP
- Xerox

## Docker-Auswirkungen

Scanner-Discovery im lokalen Netzwerk kann in Docker schwieriger sein als direkter Hostbetrieb.

Zu pruefende Punkte:

- Bridge-Netzwerk vs. Host-Netzwerk
- mDNS/Bonjour/Avahi-Sichtbarkeit
- Zugriff auf Scanner-IP bei direkter IP-Eingabe
- notwendige Ports fuer Discovery und Scanprotokolle
- Verhalten auf Linux-PC vs. Homeserver

## Provider-Leitplanken

`IScannerProvider` soll mindestens koennen:

- Scanner entdecken
- Capabilities laden
- Status lesen
- eine Seite scannen

Der Provider soll keine UI-Begriffe kennen. Er liefert Domain-Objekte und Bilddaten bzw. gespeicherte Scan-Ergebnisse.

## Teststrategie

Der Mock-Provider validiert App-Workflow und PDF-Export.

Echte Hardwaretests validieren danach:

- Discovery
- manuelle Scanner-IP als Fallback
- A4-Farbscan
- Graustufen-Scan
- DPI-Auswahl
- Flatbed
- ADF, falls verfuegbar
- Duplex, falls verfuegbar
- Timeout und Offline-Verhalten

## Verwandte Knoten

- [Architekturentscheidungen](architecture-decisions.md)
- [MVP-Roadmap](mvp-roadmap.md)
- [Offene Fragen und Risiken](open-questions-and-risks.md)
