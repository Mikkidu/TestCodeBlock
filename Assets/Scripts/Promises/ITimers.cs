
using PU.Promises;
using System;

namespace PU.Promises
{
    public interface ITimers
    {
        IPromise WaitOneFrame();
        IPromise Wait(float seconds, Action<float> progressCallback = null);
        IPromise WaitUnscaled(float seconds, Action<float> progressCallback = null);
        IPromise WaitForTrue(Func<bool> condition);
        void WaitForMainThread(Action action);
        void Stop(IPromise deferred);
    }
}
