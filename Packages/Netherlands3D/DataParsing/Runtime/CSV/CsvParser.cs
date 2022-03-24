﻿/*
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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Netherlands3D.Core
{
    /// <summary>
    /// Csv parser using regular expression
    /// Seperator being used is a semicolon
    /// </summary>
    public class CsvParser
    {
        private static string splitPattern = @";(?=(?:[^""]*""[^""]*"")*[^""]*$)";

        /// <summary>
        /// Reads a csv string and returns a list of rows and columns
        /// </summary>
        /// <param name="csv">the csv string</param>
        /// <param name="startfromrow">Start reading from given row index</param>
        /// <returns></returns>
        public static List<string[]> ReadLines(string csv, int startfromrow)
        {

            var lines = csv.Split('\n');

            var rows = new List<string[]>();

            for (int i = startfromrow; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var cells = Regex.Split(line, splitPattern);

                for (int c = 0; c < cells.Length; c++)
                {
                    cells[c] = cells[c].Trim().Trim('"');
                }
                rows.Add(cells);
            }

            return rows;
        }
    }
}