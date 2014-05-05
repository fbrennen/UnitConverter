UnitConverter
=============
Forrest Brennen
2014

Created in VS Express 2013


CONTENTS

COPYING.txt
README.txt
UnitConverter.sln
/UnitConverter
	/bin
		/Release
			UnitConverter.exe
	/Properties
		AssemblyInfo.cs
	Guard.cs
	UnitConverter.cs
	UnitConverter.csproj
/UnitConverterTest
	/Properties
		AssemblyInfo.cs
	UnitConverterTest.cs
	UnitConverterTest.csproj

	
ABOUT UNITCONVERTER

A command-line program to convert a length from one unit of measurement to another, in either Imperial or Metric units. Compiles to a standalone binary, UnitConverter.exe, which runs from the command-line. Call "UnitConverter.exe -help" to see the options for using the program.

The UnitConverter project includes the UnitConverter class itself (which in turn contains an internal class to represent a Unit), and also a Guard class to do some simple argument checking (as Code Contracts aren't available in VS Express). 

The UnitConverterTest project contains unit tests for both UnitConverter and UnitConverter.Unit.


ADDING NEW UNITS OF MEASUREMENT

A unit of measurement is denoted by a UnitConverter.Unit class instance. It's straightforward to add new units of measurement by means of adding a new instance of the Unit class to the master list of Units at the beginning of the UnitConverter class (specifically, the UnitConvert.abbreviationsToUnits dictionary). See the documentation inside UnitConverter for further information. Adding a new Unit will require recompiling the binary.