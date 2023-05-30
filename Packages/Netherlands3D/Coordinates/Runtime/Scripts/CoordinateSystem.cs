/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

namespace Netherlands3D.Coordinates
{
    /// <summary>
    /// Supported coordinate systems
    /// </summary>
    public enum CoordinateSystem
    {
        Unity = -1, // Deprecated, Unity should not be considered a coordinate system but a translation by the MovingOrigin
        EPSG_3857 = 3857, // WGS 84 / Pseudo-Mercator
        EPSG_4936 = 4936, // ETRS98-ECEF
        EPSG_7415 = 7415,

        // Commonly used aliases
        WGS84 = EPSG_3857, // As an alias for WGS84, we assume the ArcGIS/Google Maps variety
        RD = EPSG_7415, // As an alias for RD, we assume RD Amersfoort New + NAP / RD3D
    }
}
