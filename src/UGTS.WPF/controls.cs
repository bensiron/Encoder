using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace UGTS.WPF
{
	public static class MWPFDataGrid
	{
		/// <summary>
		/// Returns a list of all the values (cast to the same type as defValue) of the selected rows at the given column index, an empty list if the column index is invalid, or any other error occurs.
		/// Returns the defValue for rows where the value cannot be cast to the desired type, including Null values.
		/// </summary>
		public static List<T> XSelectedValues<T>(this DataGrid grid, int colIndex, T defValue)
		{
			List<T> functionReturnValue = null;
			var list = new List<T>();
			functionReturnValue = list;
			if (grid == null) return functionReturnValue;
			if (colIndex < 0 || colIndex >= grid.Columns.Count) return functionReturnValue;
			foreach (DataRowView r in grid.SelectedItems) {	list.Add(r.XCellValue(colIndex, defValue));	}
			return functionReturnValue;
		}

		/// <summary>
		/// Returns the value of the given column of the given row converted to match the type given by defValue, or returns defValue if any error occurs.
		/// </summary>
		public static T XCellValue<T>(this DataRowView r, int iCol, T defValue)
		{
			if (r == null)
				return defValue;
			if (iCol < 0 || iCol >= r.DataView.Table.Columns.Count)
				return defValue;
			var c = r[iCol];
			if (c == null)
				return defValue;
			return c.XTo(defValue);
		}

	}

	public static class MWPFControl
	{
		/// <summary>
		/// Updates the margin values for the values specified only
		/// </summary>
		public static void XUpdateMargin(this Control c, double left = double.NaN, double top = double.NaN, double right = double.NaN, double bottom = double.NaN)
		{
			if (c == null) return;
			var m = c.Margin;
			c.Margin = new Thickness(left.XIsNaN() ? m.Left : left, top.XIsNaN() ? m.Top : top, right.XIsNaN() ? m.Right : right, bottom.XIsNaN() ? m.Bottom : bottom);
		}

		/// <summary>
		/// Updates the margin values for the values specified only for all the controls in the list
		/// </summary>
		public static void XUpdateMargin(this IEnumerable<Control> list, double left = double.NaN, double top = double.NaN, double right = double.NaN, double bottom = double.NaN)
		{
			if (list == null) return;
			foreach (var c in list) {
				var m = c.Margin;
				c.Margin = new Thickness(left.XIsNaN() ? m.Left : left, top.XIsNaN() ? m.Top : top, right.XIsNaN() ? m.Right : right, bottom.XIsNaN() ? m.Bottom : bottom);
			}
		}

		/// <summary>
		/// Converts a boolean into a Windows.Visibility value in the standard way
		/// </summary>
		public static Visibility XToVisibility(this bool v)
		{
			return v ? Visibility.Visible : Visibility.Hidden;
		}

		/// <summary>
		/// Returns the new text of a ComboBox control from the selection change event
		/// </summary>
		public static string XNewSelection(this SelectionChangedEventArgs e)
		{
			return e.AddedItems.XFirst("");
		}

		/// <summary>
		/// Returns recursively all the children matching the given type of the given control, and returns them as a list in depth-first order
		/// </summary>
		public static IList<T> XChildren<T>(this DependencyObject w) where T : class
		{
			var list = new List<T>();
			AddChildren(w, list);
			return list;
		}

		private static void AddChildren<T>(DependencyObject w, IList<T> list) where T : class
		{
			if (w == null || list == null) return;
			if (w is T) list.Add(w as T);
			foreach (var c in LogicalTreeHelper.GetChildren(w)) { if (c is DependencyObject) AddChildren(c as DependencyObject, list); }
		}

		/// <summary>
		/// Same as Focus, but silently does nothing if the wrong object type or Nothing is passed in
		/// </summary>
		public static void XFocus(this object c)
		{
			if (c is Control) (c as Control).Focus();
		}
	}
}
