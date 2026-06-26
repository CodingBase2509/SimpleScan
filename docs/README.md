# SimpleScan Docs

Diese Dokumentation ist die persistente Knowledge-Base fuer das Projekt. Sie ist als leichter Markdown-Graph aufgebaut: jede Datei beschreibt einen stabilen Themenknoten, verweist auf verwandte Knoten und haelt Entscheidungen, offene Fragen und Arbeitsregeln fest.

## Einstieg

- [Projektbrief](kb/project-brief.md)
- [Knowledge Graph](kb/knowledge-graph.md)
- [Architekturentscheidungen](kb/architecture-decisions.md)
- [MVP-Roadmap](kb/mvp-roadmap.md)
- [Scanner-Support-Strategie](kb/scanner-support-strategy.md)
- [Arbeitsweise](kb/working-agreements.md)
- [Offene Fragen und Risiken](kb/open-questions-and-risks.md)

## Pflege-Regeln

- Neue Erkenntnisse werden zuerst in den passenden Knowledge-Base-Knoten geschrieben.
- Entscheidungen bekommen eine stabile ID im Format `ADR-000`.
- Offene Fragen bekommen eine stabile ID im Format `Q-000`.
- Risiken bekommen eine stabile ID im Format `R-000`.
- Wenn sich ein Punkt erledigt, wird er nicht geloescht, sondern mit Status und Datum aktualisiert.
- Implementierungsdetails gehoeren erst dann in die Codebasis, wenn sie aus der Knowledge-Base ausreichend klar sind.
