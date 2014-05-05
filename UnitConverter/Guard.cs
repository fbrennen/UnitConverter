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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// It looks like Code Contracts would be better than this, but those aren't
// available in VS Express. Credit to stackoverflow for the suggestion of a 
// Guard class.
// http://stackoverflow.com/questions/1597884/refactoring-guard-clauses
namespace Unit_Converter
{
    public static class Guard
    {
        public static void ArgumentIsNotNull(object value, string argument)
        {
            if (value == null)
                throw new ArgumentNullException(argument);
        }

        public static void ArgumentGreaterThan(double value, double min, string argument)
        {
            if (value <= min)
                throw new ArgumentOutOfRangeException(argument,
                    value,
                    argument + " must be greater than " + min);
        }
    }
}
