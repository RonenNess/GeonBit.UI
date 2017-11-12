// -------------------------------------------------------
// Define some engine exceptions.
//
// Author: Ronen Ness.
// Since: 2017.
// -------------------------------------------------------
using System;


namespace GeonBit.UI.Exceptions
{
    /// <summary>
    /// Thrown when something is not found (key, value, index, etc.)
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Create the exception without message.
        /// </summary>
        public NotFoundException()
        {
        }

        /// <summary>
        /// Create the exception with message.
        /// </summary>
        public NotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create the exception with message and inner exception.
        /// </summary>
        public NotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Thrown when the user provides an invalid value.
    /// </summary>
    public class InvalidValueException : Exception
    {
        /// <summary>
        /// Create the exception without message.
        /// </summary>
        public InvalidValueException()
        {
        }

        /// <summary>
        /// Create the exception with message.
        /// </summary>
        public InvalidValueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create the exception with message and inner exception.
        /// </summary>
        public InvalidValueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Thrown when the user tries to perform an action but the object / UI state does not allow it.
    /// </summary>
    public class InvalidStateException : Exception
    {
        /// <summary>
        /// Create the exception without message.
        /// </summary>
        public InvalidStateException()
        {
        }

        /// <summary>
        /// Create the exception with message.
        /// </summary>
        public InvalidStateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create the exception with message and inner exception.
        /// </summary>
        public InvalidStateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
