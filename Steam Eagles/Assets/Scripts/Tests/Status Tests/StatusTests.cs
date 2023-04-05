using System.Linq;
using NUnit.Framework;
using Statuses;

namespace Tests.Status_Tests
{
    public class StatusTests
    {

        StatusDatabase statusDatabase;

        StatusHandle organicStatusHandle;
        StatusHandle aliveStatusHandle;
        StatusHandle deadStatusHandle;

        [SetUp]
        public void SetUp()
        {
            statusDatabase = new StatusDatabase();

            organicStatusHandle = statusDatabase.RegisterStatus("Organic");
            aliveStatusHandle = statusDatabase.RegisterStatus("Alive");
            deadStatusHandle = statusDatabase.RegisterStatus("Dead");

            statusDatabase.RegisterBlockingStatus("Dead", "Alive"); //alive status is blocked by dead status

            
            deadStatusHandle.AddRequirement("Organic"); //dead status requires organic status
            aliveStatusHandle.AddRequirement("Organic"); //alive status requires organic status
        }

        [Test]
        public void AreStatusesRegistered()
        {
            Assert.True(statusDatabase.IsRegistered("Organic"));
            Assert.True(statusDatabase.IsRegistered("Alive"));
            Assert.True(statusDatabase.IsRegistered("Dead"));
        }

        [Test]
        public void IsAliveBlockedByDead()
        {
            Assert.True(aliveStatusHandle.IsBlockedBy("Dead"));
        }

        [Test]
        public void IsOrganicRequiredByAlive()
        {
            Assert.True(aliveStatusHandle.Requires("Organic"));
        }

        [Test]
        public void IsOrganicRequiredByDead()
        {
            Assert.True(deadStatusHandle.Requires("Organic"));
        }

        [Test]
        public void OrganicHasNoRequirementsOrBlockingStatuses()
        {
            Assert.True(!organicStatusHandle.BlockingStatuses.Any());
            Assert.True(!organicStatusHandle.RequiredStatuses.Any());
        }
        
        [Test]
        public void DeadHasNoBlockingStatuses()
        {
            Assert.True(!deadStatusHandle.BlockingStatuses.Any());
            Assert.AreEqual(1, deadStatusHandle.RequiredStatuses.Count());
        }
        
        [Test]
        public void AliveHasBlockingStatusesAndRequiredStatuses()
        {
            Assert.AreEqual(1, aliveStatusHandle.BlockingStatuses.Count());
            Assert.AreEqual(1, aliveStatusHandle.RequiredStatuses.Count());
        }
    }
}