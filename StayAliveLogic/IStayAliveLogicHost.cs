namespace StayAliveLogic
{
    public interface IStayAliveLogicHost
    {
        void ShowBalloonTip(string notifyMessage);
        bool IsOnPower();
    }
}