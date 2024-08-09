using System.IO;
using System.Windows.Controls;
using System.Windows;
using System;

namespace FileManager.windows
{
    public partial class MainWindow : Window
    {
        private void SelectTreeViewItem(TreeViewItem itemToSelect)
        {
            if (itemToSelect == null)
                return;

            itemToSelect.IsSelected = true;
            itemToSelect.Focus();
            itemToSelect.BringIntoView();
        }

        private void DeselectTreeViewItem(TreeViewItem item)
        {
            item.IsSelected = false;
            foreach (TreeViewItem child in item.Items)
            {
                DeselectTreeViewItem(child);
            }
        }

        private TreeViewItem FindNodeByPath(TreeViewItem parent, string path)
        {
            if (parent == null || string.IsNullOrEmpty(path))
                return null;

            string parentPath = parent.Tag as string;

            if (parentPath.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return parent;
            }

            foreach (TreeViewItem child in parent.Items)
            {
                TreeViewItem found = FindNodeByPath(child, path);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
