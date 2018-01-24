---
layout: default
---

# Anleitung IT-Admin

Diese Anleitung zeigt die Erzeugung eines Modules und die Verknüpfung mit einem Paket um entsprechende Abhängigkeiten auflösen zu können. Hierbei sind die 2 Tabs der WebAPI "Packages" und "Artifacts" von belang.

## Artefakte verwalten
Um ein Artefakt zu erstellen wählen Sie den Reiter Artefakt aus.

![](https://i.imgur.com/0m37JBM.png)

In dieser Übersicht sind alle im System verfügbaren Artefakte sichtbar. Diese können über die Buttons in der Spalte "Actions" bearbeitet (links) und gelöscht (rechts) werden.

### Artefakt (Programm / Komponente) erstellen
Im unteren Bereich können Artefakte erstellt werden. **Es muss nur das ID-Feld zwingen ausgefüllt werden!**

Bitte tragen Sie in das erste Feld eine individuelle ID für das Artefakt ein (bspw. jdk-8-headless).

Alle Eintragungen können anschließend mit dem Button “Save” gespeichert werden. **Das Vorlesungsprofil wurde nun erstellt.**

Falls Sie alle bisherigen Eingaben löschen möchten können Sie den “Clear” Button nutzen.

### Artefakt bearbeiten

Um ein Artefakt zu bearbeiten, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Artefakt aus mit Ihrem Web Browser. Nun sehen Sie eine Liste mit bereits erstellten Artefakten. Drücken Sie den “Paper&Pen”-Button um Änderungsoptionen zu erhalten. Der X-Button dient zur löschung des gesamten Artefakts.

![](https://i.imgur.com/0m37JBM.png)

Sie haben nun den Bearbeitungsmodus aktiviert. Die Anzeige hat sich verändert.

Die Eintragungen im Fenster sind anpassbar. Jedes Fenster beschreibt eine anderes Kommando. Alle Kommandos mit Beispielen finden Sie bei “Artefakt erstellen”.

Das Fenster oben links(“Install Action”) enthält ein Skript, welches das Artefakt installieren soll, wohingegen das Fenster oben rechts (“Remove Action”) ein Skript enthält, welches das Artefakt dauerhaft vom System entfernen soll.

Unten links wird die “Switch Action” gezeigt, die ein Skript bereitstellt, welches ausgeführt wird, sobald eine Konfiguration mit diesem Artefakt aktiviert wird. Das Gegenteil ist im Skript unten rechts "Unswitch Action" zu sehen, welches ausgeführt wird bevor eine andere Konfiguration aktiviert wird oder falls die aktuelle Konfiguration entladen werden soll.

![](https://i.imgur.com/P6uRP9L.png)

Um alle gewünschten Änderungen zu speichern betätigen Sie den Button “Save Changes”.

### Hinzufügen von Dateien

Ein optionaler Schritt ist das hinzufügen von Dateien. Skripte können diese Dateien benutzen, da diese im gleichen Ordner wie die Datei selbst ausgeführt werden.

Hierzu wählen Sie zuerste eine odere mehrere Datei(en) über den Button "Dateien auswählen" aus und laden diese mit "Upload Selected Files" auf ihren jeweiligen Server hoch.

![](https://i.imgur.com/PPGxyYs.png)

## Packages verwalten

### Package erstellen

Um ein Package zu erstellen, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Package aus mit Ihrem Web Browser. Nun sehen Sie eine Liste mit bereits erstellten Packages. Scrollen sie nach unten bis Sie folgendes Bild sehen

![](https://i.imgur.com/Ul1wRXr.png)

Als erstes benennen Sie das Package, hinzu wird ein individueller und selbsterklärender Name in das Textfeld links neben den Kreis eingetragen.
Dieser Name wird den Dozenten angezeigt.

Anschließend wird das zugehörige Artefakt ausgewählt.
Neben den Pfeil befindet sich ein Dropdown Menü für ihre Auswahl des zugehöriges Artefakts. Es kann nur ein Artefakt ausgewählt werden.

Letztendlich muss der Button “Save” gedrückt werden.

Das Package wurde erfolgreich erstellt.

### Package bearbeiten

Um ein Package zu erstellen, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Package aus mit Ihrem Web Browser. Nun sehen Sie eine Liste mit bereits erstellten Packages. Drücke Sie den “Paper&Pen”-Button um Änderungsoptionen zu erhalten.
Der X-Button dient zur löschung des gesamten Pakets.


![](https://i.imgur.com/cmn1lXf.png)

Hier kann dem Paket durch die Dropdown-Liste ein anderes Artefakt zugeordnet werden. **(Die Seite ist nicht mehr aktuell)**

![](https://i.imgur.com/bqhtoYM.png)

[back](./)