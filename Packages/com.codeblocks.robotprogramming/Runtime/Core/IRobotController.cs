using PU.Promises;
using UnityEngine;

namespace CodeBlocks.Core
{
    public interface IRobotController
    {
        // State properties
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        bool IsExecuting { get; }

        // Movement methods return promises that resolve when animation completes
        IPromise MoveForward(float units, float speedMultiplier = 1f);
        IPromise MoveBackward(float units, float speedMultiplier = 1f);
        IPromise TurnLeft();      // 90 degrees
        IPromise TurnRight();     // 90 degrees

        // State management
        void Reset();
        void Stop();
    }
}
