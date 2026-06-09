namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    // Swappable "interact with me" hint for a plunger. The color-flicker impl is the
    // first concrete strategy; an outline-based one can replace it without touching callers.
    internal interface IPlungerHint
    {
        void Begin();
        void End();
    }
}
