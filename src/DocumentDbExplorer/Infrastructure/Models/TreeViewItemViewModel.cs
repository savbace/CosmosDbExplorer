﻿using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;
using System.Threading.Tasks;
using DocumentDbExplorer.Messages;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public interface ITreeViewItemViewModel
    {
        ObservableCollection<TreeViewItemViewModel> Children { get; }
        bool HasDummyChild { get; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool IsLoading { get; set; }
        TreeViewItemViewModel Parent { get; }
    }

    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : ObservableObject
    {
        private static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();
        private readonly ObservableCollection<TreeViewItemViewModel> _children;

        private bool _isExpanded;

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, IMessenger messenger, bool lazyLoadChildren)
        {
            Parent = parent;
            MessengerInstance = messenger;
            _children = new ObservableCollection<TreeViewItemViewModel>();

            if (lazyLoadChildren)
            {
                _children.Add(DummyChild);
            }
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return Children.Count == 1 && Children[0] == DummyChild; }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    RaisePropertyChanged(() => IsExpanded);
                }

                // Expand all the way up to the root.
                if (_isExpanded && Parent != null)
                {
                    Parent.IsExpanded = true;
                }

                // Lazy load the child items, if necessary.
                if (HasDummyChild)
                {
                    Children.Remove(DummyChild);
                    Task.Run(() => LoadChildren());
                }
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        public void OnIsSelectedChanged()
        {
            MessengerInstance.Send(new TreeNodeSelectedMessage(this));
        }

        public bool IsLoading { get; set; }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual Task LoadChildren()
        {
            return Task.FromResult<object>(null);
        }

        public TreeViewItemViewModel Parent
        {
            get; private set;
        }

        public IMessenger MessengerInstance { get; }
    }
}
