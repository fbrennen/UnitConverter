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
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 * */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using Unit_Converter;

namespace UnitConverterTest
{
    [TestClass]
    public class UnitConverterTests
    {
        /// <summary>
        /// The static constructor's only job is to populate the unitNameToAbbreviation
        /// dictionary.
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsTrue(UnitConverter.pluralUnitNameToAbbreviation.ContainsKey("feet"),
                "Unit converter should have feet in it.");
            Assert.IsFalse(UnitConverter.pluralUnitNameToAbbreviation.ContainsKey("Feet"),
                "Unit converter should only store lowercase keys.");
        }

        /// <summary>
        /// See the Spec in UnitConverter for how the output should be structured.
        /// While the spec requires a single string as input, it seemed prudent to 
        /// support separated inputs as well, as that's how someone would actually
        /// expect to use the program.
        /// 
        /// In addition, the spec only requires abbreviations, but a user might
        /// add a period to one or not. In addition, it would be useful to support
        /// full names too, both singular and plural.
        /// </summary>
        [TestMethod]
        public void CheckOutput()
        {
            using (MemoryStream memStream = new MemoryStream())
            using (StreamWriter testStream = new StreamWriter(memStream))
            {
                string input = "12 in to ft."; // Note the period.
                UnitConverter.ParseArgs(new string[] { input }, testStream);
                testStream.Flush();
                string output = Encoding.ASCII.GetString(memStream.ToArray());
                Assert.AreEqual("12 in equals 1 ft", output);
            }

            // Should work with the arguments separated as well.
            using (MemoryStream memStream = new MemoryStream())
            using (StreamWriter testStream = new StreamWriter(memStream))
            {
                string input = "1 inch to centimetres";
                UnitConverter.ParseArgs(input.Split(), testStream);
                testStream.Flush();
                string output = Encoding.ASCII.GetString(memStream.ToArray());
                Assert.AreEqual("1 in equals 2.54 cm", output);
            }
        }

        [TestMethod]
        public void AllUnitsPresentInListUnits()
        {
            using (MemoryStream memStream = new MemoryStream())
            using (StreamWriter testStream = new StreamWriter(memStream))
            {
                UnitConverter.ListUnits(testStream);
                testStream.Flush();
                string[] output = Encoding.ASCII.GetString(memStream.ToArray()).Split('\n');
                // The -2 is for the "Available units:" at the beginning and the final newline.
                Assert.AreEqual(UnitConverter.pluralUnitNameToAbbreviation.Count, 
                    output.Length - 2,
                    "All units not found in -list-units");
                for (int i = 1; i < output.Length - 1; i++)
                {
                    // The line should be: \t<unitName> (<abbrev>)
                    string unitName = output[i].Trim().Split()[0];
                    Assert.IsTrue(UnitConverter.pluralUnitNameToAbbreviation.ContainsKey(unitName.ToLower()),
                        "Did not find unit " + unitName + " in UnitConverter");
                }
            }
        }

        [TestMethod]
        public void DefaultUnitsPresent()
        {
            string[] defaultPluralUnits = { "yards", "feet", "inches", "centimetres", "metres" };
            string[] defaultSingularUnits = { "yard", "foot", "inch", "centimetre", "metre" };
            foreach (string unit in defaultPluralUnits)
            {
                Assert.IsTrue(UnitConverter.pluralUnitNameToAbbreviation.ContainsKey(unit),
                    "Unit " + unit + " should be in the default units list.");
            }
            foreach (string unit in defaultSingularUnits)
            {
                Assert.IsTrue(UnitConverter.singularUnitNameToAbbreviation.ContainsKey(unit),
                    "Unit " + unit + " should be in the default units list.");
            }
        }

