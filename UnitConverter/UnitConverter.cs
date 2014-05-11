/* *
 *  Copyright 2014 Forrest Brennen
 * 
    This file is part of UnitConverter.

    UnitConverter is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    UnitConverter is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with UnitConverter.  If not, see <http://www.gnu.org/licenses/>.
 * */

using System;
using System.Collections.Generic;
using System.IO;

namespace Unit_Converter
{
    /// UnitConverter
    /// <summary>
    /// This converts a length from one unit to another. Either Imperial or Metric units
    /// may be used, and are defined in the Unit class (below). Each Unit should be
    /// stored in the Dictionary which maps unit abbreviations to Units (this will require
    /// recompilation when new units are added, though it would be easy to change that if
    /// required).
    /// </summary>
    /// <remarks>
    /// SPECIFICATION:
    /// <list type="Bullet">
    /// <item>Command-line application.
    /// </item>
    /// <item>Accepts a single string as input.
    /// </item>
    /// <item>The string must be of the form "[length] [unit] in [unit]".
    /// </item>
    /// <item>Outputs a single line to standard out (Console.OpenStandardOutput()).
    /// </item>
    /// <item>The output string must be of the form "[length] [unit] equals [newlength] [unit]".
    /// </item>
    /// <item>The units to be included are: yards (yd), feet (ft), inches (in), centimetres (cm),
    /// and metres (m).
    /// </item>
    /// </list>
    /// </remarks>
    internal static class UnitConverter
    {
        /// <summary>
        /// This holds all the units that may be converted between, and serves
        /// as the master record for those values and their abbreviations.
        /// </summary>
        /// <remarks>
        /// Additional units may be added by appending them to the Dictionary.
        /// Unit abbreviations must be lowercase, and contain no punctuation.
        /// 
        /// ***This is the only code which needs to be modified to add new units***
        /// </remarks>
        /// <seealso cref="Unit"/>
        private static readonly Dictionary<string, Unit> abbreviationToUnit = new Dictionary<string, Unit>()
            {   {"in", new Unit(1, "Inch", "Inches", Unit.MeasurementSystem.Imperial)},
                {"ft", new Unit(1/12.0, "Foot", "Feet", Unit.MeasurementSystem.Imperial)},
                {"yd", new Unit(1/36.0, "Yard", "Yards", Unit.MeasurementSystem.Imperial)},
                {"cm", new Unit(1, "Centimetre","Centimetres", Unit.MeasurementSystem.Metric)},
                {"m", new Unit(1/100.0, "Metre", "Metres", Unit.MeasurementSystem.Metric)}                
            };
            
        /// <summary>
        /// Holds a names-only reverse lookup table mapping (lowercase) full unit 
        /// names to their abbreviations, so we can use full unit names during 
        /// conversion. This Dictionary is generated based on the
        /// <paramref name="abbreviationToUnit"/> table. It's done automatically 
        /// to make it easier to add new Units later.
        /// </summary>
        internal static readonly Dictionary<string, string> pluralUnitNameToAbbreviation;

        /// <summary>
        /// This does the same, but for singular unit names.
        /// </summary>
        internal static readonly Dictionary<string, string> singularUnitNameToAbbreviation;

        static UnitConverter()
        {
            pluralUnitNameToAbbreviation = new Dictionary<string, string>();
            singularUnitNameToAbbreviation = new Dictionary<string, string>();
            foreach(string abbreviation in abbreviationToUnit.Keys)
            {
                singularUnitNameToAbbreviation.Add(
                    abbreviationToUnit[abbreviation].SingularName.ToLower(), 
                    abbreviation);
                pluralUnitNameToAbbreviation.Add(
                    abbreviationToUnit[abbreviation].PluralName.ToLower(), 
                    abbreviation);
            }
        }

        private static double Convert(double value, Unit source, Unit target)
        {
            return source.ConvertTo(value, target);
        }

