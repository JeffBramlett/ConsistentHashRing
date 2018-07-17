using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsistentHashRing
{
    /// <summary>
    /// The HashRing implementation
    /// </summary>
    /// <typeparam name="T">the type of item to use this HashRing with</typeparam>
    public class HashRing<T>
    {
        #region Fields
        SortedDictionary<UInt32, HashRingLocation<T>> _locations;
        #endregion

        #region Properties
        private SortedDictionary<UInt32, HashRingLocation<T>> LocationDictionary
        {
            get
            {
                _locations = _locations ?? new SortedDictionary<uint, HashRingLocation<T>>();
                return _locations;
            }
        }

        /// <summary>
        /// The locations collection
        /// </summary>
        public List<HashRingLocation<T>> Locations
        {
            get
            {
                return LocationDictionary.Values.ToList();
            }
        }
        #endregion

        #region Publics for Location
        /// <summary>
        /// Add a location to HashRing
        /// </summary>
        /// <param name="item">the item to add</param>
        public void AddLocation(T item)
        {
            item.ThrowIfNull();

            UInt32 key = Hashing.HashItem(item);

            HashRingLocation<T> location = new HashRingLocation<T>(key, item);

            LocationDictionary.Add(key, location);

            var ndx = LocationDictionary.IndexOfKey(key);
            var fromNdx = ndx < LocationDictionary.Count - 1 ? ndx + 1 : ndx;

            var fromLocation = LocationDictionary.ElementAt(fromNdx).Value;

            List<UInt32> keysToRemove = new List<uint>();
            foreach (var node in fromLocation.NodeDictionary.Where(n => n.Key <= key))
            {
                keysToRemove.Add(node.Key);
                location.NodeDictionary.Add(node.Key, node.Value);
            }

            foreach(var removekey in keysToRemove)
            {
                fromLocation.NodeDictionary.Remove(removekey);
            }
        }

        /// <summary>
        /// is there a location for this item
        /// </summary>
        /// <param name="item">the item</param>
        /// <returns>true if there is a location, false otherwise</returns>
        public bool HasLocation(T item)
        {
            item.ThrowIfNull();

            UInt32 key = Hashing.HashItem(item);
            return LocationDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Remove the locations and remap the items it contains
        /// </summary>
        /// <param name="item">the item for the location</param>
        public void RemoveLocation(T item)
        {
            item.ThrowIfNull();

            UInt32 key = Hashing.HashItem(item);
            var ndx = LocationDictionary.IndexOfKey(key);

            if(ndx >= 0)
            {
                var toNdx = ndx < LocationDictionary.Count - 1 ? ndx + 1 : ndx - 1;

                var fromlocation = LocationDictionary.ElementAt(ndx).Value;

                LocationDictionary.Remove(key);

                foreach(var node in fromlocation.NodeDictionary)
                {
                    AddItem(node.Value.Item);
                }
               
            }
        }

        /// <summary>
        /// How many locations does this HashRing have
        /// </summary>
        public int LocationCount
        {
            get { return LocationDictionary.Count; }
        }

        /// <summary>
        /// Get the location for the index
        /// </summary>
        /// <param name="index">the index of the location</param>
        /// <returns>the location at the index</returns>
        public HashRingLocation<T> LocationAt(int index)
        {
            return LocationDictionary.ElementAt(index).Value;
        }

        /// <summary>
        /// Get the index for the location item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int LocationIndexFor(T item)
        {
            item.ThrowIfNull();

            UInt32 key = Hashing.HashItem(item);

            return LocationDictionary.IndexOfKey(key);
        }
        #endregion

        #region Publics for Items
        /// <summary>
        /// Add an Item to the HashRing
        /// </summary>
        /// <param name="item">the item to add</param>
        public void AddItem(T item)
        {
            item.ThrowIfNull();

            bool putInlastNode = true;
            UInt32 key = Hashing.HashItem(item);
            HashRingNode<T> node = new HashRingNode<T>()
            {
                Key = key,
                Item = item
            };

            foreach(var locationKey in LocationDictionary.Keys)
            {
                if(key < locationKey)
                {
                    // Check for key collision
                    if (LocationDictionary[locationKey].NodeDictionary.ContainsKey(key))
                    {
                        var checkNode = LocationDictionary[locationKey].NodeDictionary[key].Next;
                        while (checkNode != null)
                        {
                            checkNode = checkNode.Next;
                        }
                        LocationDictionary[locationKey].DuplicateCount++;
                        checkNode = new HashRingNode<T>() { Key = key, Item = item };
                    }
                    else
                    {
                        LocationDictionary[locationKey].NodeDictionary.Add(key, node);
                    }
                    putInlastNode = false;
                    break;
                }
            }

            if(putInlastNode && LocationDictionary.Count > 0)
            {
                LocationDictionary.ElementAt(LocationDictionary.Count - 1).Value.NodeDictionary.Add(key, node);
            }
        }

        /// <summary>
        /// Remove the item from the HashRing
        /// </summary>
        /// <param name="item">the item to remove</param>
        public void RemoveItem(T item)
        {
            item.ThrowIfNull();

            UInt32 key = Hashing.HashItem(item);

            HashRingLocation<T> foundLocation = null;
            foreach (var locationKey in LocationDictionary.Keys)
            {
                if (key < locationKey)
                {
                    foundLocation = LocationDictionary[locationKey];
                    break;
                }
            }

            if(foundLocation != null && foundLocation.NodeDictionary.ContainsKey(key))
            {
                foundLocation.NodeDictionary.Remove(key);
            }
        }
        #endregion
    }

    #region Extensions
    static class Extensions
    {
        /// <summary>
        /// Throw ArgumentNullException if the object is null
        /// </summary>
        /// <param name="obj">the object itself</param>
        public static void ThrowIfNull(this object obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// Find the index of the key in the HashRingLocation
        /// </summary>
        /// <typeparam name="T">the type of the item</typeparam>
        /// <param name="sortedDict">the extension class</param>
        /// <param name="keyToFind">the key to find</param>
        /// <returns></returns>
        public static int IndexOfKey<T>(this SortedDictionary<uint, HashRingLocation<T>> sortedDict, UInt32 keyToFind)
        {
            var keys = sortedDict.Keys;

            if (!keys.Any(k => k == keyToFind))
                return -1;

            var foundNdx = keys.ToList().IndexOf(keyToFind);
            return foundNdx;
        }
    }
    #endregion
}
