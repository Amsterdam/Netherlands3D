using NUnit.Framework;

namespace Netherlands3D.Coordinates.Tests
{
    public class CoordinateTests
    {
        [Test]
        public void CreateCoordinateWithKnownCoordinateSystemAnd2Points()
        {
            var coordinate = new Coordinate(CoordinateSystem.EPSG_7415, 10.012, 12.04);

            Assert.AreEqual(7415, coordinate.CoordinateSystem);
            Assert.AreEqual((int)CoordinateSystem.EPSG_7415, coordinate.CoordinateSystem);
            Assert.AreEqual(2, coordinate.Points.Length);
            Assert.AreEqual(10.012, coordinate.Points[0]);
            Assert.AreEqual(12.04, coordinate.Points[1]);
        }

        [Test]
        public void CreateCoordinateWithUndefinedCoordinateSystemAnd3Points()
        {
            var coordinate = new Coordinate(29882, 10.012, 12.04, -2.1);

            Assert.AreEqual(29882, coordinate.CoordinateSystem);
            Assert.AreEqual(3, coordinate.Points.Length);
            Assert.AreEqual(10.012, coordinate.Points[0]);
            Assert.AreEqual(12.04, coordinate.Points[1]);
            Assert.AreEqual(-2.1, coordinate.Points[2]);
        }
    }
}
