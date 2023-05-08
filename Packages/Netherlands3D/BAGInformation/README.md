BAG Information
===============

With this package, it is possible to display information from the BAG for a feature that has a BAG id, such as a 
building.

The core of this package revolves around the `GeoJSONPropertyLoader` component, which can call a GeoJSON webservice and 
emits a `StringListEvent` for each property on the Feature in the response.

It is recommended to open the sample and play with that to get a feeling how this package connects with the other 
packages.

Depends on
----------

* Core package
* Interface package
* Camera package
* Input package

Usage
-----

First, make sure that you have installed the Core package and set up a TileHandler with a Buildings layer. On that 
Building layer you should make sure that a "Select Sub Objects" component is set up with the following events from 
this package:

* `Clicked On Object` should have the `ClickedOnBuilding` event 
* `Selected Ids On Click` should have the `SelectedBuildingIDs` event 
* `Clicked On Position` should have the `ClickedOnPosition` event 

After that, you can add the following prefabs:

BuildingSelection
: Emits the `Clicked On Position` event, which is picked up by the Building layer. It listens to the 
  `SelectedBuildingIDs` event; with it, it will query the BAG GeoJSON endpoint using the BAG id 
  in the event. 
: For each property in that Feature, a Key/Value pair is returned using the 
  `LoadedBAGBuildingProperty` event and/or the `LoadedBAGResidenceProperty` event.

BagBuildingPropertiesPanel
: Displays the properties of a Building when a Building is selected. This is done by listening to the 
  `LoadedBAGBuildingProperty` and adding an entry with the key as a label, and the value as the property value in a 
  list.

BAGResidencePropertiesPanel
: Displays the properties of all Residences when a Building is selected. This is done by listening to the
  `LoadedBAGResidenceProperty` and adding an entry with the key as a label, and the value as the property value in a 
  list.

Events
------

This package makes use of Scriptable Object events as provided by the Netherlands3D Core package. The following events 
are in use, in order of operation:

ClickedOnPosition (Vector3 Event)
: Invoked by the `InputEvents` component to signal to `Select Sub Objects` component on the Building layer that it should 
  attempt to find a matching sub-object.

ClickedOnBuilding (Bool Event)
: Invoked by the `Select Sub Objects` component to indicate whether an object was found or not.
: Consumed by the property panels to 1) show or hide the panel and 2) destroy the children of the ScrollView's 
  `Content`, based on the value in this event.

SelectedBuildingIDs (StringList Event)
: Invoked by the `Select Sub Objects` component to pass the BAG ids for the found objects.
: Consumed by the `GeoJSON Property Loader` component to start loading Feature Collections from a GeoJSON webservice.

LoadedNewBuilding (Trigger Event)
: Invoked by the `GeoJSON Property Loader` component for each Feature in the response

LoadedBagBuildingProperty (StringList Event)
: Invoked by the `GeoJSON Property Loader` component for each property for a Feature in the response
: Consumed by the `BAGBuildingPropertiesPanel` property panel to add a new property, the first element of this list
  is the label to display, and the second element is the value.

LoadedNewAddress (Trigger Event)
: Invoked by the `GeoJSON Property Loader` component for each Feature in the response 
: Consumed by the `BAGResidencePropertiesPanel` to add a separator in the list 

LoadedBagResidenceProperty (StringList Event)
: Invoked by the `GeoJSON Property Loader` component for each property for a Feature in the response
: Consumed by the `BAGResidencePropertiesPanel` property panel to add a new property, the first element of this list 
  is the label to display, and the second element is the value.
