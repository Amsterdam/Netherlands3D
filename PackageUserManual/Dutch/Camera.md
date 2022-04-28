# Bestuurbare Camera

Om door de wereld te navigeren hebben we een camera nodig die we zelf kunnen besturen. 
Hier hebben we natuurlijk een pakketje voor klaar staan in Netherlands3D.

Controleer eerst of je Unity project juist is ingesteld om met invoer om te gaan.

Onder ‘Edit/Project Settings/Player/Other options’ moet ‘Active input handling’ ingesteld staan op ‘Both’. 

Het is mogelijk dat Unity opnieuw moet starten na het toepassen.

![img](./imgs/camera/image1.png)



Importeer nu het Netherlands3D voorbeeld ‘Cameras’ uit de Package Manager:

![img](./imgs/camera/image2.png)

**Je kunt de Package Manager boven in Unity vinden onder ‘Window/Package Manager’*

Verwijder eerst een eventuele standaard camera uit je eigen scene. Vervolgens kan je FreeCamera en de CameraInputSystemProvider kopieren naar je eigen scene om die camera te gebruiken.

![img](./imgs/camera/image3.png)

Druk vervolgens op **Play** om je scene met de nieuwe camera te testen.



# Camera bediening

## Muis

- **Camera verslepen**: Klik en slepen Linkermuisknop
- **In- en uitzoomen**: Scrollen
- **Draai om punt**: Muiswiel indrukken of Alt + Linkermuisknop
- **Rondkijken**: Ctrl+Linkermuisknop + slepen

## Toetsenbord

- **W of pijl omhoog**: Naar voren schuiven
- **S of pijl omlaag**: Naar achteren schuiven

- **A of pijl naar links**: Naar links schuiven
- **D of pijl naar rechts**: Naar rechts schuiven
- **Q**: Naar links draaien
- **E**: Naar rechts draaien
- **R**: Omhoog draaien
- **F**: Omlaag draaien
- **PageUp**: Camera omhoog bewegen
- **PageDown**: Camera omlaag bewegen

## Gamepad

- **Linker Stick**: Camera in richting van stick bewegen
- **Rechts Stick**: Camera draaien
- **Linker Stick indrukken**: Omhoog bewegen
- **Rechter Stick indrukken**: Omlaag bewegen