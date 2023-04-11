using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.Narrative;
using CoreLib.Entities;
using NUnit.Framework;
using Statuses;

namespace Tests.Status_Tests
{
    [TestFixture]
    public class BasicEntityStatusTests
    {
        private static string[] CrewMemberStatusesKeys = new[]
        {
            "Pilot",
            "Engineer",
            "Officer"
        };
        
        
        private static string[] EngineerToolEquipStatusesKeys = new[]
        {
            "Equipped Repair Tool",
            "Equipped Build Tool",
            "Equipped Destruct Tool",
            "Equipped Recipe Tool",
        };
        
        private StatusDatabase statusDatabase;
        private StatusEntityManager manager;
        private StatusHandle crewMemberGroupHandle;
        private StatusHandle engineerToolGroupHandle;

        private Dictionary<CrewMemberStatus, StatusHandle> crewMemberStatusLookup =
            new Dictionary<CrewMemberStatus, StatusHandle>();
        private Dictionary<ToolStates, StatusHandle> toolEquipStatusLookup =
            new Dictionary<ToolStates, StatusHandle>();

        [SetUp]
        public void SetUp()
        {
            statusDatabase = new StatusDatabase();
            manager = new StatusEntityManager(statusDatabase);

            var crewMemberStatuses = statusDatabase.RegisterStatusGroup("CrewMember", CrewMemberStatusesKeys);
            this.crewMemberGroupHandle = crewMemberStatuses[0];
            crewMemberStatusLookup.Add(CrewMemberStatus.PILOT, crewMemberStatuses[1]);
            crewMemberStatusLookup.Add(CrewMemberStatus.ENGINEER, crewMemberStatuses[2]);
            crewMemberStatusLookup.Add(CrewMemberStatus.OFFICER, crewMemberStatuses[3]);
            crewMemberGroupHandle.SpecifyTarget(EntityType.CHARACTER);
            
            var engineerToolEquipStatuses = statusDatabase.RegisterStatusGroup("ToolEquip", EngineerToolEquipStatusesKeys);
            this.engineerToolGroupHandle = engineerToolEquipStatuses[0];
            toolEquipStatusLookup.Add(ToolStates.Recipe, engineerToolEquipStatuses[1]);
            toolEquipStatusLookup.Add(ToolStates.Build, engineerToolEquipStatuses[2]);
            toolEquipStatusLookup.Add(ToolStates.Destruct, engineerToolEquipStatuses[3]);
            toolEquipStatusLookup.Add(ToolStates.Repair, engineerToolEquipStatuses[3]);
            
            this.engineerToolGroupHandle.AddRequirement(crewMemberStatusLookup[CrewMemberStatus.ENGINEER].StatusName);
        }

        [Test]
        public void CanCreateNewCharacterEntity()
        {
            var newCharacterEntity = manager.CreateNewEntity("Test Character", EntityType.CHARACTER);
            Assert.IsNotNull(newCharacterEntity);
            Assert.AreEqual(EntityType.CHARACTER, newCharacterEntity.EntityType);

            foreach (var kvp in crewMemberStatusLookup)
            {
                Assert.AreEqual(StatusState.NONE, newCharacterEntity.GetStatusState(kvp.Value));
            }
        }
        
        [Test]
        public void CanAddStatusesToEntity()
        {
            var newCharacterEntity = manager.CreateNewEntity("Test Character", EntityType.CHARACTER);
            Assert.IsNotNull(newCharacterEntity);
            Assert.AreEqual(EntityType.CHARACTER, newCharacterEntity.EntityType);
            
            foreach (var kvp in crewMemberStatusLookup)
            {
                Assert.AreEqual(StatusState.NONE, newCharacterEntity.GetStatusState(kvp.Value));
                Assert.AreEqual(StatusState.ACTIVE, newCharacterEntity.AddStatus(kvp.Value));
                Assert.AreEqual(StatusState.ACTIVE, newCharacterEntity.GetStatusState(kvp.Value));
            }
        }
        
        [Test]
        public void ToolEquipsStatusInheritsTargetMaskFromRequirements()
        {
            //tool equips were not explicitly stated to be targeted a character, but the engineer status requirement was
            //explicitly stated to be targeted at a character, so the tool equips should inherit the target mask from it's requirements
            //in other words we should not be able to specify that a vehicle status should require a character status or vice versa.  All
            //status defined in a single connected status graph should be to the same entity type.
            foreach (var statusHandle in toolEquipStatusLookup)
            {
                foreach (var status in statusHandle.Value.RequiredStatuses)
                    Assert.AreEqual(EntityType.CHARACTER, statusDatabase.GetTargetMask(status));
                
                Assert.AreEqual(EntityType.CHARACTER, statusHandle.Value.TargetMask);
            }
        }
        
        [Test]
        public void ToolEquipsRequiresEngineerStatus()
        {
            foreach (var statusHandle in toolEquipStatusLookup)
            {
                var requirements = statusHandle.Value.RequiredStatuses.ToArray();
                Assert.AreEqual(1, requirements.Length);
                Assert.AreEqual(crewMemberStatusLookup[CrewMemberStatus.ENGINEER].StatusName, requirements[0]);
            }
        }
        
        [Test]
        public void CanNotAddStatusGroupToEntity()
        {
            var newCharacterEntity = manager.CreateNewEntity("Test Character", EntityType.CHARACTER);
            Assert.IsNotNull(newCharacterEntity);
            Assert.AreEqual(EntityType.CHARACTER, newCharacterEntity.EntityType);
            Assert.AreEqual(StatusState.INVALID_CANNOT_APPLY_STATUS_GROUP_TO_ENTITY, newCharacterEntity.AddStatus(crewMemberGroupHandle));
        }
        
        [Test]
        public void CanNotAddStatusToWrongEntityType()
        {
            var shipStatus = statusDatabase.RegisterStatus("Damaged Hull");
            shipStatus.SpecifyTarget(EntityType.VEHICLE);
            
            var newCharacterEntity = manager.CreateNewEntity("Test Character", EntityType.CHARACTER);
            Assert.IsNotNull(newCharacterEntity);
            Assert.AreEqual(EntityType.CHARACTER, newCharacterEntity.EntityType);
            Assert.AreEqual(StatusState.INVALID_WRONG_ENTITY_TYPE, newCharacterEntity.AddStatus(shipStatus));
        }

        



        
    }
}