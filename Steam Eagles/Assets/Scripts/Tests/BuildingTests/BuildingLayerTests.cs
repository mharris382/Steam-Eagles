using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.BuildingTilemaps;
using NUnit.Framework;

namespace Tests.BuildingTests
{
    [TestFixture]
    public class BuildingLayerTests
    {
        ///[Test]
        public void TestSolidLayer()
        {
            var l = BuildingLayers.SOLID;
            var t = typeof(SolidTilemap);
            TestLayer(l, t);
        }

       /// [Test]
        public void TestPipeLayer()
        {
            var l = BuildingLayers.PIPE;
            var t = typeof(PipeTilemap);
            TestLayer(l, t);
        }
        
       /// [Test]
        public void TestWallLayer()
        {
            var l = BuildingLayers.WALL;
            var t = typeof(WallTilemap);
            TestLayer(l, t);
        }
        
       /// [Test]
        public void TestFoundationLayer()
        {
            var l = BuildingLayers.FOUNDATION;
            var t = typeof(FoundationTilemap);
            TestLayer(l, t);
        }
        
       /// [Test]
        public void TestCoverLayer()
        {
            var l = BuildingLayers.COVER;
            var t = typeof(CoverTilemap);
            TestLayer(l, t);
        }
        
      ///  [Test]
        public void TestPlatformLayer()
        {
            var l = BuildingLayers.PLATFORM;
            var t = typeof(PlatformTilemap);
            TestLayer(l, t);
        }
        
      ///  [Test]
        public void TestDecorLayer()
        {
            var l = BuildingLayers.DECOR;
            var t = typeof(DecorTilemap);
            TestLayer(l, t);
        }
        
       /// [Test]
        public void TestWireLayer()
        {
            var l = BuildingLayers.WIRES;
            var t = typeof(WireTilemap);
            TestLayer(l, t);
        }


       /// [Test]
        public void TestCompositeLayer()
        {
            var l = BuildingLayers.WIRES | BuildingLayers.SOLID | BuildingLayers.PIPE;
            var t = new [] { typeof(WireTilemap), typeof(SolidTilemap), typeof(PipeTilemap)};
            var hs = new HashSet<Type>();
            foreach (var type in t)
            {
                hs.Add(type);
            }
            var actualType = l.GetBuildingTilemapTypes().ToArray();
            Assert.AreEqual(t.Length, actualType.Length);
            foreach (var type in actualType)
            {
                Assert.IsTrue(hs.Contains(type));
            }
        }
        

        void TestLayer(BuildingLayers layers, Type expectedType)
        {
            Assert.AreEqual(expectedType, layers.GetBuildingTilemapType());
        }
    }
}