Address Search
==============

This package provides an auto-suggest Dropdown UI element. With this, the building associated with an address 
can be found, and its BAG id returned using an event.

When the option to move the camera is enabled, it will also move the Camera to a location matching the Coordinates 
for the given object and tilt the Camera towards it.

Usage
-----

Create a Canvas and InputSystem, and drop the SearchPanel prefab on. After that you can enter Play Mode and try it out 
by typing 2 characters or more.

The SearchPanel is restricted to search for addresses confined to a single city -by default: `Amsterdam`-. To change 
this, see the next chapter on customizing.

When an address is selected, the `GotBuilding` event is fired (see the section on events) and the presented list of BAG 
identifiers can be used in your own code. Do note, by default the Camera will also move to that location; this can be 
disabled using the `Move Camera` option.

Customizing
-----------

The Search Panel contains a GameObject "SearchInput", whose Address Search component has all customizable properties, here are the most important ones and their effect:

Search within City (string)
: Restrict the search to the given name of a city.

Characters needed (int)
: Start searching after the given number of characters

Move Camera (bool)
: Whether to move the Camera to the position of the found address / object and tilt the camera down.

Events
------

The Address Search component makes use of Scriptable Object events as provided by the Netherlands3D Core package.

### Listening

Clear Input (Trigger Event)
: When this event is triggered, the input is cleared, the dropdown closed and it stops searching.

### Emitting

Toggle Clear Button (BoolEvent)
: Whether to show or hide the Clear button.

Got Building (String List Event)
: When the user selects a suggestion, this event is fired with a list of strings representing the BAG identifiers 
associated with that address.
