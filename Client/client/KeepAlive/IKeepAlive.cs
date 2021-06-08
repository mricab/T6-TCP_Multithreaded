using System;
namespace client
{
    public interface IKeepAlive
    {
        void OnServerDown(ServerDownEvent e);
        void OnKeepAliveDown(KeepAliveDownEvent e);
    }
}
