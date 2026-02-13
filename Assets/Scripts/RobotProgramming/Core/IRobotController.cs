using PU.Promises;
using UnityEngine;

namespace RobotProgramming.Core
{
    public interface IRobotController
    {
        // State properties
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        bool IsExecuting { get; }

        // Movement methods return promises that resolve when animation completes
        IPromise MoveForward(float units);
        IPromise MoveBackward(float units);
        IPromise TurnLeft();      // 90 degrees
        IPromise TurnRight();     // 90 degrees

        // State management
        void Reset();
        void Stop();
    }
}
