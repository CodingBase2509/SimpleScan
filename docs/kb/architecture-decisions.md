---
id: KB-ARCHITECTURE-DECISIONS
title: Architekturentscheidungen
tags: [architecture, adr, blazor, docker]
status: active
updated: 2026-06-26
---

# Architekturentscheidungen

## ADR-001: Blazor Web App mit Interactive Server

Status: entschieden

Die App wird als .NET Blazor Web App mit Interactive Server Render Mode gebaut.

Gruende:

- ein Projekt statt getrenntem Frontend und Backend
- serverseitiger Zugriff auf Scanner, Dateisystem, Bildverarbeitung und PDF-Export
- weniger Boilerplate fuer den MVP
- gute Passung fuer lokal oder privat gehostete Nutzung

Nicht gewaehlt:

- Angular plus separate .NET Web API
- Blazor WASM plus Web API
- .NET MAUI

## ADR-002: Linux-Docker als Zielbetrieb

Status: entschieden

Die App soll primaer in einem Linux-Docker-Container laufen. Dadurch ist der Betrieb auf PC, Homeserver oder spaeter NAS-nahem Setup konsistent.

Konsequenzen:

- Scannerzugriff muss mit Linux-kompatiblen Netzwerkprotokollen priorisiert werden.
- Host-USB-Scanner sind fuer den MVP nicht Zielpfad.
- `/data` muss als persistierbares oder temporaeres Docker-Volume funktionieren.
- Discovery und Netzwerkzugriff koennen je nach Docker-Netzwerkmodus beeinflusst werden.

## ADR-003: Zuerst Mock-Scanner, danach echte Hardware

Status: entschieden

Der erste technische Ausbau nutzt einen Mock-Scanner. Dadurch koennen Dokument-Workflow, UI, Storage und PDF-Export stabil entstehen, bevor Hardware- und Protokollfragen den Fortschritt blockieren.

Konsequenzen:

- `IScannerProvider` wird von Anfang an als Grenze eingefuehrt.
- Mock-Daten muessen realistische Scan-Ergebnisse liefern, zum Beispiel mehrseitige farbige Testbilder.
- Der echte Provider wird spaeter gegen dieselbe Schnittstelle entwickelt.

## ADR-004: Ein Workflow ist ein ScanDocument

Status: entschieden

Ein laufender Scanvorgang entspricht genau einem `ScanDocument`. Eine separate `ScanSession`-Entitaet wird im MVP nicht eingefuehrt.

Konsequenzen:

- `ScanDocument` enthaelt Scanner, Einstellungen, Status und Seitenliste.
- Statusuebergaenge gehoeren an das Dokument.
- Export und Cleanup beziehen sich auf ein Dokument.

## ADR-005: Kein Mehrbenutzer- und Parallelbetrieb im MVP

Status: entschieden

Der MVP fokussiert auf einen aktiven Nutzer und einen einfachen Arbeitsfluss. Mehrbenutzerbetrieb und parallele Scan-Workflows werden bewusst nicht umgesetzt.

Konsequenzen:

- In-Memory-Zustand ist fuer den MVP akzeptabel.
- Scanner-Locking wird simpel gehalten.
- Spaetere Erweiterung darf durch klare Service-Grenzen vorbereitet, aber nicht vorweg implementiert werden.

## ADR-006: Page Editing erst nach Scan, Layout und Export

Status: entschieden

Crop, Rotate, Resize und Canvas-Editor werden erst umgesetzt, wenn Scannen, Layouting, Seitenverwaltung und PDF-Export funktionieren.

Konsequenzen:

- Das erste Seitenmodell soll Edit-Parameter aufnehmen koennen, muss aber noch keine Bearbeitungsfunktionen anbieten.
- PDF-Export kann im ersten Schritt unveraenderte Seiten verwenden.

## ADR-007: Serverseitige Businesslogik

Status: entschieden

Razor Components duerfen UI-Zustand und Interaktion koordinieren, aber keine Scanner-, Storage-, Bild- oder PDF-Businesslogik enthalten.

Konsequenzen:

- Scannerzugriff nur ueber `IScannerProvider`.
- Dokumentoperationen ueber Application Services.
- File-Delivery und Downloads ueber dedizierte technische Endpunkte.

## ADR-008: DDD-Projektstruktur

Status: entschieden

Die Solution wird in separate Projekte fuer Web, Application, Infrastructure und Domain aufgeteilt.

Referenzrichtung:

- `SimpleScan.Domain` hat keine Projektabhaengigkeiten.
- `SimpleScan.Application` referenziert `SimpleScan.Domain`.
- `SimpleScan.Infrastructure` referenziert `SimpleScan.Application` und `SimpleScan.Domain`.
- `SimpleScan` als Blazor-Web-Projekt referenziert `SimpleScan.Application` und `SimpleScan.Infrastructure`.

