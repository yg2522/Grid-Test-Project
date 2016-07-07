using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfApplication1.Classes
{
    public class VisualDiffTextBlock : TextBlock
    {
        #region CurrentTextProperty
        public static readonly DependencyProperty CurrentTextProperty =
            DependencyProperty.Register("CurrentText", typeof(string), typeof(VisualDiffTextBlock),
            new PropertyMetadata(String.Empty, OnCurrentTextPropertyChanged));
        public string CurrentText
        {
            get { return (string)GetValue(CurrentTextProperty); }
            set { SetValue(CurrentTextProperty, value); }
        }
        private static void OnCurrentTextPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            VisualDiffTextBlock control = source as VisualDiffTextBlock;
            string currentText = (string)e.NewValue;
            control.CurrentText = currentText;
            updateContent(control);
        }
        #endregion

        #region PreviousTextProperty
        public static readonly DependencyProperty PreviousTextProperty =
            DependencyProperty.Register("PreviousText", typeof(string), typeof(VisualDiffTextBlock),
            new PropertyMetadata(String.Empty, OnPreviousTextPropertyChanged));
        public string PreviousText
        {
            get { return (string)GetValue(PreviousTextProperty); }
            set { SetValue(PreviousTextProperty, value); }
        }
        private static void OnPreviousTextPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            VisualDiffTextBlock control = source as VisualDiffTextBlock;
            string previousText = (string)e.NewValue;
            control.PreviousText = previousText;
            updateContent(control);
        }
        #endregion

        #region IsVisualDiffVisibleProperty
        public static readonly DependencyProperty IsVisualDiffVisibleProperty =
            DependencyProperty.Register("IsVisualDiffVisible", typeof(bool), typeof(VisualDiffTextBlock),
            new PropertyMetadata(false, OnIsVisualDiffVisiblePropertyChanged));
        public bool IsVisualDiffVisible
        {
            get { return (bool)GetValue(IsVisualDiffVisibleProperty); }
            set { SetValue(IsVisualDiffVisibleProperty, value); }
        }
        private static void OnIsVisualDiffVisiblePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            VisualDiffTextBlock control = source as VisualDiffTextBlock;
            bool isVisualDiffVisible = (bool)e.NewValue;
            control.IsVisualDiffVisible = isVisualDiffVisible;
            updateContent(control);
        }
        #endregion

        #region Private Functions
        private static void updateContent(VisualDiffTextBlock textBlock)
        {
            textBlock.Inlines.Clear();

            if (textBlock.IsVisualDiffVisible)
            {
                var brushConverter = new BrushConverter();
                diff_match_patch dmp = new diff_match_patch();
                List<Diff> diffList = dmp.diff_main(
                    textBlock.PreviousText.Replace("\n", "\u00b6").Replace("\r", "").Replace(' ', '\u00B7'),
                    textBlock.CurrentText.Replace("\n", "\u00b6").Replace("\r", "").Replace(' ', '\u00B7'),
                    false);

                // Apply a clean up, the default value of this function is 4 chars.
                // To change the value, you'll need to do so inside the DiffMatchPatch.cs file.
                dmp.diff_cleanupEfficiency(diffList);

                foreach(Diff diffItem in diffList)
                {
                    switch(diffItem.operation)
                    {
                        case Operation.DELETE:
                            textBlock.Inlines.Add(new Run(diffItem.text) { Background = (Brush)brushConverter.ConvertFromString("#ff6a1010"), Foreground = (Brush)brushConverter.ConvertFromString("#ffdddddd"), TextDecorations = System.Windows.TextDecorations.Strikethrough });
                            break;
                        case Operation.EQUAL:
                            textBlock.Inlines.Add(new Run(diffItem.text.Replace("\u00b6", "\u00b6" + System.Environment.NewLine)));
                            break;
                        case Operation.INSERT:
                            textBlock.Inlines.Add(new Run(diffItem.text.Replace("\u00b6", "\u00b6" + System.Environment.NewLine)) { Background = (Brush)brushConverter.ConvertFromString("#ff005e41"), Foreground = (Brush)brushConverter.ConvertFromString("#ffdddddd") });
                            break;
                    }
                }
            }
            else
            {
                textBlock.Text = textBlock.CurrentText;
            }
        }
        #endregion
    }
}
