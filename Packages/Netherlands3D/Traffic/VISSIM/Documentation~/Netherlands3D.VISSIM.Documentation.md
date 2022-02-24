# Documentation Netherlands3D VISSIM

VISSIM stands for:
"Verkehr In Städten - SIMulationsmodell"
And was created in germany.

## Traffic.File (Traffic File Importer)
Loads files used for traffic (VISSIM)
- When loaded it invokes SO with data

## Traffic.Data
Class containing data used for traffic

## VISSIM.ConverterFZP
- converts file .fzp to Traffic.Data

## Traffic.Visualizer
When Traffic (Vissim) data is loaded it gets displayed by this monobehavior
- Reference to SO with data, if it gets invoked the visualizer updates