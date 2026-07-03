---
id: KB-MVP-ROADMAP
title: MVP-Roadmap
tags: [mvp, roadmap, phases]
status: active
updated: 2026-07-03
---

# MVP-Roadmap

## Aktueller MVP-Backend-Stand

Status: in Umsetzung

Abgedeckte Business-Use-Cases:

- Scanner ueber `ScannerService.DiscoverAsync` auflisten.
- Scanner-Capabilities ueber `ScannerService.GetCapabilitiesAsync` laden und cachen.
- Scan-Dokument mit validierten Scanner-Settings erstellen.
- Scanner-Settings eines offenen Dokuments aktualisieren.
- Seite scannen, Original speichern und Preview/Thumbnail erzeugen.
- Bestehende Seiten sortieren.
- Gescannte Seite inklusive Original, Preview und Thumbnail loeschen.
- Page-Edit-Settings setzen und Preview/Thumbnail neu erzeugen.
- Download-Ticket fuer spaeteren PDF-Export vorbereiten.

Noch bewusst offen:

- Echter Scanner-Provider.
- PDF-Erzeugung.
- Technischer Download-Endpunkt.
- UI-Durchstich fuer den kompletten Workflow.

## Phase 0: Knowledge-Base und Projektklarheit

Status: gestartet

Ziele:

- Projektscope persistieren
- Architekturentscheidungen erfassen
- offene Fragen und Risiken sichtbar machen
- noch keine App-Funktionalitaet implementieren

## Phase 1: Mock-Scanner-Spike

Ziele:

- `IScannerProvider` definieren
- Mock-Scanner implementieren
- eine Seite als Bild erzeugen oder aus Testdaten liefern
- Scan-Ergebnis temporaer unter `/data/scan-documents/...` speichern

Erfolgskriterium:

- Die App kann ohne echte Hardware eine plausible Scan-Seite erzeugen und speichern.

## Phase 2: Basis-Workflow und Layout

Ziele:

- Hauptscreen fuer Scannen und Seitenverwaltung bauen
- Scanner-Auswahl mit Mock-Scanner
- Scan-Einstellungen fuer den MVP anzeigen
- Button "Neue Seite scannen"
- Preview fuer aktuelle Seite
- Thumbnail-Liste fuer gescannte Seiten

Erfolgskriterium:

- Ein Nutzer kann mehrere Mock-Seiten scannen und im UI zwischen ihnen wechseln.

## Phase 3: ScanDocument

Ziele:

- `ScanDocument` als Workflow-Modell einfuehren
- Seiten an Dokument anhaengen
- Seiten loeschen
- Seitenreihenfolge aendern
- Dokumentstatus fuehren

Erfolgskriterium:

- Ein Dokument kann mit mehreren Seiten aufgebaut und geordnet werden.

## Phase 4: PDF-Export

Ziele:

- QuestPDF oder geeignete Alternative anbinden
- Seiten in Reihenfolge in ein PDF rendern
- DownloadToken erzeugen
- PDF ueber technischen Download-Endpunkt ausliefern
- Exportdaten kurzlebig halten

Erfolgskriterium:

- Ein mehrseitiges Mock-Dokument kann als PDF heruntergeladen werden.

## Phase 5: Echter Scanner-Provider

Ziele:

- NAPS2.Sdk oder geeignete Linux-faehige Scannerintegration evaluieren
- Netzwerk-Scanner finden
- Capabilities lesen
- eine echte Seite scannen
- Fehler- und Timeoutverhalten erfassen

Erfolgskriterium:

- Mindestens ein realer Netzwerk-Scanner funktioniert im Docker-Zielsetup oder mit klar dokumentierter Host-Netzwerkanforderung.

## Phase 6: Stabilisierung

Ziele:

- Temp-Cleanup
- robuste Fehleranzeigen ohne interne Pfadleaks
- Logging
- Scannerstatus-Monitoring
- einfache Authentifizierung, falls LAN-Zugriff aktiviert wird

## Ausserhalb des ersten MVP

- Page Editing
- OCR
- Searchable PDF
- Druckmodul
- Dokumentenarchiv
- Upload zu NAS, Nextcloud oder S3
- Mehrbenutzerbetrieb
- externe REST API

## Verwandte Knoten

- [Projektbrief](project-brief.md)
- [Scanner-Support-Strategie](scanner-support-strategy.md)
- [Offene Fragen und Risiken](open-questions-and-risks.md)
