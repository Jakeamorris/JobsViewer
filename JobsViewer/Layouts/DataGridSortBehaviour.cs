using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace RJMPrintingJobs.Misc
{
    /// <summary>
    /// DataGrid SortDescriptions break when the data source changes, this fixes that
    /// </summary>
    public class DataGridSortBehaviour : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SortDescriptionsProperty = DependencyProperty.Register(
            "SortDescriptions",
            typeof(IEnumerable<SortDescription>),
            typeof(DataGridSortBehaviour),
            new FrameworkPropertyMetadata(null, SortDescriptionsPropertyChanged));

        /// <summary>
        ///     Storage for initial SortDescriptions
        /// </summary>
        private IEnumerable<SortDescription> _internalSortDescriptions;

        /// <summary>
        ///     Property for providing a Binding to Custom SortDescriptions
        /// </summary>
        public IEnumerable<SortDescription> SortDescriptions
        {
            get { return (IEnumerable<SortDescription>)GetValue(SortDescriptionsProperty); }
            set { SetValue(SortDescriptionsProperty, value); }
        }


        protected override void OnAttached()
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpd != null)
            {
                dpd.AddValueChanged(AssociatedObject, OnItemsSourceChanged);
            }
        }

        protected override void OnDetaching()
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpd != null)
            {
                dpd.RemoveValueChanged(AssociatedObject, OnItemsSourceChanged);
            }
        }

        private static void SortDescriptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridSortBehaviour)
            {
                ((DataGridSortBehaviour)d).OnItemsSourceChanged(d, EventArgs.Empty);
            }
        }

        public void OnItemsSourceChanged(object sender, EventArgs eventArgs)
        {
            // save description only on first call, SortDescriptions are always empty after ItemsSourceChanged
            if (_internalSortDescriptions == null)
            {
                // save initial sort descriptions
                var cv = (AssociatedObject.ItemsSource as ICollectionView);
                if (cv != null)
                {
                    _internalSortDescriptions = cv.SortDescriptions.ToList();
                }
            }
            else
            {
                // do not resort first time - DataGrid works as expected this time
                var sort = SortDescriptions ?? _internalSortDescriptions;

                if (sort != null)
                {
                    sort = sort.ToList();
                    var collectionView = AssociatedObject.ItemsSource as ICollectionView;
                    if (collectionView != null)
                    {
                        using (collectionView.DeferRefresh())
                        {
                            collectionView.SortDescriptions.Clear();
                            foreach (var sorter in sort)
                            {
                                collectionView.SortDescriptions.Add(sorter);
                            }
                        }
                    }
                }
            }
        }
    }
}
