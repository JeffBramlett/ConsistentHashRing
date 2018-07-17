using ConsistentHashRing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConsistentHashRingApp.ViewModels
{
    class HashRingViewModel: AbstractViewModel
    {
        #region Fields
        HashRing<string> _hashRing;
        HashRingNode<string> _currentNode;
        BindingList<HashRingLocation<string>> _locations;
        #endregion

        #region Command fields
        private ICommand _addLocationCommand;
        private ICommand _addItemCommand;
        private ICommand _deleteSelection;
        #endregion

        #region Properties
        public HashRing<string> HashRing
        {
            get
            {
                _hashRing = _hashRing ?? new HashRing<string>();
                return _hashRing;
            }
        }

        public BindingList<HashRingLocation<string>> Locations
        {
            get
            {
                if(_locations == null)
                {
                    _locations = new BindingList<HashRingLocation<string>>();
                }

                _locations.Clear();

                foreach(var loc in HashRing.Locations)
                {
                    _locations.Add(loc);
                }

                return _locations;
            }
        }

        public HashRingNode<string> CurrentNode
        {
            get { return _currentNode; }
            set
            {
                SetProperty(ref _currentNode, value, () => CurrentNode);
            }
        }

        public ICommand AddLocationCommand
        {
            get
            {
                _addLocationCommand = _addLocationCommand ?? CreateCommand(AddLocation);
                return _addLocationCommand;
            }
        }

        public ICommand AddItemCommand
        {
            get
            {
                _addItemCommand = _addItemCommand ?? CreateCommand(AddItem, HasLocation);
                return _addItemCommand;
            }
        }

        public ICommand DeleteSelectionCommand
        {
            get
            {
                _deleteSelection = _deleteSelection ?? CreateCommand(DeleteSelection, HasSelection);
                return _deleteSelection;
            }
        }
        #endregion

        #region Ctors and Dtors
        public HashRingViewModel():
            base(null)
        {

        }
        #endregion

        #region Private command execution

        private void AddLocation()
        {
            Guid g = Guid.NewGuid();

            HashRing.AddLocation(g.ToString());
            RaisePropertyChanged(() => Locations);
        }

        private bool HasLocation()
        {
            return HashRing.LocationCount > 0;
        }

        private bool HasSelection()
        {
            return CurrentNode != null;
        }

        private void AddItem()
        {
            Guid g = Guid.NewGuid();

            HashRing.AddItem(g.ToString());

            RaisePropertyChanged(() => Locations);
        }

        private void DeleteSelection()
        {
            if (CurrentNode != null)
            {
                if (HashRing.HasLocation(CurrentNode.Item))
                {
                    HashRing.RemoveLocation(CurrentNode.Item);
                }
                else
                {
                    HashRing.RemoveItem(CurrentNode.Item);
                }

                RaisePropertyChanged(() => Locations);
            }
        }
        #endregion
    }
}
