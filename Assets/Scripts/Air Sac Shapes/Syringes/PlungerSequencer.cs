using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    // Runs an ordered set of plunger steps (Left, then Right) with a fixed delay between
    // them. Holds no scene references — the caller supplies what each step does (show/hide
    // the plunger, play particle + sound). Absent sides are passed as null and skipped,
    // so single-sided shapes don't incur a phantom delay.
    internal class PlungerSequencer
    {
        internal async UniTask RunAsync(Action left, Action right, float stepDelay, CancellationToken ct)
        {
            bool playedAny = false;

            if (left != null)
            {
                left();
                playedAny = true;
            }

            if (right != null)
            {
                if (playedAny && stepDelay > 0f)
                    await UniTask.WaitForSeconds(stepDelay, cancellationToken: ct);
                right();
            }
        }
    }
}
