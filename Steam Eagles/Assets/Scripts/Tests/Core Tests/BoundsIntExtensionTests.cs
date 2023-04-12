using System.Collections.Generic;
using System.Linq;
using Buildings;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Core_Tests
{
    [TestFixture]
    public class BoundsIntExtensionTests
    {
        [Test]
        public void TestGetAllCells()
        {
            var boundSize = new Vector2Int(10, 10);
            var bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(boundSize.x, boundSize.y, 1));
            HashSet<Vector3Int> expected = new HashSet<Vector3Int>();
            for (int i = 0; i < boundSize.x; i++)
            {
                for (int j = 0; j < boundSize.y; j++)
                {
                    expected.Add(new Vector3Int(i, j, 0));
                }
            }

            var cells = bounds.GetAllCells2D().ToList();
            Assert.AreEqual(expected.Count, cells.Count );
            foreach (var cell in cells)
            {
                Assert.IsTrue(expected.Contains(cell));
            }
        }
        
        [Test]
        public void TestGetAllInnerCells()
        {
            var boundSize = new Vector2Int(10, 10);
            var bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(boundSize.x, boundSize.y, 1));
            HashSet<Vector3Int> expected = new HashSet<Vector3Int>();
            for (int i = 1; i < boundSize.x-1; i++)
            {
                for (int j = 1; j < boundSize.x-1; j++)
                {
                    expected.Add(new Vector3Int(i, j, 0));
                }
            }

            var cells = bounds.GetAllInteriorCells2D().ToList();
            Assert.AreEqual(expected.Count, cells.Count);
            foreach (var cell in cells)
            {
                Assert.IsTrue(expected.Contains(cell));
            }
        }


        [Test]
        public void TestGetAllTopBottom()
        {
            var boundSize = new Vector2Int(10, 10);
            var bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(boundSize.x, boundSize.y, 1));
            HashSet<Vector3Int> bottomExpected = new HashSet<Vector3Int>();
            HashSet<Vector3Int> topExpected = new HashSet<Vector3Int>();
            for (int i =  bounds.xMin; i < bounds.xMax; i++)
            {
                bottomExpected.Add(new Vector3Int(i, bounds.yMin, 0));
                topExpected.Add(new Vector3Int(i, bounds.yMax, 0));
            }

            var topActual = bounds.GetAllTopSideCells().ToList();
            var bottomActual = bounds.GetAllBottomSideCells().ToList();
            Assert.AreEqual(topExpected.Count, topActual.Count);
            Assert.AreEqual(bottomExpected.Count, bottomActual.Count);
            
        }
        
        [Test]
        public void TestGetAllLeftRight()
        {
            var boundSize = new Vector2Int(10, 10);
            var bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(boundSize.x, boundSize.y, 1));
            HashSet<Vector3Int> leftExpect = new HashSet<Vector3Int>();
            HashSet<Vector3Int> rightExpect = new HashSet<Vector3Int>();
            for (int i =  bounds.yMin; i < bounds.yMax; i++)
            {
                leftExpect.Add(new Vector3Int(bounds.xMin, i, 0));
                rightExpect.Add(new Vector3Int(bounds.xMax, i, 0));
            }

            var leftActual = bounds.GetAllLeftSideCells().ToList();
            var rightActual = bounds.GetAllRightSideCells().ToList();
            Assert.AreEqual(leftExpect.Count, leftActual.Count);
            Assert.AreEqual(rightExpect.Count, rightActual.Count);
        }


        [Test]
        public void TestCellOnBoundary()
        {
            const int size = 10;
            var bounds = new BoundsInt(0, 0, 0, size, size, 1);

            var left = new Vector3Int(0, Random.Range(1, size-1), 0);
            var right = new Vector3Int(size, Random.Range(1, size-1), 0);
            var top = new Vector3Int(Random.Range(1, size-1), size, 0);
            var bottom = new Vector3Int(Random.Range(1, size-1), 0, 0);
            
            Assert.IsTrue(bounds.IsCellOnLeftBoundary(left));
            Assert.IsTrue(bounds.IsCellOnRightBoundary(right));
            Assert.IsTrue(bounds.IsCellOnTopBoundary(top));
            Assert.IsTrue(bounds.IsCellOnBottomBoundary(bottom));
            
            Assert.IsFalse(bounds.IsCellOnLeftBoundary(right));
            Assert.IsFalse(bounds.IsCellOnRightBoundary(left));
            Assert.IsFalse(bounds.IsCellOnTopBoundary(bottom));
            Assert.IsFalse(bounds.IsCellOnBottomBoundary(top));
            
            Assert.IsTrue(bounds.IsCellOnBoundary(left));
            Assert.IsTrue(bounds.IsCellOnBoundary(right));
            Assert.IsTrue(bounds.IsCellOnBoundary(top));
            Assert.IsTrue(bounds.IsCellOnBoundary(bottom));
        }
    }
}