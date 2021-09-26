namespace GeonBit.UI.DataTypes
{
    /// <summary>
    /// General data / settings about a UI theme.
    /// Loaded from the theme data xml file.
    /// </summary>
    public class ThemeSettings
    {
        /// <summary>Name fot he theme.</summary>
        public string ThemeName;

        /// <summary>Theme author name.</summary>
        public string AuthorName;

        /// <summary>Theme description.</summary>
        public string Description;

        /// <summary>Theme additional credits.</summary>
        public string Credits;

        /// <summary>Theme version.</summary>
        public string Version;

        /// <summary>Theme project URL.</summary>
        public string RepoUrl;

        /// <summary>Theme license.</summary>
        public string License;
    }
}
