Coordinates
===========

Using this package it is possible to 

1. Convert between the following coordinate systems:
   1. Unity units (interpreted as **meters**)
   2. [World Geodetic System 1984 (EPSG:3857, WGS84)](https://nl.wikipedia.org/wiki/WGS_84)
   3. [Rijksdriehoekscoördinaten](https://nl.wikipedia.org/wiki/Rijksdriehoeksco%C3%B6rdinaten) + [NAP height](https://en.wikipedia.org/wiki/Amsterdam_Ordnance_Datum) (RD / [EPSG:7415](https://epsg.io/7415))
   4. [Earth-centered, Earth-fixed (EPSG:4936, ECEF)](https://en.wikipedia.org/wiki/Earth-centered,_Earth-fixed_coordinate_system)

## Usage

This package exposes a unit called Coordinate that is related to a specific Coordinate Reference System (CRS)
and represents a _coordinate_ in that CRS using 2 or 3 or more _points_.

Example, describing longitude 10.02, latitude 20.01 and height 0 in the WGS-84, or EPSG:3857, Coordinate
Reference System.

```
$coordinate = new Coordinate(CoordinateSystem.EPSG_3857, 10.02, 20.01, 0);
```

### Converting to another CoordinateSystem

With such a unit, you can to convert it to a coordinate in another CRS using the CoordinateConverter
service's ConvertTo method:

```
$rdCoordinate = CoordinateConverter(coordinate, CoordinateSystem.EPSG_7415);
```

### Converting to a Vector3

> Important: this feature is considered alpha and is subject to change.

To represent a Coordinate in Unity worldspace, it is possible for the RD/EPSG:7415 and ECEF/EPSG:4936 Coordinate Systems
to translate to and from a Unity Vector3. This is done by taking -respectively- the relativeCenter property from the 
EPSG4936 or EPSG7415 class use that as the Vector3.Zero in worldspace. The distance in **meters** between that relative 
center and the given Coordinate is calulated -in meters- and returned as a Vector3 indicating that location.

```
$rdCoordinate = CoordinateConverter(coordinate, CoordinateSystem.Unity);
```

In a future version of this package, the conversion to and from Unity units will be moved to a specialized Floating 
Origin solution (see MovingOrigin and MovingOriginFollower for a start) but for backwards compatibility this is 
supported by the CoordinateConverter.

## Backwards compatibility

In Netherlands3D, we used to make use of conversion methods on the CoordinateConverter -such as RDtoWGS84- and
Vector3 classes per Coordinate System. This architecture is not scalable to support the plethora of CRS out there,
and as such these are all deprecated and replaced by the Coordinate class and the ConvertTo method in the 
CoordinateConverter.