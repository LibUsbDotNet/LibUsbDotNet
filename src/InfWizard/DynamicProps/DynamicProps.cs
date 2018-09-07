// Copyright © 2009 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  trobinso@users.sourceforge.net
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
// 
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicProps
{
    /// 
    /// <summary>
    /// Change the order in which properties appear.
    /// </summary>
    /// <remarks>
    /// NOTE!  The PropertyGrid View ToolBar Buttons(Categorized/Alphabetical) will always
    /// set the PropertySort to either CategorizedAphabetical or Aphabetical.  This overrides
    /// any sorting done with the property descriptors.
    /// 
    /// To REMEDY this, add an event handler to the PropertySortChanged event and force the 
    /// PropertySort to either Categorized or None.
    /// </remarks>
    /// 
    public enum DynPropertySortTypes
    {
        Default,
        Natural,
        UsePropertySortAttributes
    }

    public class DynAttribute
    {
        private readonly Type mType;
        private readonly Attribute mValue;

        public DynAttribute(Attribute value)
        {
            mType = value.GetType();
            mValue = value;
        }

        public Attribute Value
        {
            get
            {
                return mValue;
            }
        }


        public Type Type
        {
            get
            {
                return mType;
            }
        }

        /// <summary>
        /// Checks of the attributes are of the same type/basetype
        /// All of the IsEqual functions end up here.
        /// </summary>
        /// <param name="tpAttribute1"></param>
        /// <param name="tpAttribute2"></param>
        /// <returns></returns>
        private static bool IsEqual(Type tpAttribute1, Type tpAttribute2)
        {
            return (tpAttribute1.IsAssignableFrom(tpAttribute2));
        }

        private static bool IsEqual(DynAttribute dynAttribute1, DynAttribute dynAttribute2)
        {
            return IsEqual(dynAttribute1.Type, dynAttribute2.Type);
        }

        private static bool IsEqual(DynAttribute dynAttribute1, Attribute sysAttribute)
        {
            return IsEqual(dynAttribute1.Type, sysAttribute.GetType());
        }

        private static bool IsEqual(DynAttribute dynAttribute1, Type tpAttribute)
        {
            return IsEqual(dynAttribute1.Type, tpAttribute);
        }

        public static Boolean operator ==(DynAttribute attr1, DynAttribute attr2)
        {
            throw new Exception("REMOVED: Use IsEqual(object obj) instead.");
        }

        public static Boolean operator !=(DynAttribute attr1, DynAttribute attr2)
        {
            throw new Exception("REMOVED: Use !IsEqual(object obj) instead.");
        }

        /// <summary>
        /// Explicitly call the Equals functions and it can compare itself to many objects types.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            // This is the "typeof" operator
            Type tp = obj.GetType();
            if (tp.Name == "RuntimeType")
                return IsEqual(this, (Type) obj);


            if (tp == typeof (DynAttribute))
                return IsEqual(this, (DynAttribute) obj);

            if (tp == typeof (Attribute))
                return IsEqual(this, (Attribute) obj);

            if (tp.IsAssignableFrom(typeof (Attribute)))
                return IsEqual(this, (Attribute) obj);

            return false;
        }

        public override int GetHashCode()
        {
            return Type.FullName.GetHashCode();
        }
    }

    /// <summary>
    /// Stores the users dynamic property information.
    /// </summary>
    public class DynAttributeOverrideList
    {
        private readonly List<DynAttribute> mList = new List<DynAttribute>();

        public DynAttribute this[int index]
        {
            get
            {
                return mList[index];
            }
            set
            {
                mList[index] = value;
            }
        }


        public int Count
        {
            get
            {
                return mList.Count;
            }
        }


        public int Add(Attribute value)
        {
            mList.Add(new DynAttribute(value));
            return mList.Count - 1;
        }

        /// <summary>
        /// Remove any existing attributes with the same type as "value", then add "value" to the override list.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Replace(Attribute value)
        {
            int iRemove = Remove(value);
            if (iRemove != -1)
                Insert(iRemove, value);
            else
                iRemove = Add(value);

            return iRemove;
        }

        public void Clear()
        {
            mList.Clear();
        }

        /// <summary>
        /// All Contains end up here
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public bool Contains(Type attributeType)
        {
            foreach (DynAttribute dynAttr in mList)
                if (dynAttr.Equals(attributeType))
                    return true;

            return false;
        }

        public bool Contains(Attribute value)
        {
            return Contains(value.GetType());
        }

        public bool Contains(DynAttribute value)
        {
            return Contains(value.Type);
        }

        /// <summary>
        /// All IndexOfs end up here
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(Type value)
        {
            for (int i = 0; i < mList.Count; i++)
                if (mList[i].Equals(value))
                    return i;
            return -1;
        }

        public int IndexOf(Attribute value)
        {
            return IndexOf(value.GetType());
        }

        public int IndexOf(DynAttribute value)
        {
            return IndexOf(value.Type);
        }

        public void Insert(int index, Attribute value)
        {
            mList.Insert(index, new DynAttribute(value));
        }

        /// <summary>
        /// All Removes end up here
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Remove(Type value)
        {
            int iRemove = -1;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].Equals(value))
                {
                    iRemove = i;
                    mList.RemoveAt(iRemove);
                }
            }
            return iRemove;
        }

        public int Remove(Attribute value)
        {
            return Remove(value.GetType());
        }

        public int Remove(DynAttribute value)
        {
            return Remove(value.Type);
        }

        public void RemoveAt(int index)
        {
            mList.RemoveAt(index);
        }


        public IEnumerator<DynAttribute> GetEnumerator()
        {
            return mList.GetEnumerator();
        }
    }

    public class DynPropertyOverride
    {
        private DynAttributeOverrideList mOverrides = new DynAttributeOverrideList();

        public DynAttributeOverrideList Overrides
        {
            get
            {
                return mOverrides;
            }
            set
            {
                mOverrides = value;
            }
        }


        public Attribute this[Type tp]
        {
            get
            {
                int iIndex;
                if ((iIndex = mOverrides.IndexOf(tp)) != -1)
                    return mOverrides[iIndex].Value;

                return null;
            }
            set
            {
                int iIndex;
                if ((iIndex = mOverrides.IndexOf(tp)) != -1)
                    mOverrides[iIndex] = new DynAttribute(value);
                else
                    mOverrides.Add(value);
            }
        }
    }

    /// <summary>
    /// Stores the users dynamic property information.
    /// </summary>
    public class DynPropertyOverrideList : Dictionary<string, DynPropertyOverride>
    {
        private Object mDefValueObject;

        /// <summary>
        /// get/set the overriden property with the specified property name.
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public new DynPropertyOverride this[string propName]
        {
            get
            {
                DynPropertyOverride rtn = null;
                if (!base.TryGetValue(propName, out rtn))
                {
                    rtn = new DynPropertyOverride();
                    base.Add(propName, rtn);
                }
                return rtn;
            }
            set
            {
                if (base.ContainsKey(propName))
                    base[propName] = value;
                else
                    base.Add(propName, value);
            }
        }

        /// <summary>
        /// Set this to a seperate instance of your class.  Default values will then be read from this instance.
        /// MUST be the same Type as mInstance.
        /// </summary>
        /// <example>
        /// <code>
        /// MyClass myClass = New MyClass();
        /// MyClass myClass_Defaults = New MyClass();
        /// DynamicPropWrapper helper = new DynamicPropWrapper(myClass);
        /// helper.OverrideProps.DefaultValueObject = myClass_Defaults;
        /// </code>
        /// You can now access all of the default values via myClass_Defaults and the read values via myClass,
        /// </example>
        public Object DefaultValueObject
        {
            get
            {
                return mDefValueObject;
            }
            set
            {
                mDefValueObject = value;
            }
        }

        public new void Clear()
        {
            base.Clear();
            mDefValueObject = null;
        }

        /// <summary>
        /// Removes the overriden attribute specified by attirbuteTypes from all properties.
        /// </summary>
        public int RemoveOverridesByType(Type[] attributeTypes)
        {
            int iRemoved = 0;
            foreach (KeyValuePair<string, DynPropertyOverride> dynProp in this)
            {
                for (int iType = 0; iType < attributeTypes.Length; iType++)
                {
                    DynPropertyOverride dynOverride = dynProp.Value;
                    for (int iOverride = 0; iOverride < dynOverride.Overrides.Count; iOverride++)
                    {
                        DynAttribute dynAttribute = dynOverride.Overrides[iOverride];
                        if (dynAttribute.Equals(attributeTypes[iType]))
                        {
                            dynOverride.Overrides.RemoveAt(iOverride);
                            iOverride--;
                            iRemoved++;
                        }
                    }
                }
            }
            return iRemoved;
        }
    }

    /// <summary>
    /// Sorts properties so they appear in the  same order as they were defined in code.
    /// </summary>
    /// <remarks></remarks>
    internal class DynSortPropsByNatural
    {
        public static List<DynamicPropDescriptor> Sort(PropertyInfo[] propertyInfos, List<DynamicPropDescriptor> descriptorList)
        {
            List<DynamicPropDescriptor> sortedList = new List<DynamicPropDescriptor>(descriptorList.Count);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                foreach (DynamicPropDescriptor propDescriptor in descriptorList)
                {
                    if (propDescriptor.Name == propertyInfo.Name)
                    {
                        sortedList.Add(propDescriptor);
                        break;
                    }
                }
            }

            return sortedList;
        }
    }

    /// <summary>
    /// Sorts properties by a string supplied by a <see cref="PropertySortAttribute"/>
    /// </summary>
    internal class DynSortPropsByAttribute : IComparer<DynamicPropDescriptor>
    {
        #region IComparer<DynamicPropDescriptor> Members

        public int Compare(DynamicPropDescriptor x, DynamicPropDescriptor y)
        {
            PropertySortAttribute psaX = (PropertySortAttribute) x.GetAttribute(typeof (PropertySortAttribute));
            PropertySortAttribute psaY = (PropertySortAttribute) y.GetAttribute(typeof (PropertySortAttribute));

            if (psaX != null && psaY != null)
                return psaX.SortPosition.CompareTo(psaY.SortPosition);
            else if (psaX != null && psaY == null)
                return 1;
            else if (psaX == null && psaY != null)
                return -1;
            else
                return x.Name.CompareTo(y.Name);
        }

        #endregion
    }

    /// <summary>
    /// Attribute for controlling the property sort order.
    /// </summary>
    public class PropertySortAttribute : Attribute
    {
        private readonly int mSort;

        public PropertySortAttribute(int sort)
        {
            mSort = sort;
        }

        public int SortPosition
        {
            get
            {
                return mSort;
            }
        }
    }
}