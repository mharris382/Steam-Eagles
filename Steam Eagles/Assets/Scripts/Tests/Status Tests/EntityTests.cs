using CoreLib.EntityTag;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Status_Tests
{
    [TestFixture]
    public class EntityTests
    {
        private Entity entity;
        private GameObject childObject;
        private GameObject remoteLink;
        
        [SetUp]
        public void SetUp()
        {
            var entityGO = new GameObject("Entity", typeof(Entity));
            entity = entityGO.GetComponent<Entity>();
            childObject = new GameObject("Entity Child");
            childObject.transform.SetParent(entityGO.transform);
            remoteLink = new GameObject("Remote Link");
        }
        
        [TearDown]
        public void TearDown()
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(entity.gameObject);
                GameObject.Destroy(childObject);
                GameObject.Destroy(remoteLink);
            }
            else
            {
                GameObject.DestroyImmediate(entity.gameObject);
                GameObject.DestroyImmediate(childObject);
                GameObject.DestroyImmediate(remoteLink);
            }
        }
        
        
        [Test]
        public void GetEntity_ReturnsEntity()
        {
            Assert.AreEqual(entity, entity.gameObject.GetEntity());
            Assert.AreEqual(entity, childObject.GetEntity());
        }
        
        [Test]
        public void GetEntity_ReturnsNull()
        {
            Assert.IsNull(remoteLink.GetEntity());
        }
        
        [Test]
        public void RemoteEntityLink_ReturnsEntity()
        {
            Assert.IsNull(remoteLink.GetEntity());
            
            remoteLink.RegisterRemoteEntityLink(entity);
            
            Assert.NotNull(remoteLink.GetEntity());
            Assert.AreEqual(entity, remoteLink.GetEntity());
        }
        
        
        [Test]
        public void CannotLinkObjectToMoreThanOneEntity()
        {
            var entity2 = new GameObject("Entity 2", typeof(Entity)).GetComponent<Entity>();
            Assert.AreEqual(entity2, entity2.gameObject.GetEntity());
            Assert.IsTrue(remoteLink.RegisterRemoteEntityLink(entity));
            Assert.IsFalse(remoteLink.RegisterRemoteEntityLink(entity2));
            Assert.AreEqual(entity, remoteLink.GetEntity());
        }

        [Test]
        public void CanCreateLinkedEntityForGameObject()
        {
            var newGO = new GameObject("New GO");
            Assert.IsNull(newGO.GetEntity());
            var newEntity = newGO.CreateEntityLinked();
            Assert.AreNotEqual(newGO, newEntity.gameObject);
            Assert.AreEqual(newEntity, newGO.GetEntity());
        }
        
        [Test]
        public void CanCreateAttachedEntityForGameObject()
        {
            var newGO = new GameObject("New GO");
            Assert.IsNull(newGO.GetEntity());
            var newEntity = newGO.CreateEntityAttached();
            Assert.AreEqual(newGO, newEntity.gameObject);
            Assert.AreEqual(newEntity, newGO.GetEntity());
        }
    }
}