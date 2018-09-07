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
using System.Reflection;

namespace DynamicProps
{
    /// <summary>
    /// Provides custom property information.
    /// </summary>
    /// <remarks>Each property has its own DynamicPropDescriptior.  This is the link between a PropertyGrid and a Class Property.</remarks>
    public class DynamicPropDescriptor : PropertyDescriptor
    {
        private readonly DynPropertyOverrideList mDynamicProps;
        private readonly PropertyDescriptor mOrigProp;
        private DynPropertyOverride mDynProp;
        private Object mInstance;

        public DynamicPropDescriptor(Object instance,
                                     PropertyDescriptor origPropertyDescriptor,
                                     DynPropertyOverrideList dynamicProps,
                                     DynPropertyOverride dynProp,
                                     Attribute[] attributes)
            : base(origPropertyDescriptor.Name, attributes)
        {
            mInstance = instance;
            mOrigProp = origPropertyDescriptor;
            mDynamicProps = dynamicProps;
            mDynProp = dynProp;
            //            CreateAttributeCollection();
        }


        public override Type ComponentType
        {
            get
            {
                return mOrigProp.ComponentType;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                Object attrReadOnly = GetAttribute(typeof (ReadOnlyAttribute));
                if (attrReadOnly != null)
                    return ((ReadOnlyAttribute) attrReadOnly).IsReadOnly;

                // Return the original attribute if no dynamic attribute exists.
                return mOrigProp.IsReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return mOrigProp.PropertyType;
            }
        }


        public override bool CanResetValue(object component)
        {
            Object defValue;
            Boolean isEqual;

            if (checkDefaultValue(component, out isEqual, out defValue))
                return !isEqual;

            bool bShouldSerialize;
            if (invokeShouldSerialize(component, out bShouldSerialize))
                return bShouldSerialize;

            MethodInfo miReset;
            miReset = FindMethod(ComponentType, "Reset" + Name, new Type[] {}, null);
            if (miReset != null)
            {
                return true;
            }

            return false;
        }

        public override object GetValue(object component)
        {
            return component.GetType().InvokeMember(Name,
                                                    BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public,
                                                    null,
                                                    component,
                                                    null);
        }

        public override void ResetValue(object component)
        {
            Object defValue;
            Boolean isEqual;

            if (checkDefaultValue(component, out isEqual, out defValue))
            {
                SetValue(component, defValue);
                return;
            }

            MethodInfo miReset = null;
            miReset = FindMethod(ComponentType, "Reset" + Name, new Type[] {}, null);
            if (miReset != null)
            {
                miReset.Invoke(component, null);
            }
        }

        public override void SetValue(object component, object value)
        {
            component.GetType().InvokeMember(Name,
                                             BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public,
                                             null,
                                             component,
                                             new object[] {value});
        }

        public override bool ShouldSerializeValue(object component)
        {
            Object defValue;
            Boolean isEqual;

            if (checkDefaultValue(component, out isEqual, out defValue))
                return !isEqual;

            bool bShouldSerialize;
            if (invokeShouldSerialize(component, out bShouldSerialize))
                return bShouldSerialize;

            return true;
        }


        private bool checkDefaultValue(object component, out Boolean IsEqual, out Object convertedDefValue)
        {
            IsEqual = false;

            if (getDefaultValue(out convertedDefValue))
            {
                Object value = GetValue(component);
                if (value.GetType() == convertedDefValue.GetType())
                {
                    IsEqual = Equals(value, convertedDefValue);
                    return true;
                }

                if (Converter.CanConvertFrom(convertedDefValue.GetType()))
                {
                    try
                    {
                        convertedDefValue = Converter.ConvertFrom(convertedDefValue);
                        IsEqual = Equals(value, convertedDefValue);
                    }
                    catch (Exception)
                    {
                        IsEqual = false;
                    }
                }
                else
                {
                    IsEqual = objCmp(value, convertedDefValue);
                }

                return true;
            }
            return false;
        }

        private bool getDefaultValue(out object value)
        {
            value = null;

            if (mDynamicProps.DefaultValueObject != null)
            {
                value = GetValue(mDynamicProps.DefaultValueObject);
                return true;
            }

            Object oDefValueAttribute = GetAttribute(typeof (DefaultValueAttribute));
            if (oDefValueAttribute != null)
            {
                value = ((DefaultValueAttribute) oDefValueAttribute).Value;
                return true;
            }

            return false;
        }

        private bool invokeShouldSerialize(object component, out Boolean bShouldSerialize)
        {
            MethodInfo miShouldSerialize;
            bShouldSerialize = true;
            miShouldSerialize = FindMethod(ComponentType, "ShouldSerialize" + Name, new Type[] {}, typeof (Boolean));
            if (miShouldSerialize != null)
            {
                Object oResult = miShouldSerialize.Invoke(component, null);
                bShouldSerialize = (Boolean) oResult;
                return true;
            }

            return false;
        }

        private static bool objCmp(object o1, object o2)
        {
            if (o1.GetType().IsClass || o2.GetType().IsClass || o1.GetType() != o2.GetType())
            {
                return (o1.Equals(o2));
            }
            else
            {
                if (o1 == o2)
                    return true;
                else
                    return false;
            }
        }


        public Object GetDefaultValue()
        {
            Object rtn;
            if (getDefaultValue(out rtn))
                return rtn;

            return null;
        }


        internal object GetAttribute(Type attrType)
        {
            foreach (Attribute attr in Attributes)
            {
                if (IsAssignableFrom(attr.GetType(), attrType))
                    return attr;
            }

            return null;
        }

        internal static bool IsAssignableFrom(Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }
    }
}