        /// <summary>
        /// A version of TryGetValue() which checks to see if a candidate unit
        /// exists in this UnitConverter. Both unit abbreviations and full unit
        /// names are checked.
        /// </summary>
        /// <param name="candidate">A string which may represent a unit.</param>
        /// <param name="unit">The corresponding Unit, if it exists.</param>
        /// <returns>True if the candidate exists (in which case the corresponding Unit
        /// will be returned in the <paramref name="unit"/> parameter), and false 
        /// otherwise.</returns>
        private static bool IsValidUnit(string candidate, out Unit unit)
        {
            string trimmedCandidate = candidate.TrimEnd(new char[] { '.' }).ToLower();
            if (abbreviationToUnit.TryGetValue(trimmedCandidate, out unit))
            {
                return true;
            }
            else 
            {
                string abbreviation;
                if (pluralUnitNameToAbbreviation.TryGetValue(trimmedCandidate, out abbreviation) ||
                    singularUnitNameToAbbreviation.TryGetValue(trimmedCandidate, out abbreviation))
                {
                    abbreviationToUnit.TryGetValue(abbreviation, out unit);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Outputs human-readable output of all the available Units contained
        /// in UnitConverter.
        /// </summary>
        internal static void ListUnits(StreamWriter outStream)
        {
            Guard.ArgumentIsNotNull(outStream, "outStream");
            outStream.WriteLine("Available units:");
            foreach (string key in abbreviationToUnit.Keys)
            {
                outStream.WriteLine("\t" + abbreviationToUnit[key].PluralName + " (" + key + ")");
            }
        }

        internal static void ParseArgs(string[] args, StreamWriter outStream)
        {
            if (args.Length == 1)
            {
                // Remove quotes at the ends and check to see if we have the three args we want.
                string[] checkForSingleString = args[0].Trim(new char[] { ' ', '\'', '"' }).Split();
                if (checkForSingleString.Length == 4)
                {
                    UnitConverter.ParseConversionArgs(checkForSingleString[0],
                        checkForSingleString[1],
                        checkForSingleString[3],
                        outStream);
                }
                else
                {
                    switch (args[0])
                    {
                        case "-?":
                        case "-help":
                            UnitConverter.PrintHelp(outStream);
                            break;
                        case "-list-units":
                            UnitConverter.ListUnits(outStream);
                            break;
                        default:
                            outStream.WriteLine("Unknown option: " + args[0]);
                            outStream.Write("Try 'UnitConverter.exe -help' for options.");
                            break;
                    }
                }
            }
            else if (args.Length == 4)
            {
                UnitConverter.ParseConversionArgs(args[0], args[1], args[3], outStream);
            }
            else
            {
                outStream.WriteLine("Wrong number of arguments: " + args.Length + " (expected 1 or 4).");
                outStream.Write("Try 'UnitConverter.exe -help' for options.");
            }
        }

        private static void ParseConversionArgs(string length, string unit1, string unit2, StreamWriter outStream)
        {
            double sourceLength;
            if (!Double.TryParse(length, out sourceLength))
            {
                outStream.WriteLine("Error: " + length + " does not appear to be a number!");
                return;
            }

            Unit sourceUnit;
            if (!UnitConverter.IsValidUnit(unit1, out sourceUnit))
            {
                outStream.WriteLine("Error: " + unit1 + " is not a valid unit for conversion.");
                outStream.WriteLine("Try -list-units to see a list of valid units.");
                return;
            }

            Unit targetUnit;
            if (!UnitConverter.IsValidUnit(unit2, out targetUnit))
            {
                outStream.WriteLine("Error: " + unit2 + " is not a valid unit for conversion.");
                outStream.WriteLine("Try -list-units to see a list of valid units.");
                return;
            }

            double result = UnitConverter.Convert(sourceLength, sourceUnit, targetUnit);
            outStream.Write(length + " " + UnitConverter.pluralUnitNameToAbbreviation[sourceUnit.PluralName.ToLower()] + 
                " equals " + 
                result + " " + UnitConverter.pluralUnitNameToAbbreviation[targetUnit.PluralName.ToLower()]);
        }

        private static void PrintHelp(StreamWriter outStream)
        {
            outStream.WriteLine("NAME");
            outStream.WriteLine("\tUnitConverter.exe -- convert a length from one unit to another.");
            outStream.WriteLine();
            outStream.WriteLine("SYNOPSIS");
            outStream.WriteLine("\tUnitConverter.exe -? | -help");
            outStream.WriteLine("\tUnitConverter.exe -list-units");
            outStream.WriteLine("\tUnitConverter.exe [\"]<length> <unit1> in <unit2>[\"]"); // Why you would want a single string, I dunno... =)
            outStream.WriteLine();
            outStream.WriteLine("DESCRIPTION");
            outStream.WriteLine("\t-? or -help: displays this message.");
            outStream.WriteLine("\t-list-units: lists the names of the units accepted by the application.");
            outStream.WriteLine("\t<length>: a decimal length to convert. Negative lengths are recognized.");
            outStream.WriteLine("\t<unit1>: the starting units of <length>.");
            outStream.WriteLine("\t<unit2>: the units to convert <length> to.");
            outStream.WriteLine();
            outStream.WriteLine("EXAMPLES");
            outStream.WriteLine("\tUnitConverter.exe 10 ft in cm");
            outStream.WriteLine("\tUnitConverter.exe 2.5 metres in inches");
        }

        static void Main(string[] args)
        {
            using (StreamWriter outStream = new StreamWriter(Console.OpenStandardOutput()))
            {
                UnitConverter.ParseArgs(args, outStream);
            }
        }
           
        /// Unit
        /// <summary>
        /// Represents a specific unit of measurement, and contains all the
        /// necessary information to convert from another Unit to this one.
        /// </summary>
        /// <remarks>
        /// One Unit is converted to another by in a three step process:
        /// <list type="bullet">
        /// <item>convert the source value to a base unit in the same measurement 
        /// system (either cm or in).
        /// </item>
        /// <item>convert the target value from one measurement system to another.
        /// </item>
        /// <item>convert from the new measurement system's base units to the 
        /// target Unit.
        /// </item>
        /// </list>
        /// </remarks>
        internal class Unit
        {            
            public enum MeasurementSystem { Metric, Imperial };
            public static readonly double InToCm = 2.54;                    

            /// <summary>
            /// Initialize a specific unit of measurement, and fix all of its
            /// parameters. 
            /// </summary>
            /// <param name="baseUnitConversionFactor">How to convert to this Unit from the 
            /// measurement system's base value (either cm or in). For example,
            /// <paramref name="baseUnitConversionFactor"/> for meters would be 0.01,
            /// because 1cm = 0.01m .</param>
            /// <param name="pluralName">The unit name.</param>
            /// <param name="system">The measurement system of the unit.</param>
            public Unit(double baseUnitConversionFactor, 
                string singularName, 
                string pluralName, 
                MeasurementSystem system)
            {
                Guard.ArgumentGreaterThan(baseUnitConversionFactor, 0, "baseUnitConversionFactor");

                if (singularName.EndsWith(".") || pluralName.EndsWith("."))
                {
                    throw new ArgumentException(
                        "Unit names should not be abbreviated or end with a period.");
                }

                BaseUnitConversionFactor = baseUnitConversionFactor;
                SingularName = singularName;
                PluralName = pluralName;
                Type = system;
            }

            public double BaseUnitConversionFactor { get; private set; }
            public string SingularName { get; private set; }
            public string PluralName { get; private set; }
            public MeasurementSystem Type { get; private set; }  

            /// <summary>
            /// Converts a value from this Unit to a different one.
            /// </summary>
            /// <param name="value">The value to convert.</param>
            /// <param name="target">The unit to convert <paramref name="value"/> to.</param>
            /// <returns><paramref name="value"/> converted into the target Unit.</returns>
            public double ConvertTo(double value, Unit target)
            {
                double sourceToBaseUnits = value / this.BaseUnitConversionFactor;
                double unitClassConversion = 1;
                if (this.Type != target.Type)
                {
                    unitClassConversion = (this.Type == Unit.MeasurementSystem.Imperial ? 
                        InToCm : 
                        1 / InToCm);
                }                
                double targetFromBaseUnits = target.BaseUnitConversionFactor;

                return sourceToBaseUnits * unitClassConversion * targetFromBaseUnits;
            }
        }
    }
}
