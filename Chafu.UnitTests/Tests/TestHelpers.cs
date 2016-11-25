using System;

namespace Chafu.UnitTests.Tests
{
    public class TestHelpers
    {
        public static bool AboutEqual(nfloat x, nfloat y, int prescision)
        {
            double epsilon = Math.Max(
                Math.Abs(x), Math.Abs(y)) * Math.Pow(1, -prescision);
            return Math.Abs(x - y) <= epsilon;
        }
    }
}
