/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System.Collections.Generic;

namespace ZScript.Utils
{
    /// <summary>
    /// Collection of List utility extension methods
    /// </summary>
    public static class ListUtils
    {
        /// <summary>
        /// Returns whether a list contains the reference of one object in it
        /// </summary>
        /// <typeparam name="T">The type of object contained in the list</typeparam>
        /// <param name="list">The list of items to search in</param>
        /// <param name="obj">The object to find in the list</param>
        /// <returns>True if the item is in the list, false otherwise</returns>
        public static bool ContainsReference<T>(this IList<T> list, T obj)
        {
            return list.IndexOfReference(obj) != -1;
        }

        /// <summary>
        /// Removes an element from an array by reference and returns whether the item was in the list
        /// </summary>
        /// <typeparam name="T">The type of object contained in the list</typeparam>
        /// <param name="list">The list of items to operate on</param>
        /// <param name="obj">The object to remove from the list</param>
        /// <returns>True if the item was in the list and was removed, false otherwise</returns>
        public static bool RemoveReference<T>(this IList<T> list, T obj)
        {
            int index = list.IndexOfReference(obj);

            if (index == -1)
                return false;

            list.RemoveAt(index);

            return true;
        }

        /// <summary>
        /// Returns the index of the first item that matches the reference of another given object
        /// </summary>
        /// <typeparam name="T">The type of object contained in the list</typeparam>
        /// <param name="list">The list of items to search in</param>
        /// <param name="obj">The object to find in the list</param>
        /// <returns>Returns a zero-based index of the first item that matches the reference of another object, or -1, if none is found</returns>
        public static int IndexOfReference<T>(this IList<T> list, T obj)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], obj))
                    return i;
            }

            return -1;
        }
    }
}