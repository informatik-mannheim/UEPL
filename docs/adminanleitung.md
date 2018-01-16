---
layout: default
---

## Anleitung IT-Admin


# [](#header-1)Artefakt erstellen
Um ein Artefakt zu erstellen, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Artefakt aus mit Ihrem Web Browser.
In dieser Übersicht sind alle bisher erstellten Artefakte sichtbar. Bearbeitung und Löschung dieser ist durch die jeweiligen Button in der Spalte möglich.
Im Unteren Bereich können Artefakte erstellt werden.

**Alle Felder müssen ausgefüllt werden.**

Bitte tragen Sie in das erste Feld eine individuelle ID für das Artefakt ein.

Das nächste Feld enthält ein Kommando, welches das Artefakt (Anwendung) installieren soll. Hier werden typischerweise ein oder mehrere Verzeichnisse angelegt.
Anschließend könnte beispielsweise in das eben erzeugte Verzeichnis gewechselt und anschließend das Archiv mit der Dateiendung tar.gz entpackt werden.

Das mittlere Feld enthält ein Kommando, welches das Artefakt dauerhaft löschen soll.

Das nächste Feld enthält ein Kommando, welches das Artefakt (Anwendung) starten soll, um einen Wechsel von einem anderen Workspace zu ermöglichen: Typische Aktion wäre das Starten der jeweiligen Anwendung. Eine weitere Möglichkeit ist die Erstellung einer Verknüpfung z.B Eclipse auf dem Desktop, die dann vom Studenten aufgerufen werden kann.

Das nächste Feld enthält ein Kommando, welches das Artefakt (Anwendung) beenden soll um einen Wechsel zu einer anderen Workspace zu ermöglichen. Typische Aktion wäre in diesem Fall das Stoppen der Applikation z.B einer Datenbank. Wurde eine Verknüpfung beim Wechseln in den Kontext erstellt, so kann diese im Unswitch Skript wieder entfernt werden.

Falls Sie alle bisherigen Eingaben löschen möchten, bevor Sie “Save” gedrückt haben gibt es die möglichkeit mit den “Clear” Button alle änderungen zu löschen.
All diese Eintragungen können anschließend mit dem Button “Save” gespeichert werden. 

![](https://i.imgur.com/0m37JBM.png)

Das Vorlesungsprofil wurde nun erstellt.

# [](#header-1)Artefakt bearbeiten

Um ein Artefakt zu bearbeiten, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Artefakt aus mit Ihrem Web Browser. Nun sehen Sie eine Liste mit bereits erstellten Artefakten. Drücken Sie den “Paper&Pen”-Button um Änderungsoptionen zu erhalten. Der X-Button dient zur löschung des gesamten Artefakts.

![](https://i.imgur.com/0m37JBM.png)

Sie haben nun den Bearbeitungsmodus aktiviert. Die Anzeige hat sich verändert.

Die Eintragungen im Fenster sind anpassbar. Jedes Fenster beschreibt eine anderes Kommando. Alle Kommandos mit Beispielen finden Sie bei “Artefakt erstellen”.

Das Fenster oben links(“Install Action”) enthält ein Kommando, welches das Artefakt (Anwendung) installieren soll.

Das Fenster oben rechts (“Remove Action”) enthält ein Kommando, welches das Artefakt (Anwendung) dauerhaft löschen soll.

Das Fenster unten links (“Switch Action”) enthält ein Kommando, welches das Artefakt (Anwendung) beenden soll, um einen Wechsel zu einer anderen Workspace zu ermöglichen.

Das Fenster unten rechts (“Unswitch Action”) enthält ein Kommando, welches das Artefakt (Anwendung) starten soll, um einen Wechsel von einem anderen Workspace zu ermöglichen.

![](https://i.imgur.com/P6uRP9L.png)

Ein optionaler Schritt ist das hinzufügen von Dateien. Kommandos können auf die Dateien verweisen.

Zuerst betätigen Sie den Button “Datei auswählen”.
Nun wählen Sie eine oder mehrere Dateien aus zur Unterstützung der Kommandos. Diese Datei wird auf den Hochschulserver hinterlegt.

Zum letztendlichen Hochladen bestätigen Sie die Auswahl mit den Button “Upload Selected Files”.

![](https://i.imgur.com/PPGxyYs.png)

Um alle gewünschten Änderungen zu speichern betätigen Sie den Button “Save Changes”.

Nun wurden alle änderungen, welche Sie in den Textfelder vollzogen haben und eventuelle hinzugefügte Dateien gespeichert.



# [](#header-1)Package erstellen

Um ein Package zu erstellen, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Package aus mit Ihrem Web Browser. Nun sehen Sie eine Liste mit bereits erstellten Packages. Scrollen sie nach unten bis Sie folgendes Bild sehen

![](https://i.imgur.com/Ul1wRXr.png)

Als erstes benennen Sie das Package, hinzu wird ein individueller und selbsterklärender Name in das Textfeld links neben den Kreis eingetragen.
Dieser Name wird den Dozenten angezeigt.

Anschließend wird das zugehörige Artefakt ausgewählt.
Neben den Pfeil befindet sich ein Dropdown Menü für ihre Auswahl des zugehöriges Artefakts. Es kann nur ein Artefakt ausgewählt werden.

Letztendlich muss der Button “Save” gedrückt werden.

Das Package wurde erfolgreich erstellt.

# [](#header-1)Paket bearbeiten

Um ein Package zu erstellen, öffnen Sie die ProjectAPI (ÜPL) und wählen Sie den Reiter Package aus mit Ihrem Web Browser. Nun sehen Sie eine Liste mit bereits erstellten Packages. Drücke Sie den “Paper&Pen”-Button um Änderungsoptionen zu erhalten.
Der X-Button dient zur löschung des gesamten Pakets.


![](https://i.imgur.com/cmn1lXf.png)

Hier kann den Paket durch die Dropdown-Liste ein anderes Artefakt zugeordnet werden.
Diese Zuordnung wird dementsprechend sofort gespeichert.
Änderungen sind jederzeit möglich.

![](https://i.imgur.com/bqhtoYM.png)

Das Package wurde erfolgreich bearbeitet.

[back](./)