using ShuZai.Wpf.Toolkit.Core.Utilities;
using ShuZai.Wpf.Toolkit.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ShuZai.Wpf.Toolkit
{
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    [TemplatePart(Name = PART_SearchBox, Type = typeof(SearchBox))]
    public class ComboBoxExt : SelectAllSelector
    {
        private const string PART_Popup = "PART_Popup";
        private const string PART_SearchBox = "PART_SearchBox";

        #region Members

        private ValueChangeHelper _displayMemberPathValuesChangeHelper;
        private bool _ignoreTextValueChanged;
        private Popup _popup;
        private List<object> _initialValue = new List<object>();
        private SearchBox _searchBox;
        private bool _isSearchEnabled;

        #endregion

        #region Constructors

        static ComboBoxExt()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxExt), new FrameworkPropertyMetadata(typeof(ComboBoxExt)));
        }

        public ComboBoxExt()
        {

            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
            _displayMemberPathValuesChangeHelper = new ValueChangeHelper(this.OnDisplayMemberPathValuesChanged);
        }

        #endregion //Constructors

        #region Properties

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(ComboBoxExt)
          , new UIPropertyMetadata(false));
        public bool IsEditable
        {
            get
            {
                return (bool)GetValue(IsEditableProperty);
            }
            set
            {
                SetValue(IsEditableProperty, value);
            }
        }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ComboBoxExt)
          , new UIPropertyMetadata(null, OnTextChanged));
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var checkComboBox = o as ComboBoxExt;
            if (checkComboBox != null)
                checkComboBox.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnTextChanged(string oldValue, string newValue)
        {
            if (!this.IsInitialized || _ignoreTextValueChanged || !this.IsEditable)
                return;

            this.UpdateFromText();
        }

        #endregion

        #region IsDropDownOpen

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(ComboBoxExt), new UIPropertyMetadata(false, OnIsDropDownOpenChanged));
        public bool IsDropDownOpen
        {
            get
            {
                return (bool)GetValue(IsDropDownOpenProperty);
            }
            set
            {
                SetValue(IsDropDownOpenProperty, value);
            }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxExt comboBoxExt = o as ComboBoxExt;
            if (comboBoxExt != null)
                comboBoxExt.OnIsDropDownOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsDropDownOpenChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                _initialValue.Clear();
                foreach (object o in SelectedItems)
                {
                    _initialValue.Add(o);
                }
                base.RaiseEvent(new RoutedEventArgs(ComboBoxExt.OpenedEvent, this));
            }
            else
            {
                _initialValue.Clear();
                base.RaiseEvent(new RoutedEventArgs(ComboBoxExt.ClosedEvent, this));
            }
        }

        #endregion //IsDropDownOpen

        #region MaxDropDownHeight

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(ComboBoxExt), new UIPropertyMetadata(SystemParameters.PrimaryScreenHeight / 3.0, OnMaxDropDownHeightChanged));
        public double MaxDropDownHeight
        {
            get
            {
                return (double)GetValue(MaxDropDownHeightProperty);
            }
            set
            {
                SetValue(MaxDropDownHeightProperty, value);
            }
        }

        private static void OnMaxDropDownHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxExt comboBoxExt = o as ComboBoxExt;
            if (comboBoxExt != null)
                comboBoxExt.OnMaxDropDownHeightChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnMaxDropDownHeightChanged(double oldValue, double newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion

        #region IsSearchEnabled

        public static readonly DependencyProperty IsSearchEnabledProperty = DependencyProperty.Register(
            "IsSearchEnabled",
            typeof(bool),
            typeof(ComboBoxExt),
            new UIPropertyMetadata(false, IsSearchEnabledChanged));

        private static void IsSearchEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as ComboBoxExt;
            if (selector != null)
                selector.IsSearchEnabledChanged((bool)e.NewValue);
        }

        protected void IsSearchEnabledChanged(bool value)
        {
            _isSearchEnabled = value;
            if (_searchBox != null)
            {
                SetFilter(value);
            }
        }

        /// <summary>
        /// 是否启用搜索栏
        /// </summary>
        public bool IsSearchEnabled
        {
            get
            {
                return (bool)GetValue(IsSearchEnabledProperty);
            }
            set
            {
                SetValue(IsSearchEnabledProperty, value);
            }
        }

        private void SetFilter(bool value)
        {
            _searchBox.Clear(false);
            _searchBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion //IsSearchEnabled

        #endregion //Properties

        #region Base Class Overrides

        protected override void OnSelectedValueChanged(string oldValue, string newValue)
        {
            base.OnSelectedValueChanged(oldValue, newValue);
            UpdateText();
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);
            this.UpdateDisplayMemberPathValuesBindings();
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            this.UpdateDisplayMemberPathValuesBindings();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_popup != null)
                _popup.Opened -= Popup_Opened;

            _popup = GetTemplateChild(PART_Popup) as Popup;

            if (_popup != null)
                _popup.Opened += Popup_Opened;


            _searchBox = GetTemplateChild(PART_SearchBox) as SearchBox;
            SetFilter(_isSearchEnabled);
        }


        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown(true);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsDropDownOpen)
            {
                if (KeyboardUtilities.IsKeyModifyingPopupState(e))
                {
                    IsDropDownOpen = true;
                    // Popup_Opened() will Focus on ComboBoxItem.
                    e.Handled = true;
                }
            }
            else
            {
                if (KeyboardUtilities.IsKeyModifyingPopupState(e))
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    SelectedItems.Clear();
                    foreach (object o in _initialValue)
                        SelectedItems.Add(o);
                    CloseDropDown(true);
                    e.Handled = true;
                }
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            UIElement item = ItemContainerGenerator.ContainerFromItem(SelectedItem) as UIElement;
            if ((item == null) && (Items.Count > 0))
                item = ItemContainerGenerator.ContainerFromItem(Items[0]) as UIElement;
            if (item != null)
                item.Focus();
        }

        #endregion //Event Handlers

        #region Closed Event

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(EventHandler), typeof(ComboBoxExt));
        public event RoutedEventHandler Closed
        {
            add
            {
                AddHandler(ClosedEvent, value);
            }
            remove
            {
                RemoveHandler(ClosedEvent, value);
            }
        }

        #endregion //Closed Event

        #region Opened Event

        public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(EventHandler), typeof(ComboBoxExt));
        public event RoutedEventHandler Opened
        {
            add
            {
                AddHandler(OpenedEvent, value);
            }
            remove
            {
                RemoveHandler(OpenedEvent, value);
            }
        }

        #endregion //Opened Event

        #region Methods

        protected virtual void UpdateText()
        {
#if VS2008
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemDisplayValue( x ).ToString() ).ToArray() ); 
#else
            string newValue = String.Join(Delimiter, SelectedItems.Cast<object>().Select(x => GetItemDisplayValue(x)));
