/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System;
using System.Text.RegularExpressions;

namespace Netherlands3D.WMS
{
    public class WMSImageUrlTemplate
    {
        //Mandatory bbox parameters
        public const string bboxXMin = "{XMin}";
        public const string bboxYMin = "{YMin}";

        public const string bboxXMax = "{XMax}";
        public const string bboxYMax = "{YMax}";

        //Optional
        public const string widthPlaceholder = "{Width}";
        public const string heightPlaceholder = "{Height}";

        private readonly string url;
        public string Url => url;

        private int width = 256;
        private int height = 256;

        public WMSImageUrlTemplate(string url, int defaultWidth = -1, int defaultHeight = -1)
        {
            if (!HasBboxCoordinates(url))
            {
                throw new ArgumentException("The WMS Url is invalid", nameof(url));
            }

            //Apply new default height if parameters exist
            if (HasSize(url) && defaultWidth > 0 && defaultHeight > 0)
            {
                this.width = defaultWidth;
                this.height = defaultHeight;

                //Try to replace width and height placeholders
                this.url = this.url
                        .Replace(widthPlaceholder, this.width.ToString())
                        .Replace(heightPlaceholder, this.height.ToString());
            }
        }
        public bool HasBboxCoordinates(string url)
        {
            //Validate if the URL at least contains the bbox coordinates
            string pattern = $@"{bboxXMin}|{bboxYMin}|{bboxXMax}|{bboxYMax}";
            return Regex.IsMatch(url, pattern);
        }

        public bool HasSize(string url)
        {
            //Validate if the URL has width and height values
            string pattern = $@"{widthPlaceholder}|{heightPlaceholder}";
            return Regex.IsMatch(url, pattern);
        }

        public string GetUrl(double xMin = 0, double yMin = 0, double xMax = 0, double yMax = 0)
        {
            //Replace bbox parameter placeholders
            string url = this.url
                .Replace(bboxXMin, xMin.ToString())
                .Replace(bboxYMin, yMin.ToString())
                .Replace(bboxXMax, xMax.ToString())
                .Replace(bboxYMax, yMax.ToString());

            return url;
        }
    }
}
