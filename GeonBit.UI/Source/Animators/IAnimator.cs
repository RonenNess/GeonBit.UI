

namespace GeonBit.UI.Animators
{
    /// <summary>
    /// Animator that can animate different entities.
    /// </summary>
    public abstract class IAnimator
    {
        /// <summary>
        /// Target entity this animator operates on.
        /// </summary>
        public Entities.Entity TargetEntity { get; private set; }

        /// <summary>
        /// Set the entity this animator works on.
        /// </summary>
        /// <param name="entity">Target entity to animate.</param>
        internal void SetTargetEntity(Entities.Entity entity)
        {
            if (TargetEntity != null && entity != null)
            {
                throw new Exceptions.InvalidStateException("Cannot attach animator to entity after it was already attached!");
            }
            TargetEntity = entity;
        }

        /// <summary>
        /// Do animation.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Reset animation.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Should remove this animator when done?
        /// </summary>
        public bool ShouldRemoveWhenDone { get; set; }

        /// <summary>
        /// Did this animator finish?
        /// </summary>
        public virtual bool IsDone { get; }

        /// <summary>
        /// Is this animator currently running?
        /// </summary>
        public bool Enabled = true;
    }
}
