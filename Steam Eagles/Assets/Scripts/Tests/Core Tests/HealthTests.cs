using System.Security.Policy;

namespace Tests.Core_Tests
{
    public class HealthTests
    {

        public void TestDamage()
        {
            
        }
}

    public class HealthTestDummy
    {
        public TestStat Health ;
        public TestStat Stamina;
        public TestStat Oxygen;
        public HealthTestDummy(int maxHealth = 10, int maxStamina = 10, int maxOxygen = 10)
        {
            Health = new TestStat(maxHealth);
            Stamina = new TestStat(maxOxygen);
            Oxygen = new TestStat(maxStamina);
        }
    }
    
    
    
     
}