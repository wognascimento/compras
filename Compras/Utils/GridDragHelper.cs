using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Utils
{
    internal static class GridDragHelper
    {
        public static bool CanStartDrag(
            RadGridView owner,
            DependencyObject? source)
        {
            var encontrouLinha = false;

            while (source != null && !ReferenceEquals(source, owner))
            {
                if (source is ScrollBar or Thumb or ButtonBase or CheckBox ||
                    source is GridViewHeaderCell)
                {
                    return false;
                }

                if (source is RadGridView grid && !ReferenceEquals(grid, owner))
                    return false;

                if (source is GridViewRow)
                    encontrouLinha = true;

                source = VisualTreeHelper.GetParent(source);
            }

            return encontrouLinha;
        }
    }
}
