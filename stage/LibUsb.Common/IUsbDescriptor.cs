using System;
using System.Collections.Generic;
using System.Text;

namespace LibUsb.Common
{
    public interface IUsbDescriptor
    {
        ///<summary>
        ///Returns a <see cref="T:System.String"/> that represents the current <see cref="UsbInterfaceDescriptor"/>.
        ///</summary>
        ///
        ///<param name="prefixSeperator">The field prefix string.</param>
        ///<param name="entitySperator">The field/value seperator string.</param>
        ///<param name="suffixSeperator">The value suffix string.</param>
        ///<returns>A formatted representation of the <see cref="UsbInterfaceDescriptor"/>.</returns>
        string ToString(string prefixSeperator, string entitySperator, string suffixSeperator);
    }
}