#endif

            if (String.IsNullOrEmpty(Text) || !Text.Equals(newValue))
            {
                _ignoreTextValueChanged = true;
#if VS2008
        Text = newValue;
#else
                this.SetCurrentValue(ComboBoxExt.TextProperty, newValue);
#endif
                _ignoreTextValueChanged = false;
            }
        }

        private void UpdateDisplayMemberPathValuesBindings()
        {
            _displayMemberPathValuesChangeHelper.UpdateValueSource(ItemsCollection, this.DisplayMemberPath);
        }

        private void OnDisplayMemberPathValuesChanged()
        {
            this.UpdateText();
        }

        /// <summary>
        /// Updates the SelectedItems collection based on the content of
        /// the Text property.
        /// </summary>
        private void UpdateFromText()
        {
            List<string> selectedValues = null;
            if (!String.IsNullOrEmpty(this.Text))
            {
                selectedValues = this.Text.Replace(" ", string.Empty).Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            this.UpdateFromList(selectedValues, this.GetItemDisplayValue);
        }

        protected object GetItemDisplayValue(object item)
        {
            if (String.IsNullOrEmpty(this.DisplayMemberPath))
                return item;

            string[] nameParts = this.DisplayMemberPath.Split('.');
            if (nameParts.Length == 1)
            {
                var property = item.GetType().GetProperty(this.DisplayMemberPath);
                if (property != null)
                    return property.GetValue(item, null);
                return item;
            }

            for (int i = 0; i < nameParts.Count(); ++i)
            {
                var type = item.GetType();
                var info = type.GetProperty(nameParts[i]);
                if (info == null)
                {
                    return item;
                }

                if (i == nameParts.Count() - 1)
                {
                    return info.GetValue(item, null);
                }
                else
                {
                    item = info.GetValue(item, null);
                }
            }
            return item;
        }

        private void CloseDropDown(bool isFocusOnComboBox)
        {
            if (IsDropDownOpen)
                IsDropDownOpen = false;
            ReleaseMouseCapture();

            if (isFocusOnComboBox)
                Focus();
        }

        #endregion //Methods
    }
}
