using System.Collections.Generic;
using System.Linq;
using GeonBit.UI.Entities;
using GeonBit.UI.Validators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GeonBit.UI.Systems
{
    /// <summary>
    /// Allows you to add an collection of entities to this list, and then have the ability to tab through them.
    /// </summary>
    public class TabList : ISystem
    {
        
        /// <summary>
        /// Wraps an entity for use with tab list, to keep track of an entities default properties.
        /// </summary>
        public class TabEntity
        {
            /// <summary>
            /// The wrapped entity
            /// </summary>
            public Entity Entity { get; private set; }
            /// <summary>
            /// The entities default fill
            /// </summary>
            public Color Fill { get; private set; }
        
            /// <summary>
            /// Wraps an entity with its default properties.
            /// </summary>
            /// <param name="entity">The entity to wrap</param>
            public TabEntity(Entity entity)
            {
                Entity = entity;
                Fill = entity.FillColor;
            }
        }
        
        private KeyboardState _lastKeyState;
        private KeyboardState _keyState;
        
        private TabEntity[] _entities;
        private readonly Keys _cycleKey;
        private readonly Keys _selectKey;
        private readonly bool _wraparound;
        private readonly Color _cycleFill;
        private int _currentSelection = -1;


        /// <summary>
        /// Creates a tab list for a given set of entities.
        /// </summary>
        /// <param name="entities">The entities to store in the tab list</param>
        /// <param name="cycleFill">The fill color of the entity when cycled to</param>
        /// <param name="cycleKey">The key that must be pressed to focus next entity</param>
        /// <param name="selectKey">The key that must be pressed to select entity</param>
        /// <param name="wraparound">Whether or not tab will reset to zero at end of the list</param>
        public TabList(IEnumerable<Entity> entities, Color cycleFill = default, Keys cycleKey = Keys.Tab, Keys selectKey = Keys.Enter, bool wraparound = true)
        {
            _cycleKey = cycleKey;
            _selectKey = selectKey;
            _wraparound = wraparound;
            _cycleFill = cycleFill;

            SetupEntities(entities);
        }

        /// <summary>
        /// Performs cycle and select logic based on keyboard input.
        /// </summary>
        public void Update()
        {
            if (UserInterface.Active == null) return;

            _keyState = Keyboard.GetState();
            
            if (KeyPressed(_cycleKey))
            {
                CycleNext();
            } else if (KeyPressed(_selectKey))
            {
                UseCurrent();
            }

            _lastKeyState = _keyState;
        }
        
        /// <summary>
        /// Creates a new list of entities and wraps them in an tab entity object.
        /// </summary>
        /// <param name="entities">The entities to wrap.</param>
        private void SetupEntities(IEnumerable<Entity> entities)
        {
            var enumerable = entities as Entity[] ?? entities.ToArray();
            AddIgnoreKeyValidator(enumerable);
            _entities = enumerable.Select(entity => new TabEntity(entity)).ToArray();
        }

        /// <summary>
        /// Add the ignore key validator to each applicable entity.
        /// </summary>
        /// <param name="entities"></param>
        private void AddIgnoreKeyValidator(IEnumerable<Entity> entities)
        {
            foreach (var tabEntity in entities)
            {
                if (tabEntity.GetType() != typeof(TextInput)) continue;
                
                var textInput = tabEntity as TextInput;
                textInput?.Validators.Add(new IgnoreKeyValidator(_cycleKey));
            }
        }
        
        /// <summary>
        /// Resets the styling of a given entity to its original properties, and removes the focus.
        /// </summary>
        private void DeselectLastCycled()
        {
            if (!IsValidSelection()) return;

            _entities[_currentSelection].Entity.IsFocused = false;
            _entities[_currentSelection].Entity.FillColor = _entities[_currentSelection].Fill;
        }
        
        /// <summary>
        /// Keeps the current selection in the bounds of the list, or wraps around to zero.
        /// </summary>
        private void ConstrainSelection()
        {
            if (_wraparound && _currentSelection >= _entities.Length)
                _currentSelection = 0;
            else 
                MathHelper.Clamp(_currentSelection, 0, _entities.Length);
        }

        /// <summary>
        /// Cycles to the next selection, removes focus from the old, and styles the new.
        /// </summary>
        private void CycleNext()
        {
            DeselectLastCycled();
             _currentSelection++;
            ConstrainSelection();

            _entities[_currentSelection].Entity.FillColor = _cycleFill;
            _entities[_currentSelection].Entity.IsFocused = true;
        }

        /// <summary>
        /// Activates any of the actions of a given entity.
        /// </summary>
        private void UseCurrent()
        {
            if (!IsValidSelection()) return;
            _entities[_currentSelection].Entity.OnClick?.Invoke(_entities[_currentSelection].Entity);
        }

        /// <summary>
        /// Checks if a key has been pressed this update.
        /// </summary>
        private bool KeyPressed(Keys key) => _keyState.IsKeyDown(key) && _lastKeyState.IsKeyUp(key);

        /// <summary>
        /// Checks if the current selection is within the bounds of the list.
        /// </summary>
        private bool IsValidSelection() => _currentSelection >= 0 && _currentSelection < _entities.Length;
        
    }

}