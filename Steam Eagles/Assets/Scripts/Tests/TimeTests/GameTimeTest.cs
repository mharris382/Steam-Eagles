using System.Collections;
using System.ComponentModel;
using CoreLib.GameTime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace Tests.TimeTests
{
    public class GameTimeTest : ZenjectUnitTestFixture
    {
        private const float SEC_PER_GAME_MINUTE = 0.1f;
        private const float SEC_PER_GAME_MINUTE_FAST = 0.01f;

        private class TestTimeConfig : IGameTimeConfig
        {
            public void Log(string msg)
            {
                Debug.Log(msg);
            }

            public TestTimeConfig(float realSecondsInGameMinute)
            {
                RealSecondsInGameMinute = realSecondsInGameMinute;
            }

            public float RealSecondsInGameMinute { get; }
        }

        bool _canContinue = false;

        [Test]
        public void TestConfigGetsCorrectTime()
        {
            TimeInstaller.Install(Container);
            Container.Rebind<IGameTimeConfig>().To<TestTimeConfig>()
                .FromInstance(new TestTimeConfig(SEC_PER_GAME_MINUTE)).AsSingle().NonLazy();
            Container.Bind<CoroutineCaller>().FromNewComponentOnNewGameObject().WithGameObjectName("CoroutineCaller")
                .AsSingle().NonLazy();

            var config = Container.Resolve<IGameTimeConfig>();
            Assert.AreEqual(SEC_PER_GAME_MINUTE, config.RealSecondsInGameMinute);
        }

        public class TimeOfDayTests
        {
            [Test]
            public void _8IsMorning()
            {
                Assert.AreEqual(TimeOfDay.MORNING, new GameTime(8, 0).TimeOfDay);
            }

            [Test]
            public void _2IsAfternoon()
            {
                Assert.AreEqual(TimeOfDay.AFTERNOON, new GameTime(14, 0).TimeOfDay);
            }

            [Test]
            public void _7IsEvening()
            {
                Assert.AreEqual(TimeOfDay.EVENING, new GameTime(19, 0).TimeOfDay);
            }

            [Test]
            public void _3IsNight()
            {
                Assert.AreEqual(TimeOfDay.NIGHT, new GameTime(3, 0).TimeOfDay);
            }

            [Test]
            public void MorningIs0To6()
            {
                var morningStart = new GameTime(6, 1);
                var morningEnd = new GameTime(11, 59);
                Assert.AreEqual(TimeOfDay.MORNING, morningStart.TimeOfDay);
                Assert.AreEqual(TimeOfDay.MORNING, morningEnd.TimeOfDay);
            }

            [Test]
            public void AfternoonIs12To6()
            {
                var afternoonStart = new GameTime(12, 1);
                var afternoonEnd = new GameTime(12 + 5, 59);
                Assert.AreEqual(TimeOfDay.AFTERNOON, afternoonStart.TimeOfDay);
                Assert.AreEqual(TimeOfDay.AFTERNOON, afternoonEnd.TimeOfDay);
            }

            [Test]
            public void EveningIs18To24()
            {
                var eveningStart = new GameTime(18, 1);
                var eveningEnd = new GameTime(23, 59);
                Assert.AreEqual(TimeOfDay.EVENING, eveningStart.TimeOfDay);
                Assert.AreEqual(TimeOfDay.EVENING, eveningEnd.TimeOfDay);
            }

            [Test]
            public void NightIs0To6()
            {
                var nightStart = new GameTime(0, 1);
                var nightEnd = new GameTime(5, 59);
                Assert.AreEqual(TimeOfDay.NIGHT, nightStart.TimeOfDay);
                Assert.AreEqual(TimeOfDay.NIGHT, nightEnd.TimeOfDay);
            }
        }

        public class GameDateTests
        {
            [Test]
            public void TestGameDateNotEquals()
            {
                var gameTime1 = new GameDate(12, 30);
                var gameTime2 = new GameDate(11, 30);
                Assert.AreNotEqual(gameTime1, gameTime2);
            }

            [Test]
            public void TestGameDateEquals()
            {
                var gameTime1 = new GameDate(12, 30);
                var gameTime2 = new GameDate(12, 30);
                Assert.AreEqual(gameTime1, gameTime2);
            }

            [Test]
            public void TestGameTimeGreaterThan()
            {
                var gameTimeBefore = new GameDate(5, 30);
                var gameTimeAfter = new GameDate(6, 40);
                Assert.IsTrue(gameTimeAfter > gameTimeBefore);
            }

            [Test]
            public void TestGameDateLessThan()
            {
                var gameTimeBefore = new GameDate(5, 30);
                var gameTimeAfter = new GameDate(6, 40);
                Assert.IsTrue(gameTimeBefore < gameTimeAfter);
            }

            [Test]
            public void TestGameDateGreaterThanOrEqual()
            {
                var gameTimeBefore = new GameDate(5, 30);
                var gameTimeAfter = new GameDate(6, 40);
                var gameTimeAfter2 = new GameDate(5, 30);
                Assert.IsTrue(gameTimeAfter >= gameTimeBefore);
                Assert.IsTrue(gameTimeAfter2 >= gameTimeBefore);
            }

            [Test]
            public void TestGameDateLessThanOrEqual()
            {
                var gameTimeBefore = new GameDate(5, 30);
                var gameTimeAfter = new GameDate(6, 40);
                var gameTimeAfter2 = new GameDate(5, 30);
                Assert.IsTrue(gameTimeBefore <= gameTimeAfter);
                Assert.IsTrue(gameTimeBefore <= gameTimeAfter2);
            }
        }

        public class GameTimeTests
        {
            [Test]
            public void TestGameTimeNotEquals()
            {
                var gameTime1 = new GameDate(12, 30);
                var gameTime2 = new GameTime(11, 30);
                Assert.AreNotEqual(gameTime1, gameTime2);
            }

            [Test]
            public void TestGameTimeEquals()
            {
                var gameTime1 = new GameTime(12, 30);
                var gameTime2 = new GameTime(12, 30);
                Assert.AreEqual(gameTime1, gameTime2);
            }

            [Test]
            public void TestGameDateGreaterThan()
            {
                var gameTimeBefore = new GameDate(5, 30);
                var gameTimeAfter = new GameDate(6, 40);
                Assert.IsTrue(gameTimeAfter > gameTimeBefore);
            }

            [Test]
            public void TestGameTimeLessThan()
            {
                var gameTimeBefore = new GameTime(5, 30);
                var gameTimeAfter = new GameTime(6, 40);
                Assert.IsTrue(gameTimeBefore < gameTimeAfter);
            }

            [Test]
            public void TestGameTimeGreaterThanOrEqual()
            {
                var gameTimeBefore = new GameTime(5, 30);
                var gameTimeAfter = new GameTime(6, 40);
                var gameTimeAfter2 = new GameTime(5, 30);
                Assert.IsTrue(gameTimeAfter >= gameTimeBefore);
                Assert.IsTrue(gameTimeAfter2 >= gameTimeBefore);
            }

            [Test]
            public void TestGameTimeLessThanOrEqual()
            {
                var gameTimeBefore = new GameTime(5, 30);
                var gameTimeAfter = new GameTime(6, 40);
                var gameTimeAfter2 = new GameTime(5, 30);
                Assert.IsTrue(gameTimeBefore <= gameTimeAfter);
                Assert.IsTrue(gameTimeBefore <= gameTimeAfter2);
            }
        }

        public class GameDateTimeTests
        {
            [Test]
            public void TestGameDateTimeNotEquals()
            {
                var gameTime1 = new GameDateTime(1, 1, 2, 30);
                var gameTime2 = new GameDateTime(1, 1, 1, 30);
                Assert.AreNotEqual(gameTime1, gameTime2);
            }

            [Test]
            public void TestGameDateTimeEquals()
            {
                var gameTime1 = new GameDateTime(1, 1, 2, 30);
                var gameTime2 = new GameDateTime(1, 1, 2, 30);
                Assert.AreEqual(gameTime1, gameTime2);
            }

            [Test]
            public void TestGameDateGreaterThan()
            {
                var gameTimeBefore = new GameDateTime(1, 1, 5, 30);
                var gameTimeAfter = new GameDateTime(1, 1, 6, 40);
                Assert.IsTrue(gameTimeAfter > gameTimeBefore);
            }

            [Test]
            public void TestGameDateTimeLessThan()
            {
                var gameTimeBefore = new GameDateTime(1, 1, 5, 30);
                var gameTimeAfter = new GameDateTime(1, 1, 6, 40);
                Assert.IsTrue(gameTimeBefore < gameTimeAfter);
            }

            [Test]
            public void TestGameDateTimeGreaterThanOrEqual()
            {
                var gameTimeBefore = new GameDateTime(1, 1, 5, 30);
                var gameTimeAfter = new GameDateTime(1, 1, 6, 40);
                var gameTimeAfter2 = new GameDateTime(1, 1, 5, 30);
                Assert.IsTrue(gameTimeAfter >= gameTimeBefore);
                Assert.IsTrue(gameTimeAfter2 >= gameTimeBefore);
            }

            [Test]
            public void TestGameDateTimeLessThanOrEqual()
            {
                var gameTimeBefore = new GameDateTime(1, 1, 5, 30);
                var gameTimeAfter = new GameDateTime(1, 1, 6, 40);
                var gameTimeAfter2 = new GameDateTime(1, 1, 5, 30);
                Assert.IsTrue(gameTimeBefore <= gameTimeAfter);
                Assert.IsTrue(gameTimeBefore <= gameTimeAfter2);
            }
        }

        [UnityTest]
        public IEnumerator TimeRunnerIncrementsMinutes()
        {
            Bindings(SEC_PER_GAME_MINUTE);
            var gameTimeState = Container.Resolve<GameTimeState>();
            var timeRunner = Container.Resolve<TimeRunner>();
            timeRunner.Initialize();

            yield return null;
            Assert.IsTrue(timeRunner.Running);
            gameTimeState.Mode = TimeMode.NORMAL;
            var startState = gameTimeState.CurrentTime;
            float timePassed = 0;
            while (timePassed < SEC_PER_GAME_MINUTE * 1.5f)
            {
                timePassed += Time.deltaTime;
                yield return null;
            }

            Assert.AreNotEqual(0, timeRunner.IncrementsMin);
            Assert.AreEqual(timeRunner.IncrementsMin, timeRunner.IncrementsMinMoved);
            var endState = gameTimeState.CurrentTime;
            Assert.IsTrue(endState.gameTime.gameMinute > startState.gameTime.gameMinute,
                GetDebugString(startState, endState));
            Assert.IsTrue(endState.gameTime.gameHour == startState.gameTime.gameHour,
                GetDebugString(startState, endState));
        }

        [UnityTest]
        public IEnumerator TimeRunnerIncrementsHours()
        {
            Bindings(SEC_PER_GAME_MINUTE_FAST);
            var gameTimeState = Container.Resolve<GameTimeState>();
            var timeRunner = Container.Resolve<TimeRunner>();
            timeRunner.Initialize();
            gameTimeState.Mode = TimeMode.NORMAL;
            var startState = gameTimeState.CurrentTime;
            float timePassed = 0;
            while (timePassed < SEC_PER_GAME_MINUTE * 90)
            {
                if (gameTimeState.GlobalGameTime.gameHour > startState.gameTime.gameHour)
                    break;
                timePassed += Time.deltaTime;
                yield return null;
            }

            var endState = gameTimeState.CurrentTime;
            Assert.IsTrue(endState.gameTime.gameHour > startState.gameTime.gameHour,
                GetDebugString(startState, endState));
        }

        [UnityTest]
        public IEnumerator TimeRunnerHaltsWhenMaximumIsSet()
        {
            _canContinue = false;
            Bindings(SEC_PER_GAME_MINUTE);
            var gameTimeState = Container.Resolve<GameTimeState>();

            var startTime = gameTimeState.CurrentTime;

            var haltTime = startTime;
            haltTime.Minutes += 30;

            var timeRunner = Container.Resolve<TimeRunner>();
            gameTimeState.Mode = TimeMode.NORMAL;
            gameTimeState.SetTimeMaximum(haltTime, () => _canContinue);
            timeRunner.Initialize();
            yield return new WaitForSeconds(SEC_PER_GAME_MINUTE * 40);
            Assert.IsTrue(timeRunner.Running);
            Assert.IsTrue(gameTimeState.CurrentTime == haltTime, GetDebugString(gameTimeState.CurrentTime, haltTime));
            yield return new WaitForSeconds(SEC_PER_GAME_MINUTE);
            Assert.IsTrue(gameTimeState.CurrentTime == haltTime, GetDebugString(gameTimeState.CurrentTime, haltTime));
            _canContinue = true;
            yield return new WaitForSeconds(SEC_PER_GAME_MINUTE);
            Assert.IsTrue(gameTimeState.CurrentTime > haltTime, GetDebugString(gameTimeState.CurrentTime, haltTime));
        }


        [Test]
        public void TimeConfigResourceExists()
        {
            TimeInstaller.Install(Container);
            Container.Bind<CoroutineCaller>().FromNewComponentOnNewGameObject().WithGameObjectName("CoroutineCaller").AsSingle().NonLazy();

            var config = Container.Resolve<IGameTimeConfig>();
            Assert.IsNotNull(config);
        }

    private void Bindings(float time)
        {
            Container.Rebind<IGameTimeConfig>().To<TestTimeConfig>()
                .FromInstance(new TestTimeConfig(time)).AsSingle().NonLazy();
            TimeInstaller.Install(Container);
            Container.Bind<CoroutineCaller>().FromNewComponentOnNewGameObject().WithGameObjectName("CoroutineCaller").AsSingle()
                .NonLazy();
        }

        string GetDebugString(GameDateTime dateTime1, string label)
        {
            return $"{label}:{dateTime1.ToString()}";
        }

        string GetDebugString(GameDateTime dateTime1, GameDateTime dateTime2)
        {
            return $"{GetDebugString(dateTime1, "Before")}\n{GetDebugString(dateTime2, "After")}";
        }

        
        
    }
}