using System.Collections;
using Characters.Narrative;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.CharacterTests
{
    [TestFixture]
    public class CharacterLoadTests
    {
        [UnityTest]
        public IEnumerator CanLoadBuilderInfo()
        {
            yield return UniTask.ToCoroutine(async () => Assert.IsNotNull(await CharacterManager.Instance.LoadCharacterDescriptionAsync("Builder")));
        }
        
        [UnityTest]
        public IEnumerator CanLoadTransporterInfo()
        {
            yield return UniTask.ToCoroutine(async () => Assert.IsNotNull(await CharacterManager.Instance.LoadCharacterDescriptionAsync("Transporter")));
        }
        
        [UnityTest]
        public IEnumerator CanLoadCaptainInfo()
        {
            yield return UniTask.ToCoroutine(async () => Assert.IsNotNull(await CharacterManager.Instance.LoadCharacterDescriptionAsync("Captain")));
        }
        
        [UnityTest]
        public IEnumerator CanLoadBuilderCharacter()
        {
            yield return UniTask.ToCoroutine(async () => Assert.IsNotNull(await CharacterManager.Instance.LoadCharacterAsync("Builder")));
            
        }
        
        [UnityTest]
        public IEnumerator CanLoadTransporterCharacter()
        {
            yield return UniTask.ToCoroutine(async () => Assert.IsNotNull(await CharacterManager.Instance.LoadCharacterAsync("Transporter")));
            
        }
        
        [UnityTest]
        public IEnumerator CanLoadCaptainCharacter()
        {
            yield return UniTask.ToCoroutine(async () => Assert.IsNotNull(await CharacterManager.Instance.LoadCharacterAsync("Captain")));
        }
    }
}