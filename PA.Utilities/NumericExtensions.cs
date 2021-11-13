


using System;

namespace PA.Utilities
{
    public static class NumericExtensions
    {


        public static bool NearlyEquals(this float a, float b)
        {

            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a.CompareTo(b) == 0)
            { // shortcut, handles infinities
                return true;
            }
            else if (a.CompareTo(0f) == 0 || b.CompareTo(0f) == 0 || diff < float.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (float.Epsilon * float.MinValue);
            }
            else
            { // use relative error
                return diff / Math.Min(absA + absB, float.MaxValue) < float.Epsilon;
            }
        }

        public static bool NearlyEquals(this double a, double b)
        {

            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a.CompareTo(b) == 0)
            { // shortcut, handles infinities
                return true;
            }
            else if (a.CompareTo(0f) == 0 || b.CompareTo(0f) == 0 || diff < double.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (double.Epsilon * double.MinValue);
            }
            else
            { // use relative error
                return diff / Math.Min(absA + absB, double.MaxValue) < double.Epsilon;
            }
        }

    }
}

