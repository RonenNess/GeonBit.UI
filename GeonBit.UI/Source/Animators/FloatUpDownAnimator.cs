using Microsoft.Xna.Framework;


namespace GeonBit.UI.Animators
{
    /// <summary>
    /// An animator that makes an entity float up and down.
    /// Note: this animator override the SpaceBefore and SpaceAfter properties.
    /// </summary>
    public class FloatUpDownAnimator : IAnimator
    {
        /// <summary>
        /// If set, represent number of seconds for this animator to run.
        /// When reach 0, will stop animation (and begin slowing down gradually until reaching 0).
        /// </summary>
        public float? Duration
        {
            set
            {
                _maxDuration = _timeLeft = value;
            }
            get
            {
                return _maxDuration;
            }
        }

        // max duration property
        private float? _maxDuration;

        // current time left
        private float? _timeLeft;

        // curr animator step
        private float _step;

        /// <summary>
        /// Floating animation speed.
        /// </summary>
        public float SpeedFactor = 2f;

        /// <summary>
        /// How much the entity moves up and down.
        /// </summary>
        public int FloatingDistance = 5;

        /// <summary>
        /// Do animation.
        /// </summary>
        public override void Update()
        {
            // update animation and set floating factor
            var dt = (float)UserInterface.Active.CurrGameTime.ElapsedGameTime.TotalSeconds;
            _step += dt * SpeedFactor;
            var currVal = (float)System.Math.Sin(_step) * (float)FloatingDistance;

            // apply duration
            if (_timeLeft.HasValue)
            {
                _timeLeft -= dt;
                if (_timeLeft.Value < 1f) currVal *= _timeLeft.Value;
            }

            // update target entity
            TargetEntity.SpaceBefore = new Vector2(TargetEntity.SpaceBefore.X, currVal);
            TargetEntity.SpaceAfter = new Vector2(TargetEntity.SpaceAfter.X, -currVal);
        }

        /// <summary>
        /// Reset animation.
        /// </summary>
        public override void Reset()
        {
            _step = 0f;
            _timeLeft = _maxDuration;
        }

        /// <summary>
        /// Did this animator finish?
        /// </summary>
        public override bool IsDone
        {
            get
            {
                return _timeLeft.HasValue && _timeLeft.Value <= 0f;
            }
        }
    }
}
