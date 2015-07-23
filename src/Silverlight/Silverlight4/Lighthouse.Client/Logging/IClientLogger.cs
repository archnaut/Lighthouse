namespace Lighthouse.Client.Logging
{
    public interface IClientLogger
    {
        bool SendClientLogMessage(string message);
    }
}