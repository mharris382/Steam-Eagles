using System.Collections;
using System.Collections.Generic;
using Buildables.Wireframe;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.BuildingTests
{
    [TestFixture]
    public class WireframeTests
    {
        [UnityTest]
        public IEnumerator WireframePrefabExistsAsAddressable()
        {
            UniTask.ToCoroutine(async () =>
            {
                var prefab = await WireframeInstance.GetWireframePrefabAsync();
                Assert.IsNotNull(prefab);
                var wireFrame = prefab.GetComponent<WireframeInstance>();
                Assert.IsNotNull(wireFrame);
            });
            yield return null;
        }
    }
}