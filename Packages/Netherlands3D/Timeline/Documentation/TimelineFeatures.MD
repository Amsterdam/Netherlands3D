# Timeline Features
A simple document to descripe features needed for timeline

1. A base scriptable object where all data is kept
2. Switch function from year to year to specifics of a year
3. A year has layers (for trees, buildings, events etc.) A layer is color coded
4. A moveable line that the user can drag to see the changes in the scene
5. dropdown to change persision, years, months, days

Event: startdate-enddate, category, name, description


---
Development

- Time bar has a width, based on a hardcoded width the time bar width gets devided to place its dates on it evenly.
- The current center bar, based on its x position from 0, produces its selected pixel value.
Then in getting the info from the date bar get the assosiated date through the pixel value

processing data
- know what the date range is that is displayed
- based on current date, loop trough data, and display if the dates ranges in visable date range

event update
- on timebar move
- check if we need to add or remove events based on minmax date
- loop trough all events
- for each event get corresponding -x / x position based on its date
- dictionary that keeps track off all visable dates, and its corresponding local x positions