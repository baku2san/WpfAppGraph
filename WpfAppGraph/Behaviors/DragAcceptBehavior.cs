﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;

namespace WpfAppGraph.Behaviors
{
    public sealed class DragAcceptBehavior : Behavior<FrameworkElement>
    {
        public DragAcceptDescription Description
        {
            get { return (DragAcceptDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DragAcceptDescription),
            typeof(DragAcceptBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewDragOver += AssociatedObject_DragOver;
            this.AssociatedObject.PreviewDrop += AssociatedObject_Drop;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewDragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.PreviewDrop -= AssociatedObject_Drop;
            base.OnDetaching();
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            var desc = Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            var desc = Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDrop(e);
            e.Handled = true;
        }
    }
}