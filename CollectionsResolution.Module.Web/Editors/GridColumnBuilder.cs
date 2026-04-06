using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CollectionsResolution.Module.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace CollectionsResolution.Module.Web.Editors
{
    /// <summary>
    /// Utility class to build grid columns automatically from business object properties using reflection.
    /// </summary>
    public static class GridColumnBuilder
    {
        /// <summary>
        /// Gets column definitions for a given type by inspecting its properties using reflection.
        /// </summary>
        /// <param name="elementType">The type of objects in the collection</param>
        /// <returns>List of column definitions ordered by their Order attribute</returns>
        public static List<ColumnDefinition> GetColumnDefinitions(Type elementType)
        {
            if (elementType == null)
                throw new ArgumentNullException(nameof(elementType));

            var columnDefs = new List<ColumnDefinition>();

            // Get all public instance properties
            var properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => ShouldIncludeProperty(p))
                .OrderBy(p => GetPropertyOrder(p));

            foreach (var prop in properties)
            {
                var colDef = CreateColumnDefinition(prop);
                if (colDef != null)
                    columnDefs.Add(colDef);
            }

            return columnDefs;
        }

        /// <summary>
        /// Determines if a property should be included as a grid column.
        /// </summary>
        private static bool ShouldIncludeProperty(PropertyInfo property)
        {
            // Skip Oid - it's handled separately as key field
            if (property.Name == "Oid")
                return false;

            // Skip properties marked with [Browsable(false)]
            var browsableAttr = property.GetCustomAttribute<BrowsableAttribute>();
            if (browsableAttr != null && !browsableAttr.Browsable)
                return false;

            // Skip collection properties (XPCollection, IList, etc.)
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && 
                property.PropertyType != typeof(string))
                return false;

            // Skip properties marked with Association (reference properties to parent)
            // unless they are lookups we want to display
            var associationAttr = property.GetCustomAttribute<AssociationAttribute>();
            if (associationAttr != null)
            {
                // Skip if it's a reference back to parent (not a lookup)
                // We detect parent references by checking if they're not marked as lookup
                var lookupAttr = property.GetCustomAttribute<LookupEditorModeAttribute>();
                if (lookupAttr == null)
                    return false;
            }

            // Skip non-readable properties
            if (!property.CanRead)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the display order for a property.
        /// </summary>
        private static int GetPropertyOrder(PropertyInfo property)
        {
            var gridColAttr = property.GetCustomAttribute<GridColumnDefinitionAttribute>();
            if (gridColAttr != null)
                return gridColAttr.Order;

            // Use ModelDefault Order if available
            var modelDefaultAttrs = property.GetCustomAttributes<ModelDefaultAttribute>();
            var orderAttr = modelDefaultAttrs.FirstOrDefault(a => a.PropertyName == "Index");
            if (orderAttr != null && int.TryParse(orderAttr.PropertyValue, out int order))
                return order;

            // Default order
            return 100;
        }

        /// <summary>
        /// Creates a column definition from a property.
        /// </summary>
        private static ColumnDefinition CreateColumnDefinition(PropertyInfo property)
        {
            var gridColAttr = property.GetCustomAttribute<GridColumnDefinitionAttribute>();
            var displayNameAttr = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            
            // Determine caption
            string caption = gridColAttr?.Caption;
            if (string.IsNullOrEmpty(caption))
                caption = displayNameAttr?.DisplayName;
            if (string.IsNullOrEmpty(caption))
                caption = SplitCamelCase(property.Name);

            // Determine width
            int width = gridColAttr?.Width ?? 120;

            // Determine if editable
            bool allowEdit = gridColAttr?.AllowEdit ?? true;
            
            // Check if property is read-only (no setter or ReadOnly attribute)
            if (!property.CanWrite)
                allowEdit = false;

            var readOnlyAttr = property.GetCustomAttribute<ReadOnlyAttribute>();
            if (readOnlyAttr != null && readOnlyAttr.IsReadOnly)
                allowEdit = false;

            // Check for PersistentAlias (calculated fields are read-only)
            var persistentAliasAttr = property.GetCustomAttribute<PersistentAliasAttribute>();
            if (persistentAliasAttr != null)
                allowEdit = false;

            // Determine visibility
            bool visible = gridColAttr?.Visible ?? true;

            // Determine column type based on property type
            var columnType = GetColumnType(property.PropertyType);

            return new ColumnDefinition
            {
                FieldName = property.Name,
                Caption = caption,
                Width = width,
                AllowEdit = allowEdit,
                Visible = visible,
                ColumnType = columnType,
                DecimalPlaces = gridColAttr?.DecimalPlaces ?? 2,
                PropertyType = property.PropertyType
            };
        }

        /// <summary>
        /// Determines the grid column type based on the property type.
        /// </summary>
        private static ColumnType GetColumnType(Type propertyType)
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlyingType == typeof(bool))
                return ColumnType.CheckBox;
            
            if (underlyingType == typeof(int) || underlyingType == typeof(long) || 
                underlyingType == typeof(short) || underlyingType == typeof(byte))
                return ColumnType.Integer;
            
            if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || 
                underlyingType == typeof(float))
                return ColumnType.Decimal;
            
            if (underlyingType == typeof(DateTime))
                return ColumnType.Date;
            
            if (underlyingType.IsEnum)
                return ColumnType.Enum;

            // Default to text
            return ColumnType.Text;
        }

        /// <summary>
        /// Splits a camel case string into words.
        /// Example: "UnitPrice" -> "Unit Price"
        /// </summary>
        private static string SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Text.RegularExpressions.Regex.Replace(
                input, 
                "([a-z])([A-Z])", 
                "$1 $2"
            );
        }
    }

    /// <summary>
    /// Represents a column definition for the grid.
    /// </summary>
    public class ColumnDefinition
    {
        public string FieldName { get; set; }
        public string Caption { get; set; }
        public int Width { get; set; }
        public bool AllowEdit { get; set; }
        public bool Visible { get; set; }
        public ColumnType ColumnType { get; set; }
        public int DecimalPlaces { get; set; }
        public Type PropertyType { get; set; }
    }

    /// <summary>
    /// Enum representing the type of column to create.
    /// </summary>
    public enum ColumnType
    {
        Text,
        Integer,
        Decimal,
        Date,
        CheckBox,
        Enum
    }
}
