using System;
namespace client
{
    public interface IServerHandler
    {
        void OnUserQuitted(UserQuitEvent e);
    }
}
