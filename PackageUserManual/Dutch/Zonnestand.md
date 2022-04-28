# Zonnestand + Shaduwen

Het Netherlands3D pakketje bevat een voorbeeld om de zonnestand en schaduwen te laten bepalen aan de hand van een locatie en de datum/tijd.

Importeer het voorbeeld vanuit de Package Manager*:
![img](./imgs/zonnestand/image1.png)*Je kunt de Package Manager boven in Unity vinden onder ‘Window/Package Manager’*

![img](./imgs/zonnestand/image2.png)

De voorbeeld scene heeft een prefab met de naam ‘Sun’.
Deze bevat het script SunTime waarmee je de locatie en tijd kan instellen. 
Stel de GPS locatie van dit script in via de Inspector:

![img](./imgs/zonnestand/image3.png)

Het Canvas in de Scene laat een voorbeeld interface zien waarmee een gebruiker de tijd en animatie snelheid kan aanpassen.
Je kan de Sun prefab kopieren naar je eigen scene.
Zorg er wel voor dat je een eventueel bestaand ‘Directional Light’ verwijder uit je scene. 
De Directional Light in de Sun prefab gaat namelijk zorgen voor de belichting en schaduwen.
Wanneer je ook het stukje interface wil gebruiken kan je ook het Canvas en EventSystem naar je eigen scene kopieren.