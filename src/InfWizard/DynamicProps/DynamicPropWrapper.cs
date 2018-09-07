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
using System.ComponentModel;
using System.Xml.Serialization;

namespace DynamicProps
{
    /// <summary>
    /// MAIN CLASS
    /// </summary>
    /// <remarks>
    /// Wrap this class around your class object and point the PropertyGrid SelectedObject property to DynamicPropWrapper.DynamicDescriptor for dynamic property control.
    /// </remarks>
    public class DynamicPropWrapper
    {
        private DynamicTypeDescriptor mDynamicTypeDescriptor;
        private Object mInstance;
        private DynPropertySortTypes mPropertySortType = DynPropertySortTypes.Default;

        /// <summary>
        /// ALWAYS USE this constructor when wrapping a class.
        /// </summary>
        /// <param name="instance">The object whose properties are to be filtered.</param>
        public DynamicPropWrapper(object instance)
        {
            mInstance = instance;
            mDynamicTypeDescriptor = new DynamicTypeDescriptor(mPropertySortType, mInstance, false);
        }

        /// <summary>
        /// This constructor is only used if DynamicPropWrapper is inherited.
        /// </summary>
        public DynamicPropWrapper()
        {
            mInstance = this;
            mDynamicTypeDescriptor = new DynamicTypeDescriptor(mPropertySortType, mInstance, true);
        }

        /// <summary>
        /// Used to maniplulate dynamic property information.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public DynPropertyOverrideList OverrideProps
        {
            get
            {
                return mDynamicTypeDescriptor.mOverrideProps;
            }
        }

        /// <summary>
        /// Indexer fot Accessing the OverrideProps property.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public DynPropertyOverride this[string propName]
        {
            get
            {
                return mDynamicTypeDescriptor.mOverrideProps[propName];
            }
            set
            {
                mDynamicTypeDescriptor.mOverrideProps[propName] = value;
            }
        }

        /// <summary>
        /// Returns the DynamicDescriptor object which is used by the PropertyGrid.
        /// </summary>
        [Browsable(false)]
        public DynamicTypeDescriptor DynamicDescriptor
        {
            get
            {
                return mDynamicTypeDescriptor;
            }
        }

        /// <summary>
        /// An instance of the class whose properties you want to dynamically filter/modify.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public Object Instance
        {
            get
            {
                return mInstance;
            }
            set
            {
                mInstance = value;

                mDynamicTypeDescriptor = new DynamicTypeDescriptor(mPropertySortType, mInstance, false);
            }
        }


        /// <summary>
        /// Change the order in which properties are listed in the PropertyGrid
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public DynPropertySortTypes PropertySortType
        {
            get
            {
                return mDynamicTypeDescriptor.mPropertySortType;
            }
            set
            {
                mPropertySortType = value;
                mDynamicTypeDescriptor.mPropertySortType = mPropertySortType;
            }
        }


        /// <summary>
        /// Returns the default value via the DynamicTypeDescriptor.  This is the same value that the PropertyGrid will see when it asks for the DefaultValue.
        /// </summary>
        public object GetDefaultPropValue(string propName)
        {
            foreach (DynamicPropDescriptor dynProp in mDynamicTypeDescriptor.mPropDescriptors)
            {
                if (dynProp.Name == propName)
                    return dynProp.GetDefaultValue();
            }
            return null;
        }

        /// <summary>
        /// Returns the property value via the DynamicTypeDescriptor.  This is the same value that the PropertyGrid will see when it asks for the property value.
        /// </summary>
        public object GetPropValue(string propName)
        {
            foreach (DynamicPropDescriptor dynProp in mDynamicTypeDescriptor.mPropDescriptors)
            {
                if (dynProp.Name == propName)
                    return dynProp.GetValue(mInstance);
            }
            return null;
        }
    }
}