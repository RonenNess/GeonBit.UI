namespace GeonBit.UI.DataTypes
{
    /// <summary>
    /// General data / settings about a UI theme.
    /// Loaded from the theme data xml file.
    /// </summary>
    public class ThemeSettings
    {
        /// <summary>Name fot he theme.</summary>
        public string ThemeName = null!;

        /// <summary>Theme author name.</summary>
        public string AuthorName = null!;

        /// <summary>Theme description.</summary>
        public string Description = null!;

        /// <summary>Theme additional credits.</summary>
        public string Credits = null!;

        /// <summary>Theme version.</summary>
        public string Version = null!;

        /// <summary>Theme project URL.</summary>
        public string RepoUrl = null!;

        /// <summary>Theme license.</summary>
        public string License = null!;
    }
}
