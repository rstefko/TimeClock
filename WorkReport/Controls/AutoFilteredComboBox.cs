﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TimeClock.Controls
{
    public class AutoFilteredComboBox : ComboBox
    {
        bool _ignoreTextChanged;
        string _currentText;

        TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;

        /// <summary>
        /// Creates a new instance of <see cref="AutoFilteredComboBox" />.
        /// </summary>
        public AutoFilteredComboBox()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
        }

        public event Func<object, string, bool> FilterItem;
        public event Action<string> FilterList;

        #region IsCaseSensitive Dependency Property
        /// <summary>
        /// The <see cref="DependencyProperty"/> object of the <see cref="IsCaseSensitive" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCaseSensitiveProperty =
            DependencyProperty.Register("IsCaseSensitive", typeof(bool), typeof(AutoFilteredComboBox), new UIPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the way the combo box treats the case sensitivity of typed text.
        /// </summary>
        /// <value>The way the combo box treats the case sensitivity of typed text.</value>
        [Description("The way the combo box treats the case sensitivity of typed text.")]
        [Category("AutoFiltered ComboBox")]
        [DefaultValue(true)]
        public bool IsCaseSensitive
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return (bool)this.GetValue(IsCaseSensitiveProperty);
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                this.SetValue(IsCaseSensitiveProperty, value);
            }
        }
        #endregion

        #region DropDownOnFocus Dependency Property
        /// <summary>
        /// The <see cref="DependencyProperty"/> object of the <see cref="DropDownOnFocus" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropDownOnFocusProperty =
            DependencyProperty.Register("DropDownOnFocus", typeof(bool), typeof(AutoFilteredComboBox), new UIPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the way the combo box behaves when it receives focus.
        /// </summary>
        /// <value>The way the combo box behaves when it receives focus.</value>
        [Description("The way the combo box behaves when it receives focus.")]
        [Category("AutoFiltered ComboBox")]
        [DefaultValue(false)]
        public bool DropDownOnFocus
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return (bool)this.GetValue(DropDownOnFocusProperty);
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                this.SetValue(DropDownOnFocusProperty, value);
            }
        }
        #endregion

        #region | Handle focus |
        /// <summary>
        /// Invoked whenever an unhandled <see cref="UIElement.GotFocus" /> event
        /// reaches this element in its route.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (this.ItemsSource != null && this.DropDownOnFocus)
            {
                this.IsDropDownOpen = true;
            }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
            KeyUp += AutoFilteredComboBox_KeyUp;
            this.IsTextSearchEnabled = false;
        }

        void AutoFilteredComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (this.IsDropDownOpen == true)
                {
                    // Ensure that focus is given to the dropdown list
                    if (Keyboard.FocusedElement is TextBox)
                    {
                        Keyboard.Focus(this);
                        if (this.Items.Count > 0)
                        {
                            if (this.SelectedIndex == -1 || this.SelectedIndex == 0)
                                this.SelectedIndex = 0;
                        }
                    }
                }
            }
            if (Keyboard.FocusedElement is TextBox)
            {
                if (e.OriginalSource is TextBox)
                {
                    // Avoid the automatic selection of the first letter (As next letter will cause overwrite)
                    TextBox textBox = e.OriginalSource as TextBox;
                    if (textBox.Text.Length == 1 && textBox.SelectionLength == 1)
                    {
                        textBox.SelectionLength = 0;
                        textBox.SelectionStart = 1;
                    }
                }
            }
        }

        #region | Handle filtering |
        private void RefreshFilter()
        {
            if (this.ItemsSource == null)
                return;

            this.FreezeTextBoxState(() =>
            {
                Action<string> filterList = FilterList;
                if (filterList != null)
                {
                    filterList(_currentText);
                }
                else
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);
                    view.Refresh();
                }
            });

            // Prepare so arrow down selects first
            this.SelectedIndex = -1;
            this.IsDropDownOpen = true;

            // Also scroll to the top, when there are more items they are scrolled to the bottom
            var scrollViewer = this.Template.FindName("DropDownScrollViewer", this) as ScrollViewer;
            scrollViewer?.ScrollToTop();
        }

        private void FreezeTextBoxState(Action action)
        {
            var text = this.Text;
            var selectionStart = this.EditableTextBox.SelectionStart;
            var selectionLength = this.EditableTextBox.SelectionLength;
            
            action();

            if (this.Text == text)
                return;

            this.Text = text;
            this.EditableTextBox.SelectionStart = selectionStart;
            this.EditableTextBox.SelectionLength = selectionLength;
        }

        private bool FilterPredicate(object value)
        {
            // We don't like nulls.
            if (value == null)
                return false;

            // If there is no text, there's no reason to filter.
            if (string.IsNullOrEmpty(_currentText))
                return true;

            Func<object, string, bool> filterItem = FilterItem;
            if (filterItem != null)
                return filterItem(value, _currentText);

            if (IsCaseSensitive)
                return value.ToString().Contains(_currentText);
            else
                return value.ToString().IndexOf(_currentText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            try
            {
                _ignoreTextChanged = true;  // Ignore the following TextChanged
                base.OnSelectionChanged(e);
            }
            finally
            {
                _ignoreTextChanged = false;
            }
        }

        /// <summary>
        /// Called when the source of an item in a selector changes.
        /// </summary>
        /// <param name="oldValue">Old value of the source.</param>
        /// <param name="newValue">New value of the source.</param>
        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            if (newValue != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(newValue);
                if (FilterList == null)
                    view.Filter += this.FilterPredicate;
            }

            if (oldValue != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(oldValue);
                view.Filter -= this.FilterPredicate;
            }
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_ignoreTextChanged)
                return;

            _currentText = Text;

            if (!this.IsTextSearchEnabled)
            {
                this.RefreshFilter();
            }
        }
    }
}