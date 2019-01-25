using Microsoft.Xna.Framework;


namespace GeonBit.UI.Animators
{
    /// <summary>
    /// An animator that add wave animation to rich paragraphs.
    /// </summary>
    public class TextWaveAnimator : IAnimator
    {
        // current wave peak position.
        private float _currPosition = 0f;
        
        /// <summary>
        /// Wave movement speed.
        /// </summary>
        public float SpeedFactor = 10f;

        /// <summary>
        /// Wave height factor.
        /// </summary>
        public float WaveHeight = 18f;

        /// <summary>
        /// Wave length / width factor.
        /// </summary>
        public float WaveLengthFactor = 3.5f;

        /// <summary>
        /// Should play this animation in a loop?
        /// </summary>
        public bool Loop = true;
        
        /// <summary>
        /// Return if an entity type is compatible with this animator.
        /// </summary>
        /// <param name="entity">Entity to test.</param>
        /// <returns>True if compatible, false otherwise.</returns>
        public override bool CheckEntityCompatibility(Entities.Entity entity)
        {
            return entity is Entities.RichParagraph;
        }

        /// <summary>
        /// Do animation.
        /// </summary>
        public override void Update()
        {
            // finished animation? skip
            if (IsDone)
            {
                return;
            }

            // get target entity as rich paragraph
            var paragraph = TargetEntity as Entities.RichParagraph;

            // update animation
            var dt = (float)UserInterface.Active.CurrGameTime.ElapsedGameTime.TotalSeconds;
            _currPosition += dt * SpeedFactor;

            // wrap position
            if (_currPosition > paragraph.Text.Length + WaveLengthFactor * 5)
            {
                Reset();
            }
        }
        /// <summary>
        /// Called after attached to an entity.
        /// </summary>
        protected override void OnAttached()
        {
            (TargetEntity as Entities.RichParagraph).PerCharacterManipulators += PerCharacterManipulationFunc;
            Reset();
        }

        /// <summary>
        /// Called right before detached from an entity.
        /// </summary>
        protected override void OnDetach()
        {
            (TargetEntity as Entities.RichParagraph).PerCharacterManipulators -= PerCharacterManipulationFunc;
        }

        /// <summary>
        /// Reset animation.
        /// </summary>
        public override void Reset()
        {
            _currPosition = -WaveLengthFactor * 5;
        }

        /// <summary>
        /// Did this animator finish?
        /// </summary>
        public override bool IsDone
        {
            get
            {
                return !Loop && (int)_currPosition > (TargetEntity as Entities.RichParagraph).Text.Length;
            }
        }

        /// <summary>
        /// The function we register as per-character manipulator to apply this animator.
        /// </summary>
        private void PerCharacterManipulationFunc(Entities.RichParagraph paragraph, char currChar, int index, ref Color fillColor, ref Color outlineColor, ref int outlineWidth, ref Vector2 offset, ref float scale)
        {
            float distance = System.Math.Abs((float)index - _currPosition);
            offset.Y = -WaveHeight / (1f + ((distance * distance) / WaveLengthFactor));
        }
    }
}