Konsequenzen:

- Domain-Modelle bleiben frei von UI- und Infrastrukturdetails.
- Application definiert Use-Cases und Ports.
- Infrastructure implementiert technische Adapter.
- Web bleibt Composition Root und UI-Schicht.

## ADR-009: Zentrales Package Management

Status: entschieden

NuGet-Package-Versionen werden ueber `Directory.Packages.props` auf Solution-Ebene zentral gepflegt.

Konsequenzen:

- Projektdateien referenzieren Pakete spaeter ohne lokale Versionsangaben.
- Versionen werden an einer Stelle aktualisiert.
- Neue Pakete bekommen zuerst einen `PackageVersion`-Eintrag in `Directory.Packages.props`.

## ADR-010: SLNX als Solution-Format

Status: entschieden

Die Solution verwendet das neue `.slnx`-Format statt des klassischen `.sln`-Formats.

Konsequenzen:

- `SimpleScan.slnx` ist die massgebliche Solution-Datei.
- Die alte `SimpleScan.sln` wird nicht weiter gepflegt.
- Solution-Kommandos sollen gegen `SimpleScan.slnx` ausgefuehrt werden.

## ADR-011: Domain-Modell mit leichten Invarianten

Status: entschieden

Die Domain-Schicht enthaelt nicht nur anemische DTOs, sondern kontrolliert einfache fachliche Invarianten direkt in den Domain-Typen.

Aktuelle Domain-Bausteine:

- Documents: `ScanDocument`, `ScannedPage`, `ScanDocumentStatus`
- Scanning: `ScanSettings`, `ScanColorMode`
- Scanners: `ScannerDevice`, `ScannerCapabilities`, `ScannerStatus`
- Seitenbearbeitung: `PageEditSettings`, `CropArea`, `ResizeSettings`
- Export: `PdfExportSettings`, `PdfCompressionLevel`
- Downloads: `DownloadTicket`

Konsequenzen:

- Seitenreihenfolge und Page-Nummern werden im `ScanDocument` konsistent gehalten.
- Geschlossene Dokumente koennen nicht weiter bearbeitet oder exportiert werden.
- Export ohne Seiten ist nicht erlaubt.
- Ablauffristen von DownloadTickets sind Domain-Logik.
- Infrastructure darf Dateipfade erzeugen, aber Domain-Typen validieren, dass keine leeren Pfade gespeichert werden.

## ADR-012: Application-Struktur nach Scan-App-Arbeitsbereichen

Status: entschieden

Das Application-Projekt wird nach den fachlichen Arbeitsbereichen der Scan-App strukturiert. Ziel ist eine pragmatische Services-Schicht mit klar benannten Ports statt generischer Handler- oder Repository-Abstraktionen.

Aktuelle Bereiche:

- `Downloads`
- `Events`
- `FileStorage`
- `PageEditing`
- `Processing/Images`
- `Processing/Pdf`
- `Scanning`
- `Scanners`
- `Stores`

Konsequenzen:

- `Scanning` enthaelt spaeter die Scan-Workflows.
- `Scanners` enthaelt den Port fuer Hardware-Interaktion.
- `FileStorage` enthaelt Datei-Ports und einfache Datei-Modelle.
- `Processing` trennt Bildverarbeitung von PDF-Erzeugung.
- `Stores` haelt Metadaten und aktuellen App-Zustand, aber keine Dateien.
- `Downloads` arbeitet mit einfachen Download-Tickets, die auf exportierte Dateien im FileStorage zeigen.

## ADR-013: Gemeinsame Build-Defaults und Global Usings

Status: entschieden

Gemeinsame .NET-Projekteinstellungen werden ueber `Directory.Build.props` auf Repository-Ebene gepflegt.

Aktuelle gemeinsame Defaults:

- `TargetFramework`: `net10.0`
- `ImplicitUsings`: `enable`
- `Nullable`: `enable`

Zusaetzlich enthalten `SimpleScan.Domain` und `SimpleScan.Application` je eine `GlobalUsings.cs` fuer haeufig verwendete projektinterne Namespaces.

Konsequenzen:

- Neue Projekte erben die gemeinsamen Compiler-Defaults automatisch.
- Projektdateien enthalten weniger wiederholte Konfiguration.
- SDK-Implicit-Usings werden weiterhin vom Build in `obj/.../*.GlobalUsings.g.cs` erzeugt.
- Wenn `obj/` geloescht wurde, kann die IDE erst nach Restore/Build alle generierten implicit usings sehen.

## Verwandte Knoten

- [Projektbrief](project-brief.md)
- [MVP-Roadmap](mvp-roadmap.md)
- [Arbeitsweise](working-agreements.md)
