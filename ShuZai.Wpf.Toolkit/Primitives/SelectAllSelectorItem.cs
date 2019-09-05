namespace ShuZai.Wpf.Toolkit.Primitives
{
    public class SelectAllSelectorItem : SelectorItem
    {
        #region Members

        private bool _ignoreSelectorChanges = false;

        #endregion

        #region Overrides

        // Do not raise an event when this item is Selected/UnSelected.
        protected override void OnIsSelectedChanged(bool? oldValue, bool? newValue)
        {
            if (_ignoreSelectorChanges)
                return;

            var templatedParent = this.TemplatedParent as SelectAllSelector;
            if (templatedParent != null)
            {
                if (newValue.HasValue)
                {
                    // Select All
                    if (newValue.Value)
                    {
                        templatedParent.SelectAll();
                    }
                    // UnSelect All
                    else
                    {
                        templatedParent.UnSelectAll();
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        internal void ModifyCurrentSelection(bool? newSelection)
        {
            _ignoreSelectorChanges = true;
            this.IsSelected = newSelection;
            _ignoreSelectorChanges = false;
        }

        #endregion
    }
}
