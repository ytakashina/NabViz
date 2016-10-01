using Microsoft.VisualStudio.TestTools.UnitTesting;
using NabViz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabViz.Tests
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
            var array = new[]
            {
                0.00, 0.05, 0.10, 0.20, 0.30, 0.40, 0.50, 1.00, 1.50, 2.00
            };
            var ans = new[]
            {
                0.0000000, 0.0563720, 0.1124629, 0.2227026, 0.3286268,
                0.4283924, 0.5204999, 0.8427008, 0.9661051, 0.9953223
            };
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[0]) - ans[0]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[1]) - ans[1]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[2]) - ans[2]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[3]) - ans[3]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[4]) - ans[4]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[5]) - ans[5]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[6]) - ans[6]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[7]) - ans[7]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[8]) - ans[8]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.Erf(array[9]) - ans[9]) < 0.000001);
        }

        [TestMethod()]
        public void ErfcTest()
        {
            // Erf と同様。
        }

        [TestMethod()]
        public void QFunctionTest()
        {
            var array = new[]
            {
                0.0, 0.1, 0.2, 0.3, 0.4, -0.5, -1.0, -1.5, -2.0, -3.0
            };
            var ans = new[]
            {
                0.500000000, 0.460172163, 0.420740291, 0.382088578, 0.344578258,
                0.308537539, 0.158655254, 0.066807201, 0.022750132, 0.001349898
            };
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[0], 0, 1) - ans[0]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[1], 0, 1) - ans[1]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[2], 0, 1) - ans[2]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[3], 0, 1) - ans[3]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[4], 0, 1) - ans[4]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[5], 0, 1) - ans[5]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[6], 0, 1) - ans[6]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[7], 0, 1) - ans[7]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[8], 0, 1) - ans[8]) < 0.000001);
            Assert.IsTrue(Math.Abs(MathExtensions.QFunction(array[9], 0, 1) - ans[9]) < 0.000001);
        }
    }
}