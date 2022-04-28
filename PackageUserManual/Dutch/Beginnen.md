# Beginnen met Netherlands3D in Unity

Download en installeer Unity3D (<https://store.unity.com/>)

Open een nieuw 3D-project

Open de package-manager (window package manager)

![](./imgs/beginnen/image1.png)

Klik op het + icoon in de linkerbovenhoek, kies voor "Add package from git URL", vul daar in "<https://github.com/Amsterdam/Netherlands3D.git?path=/Packages/Netherlands3D/>" en klik op "Add".

![](./imgs/beginnen/image2.png)

![](./imgs/beginnen/image3.png)

Unity gaat nu even iets voor zichzelf doen en laat daarna, als alles goed gegaan is, de package Netherlands3D zien in de package-manager.

![Afbeelding met tekst Automatisch gegenereerde
beschrijving](./imgs/beginnen/image4.png)

Onder het kopje "Samples" staat een lijst met pakketjes, dit zijn de modules in netherlands3D die je kunt toevoegen aan je eigen project.

![](./imgs/beginnen/image5.png)

We beginnen met het TileSystem. Dit is de module die het 3D-model van de stad laat zien. 
Klik hiervoor op het knopje "Import" naast "TileSystem".

Unity heeft nu in de AssetFolder een mapje "Samples"aangemaakt met daarin een mapje "Netherlands3D" en daarin een mapje met een
versienummer. In dit mapje staat alles wat je nodig hebt om het 3D-model van de stad in het project te laten zien.

Unity heeft ook in de Console-window een berg foutmeldingen gegenereerd.
Dit komt omdat netherlands3D nog een extra package nodig heeft, genaamd "Universal Render Pipeline".

## Universal Render Pipeline

Om de Universal Render Pipeline toe te voegen aan ons project gaan we weer naar de packageManager en kiezen we in het pulldown-menu voor "Unity Registry":

![](./imgs/beginnen/image6.png)

In de lijst met beschikbare packages selecteren we "Univeral RP" en klikken op "Install".

![Afbeelding met tekst Automatisch gegenereerde
beschrijving](./imgs/beginnen/image7.png)

Unity gaat vervolgens weer even aan de slag met importeren van de package. 
Wanneer we nu in de console-window op "clear" klikken zouden er geen foutmeldingen meer tevoorschijn moeten komen.

Nu maken we in de AssetFolder een nieuwe map aan met de naam "RenderPipeline".

![Afbeelding met tekst Automatisch gegenereerde
beschrijving](./imgs/beginnen/image8.png)

Geef een rechter-muisklik op deze nieuwe map en kies voor CreateRenderingUniversal Render PipelinePipeline Asset (Forward Renderer)

![](./imgs/beginnen/image9.png)

Unity geeft je nu de optie om de naam van dit nieuwe bestand te wijzigen, maar dat is niet nodig. Geef een enter om de voorgestelde naam te accepteren. Direct na het accepteren van de bestandnaam maakt Unity nog een tweede bestand aan.

![](./imgs/beginnen/image10.png)

De laatste handeling die we moeten doen is aan Unity vertellen dat deze, zojuist gemaakt bestandjes, gebruikt moeten worden door de Universal
Render Pipeline. Dit doen we in de project Settings ( Edit project Settings. In het project settings menu kiezen we in de linker kolom voor "Graphics".)

![](./imgs/beginnen/image11.png)

Boven in het Graphicsmenu klikken we op het rondje naast het invulveld bij Scriptable Render Pipeline Settings.

![](./imgs/beginnen/image12.png)

In het menuutje dat opent dubbelklikken we op het bestandje dat we zojuist hebben aangemaakt.

![](./imgs/beginnen/image13.png)

## TileSystem gebruiken

Om het tilesystem in actie te zien gaan we in de assetsFolder naar de eerder geimporteerde Sample. 
In deze sample openen we de map "Scenes".

![](./imgs/beginnen/image14.png)

Vervolgens slepen we het bestand "LoadLayersFromScene" naar de hierarchy.

![Afbeelding met tekst Automatisch gegenereerde
beschrijving](./imgs/beginnen/image15.png)

Om een goed beeld te kunne krijgen willen we de Main Camera nog even verplaatsen. Dit doen we door in de Hierarchy de Main Camera aan te klikken:

![Afbeelding met tekst Automatisch gegenereerde
beschrijving](./imgs/beginnen/image16.png)

En vervolgens in de inspector de Position en Rotation aan te passen.

![Afbeelding met tekst, schermafbeelding, monitor, zwart Automatisch
gegenereerde
beschrijving](./imgs/beginnen/image17.png)

Nu we toch in de Inspector voor de camera bezig zijn zetten we ook onder het kopje "Camera", "Projection", "Clipping plane" de afstand voor Far even van 1000 op 6000.

Klik vervolgens op **"Play"**, et voila, we zien het 3D-model van Amsterdam.
