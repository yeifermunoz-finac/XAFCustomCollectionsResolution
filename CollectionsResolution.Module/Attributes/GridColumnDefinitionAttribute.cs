using System;

namespace CollectionsResolution.Module.Attributes
{
    /// <summary>
    /// Attribute to customize how a property is displayed as a column in the grid.
    /// Apply this to business object properties to control their grid column appearance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GridColumnDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the display caption for the column. If not set, the property name is used.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the column width in pixels. Default is 120.
        /// </summary>
        public int Width { get; set; } = 120;

        /// <summary>
        /// Gets or sets whether the column is editable. Default is true.
        /// </summary>
        public bool AllowEdit { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the column is visible. Default is true.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the display order. Lower values appear first. Default is 100.
        /// </summary>
        public int Order { get; set; } = 100;

        /// <summary>
        /// Gets or sets the number of decimal places for decimal columns. Default is 2.
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;
    }
}
