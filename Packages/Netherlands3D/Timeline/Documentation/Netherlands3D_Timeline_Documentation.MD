# Netherlands3D Timeline Documentation

## Gebruiken

### Timeline UI Prefab
1. Open een lege scene (of een scene waarin je het wilt gebruiken).
2. Create een Canvas gameobject in de hierarchy
    2.1 Als je niet weet hoe dat moet volg deze stappen
    2.2 Ga met de muis naar de hierarchy window
    2.3 Rechtermuisklik terwijl de muis niet op een gameobject staat om het "help" menu te openen.
    2.4 Navigeer naar UI > Canvas
3. Navigeer nu naar Packages > Netherlands3D > Timeline > Runtime > Prefabs > Timeline UI
4. Sleep deze prefab in de hierarchy window op het net aangemaakte Canvas (of een bestaande canvas gameobject) zodat het dus een kind gameobject is van de canvas
5. De data die word ingeladen voor de UI bevind zich in een timeline data Scriptable Object. Als deze niet geassigned is in het timeline UI inspector script moet je de sample folder nog importeren vanuit de package manager bij Netherlands3D

### Eigen Data Toevoegen
1. De timeline werkt dus met scriptable object data. Om deze aan te maken navigeer je naar de project window en in een folder van keuze rechtermuisklik > Create > ScriptableObjects > Timeline > Timeline Data
2. Deze asset is een bestand dat een verzameling van `Time Periods` vasthoud. Zo kun je meerdere Timeline Data assets hebben om je data te orderen.
3. Om een `Time Period` assset aan te maken (een asset waar dus informatie zit over een bepaalde gebeurtenis met begin en eindtijd) doe je in de project window rechtermuisklik > Create > ScriptableObjects > Timeline > Time Period
4. Dit bestand houd dus informatie vast over een gebeurtenis in de tijd waaraan code kan worden gekoppelt. Voor meer informatie over wat alles doet kun je met de muis over de inspector elementen heengaan voor tooltips.
5. Er zitten ook een aantal events aan die worden getriggerd door de Timeline UI als deze data is ingeladen en hieraan kun je dus je code koppelen.
6. Voor meer informatie hoe je dit zou kunnen koppelen kun je de samples van Timeline erbij importeren (via de package manager en dan in Netherlands3D) en dan in het script `EventsCallbackTest` kijken.