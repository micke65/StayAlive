using System;
using NUnit.Framework;
using StayAliveLogic;

namespace StayAliveLogicTest
{
    [TestFixture]
    public class ServiceTest
    {
        private Service _target;
        private ServiceHostForTest _serviceHostForTest;
        private OsFunctionsForTest _osFunctionsForTest;

        [SetUp]
        public void SetUp()
        {
            _serviceHostForTest = new ServiceHostForTest();
            _osFunctionsForTest = new OsFunctionsForTest();
            _target = new Service(_serviceHostForTest, _osFunctionsForTest);
        }

        [TearDown]
        public void TearDown()
        {
            _target.Dispose();
        }

        [Test]
        public void ShouldSetPresentationModeOnStartUp()
        {
            _target.Start(1, 0, true);
            Assert.IsTrue(_target.IsActive);
        }

        [Test]
        public void ShouldUnsetPresentationModeAfterElapsedIdleTime()
        {
            _target.Start(1, 1, true);
            Assert.That(() =>_target.IsActive, Is.False.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds,100));
        }

        [Test]
        public void ShouldNotUnsetPresentationModeAfterElapsedIdleTimeIfTimeOutIsZero()
        {
            _target.Start(0, 0, true);
            Assert.That(() => _target.IsActive, Is.True.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldNotUnsetPresentationModeUntilElapsedIdleTime()
        {
            _target.Start(2, 0, true);
            Assert.That(() => _target.IsActive, Is.True.After((int)TimeSpan.FromMinutes(1).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldLockWorkStationAfterGraceTime()
        {
            var lockWsCalled = false;
            _osFunctionsForTest.LockWorkStationCalled += (sender, args) => { lockWsCalled = true; };
            _target.Start(1, 0, true);
            Assert.That(() => _target.IsActive, Is.True.After((int)TimeSpan.FromMinutes(.5).TotalMilliseconds, 100));
            Assert.That(() => lockWsCalled, Is.True.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldInActivateIfComputerIsRemovedFromPowerLineAndPowerLineDetectionIsTrue()
        {
            _target.Start(3, 0, true);
            Assert.IsTrue(_target.IsActive);
            _serviceHostForTest.SetIsOnPower(false);
            Assert.That(() => _target.IsActive, Is.False.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldNotInActivateIfComputerIsRemovedFromPowerLineAndPowerLineDetectionIsFalse()
        {
            _target.Start(3, 0, true);
            Assert.IsTrue(_target.IsActive);
            _serviceHostForTest.SetIsOnPower(false);
            Assert.That(() => _target.IsActive, Is.True.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldNotActivateIfComputerIsNotOnPowerLineWhenStartedIfPowerLineDetectionIsTrue()
        {
            _serviceHostForTest.SetIsOnPower(false);
            _target.Start(1, 0, true);
            Assert.IsFalse(_target.IsActive);
        }

        [Test]
        public void ShouldAlertWhenMainTimerElapsed()
        {
            var alertWasCalled = false;
            _serviceHostForTest.ShowBalloonCalled += (sender, args) => { alertWasCalled = true; };
            _target.Start(1, 0, true);
            Assert.That(() => alertWasCalled, Is.True.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldAlertIfComputerIsNotOnPowerLineWhenStartedIfPowerLineDetectionIsTrue()
        {
            _serviceHostForTest.SetIsOnPower(false);
            var alertWasCalled = false;
            _serviceHostForTest.ShowBalloonCalled += (sender, args) => { alertWasCalled = true; };
            _target.Start(1, 0, true);
            Assert.That(() => alertWasCalled, Is.True.After((int)TimeSpan.FromMinutes(.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldAlertIfComputerIsRemovedFromPowerLineAndPowerLineDetectionIsTrue()
        {
            var alertWasCalled = false;
            _serviceHostForTest.ShowBalloonCalled += (sender, args) => { alertWasCalled = true; };
            _target.Start(3, 0, true);
            Assert.IsTrue(_target.IsActive);
            _serviceHostForTest.SetIsOnPower(false);
            Assert.That(() => alertWasCalled, Is.True.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
        }

        [Test]
        public void ShouldNotAlertIfInactive()
        {
            var alertWasCalled = false;
            _serviceHostForTest.ShowBalloonCalled += (sender, args) => { alertWasCalled = true; };
            _target.Start(1, 1, true);
            Assert.That(() => alertWasCalled, Is.True.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds, 100));
            alertWasCalled = false;
            Assert.That(() => alertWasCalled, Is.False.After((int)TimeSpan.FromMinutes(1.5).TotalMilliseconds));
        }
    }

    public class ServiceHostForTest : IStayAliveLogicHost
    {
        private bool _isOnPower = true;

        public event EventHandler ShowBalloonCalled;

        public void SetIsOnPower(bool isOnPower)
        {
            _isOnPower = isOnPower;

        }
        public void ShowBalloonTip(string notifyMessage)
        {
           ShowBalloonCalled?.Invoke(this, new EventArgs());
        }

        public bool IsOnPower()
        {
            return _isOnPower;
        }
    }   

    public class OsFunctionsForTest : IOsFunctions
    {
        private DateTime _startTime = DateTime.UtcNow;
        public event EventHandler LockWorkStationCalled;
        public void DoLockWorkStation()
        {
            LockWorkStationCalled?.Invoke(this, new EventArgs());
        }

        public void SetPresentationMode()
        {
            
        }

        public void UnSetPresentationMode()
        {
            
        }

        public int LastInputSeconds()
        {
            return (int)DateTime.UtcNow.Subtract(_startTime).TotalSeconds;
        }
    }
}