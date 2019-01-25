

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
            // validate compatibility
            if (!CheckEntityCompatibility(entity))
            {
                throw new Exceptions.InvalidValueException("Entity type not compatible with this animator!");
            }

            // make sure not already attached
            if (TargetEntity != null && entity != null)
            {
                throw new Exceptions.InvalidStateException("Cannot attach animator to entity after it was already attached!");
            }

            // set entity
            if (entity == null) { OnDetach(); }
            TargetEntity = entity;
            if (entity != null) { OnAttached(); }
        }

        /// <summary>
        /// Called after attached to an entity.
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called right before detached from an entity.
        /// </summary>
        protected virtual void OnDetach()
        {
        }

        /// <summary>
        /// Return if an entity type is compatible with this animator.
        /// </summary>
        /// <param name="entity">Entity to test.</param>
        /// <returns>True if compatible, false otherwise.</returns>
        public virtual bool CheckEntityCompatibility(Entities.Entity entity)
        {
            return true;
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
