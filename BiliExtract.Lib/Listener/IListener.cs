using System;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Listener;

public interface IListener<TEventArgs> where TEventArgs : EventArgs
{
    event EventHandler<TEventArgs>? Changed;

    Task StartAsync();

    Task StopAsync();
}
