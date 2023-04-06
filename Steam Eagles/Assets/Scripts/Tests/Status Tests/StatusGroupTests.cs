using NUnit.Framework;
using Statuses;

namespace Tests.Status_Tests
{
    [TestFixture]
    public class StatusGroupTests
    {
        private StatusDatabase statusDatabase;
        private StatusHandle[] statusGroupHandles;
        private StatusHandle inEngineeringRoomHandle;
        private StatusHandle inCutsceneHandle;
        private StatusHandle inDialogueHandle;
        
        private StatusHandle[] equipToolBlockHandles;

        [SetUp]
        public void SetUp()
        {
            statusDatabase = new StatusDatabase();
            var equipToolGroup = new StatusGroup("Tool Equipped", 
                "Repair Tool Equipped", "Build Tool Equipped",
                "Recipe Tool Equipped", "Destruct Tool Equipped");
            
            this.statusGroupHandles = statusDatabase.RegisterStatusGroup(equipToolGroup);
            
            this.inEngineeringRoomHandle = statusDatabase.RegisterStatus("In Engineering Room");
            
            this.inCutsceneHandle = statusDatabase.RegisterStatus("In Cutscene");
            this.inDialogueHandle = statusDatabase.RegisterStatus("In Dialogue");
            
            var equipToolBlockingGroup = new StatusGroup("Block Tool Equips",
                inCutsceneHandle.StatusName, inDialogueHandle.StatusName);
            
            this.equipToolBlockHandles = statusDatabase.RegisterStatusGroup(equipToolBlockingGroup);
            
            statusGroupHandles[0].AddRequirement(inEngineeringRoomHandle.StatusName);
            statusGroupHandles[0].AddBlockingStatus(inCutsceneHandle.StatusName);
            //NOTE: in cutscene which was just added to the blocking statuses of the tool equipped status is already a blocking status, that is OK. 
            statusGroupHandles[0].AddBlockingStatus(equipToolBlockHandles[0].StatusName);
            
        }
        
        [Test]
        public void StatusGroupRegisteredCorrectly()
        {
            Assert.True(statusDatabase.IsRegistered("Tool Equipped"));
            Assert.True(statusDatabase.IsRegistered("Repair Tool Equipped"));
            Assert.True(statusDatabase.IsRegistered("Build Tool Equipped"));
            Assert.True(statusDatabase.IsRegistered("Recipe Tool Equipped"));
            Assert.True(statusDatabase.IsRegistered("Destruct Tool Equipped"));
            
            Assert.AreEqual(5, statusGroupHandles.Length);
            
            Assert.AreEqual("Tool Equipped", statusGroupHandles[0].StatusName);
            Assert.IsTrue(statusGroupHandles[0].IsStatusGroup);
            
            //Sub statuses should be treated as regular statuses
            Assert.AreEqual("Repair Tool Equipped", statusGroupHandles[1].StatusName);
            Assert.IsFalse(statusGroupHandles[1].IsStatusGroup);
            
            Assert.AreEqual("Build Tool Equipped", statusGroupHandles[2].StatusName);
            Assert.IsFalse(statusGroupHandles[2].IsStatusGroup);
            
            Assert.AreEqual("Recipe Tool Equipped", statusGroupHandles[3].StatusName);
            Assert.IsFalse(statusGroupHandles[3].IsStatusGroup);
            
            Assert.AreEqual("Destruct Tool Equipped", statusGroupHandles[4].StatusName);
            Assert.IsFalse(statusGroupHandles[4].IsStatusGroup);
        }

        
        [Test]
        public void StatusGroupHasRequirement()
        {
            foreach (var statusHandle in statusGroupHandles)
            {
                Assert.IsTrue(statusHandle.Requires(inEngineeringRoomHandle.StatusName));
            }
        }
        
        
        [Test]
        public void StatusGroupHasBlockingStatuses()
        {
            foreach (var statusHandle in statusGroupHandles)
            {
                Assert.IsTrue(statusHandle.IsBlockedBy(inCutsceneHandle.StatusName));
                Assert.IsTrue(statusHandle.IsBlockedBy(inDialogueHandle.StatusName));
            }
        }
        
        [Test]
        public void StatusGroupHasBlockingStatusGroup()
        {
            foreach (var statusHandle in statusGroupHandles)
            {
                Assert.IsTrue(statusHandle.IsBlockedBy(equipToolBlockHandles[0].StatusName));
            }
        }
    }
}