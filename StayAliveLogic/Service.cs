using System;
using System.Timers;

namespace StayAliveLogic
{
    public class Service : IDisposable
    {
        private readonly IStayAliveLogicHost _view;
        private readonly Timer _graceTimer = new Timer();
        private readonly Timer _checkPowerAndLastInputTimer = new Timer();
        private readonly IOsFunctions _osFunctions;
        private bool _isActive;
        private int _timeOutMinutes;
        private bool _disableWhenNotOnPower;
        private int _graceTimeOutMinutes;

        public Service(IStayAliveLogicHost view, IOsFunctions osFunctions)
        {
            _osFunctions = osFunctions;
            _view = view;
        }

        public bool IsActive => _isActive;

        public void Start(int currentTimeoutMinutes, int graceTimeOutMinutes, bool disableWhenNotOnPowerLine)
        {
            _graceTimeOutMinutes = graceTimeOutMinutes;
            _timeOutMinutes = currentTimeoutMinutes;
            _disableWhenNotOnPower = disableWhenNotOnPowerLine;
            SetupTimer();
            if (!IsOnPower() && _disableWhenNotOnPower)
                Inactivate(@"Computer is not on power line /r will be activated when power is restored");

            EnableIfInActive();
        }

        public void SetTimeout(int currentTimeoutMinutes)
        {
            _timeOutMinutes = currentTimeoutMinutes;
            SetupTimer();
        }

        public void SetDisableWhenNotOnPower(bool disableWhenNotOnPower)
        {
            _disableWhenNotOnPower = disableWhenNotOnPower;
            EnableIfInActive();
        }

        internal void EnableIfInActive()
        {
            if (_isActive && !IsOnPower() && _disableWhenNotOnPower)
            {
                Inactivate("Computer is not connected to power line");
                return;
            }

            if (!IsOnPower() && _disableWhenNotOnPower)
                return;

            ResetTimer();
            if (_isActive)
                return;

            _osFunctions.SetPresentationMode();
            _isActive = true;
        }

        private void ResetTimer()
        {
            if (_timeOutMinutes <= 0)
                return;

            _graceTimer.Stop();
        }

        private void Inactivate(string notifyMessage)
        {
            _view.ShowBalloonTip(notifyMessage);
            _osFunctions.UnSetPresentationMode();
            _isActive = false;
        }

        private void SetupTimer()
        {
            _graceTimer.Stop();
            _graceTimer.Interval = TimeSpan.FromMinutes(_graceTimeOutMinutes).TotalMilliseconds + 1;
            _graceTimer.Elapsed += (o, args) =>
            {
                _osFunctions.DoLockWorkStation();
                _graceTimer.Stop();
            };
            _checkPowerAndLastInputTimer.Interval = TimeSpan.FromSeconds(15).TotalMilliseconds;
            _checkPowerAndLastInputTimer.Start();
            _checkPowerAndLastInputTimer.Elapsed += (o, args) =>
            {
                if (!IsOnPower() && _isActive)
                {
                    Inactivate("Computer is not connected to power line");
                    return;
                }

                if (_osFunctions.LastInputSeconds() < _timeOutMinutes * 60)
                {
                    EnableIfInActive();
                }
                else
                {
                    if (_timeOutMinutes <= 0 || !_isActive) 
                        return;

                    Inactivate("Click to re enable");
                    _graceTimer.Start();
                }
            };
        }

        private bool IsOnPower()
        {
            return _view.IsOnPower();
        }

        public void Dispose()
        {
            _graceTimer?.Dispose();
            _checkPowerAndLastInputTimer?.Dispose();
        }
    }
}