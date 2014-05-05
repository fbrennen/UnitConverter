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
