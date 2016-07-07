using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApplication1.Classes
{
    public class DecoratedTextBlock : TextBlock
    {
        public DecoratedTextBlock()
        {
            this.TextBlockDecorations = new FreezableCollection<TextBlockDecoration>();
        }

        #region TextBlockDecorationsProperty
        public static readonly DependencyProperty TextBlockDecorationsProperty = DependencyProperty.Register("TextBlockDecorations", typeof(FreezableCollection<TextBlockDecoration>), typeof(DecoratedTextBlock),
            new FrameworkPropertyMetadata(new FreezableCollection<TextBlockDecoration>(), OnTextBlockDecorationsPropertyChanged));
        public FreezableCollection<TextBlockDecoration> TextBlockDecorations
        {
            get { return (FreezableCollection<TextBlockDecoration>)GetValue(TextBlockDecorationsProperty); }
            set { SetValue(TextBlockDecorationsProperty, value); }
        }
        private static void OnTextBlockDecorationsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            DecoratedTextBlock control = target as DecoratedTextBlock;
            if (control == null)
                return;

            FreezableCollection<TextBlockDecoration> oldTextDecorations = args.OldValue as FreezableCollection<TextBlockDecoration>;
            if(oldTextDecorations != null)
            {
                //clear old events
                foreach(var oldTextDecoration in oldTextDecorations)
                {
                    oldTextDecoration.StylesUpdated -= control.onUpdateTextBlock;
                }
            }

            //setup new command
            FreezableCollection<TextBlockDecoration> newTextDecorations = args.NewValue as FreezableCollection<TextBlockDecoration>;
            if (newTextDecorations == null)
                return;

            foreach (var newTextDecoration in newTextDecorations)
            {
                //add new events to new collection
                newTextDecoration.StylesUpdated += control.onUpdateTextBlock;
            }
            control.TextBlockDecorations = newTextDecorations;

            //update the textblock
            updateTextBlock(control);
        }
        #endregion

        #region IsSelected Property
        public static readonly DependencyProperty ShowDecorationsProperty = DependencyProperty.Register("ShowDecorationsProperty", typeof(bool), typeof(DecoratedTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowDecorationsPropertyChanged));
        public bool ShowDecorations
        {
            get { return (bool)GetValue(ShowDecorationsProperty); }
            set { SetValue(ShowDecorationsProperty, value); }
        }
        private static void OnShowDecorationsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DecoratedTextBlock control = source as DecoratedTextBlock;
            bool show = (bool)e.NewValue;
            control.ShowDecorations = show;
            updateTextBlock(control);
        }
        #endregion

        #region IsSelectedForeground
        public static readonly DependencyProperty IsSelectedForegroundProperty =
            DependencyProperty.Register("IsSelectedForeground", typeof(Brush), typeof(DecoratedTextBlock),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedForegroundPropertyChanged));
        public Brush IsSelectedForeground
        {
            get { return (Brush)GetValue(IsSelectedForegroundProperty); }
            set { SetValue(IsSelectedForegroundProperty, value); }
        }
        private static void OnIsSelectedForegroundPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DecoratedTextBlock control = source as DecoratedTextBlock;
            Brush currentBrush = (Brush)e.NewValue;
            control.IsSelectedForeground = currentBrush;
            updateTextBlock(control);
        }
        #endregion

        #region IsSelectedBackground
        public static readonly DependencyProperty IsSelectedBackgroundProperty =
            DependencyProperty.Register("IsSelectedBackground", typeof(Brush), typeof(DecoratedTextBlock),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedBackgroundPropertyChanged));
        public Brush IsSelectedBackground
        {
            get { return (Brush)GetValue(IsSelectedBackgroundProperty); }
            set { SetValue(IsSelectedBackgroundProperty, value); }
        }
        private static void OnIsSelectedBackgroundPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DecoratedTextBlock control = source as DecoratedTextBlock;
            Brush currentBrush = (Brush)e.NewValue;
            control.IsSelectedBackground = currentBrush;
            updateTextBlock(control);
        }
        #endregion

        #region BoundText
        public static readonly DependencyProperty BoundTextProperty =
            DependencyProperty.Register("BoundText", typeof(string), typeof(DecoratedTextBlock),
            new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundTextPropertyChanged));
        public string BoundText
        {
            get { return (string)GetValue(BoundTextProperty); }
            set { SetValue(BoundTextProperty, value); }
        }
        private static void OnBoundTextPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DecoratedTextBlock control = source as DecoratedTextBlock;
            string currentText = (string)e.NewValue;
            control.BoundText = currentText;
            updateTextBlock(control);
        }
        #endregion

        //event handler for textdecoration changes
        private void onUpdateTextBlock(object sender, TextBlockDecoration e)
        {
            updateTextBlock(this);
        }

        //internal class only used to help keep track of a textblockdecoration style
        private class TextDecoration
        {
            public int Index { get; set; }
            public string Text { get; set; }
            public DecorationType DecorationFlags { get; set;}
            public System.Windows.Media.Color? Background { get; set; }
            public System.Windows.Media.Color? Foreground { get; set; }
            public FontStyle? FontStyle { get; set; }
            public FontWeight? FontWeight { get; set; }
        }

        //internal enum used to help keep track what decoration is being tracked
        [Flags]
        private enum DecorationType
        {
            None = 0,
            Background = 1 << 0,
            Foreground = 1 << 1,
            Style = 1 << 2,
            Weight = 1 << 3,
        }

        //redraw textblock
        private static void updateTextBlock(DecoratedTextBlock textblock)
        {
            var defaultIsSelectedForeground = textblock.IsSelectedForeground ?? textblock.Foreground;
            var defaultIsSelectedBackground = textblock.IsSelectedBackground ?? textblock.Background;

            //reset the textblock
            textblock.Inlines.Clear();

            //if we have no datacontext dont even bother trying to update the textblock
            if (textblock.DataContext == null)
                return;

            //don't decorate if selection is not set
            if (!textblock.ShowDecorations)
            {
                textblock.Text = textblock.BoundText;
                return;
            }

            //setup decorations for pulling by index
            Dictionary<int, List<TextDecoration>> decorations = new Dictionary<int, List<TextDecoration>>();
            foreach (var dec in textblock.TextBlockDecorations)
            {
                //iterate over each text inside the decoration and add to dictionary according to the index the text was found in (for quick lookup)
                if (dec.DecoratedText != null)
                {
                    foreach (var text in dec.DecoratedText)
                    {
                        if (!decorations.ContainsKey(text.Start))
                        {
                            decorations.Add(text.Start, new List<TextDecoration>());
                        }
                        //create a decrated text for this text
                        DecorationType flag = (dec.BackgroundColor.HasValue ? DecorationType.Background : DecorationType.None) 
                            | (dec.ForegroundColor.HasValue ? DecorationType.Foreground : DecorationType.None)
                            | (dec.FontStyle.HasValue ? DecorationType.Style : DecorationType.None)
                            | (dec.FontWeight.HasValue ? DecorationType.Weight : DecorationType.None);
                        TextDecoration textdec = new TextDecoration()
                        {
                            Index = text.Start,
                            Text = text.Item,
                            Background = dec.BackgroundColor,
                            FontStyle = dec.FontStyle,
                            Foreground = dec.ForegroundColor,
                            FontWeight = dec.FontWeight,
                            DecorationFlags = flag,
                        };
                        decorations[text.Start].Add(textdec);
                    }
                }
            }

            StringBuilder builder = new StringBuilder();
            Dictionary<DecorationType, TextDecoration> decorationMap = new Dictionary<DecorationType, TextDecoration>();
            decorationMap.Add(DecorationType.Background, null);
            decorationMap.Add(DecorationType.Foreground, null);
            decorationMap.Add(DecorationType.Style, null);
            decorationMap.Add(DecorationType.Weight, null);
            System.Windows.Media.Color? currentBackground = null;
            System.Windows.Media.Color? currentForeground = null;
            FontStyle? currentStyle = null;
            FontWeight? currentWeight = null;
            bool backgroundChanged = false;
            bool foregroundChanged = false;
            bool styleChanged = false;
            bool weightChanged = false;
            //iterate through string and detect when a decration is added/removed to create runs for each decoration change
            List<TextDecoration> decorationMem = new List<TextDecoration>();
            for (int i = 0; i < textblock.BoundText.Length; i++)
            {
                //check if we need to remove the current decoration for each decoration type
                if(decorationMap[DecorationType.Background] != null 
                    && decorationMap[DecorationType.Background].Background != null 
                    && decorationMap[DecorationType.Background].Index + decorationMap[DecorationType.Background].Text.Length <= i)
                {
                    backgroundChanged = true;
                }
                if (decorationMap[DecorationType.Foreground] != null
                    && decorationMap[DecorationType.Foreground].Foreground != null
                    && decorationMap[DecorationType.Foreground].Index + decorationMap[DecorationType.Foreground].Text.Length <= i)
                {
                    foregroundChanged = true;
                }
                if (decorationMap[DecorationType.Style] != null
                    && decorationMap[DecorationType.Style].FontStyle != null
                    && decorationMap[DecorationType.Style].Index + decorationMap[DecorationType.Style].Text.Length <= i)
                {
                    styleChanged = true;
                }
                if (decorationMap[DecorationType.Weight] != null
                    && decorationMap[DecorationType.Weight].FontWeight != null
                    && decorationMap[DecorationType.Weight].Index + decorationMap[DecorationType.Weight].Text.Length <= i)
                {
                    weightChanged = true;
                }

                //check for new decorations
                if (decorations.ContainsKey(i))
                {
                    //go through the dictionary at the current index
                    foreach (var dec in decorations[i])
                    {
                        //if the decoration has a change and there is nothing currently tracked, flag as changed and add to the decoration memory
                        if (dec.Background != null && decorationMap[DecorationType.Background] == null)
                        {
                            backgroundChanged = true;
                        }
                        if (dec.Foreground != null && decorationMap[DecorationType.Foreground] == null)
                        {
                            foregroundChanged = true;
                        }
                        if (dec.FontStyle != null && decorationMap[DecorationType.Style] == null)
                        {
                            styleChanged = true;
                        }
                        if (dec.FontWeight != null && decorationMap[DecorationType.Weight] == null)
                        {
                            weightChanged = true;
                        }
                        decorationMem.Add(dec);
                    }
                }

                //get the previous background and reset the kept decoration that was changed
                if(backgroundChanged || foregroundChanged || styleChanged || weightChanged)
                {
                    currentBackground = decorationMap[DecorationType.Background] != null ? decorationMap[DecorationType.Background].Background : null;
                    currentForeground = decorationMap[DecorationType.Foreground] != null ? decorationMap[DecorationType.Foreground].Foreground : null;
                    currentStyle = decorationMap[DecorationType.Style] != null ? decorationMap[DecorationType.Style].FontStyle : null;
                    currentWeight = decorationMap[DecorationType.Weight] != null ? decorationMap[DecorationType.Weight].FontWeight : null;

                    //reset the changed values to null for updating
                    if(backgroundChanged)
                    {
                        decorationMap[DecorationType.Background] = null;
                    }
                    if(foregroundChanged)
                    {
                        decorationMap[DecorationType.Foreground] = null;
                    }
                    if(styleChanged)
                    {
                        decorationMap[DecorationType.Style] = null;
                    }
                    if(weightChanged)
                    {
                        decorationMap[DecorationType.Weight] = null;
                    }
                }

                //check if we need to remove decorations in memory and update current decoration if need be
                if (decorationMem.Count > 0)
                {
                    List<TextDecoration> remaining = new List<TextDecoration>();
                    //go through each of the queued decorations
                    foreach (var dec in decorationMem)
                    {
                        //check if our position is past the word in the decoration
                        if (dec.Index + dec.Text.Length > i)
                        {
                            //check if all styling for the decoration is open, if not then skip the decoration (aka make sure all decorations for a textblockdecoration is done rather than partial)
                            DecorationType openDecorations = (decorationMap[DecorationType.Background] == null ? DecorationType.Background : DecorationType.None)
                                | (decorationMap[DecorationType.Foreground] == null ? DecorationType.Foreground : DecorationType.None)
                                | (decorationMap[DecorationType.Style] == null ? DecorationType.Style : DecorationType.None)
                                | (decorationMap[DecorationType.Weight] == null ? DecorationType.Weight : DecorationType.None);

                            if (openDecorations.HasFlag(dec.DecorationFlags))
                            {
                                //check which styles is open so we can add new style if the decoration is open
                                if (decorationMap[DecorationType.Background] == null && dec.Background.HasValue)
                                {
                                    decorationMap[DecorationType.Background] = dec;
                                }
                                if (decorationMap[DecorationType.Foreground] == null && dec.Foreground.HasValue)
                                {
                                    decorationMap[DecorationType.Foreground] = dec;
                                }
                                if (decorationMap[DecorationType.Style] == null && dec.FontStyle.HasValue)
                                {
                                    decorationMap[DecorationType.Style] = dec;
                                }
                                if (decorationMap[DecorationType.Weight] == null && dec.FontWeight.HasValue)
                                {
                                    decorationMap[DecorationType.Weight] = dec;
                                }
                            }
                            //add back to mem since we're still tracking this decoration
                            remaining.Add(dec);
                        }
                    }

                    //set the mem to whatever the remaining strings are
                    decorationMem = remaining;
                }

                //if anything has changed add previous builder and create a new run
                if (backgroundChanged || foregroundChanged || styleChanged || weightChanged)
                {
                    if (builder != null && builder.Length > 0)
                    {
                        System.Windows.Documents.Run run = new System.Windows.Documents.Run(builder.ToString());
                        //set the current run values
                        run.Background = currentBackground.HasValue ? new SolidColorBrush(currentBackground.Value) : defaultIsSelectedBackground;
                        run.Foreground = currentForeground.HasValue ? new SolidColorBrush(currentForeground.Value) : defaultIsSelectedForeground;
                        run.FontStyle = currentStyle.HasValue ? currentStyle.Value : textblock.FontStyle;
                        run.FontWeight = currentWeight.HasValue ? currentWeight.Value : textblock.FontWeight;

                        //reset the  values
                        currentBackground = null;
                        currentForeground = null;
                        currentStyle = null;
                        currentWeight = null;

                        textblock.Inlines.Add(run); //add the new run decoration
                        builder = new StringBuilder(); //reset the builder for the next run
                    }
                    
                    //reset the changed flags
                    backgroundChanged = false;
                    foregroundChanged = false;
                    styleChanged = false;
                    weightChanged = false;

                }

                builder.Append(textblock.BoundText[i]);
            }

            //add the final run
            System.Windows.Documents.Run finalRun = new System.Windows.Documents.Run(builder.ToString());
            finalRun.Background = decorationMap[DecorationType.Background] != null && decorationMap[DecorationType.Background].Background.HasValue ? new SolidColorBrush(decorationMap[DecorationType.Background].Background.Value) : defaultIsSelectedBackground;
            finalRun.Foreground = decorationMap[DecorationType.Foreground] != null && decorationMap[DecorationType.Foreground].Foreground.HasValue ? new SolidColorBrush(decorationMap[DecorationType.Foreground].Foreground.Value) : defaultIsSelectedForeground;
            finalRun.FontStyle = decorationMap[DecorationType.Style] != null && decorationMap[DecorationType.Style].FontStyle.HasValue ? decorationMap[DecorationType.Style].FontStyle.Value : textblock.FontStyle;
            finalRun.FontWeight = decorationMap[DecorationType.Weight] != null && decorationMap[DecorationType.Weight].FontWeight.HasValue ? decorationMap[DecorationType.Weight].FontWeight.Value : textblock.FontWeight;

            textblock.Inlines.Add(finalRun); //add the new run decoration

        }
    }

    public class TextBlockDecoration : Freezable
    {
        #region DecoratedText
        public static readonly DependencyProperty DecoratedTextProperty =
            DependencyProperty.Register("DecoratedText", typeof(IEnumerable<TextSelection>), typeof(TextBlockDecoration),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDecoratedTextPropertyChanged));

        public IEnumerable<TextSelection> DecoratedText
        {
            get { return (IEnumerable<TextSelection>)this.GetValue(DecoratedTextProperty); }
            set { this.SetValue(DecoratedTextProperty, value); }
        }

        private static void OnDecoratedTextPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            TextBlockDecoration control = target as TextBlockDecoration;
            if (control == null)
                return;

            //setup new command
            IEnumerable<TextSelection> newText = args.NewValue as IEnumerable<TextSelection>;
            if (newText == null)
                return;

            control.DecoratedText = newText;
            control.onStylesUpdated();
        }
        #endregion

        #region Background Color
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(System.Windows.Media.Color?), typeof(TextBlockDecoration),
            new PropertyMetadata(null, OnBackgroundColorPropertyChanged));

        public System.Windows.Media.Color? BackgroundColor
        {
            get { return (System.Windows.Media.Color?)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        private static void OnBackgroundColorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TextBlockDecoration control = source as TextBlockDecoration;
            System.Windows.Media.Color? currentColor = (System.Windows.Media.Color?)e.NewValue;
            control.BackgroundColor = currentColor;
            control.onStylesUpdated();
        }
        #endregion

        #region Foreground Color
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor", typeof(System.Windows.Media.Color?), typeof(TextBlockDecoration),
            new PropertyMetadata(null, OnForegroundColorPropertyChanged));

        public System.Windows.Media.Color? ForegroundColor
        {
            get { return (System.Windows.Media.Color?)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        private static void OnForegroundColorPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TextBlockDecoration control = source as TextBlockDecoration;
            System.Windows.Media.Color? currentColor = (System.Windows.Media.Color?)e.NewValue;
            control.ForegroundColor = currentColor;
            control.onStylesUpdated();
        }
        #endregion

        #region FontStyle
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle?), typeof(TextBlockDecoration),
            new PropertyMetadata(null, OnFontStylePropertyChanged));

        public FontStyle? FontStyle
        {
            get { return (FontStyle?)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        private static void OnFontStylePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TextBlockDecoration control = source as TextBlockDecoration;
            FontStyle? currentStyle = (FontStyle?)e.NewValue;
            control.FontStyle = currentStyle;
            control.onStylesUpdated();
        }
        #endregion

        #region FontWeight
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight?), typeof(TextBlockDecoration),
            new PropertyMetadata(null, OnFontWeightPropertyChanged));

        public FontWeight? FontWeight
        {
            get { return (FontWeight?)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        private static void OnFontWeightPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TextBlockDecoration control = source as TextBlockDecoration;
            FontWeight? currentWeight = (FontWeight?)e.NewValue;
            control.FontWeight = currentWeight;
            control.onStylesUpdated();
        }
        #endregion

        public event EventHandler<TextBlockDecoration> StylesUpdated;
        private void onStylesUpdated()
        {
            if (this.StylesUpdated != null)
            {
                this.StylesUpdated(this, this);
            }
        }

        // All that's needed to make this Freezable
        protected override Freezable CreateInstanceCore()
        {
            return new TextBlockDecoration();
        }
    }
}
