---
id: KB-PROJECT-BRIEF
title: Projektbrief
tags: [vision, scope, mvp]
status: active
updated: 2026-06-26
---

# Projektbrief

## Ziel

SimpleScan soll eine selbst gehostete Scanner- und spaeter Drucker-Web-App werden. Der konkrete Nutzerbedarf ist, Scanner und Drucker im lokalen Netzwerk nutzen zu koennen, ohne fuer jedes Geraet eine eigene Hersteller-App auf Laptop oder Smartphone installieren oder bezahlen zu muessen.

## Aktueller Fokus

Der MVP konzentriert sich ausschliesslich auf Scannen:

- Scanner im Netzwerk finden
- Scanner auswaehlen
- Scan-Einstellungen setzen
- Seiten fuer ein Dokument scannen
- Seiten anzeigen, auswaehlen, sortieren und loeschen
- Seiten als PDF exportieren
- PDF herunterladen
- temporaere Daten wieder entfernen

Drucken ist ein spaeteres separates Modul und gehoert nicht in den Scan-MVP.

## Ziel-Deployment

Die App soll in einem Linux-Docker-Container laufen. Sie soll sowohl lokal auf einem PC als auch auf einem Homeserver betrieben werden koennen.

Der temporaere Speicherpfad ist `/data/scan-documents/...`. In Docker soll dieser Pfad als Volume nutzbar sein.

## MVP-Grenzen

- Kein Mehrbenutzerbetrieb im MVP.
- Keine parallelen Workflows im MVP.
- Kein Page Editing im ersten MVP.
- Kein OCR im MVP.
- Kein Dokumentenarchiv im MVP.
- Kein Druckmodul im MVP.
- Kein externes REST-API im MVP, ausser technische File-Delivery- und Download-Endpunkte.

## Vorgehen

Zuerst wird mit einem Mock-Scanner gearbeitet, damit UI, Dokumentmodell, Storage, PDF-Export und Workflow ohne echte Hardware stabil aufgebaut werden koennen.

Danach wird gegen echte Scanner und Drucker getestet. Ziel ist eine moeglichst breite Unterstuetzung gaengiger Hersteller und Protokolle.

## Verwandte Knoten

- [Architekturentscheidungen](architecture-decisions.md)
- [MVP-Roadmap](mvp-roadmap.md)
- [Scanner-Support-Strategie](scanner-support-strategy.md)
- [Offene Fragen und Risiken](open-questions-and-risks.md)
