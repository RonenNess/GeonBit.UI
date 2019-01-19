using Microsoft.Xna.Framework;


namespace GeonBit.UI.Animators
{
    /// <summary>
    /// An animator that makes an entity fade out.
    /// Note: this animator override the Opacity property.
    /// </summary>
    public class FadeOutAnimator : IAnimator
    {
        // current time left for fading
        private float _timeLeft = 1f;

        /// <summary>
        /// Fading animation speed.
        /// </summary>
        public float SpeedFactor = 1f;

        /// <summary>
        /// Do animation.
        /// </summary>
        public override void Update()
        {
            // update animation and calc new opacity
            var dt = (float)UserInterface.Active.CurrGameTime.ElapsedGameTime.TotalSeconds;
            _timeLeft -= dt * SpeedFactor;
            var newOpacity = System.Math.Max(0f, _timeLeft);

            // update target entity
            TargetEntity.Opacity = (byte)(newOpacity * (float)byte.MaxValue);
        }

        /// <summary>
        /// Reset animation.
        /// </summary>
        public override void Reset()
        {
            _timeLeft = 1f;
        }

        /// <summary>
        /// Did this animator finish?
        /// </summary>
        public override bool IsDone
        {
            get
            {
                return _timeLeft <= 0f;
            }
        }
    }
}
