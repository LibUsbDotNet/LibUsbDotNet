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
using System.ComponentModel;
using System.Reflection;

namespace DynamicProps
{
    public class DynamicTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly bool bInherited;
        private readonly Object mInstance;

        internal DynPropertyOverrideList mOverrideProps = new DynPropertyOverrideList();
        internal List<DynamicPropDescriptor> mPropDescriptors = new List<DynamicPropDescriptor>();
        internal DynPropertySortTypes mPropertySortType;

        public DynamicTypeDescriptor(DynPropertySortTypes propertySortType, object instance, bool inherited)
        {
            mPropertySortType = propertySortType;
            mInstance = instance;
            bInherited = inherited;
        }

        #region ICustomTypeDescriptor Members

        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(mInstance, bInherited);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(mInstance, bInherited);
        }

        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(mInstance, bInherited);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(mInstance, bInherited);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(mInstance, bInherited);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(mInstance, bInherited);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(mInstance, editorBaseType, bInherited);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(mInstance, attributes, bInherited);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(mInstance, bInherited);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(mInstance, bInherited);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return mInstance;
        }

        /// <summary>
        /// Provides custom type information for properties.
        /// </summary>
        /// <remarks>This is where the properties we want exposed are created, each with their own DynamicPropDescriptor Class wich handles the communication with the PropertyGrid.</remarks>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection mOriginalPropertyDescriptors = TypeDescriptor.GetProperties(mInstance, bInherited);

            List<DynamicPropDescriptor> newPropDescriptiors = new List<DynamicPropDescriptor>(mOriginalPropertyDescriptors.Count);
            PropertyDescriptor origProp;
            DynPropertyOverride dynProp;
            for (int i = 0; i < mOriginalPropertyDescriptors.Count; i++)
            {
                origProp = mOriginalPropertyDescriptors[i];
                List<Attribute> newAttributeList = new List<Attribute>(origProp.Attributes.Count);


                foreach (Attribute origAttr in origProp.Attributes)
                    newAttributeList.Add(origAttr);

                dynProp = null;
                if (mOverrideProps.TryGetValue(origProp.Name, out dynProp))
                {
                    if (dynProp.Overrides.Count > 0)
                        doAttributeOverrides(dynProp, newAttributeList);

                    // ADD / MODIFY / REMOVE Property information HERE //
                    // ..
                    /////////////////////////////////////////////////////
                }
                Attribute[] newAttributeArray = new Attribute[newAttributeList.Count];
                newAttributeList.CopyTo(newAttributeArray);

                DynamicPropDescriptor dynNew = new DynamicPropDescriptor(mInstance, origProp, mOverrideProps, dynProp, newAttributeArray);
                newPropDescriptiors.Add(dynNew);
            }

            return sortedPropertyDescriptorCollection(newPropDescriptiors);
        }

        #endregion

        private PropertyDescriptorCollection sortedPropertyDescriptorCollection(List<DynamicPropDescriptor> dynamicPropDescriptors)
        {
            switch (mPropertySortType)
            {
                case DynPropertySortTypes.Natural:
                    PropertyInfo[] propertyInfos = mInstance.GetType().GetProperties();
                    dynamicPropDescriptors = DynSortPropsByNatural.Sort(propertyInfos, dynamicPropDescriptors);

                    break;
                case DynPropertySortTypes.UsePropertySortAttributes:
                    dynamicPropDescriptors.Sort(new DynSortPropsByAttribute());
                    break;
            }
            mPropDescriptors = dynamicPropDescriptors;

            PropertyDescriptor[] newProps = new PropertyDescriptor[dynamicPropDescriptors.Count];
            for (int i = 0; i < newProps.Length; i++)
                newProps[i] = dynamicPropDescriptors[i];

            return new PropertyDescriptorCollection(newProps);
        }

        /// <summary>
        /// Replaces/adds the overriden attributes
        /// </summary>
        private void doAttributeOverrides(DynPropertyOverride dynProp, List<Attribute> newAttributeList)
        {
            int iAttrIndex;

            foreach (DynAttribute dynAttribute in dynProp.Overrides)
            {
                iAttrIndex = -1;
                if (dynAttribute.Value == null)
                    continue;
                for (int iCurAttrIndex = 0; iCurAttrIndex < newAttributeList.Count; iCurAttrIndex++)
                {
                    if (dynAttribute.Equals(newAttributeList[iCurAttrIndex]))
                    {
                        iAttrIndex = iCurAttrIndex;
                        break;
                    }
                }

                if (iAttrIndex != -1)
                {
                    newAttributeList.RemoveAt(iAttrIndex);
                    newAttributeList.Insert(iAttrIndex, dynAttribute.Value);
                }
                else
                {
                    newAttributeList.Add(dynAttribute.Value);
                }
            }
        }
    }
}