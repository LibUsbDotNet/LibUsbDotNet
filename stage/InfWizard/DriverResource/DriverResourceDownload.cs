// Copyright © 2006-2010 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace InfWizard
{
    public class DriverResourceDownload
    {
        private string mDescription;
        private string mDisplayName;
        private string mUrl;

        public string Url
        {
            get { return mUrl; }
            set { mUrl = value; }
        }

        [XmlAttribute]
        public string DisplayName
        {
            get { return mDisplayName; }
            set { mDisplayName = value; }
        }

        public string Description
        {
            get { return mDescription; }
            set { mDescription = value; }
        }

        public override string ToString() { return DisplayName; }
    }

    public class DriverResourceDownloads : List<DriverResourceDownload>
    {
        private static XmlSerializer _driverResourcesSerializer;

        public static XmlSerializer Serializer
        {
            get
            {
                if (ReferenceEquals(_driverResourcesSerializer, null))
                    _driverResourcesSerializer = new XmlSerializer(typeof (DriverResourceDownloads));
                return _driverResourcesSerializer;
            }
        }

        public static DriverResourceDownloads Load(Stream stream) { return Serializer.Deserialize(stream) as DriverResourceDownloads; }


        public void Save(Stream stream) { Serializer.Serialize(stream, this); }
    }
}