        [TestMethod]
        public void ErrorsOnBadArguments()
        {
            using (MemoryStream memStream = new MemoryStream())
            using (StreamWriter testStream = new StreamWriter(memStream))
            {
                string aPoorChoice = "Q inches in ft";
                UnitConverter.ParseArgs(new string[] { aPoorChoice }, testStream);
                testStream.Flush();
                string[] output = Encoding.ASCII.GetString(memStream.ToArray()).Split('\n');
                Assert.AreEqual("Error: Q does not appear to be a number!",
                    output[0].Trim(),
                    "Invalid length didn't produce an error message.");
            }

            using (MemoryStream memStream = new MemoryStream())
            using (StreamWriter testStream = new StreamWriter(memStream))
            {
                string aPoorChoice = "12 qubits in ft";
                UnitConverter.ParseArgs(new string[] { aPoorChoice }, testStream);
                testStream.Flush();
                string[] output = Encoding.ASCII.GetString(memStream.ToArray()).Split('\n');
                Assert.AreEqual("Error: qubits is not a valid unit for conversion.",
                    output[0].Trim(),
                    "Invalid unit didn't produce an error message.");
            }
        }
    }


    [TestClass]
    public class TestsOfTheUnitClass // Or, Unit Unit Tests!
    {
        [TestMethod]
        public void ConstructorTest()
        {
            UnitConverter.Unit testUnit = new UnitConverter.Unit(63, 
                "groat-stadium",
                "groat-stadia", 
                UnitConverter.Unit.MeasurementSystem.Imperial);
            Assert.AreEqual("groat-stadia", 
                testUnit.PluralName, 
                "Plural name not created correctly.");
            Assert.AreEqual("groat-stadium",
                testUnit.SingularName,
                "Singular name not created correctly.");
            Assert.AreEqual(63, 
                testUnit.BaseUnitConversionFactor, 
                "Base unit conversion factor not created correctly.");
            Assert.AreEqual(UnitConverter.Unit.MeasurementSystem.Imperial, 
                testUnit.Type, 
                "Measurement system type not created correctly.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExceptionOnNegativeConversionFactor()
        {
            UnitConverter.Unit testUnit = new UnitConverter.Unit(-1,
                "testUnit",
                "testUnit", 
                UnitConverter.Unit.MeasurementSystem.Imperial);
            Assert.Fail("Creating a Unit with a negative conversion factor should fail.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionOnUnitsWithPeriods()
        {
            UnitConverter.Unit testUnit = new UnitConverter.Unit(1,
                "singularName",
                "seeThePeriod.",
                UnitConverter.Unit.MeasurementSystem.Imperial);
            Assert.Fail("Creating a Unit with a name ending in a period should fail.");
        }

        /// <summary>
        /// 1 inch = 2.54 cm, so 1 semi-inch = .5 inch = 2.54/2 cm = 2.54/4 double-centimetres
        /// </summary>
        [TestMethod]
        public void ConvertBetweenSystems()
        {
            UnitConverter.Unit imperialUnit = new UnitConverter.Unit(2, 
                "semi-inch",
                "semi-inches", 
                UnitConverter.Unit.MeasurementSystem.Imperial);
            UnitConverter.Unit metricUnit = new UnitConverter.Unit(.5, 
                "double-centimetre",
                "double-centimetres", 
                UnitConverter.Unit.MeasurementSystem.Metric);
            Assert.AreEqual(2.54 / 4, 
                imperialUnit.ConvertTo(1, metricUnit), 
                "Imperial to metric conversion failure.");
            Assert.AreEqual(4 / 2.54, 
                metricUnit.ConvertTo(1, imperialUnit), 
                "Metric to imperial conversion failure.");
            Assert.AreEqual(-4 / 2.54,
                metricUnit.ConvertTo(-1, imperialUnit),
                "Negative metric to imperial conversion failure.");
        }

        [TestMethod]
        public void ConvertWithinSystems()
        {
            UnitConverter.Unit inch = new UnitConverter.Unit(1,
                "inch",
                "inches",
                UnitConverter.Unit.MeasurementSystem.Imperial);
            UnitConverter.Unit foot = new UnitConverter.Unit(1 / 12.0,
                "foot",
                "footses",
                UnitConverter.Unit.MeasurementSystem.Imperial);
            Assert.AreEqual(12, 
                foot.ConvertTo(1, inch), 
                "Foot to inch conversion failure");
            Assert.AreEqual(-12,
                foot.ConvertTo(-1, inch),
                "Negative foot to inch conversion failure");
        }
    }
}
