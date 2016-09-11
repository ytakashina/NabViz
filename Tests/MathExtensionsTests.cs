using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZetaOne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZetaOne.Tests
{
    [TestClass()]
    public class MathExtensionsTests
    {
        [TestMethod()]
        public void StandardDeviationTest()
        {
            var array = new[]
            {
                0.02892409, -1.66042677, -0.080496655, -0.366465811, 1.849577609,
                1.095972758, -1.11106541, -1.791155907, -0.667593321, 0.935435276,
                0.576475074, -0.481830178, -0.144877877, 0.671829658, 0.378967537,
                -0.422408266, 0.283432436, -2.231374453, 1.287553811, 0.765436671,
                0.314779919, -1.097921873, -1.312812375, 0.410876843, 1.560716231,
                1.423515551, -0.528162483, -0.188649996, 0.131540161, 0.044076542,
                -0.270954991, -0.828426189, -0.73530999, -1.822625172, 0.266217005,
                0.652560369, 1.548652754, -0.310733574, 0.186242045, -2.014433303,
                0.248843156, 1.001751933, 1.205926385, -0.757798403, 0.263779787,
                0.559835414, 0.921756661, 1.490190549, 0.73538685, 0.167039405
            };
            var stdev = array.StandardDeviation();
            Assert.IsTrue(Math.Abs(0.986572025 - stdev) < 0.000001);
        }

        [TestMethod()]
        public void ErfTest()
        {
        }

        [TestMethod()]
        public void ErfcTest()
        {
        }

        [TestMethod()]
        public void QFunctionTest()
        {
        }
    }
}