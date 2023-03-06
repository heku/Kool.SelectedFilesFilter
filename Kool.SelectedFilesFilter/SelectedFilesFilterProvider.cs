using Community.VisualStudio.Toolkit;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Kool.SelectedFilesFilter;

// https://github.com/MicrosoftDocs/visualstudio-docs/blob/main/docs/extensibility/extending-the-solution-explorer-filter.md

[SolutionTreeFilterProvider(Ids.PACKAGE, Ids.SELECTED_FILES_FILTER_CMD_ID)]
public sealed class SelectedFilesFilterProvider : HierarchyTreeFilterProvider
{
    private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

    [ImportingConstructor]
    public SelectedFilesFilterProvider(IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
    {
        _hierarchyCollectionProvider = hierarchyCollectionProvider;
    }

    protected override HierarchyTreeFilter CreateFilter()
    {
        return new SelectedFilesFilter(_hierarchyCollectionProvider);
    }

    private sealed class SelectedFilesFilter : HierarchyTreeFilter
    {
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

        public SelectedFilesFilter(IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            _hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        // Gets the items to be included from this filter provider.
        // rootItems is a collection that contains the root of your solution
        // Returns a collection of items to be included as part of the filter
        protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
        {
            var root = HierarchyUtilities.FindCommonAncestor(rootItems);
            var sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(root.HierarchyIdentity.NestedHierarchy, CancellationToken);
            var selectedItems = await GetSelectionAsync();
            var filter = CreateFilterDelegate(selectedItems);
            return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(sourceItems, filter, CancellationToken);
        }

        private static async Task<IVsHierarchyItem[]> GetSelectionAsync()
        {
            var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            var selectedItems = (await solutionExplorer.GetSelectionAsync()).ToList();
            var hierarchyItems = new IVsHierarchyItem[selectedItems.Count];
            for (var i = 0; i < selectedItems.Count; i++)
            {
                selectedItems[i].GetItemInfo(out _, out _, out hierarchyItems[i]);
            }
            return hierarchyItems;
        }

        private static Predicate<IVsHierarchyItem> CreateFilterDelegate(IEnumerable<IVsHierarchyItem> selectedItems)
        {
            return IsInSelectedPath;

#pragma warning disable IDE1006 // Naming Styles
            bool IsInSelectedPath(IVsHierarchyItem hierarchyItem)
            {
                if (hierarchyItem is null)
                {
                    return false;
                }
                if (selectedItems.Contains(hierarchyItem))
                {
                    return true;
                }
                return IsInSelectedPath(hierarchyItem.Parent);
            }
#pragma warning restore IDE1006 // Naming Styles
        }
    }
}