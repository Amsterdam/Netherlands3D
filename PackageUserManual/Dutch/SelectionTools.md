# SelectionTools (Geavanceerde gebruikers)

Het 'SelectionTools' pakket bied enkele voorbeelden om gebieden of lijnen te selecteren door met de muis te tekenen.
Deze tool is bedoeld voor geavanceerde Netherlands3D gebruikers met Unity ervaring.
Deze tools kunnen gebruikt worden om, als voorbeeld, API's te bevragen met boundingbox of polygoon queries.

## Rechthoekig gebied selecteren

Om een rechthoeking gebied te selecteren gebruik je de AreaSelection tool, te vinden in de voorbeeldscene 'AreaSelection'

Om deze tool samen te laten werken met de FreeCamera moet **Shift ** ingedrukt worden om vervolgens met de klik+sleep actie van de muis de selectie te maken. (Shift blokkeert de sleepactie van de camera)

De output van dit systeem is vervolgens een event met een Bounds object.
In het voorbeeld wordt deze omgezet naar een RD boundingbox zoals deze vaak gebruikt wordt in API's, en in beeld getoond als tekst:

**bbox=120700,486800,121200,487200**

## Polygoon selecteren

De voorbeeldscenes PolygonSelection en PolygonSelection_WithHandles laten een voorbeeld zien van het PolygonSelection systeem waarmee een gebruiker met een muisklik punten kan plaatsen om zo de lijn van een polygoon te tekenen.

De scenes gebruiken dezelfde tool, alleen is bij de 'WithHandles' scene de mogelijkheid om handles (handgrepen) te plaatsen bij het plaatsen van punten ingeschakeld. Deze handles maken het mogelijk om de lijn van de polygoon te 'finetunen' door de punten te verschuiven na het tekenen van de vorm.

De output van het PolygonSelection systeem is een event met een Vector3 List. Deze lijst bevat alle coordinaten van de punten in de volgorde hoe deze geplaatst zijn. 
Met de 'Winding Order' kan deze lijst op volgorde gezet worden zodat de punten met de klok mee lopen, of juist tegen de klok in.

Met twee losse events voor 'Selected Polygon Area' en 'LineHasChanged' kan de lijst verstuurd worden bij het plaatsen van een punt, en/of bij het afronden/sluiten van de vorm of lijn.

## Lijn plaatsen

HetPolygonSelection systeem kan ook niet-gesloten polygonen tekenen. In dat geval hebben we het over een lijn.
Door 'Require Closed Polygon' uit te zetten kan een polygoon afgerond worden met **Enter ** zonder dat deze afgesloten wordt.
