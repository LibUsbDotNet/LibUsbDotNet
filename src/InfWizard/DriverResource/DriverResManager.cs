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
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace InfWizard
{
    internal static class DriverResManager
    {
        private static DriverResourceList mDriverResourceList;

        public static DriverResourceList ResourceList
        {
            get
            {
                if (ReferenceEquals(mDriverResourceList, null))
                    mDriverResourceList = new DriverResourceList();

                return mDriverResourceList;
            }
        }

        private static void loadResourcesFromPath(string path)
        {
            // try loading resources from the working directory.
            try
            {

                if (!Directory.Exists(path)) return;

#if DEBUG
                InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Info, "loading resource(s) from {0}", path);
#endif
                string[] files = Directory.GetFiles(path, "*.driver.resources");
                foreach (string file in files)
                {
                    DriverResource newDriverResource = new DriverResource(file);
                    if (!ResourceList.Add(newDriverResource))
                    {
#if DEBUG
                        InfWizardStatus.Log(CategoryType.DriverResource,
                                            StatusType.Warning,
                                            "skipping duplicate driver resource {0} found in {1}",
                                            newDriverResource.DisplayName,
                                            file);
#endif
                    }
                    else
                    {
                        InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Info, "{0} loaded", newDriverResource.DisplayName);
                    }
                }
                if (ResourceList.Count > 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Info, "failed loading driver resources from working directory :{0}", ex.ToString());
            }
            return;
        }

        private static void loadResourcesFromAssembly(Assembly assembly)
        {
            // try loading resources from the executing assembly.
            try
            {
                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    if (resourceName.ToLower().EndsWith(".driver.resources"))
                    {
                        DriverResource newDriverResource = new DriverResource(assembly.GetManifestResourceStream(resourceName));
                        ResourceList.Add(newDriverResource);
                        InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Info, "{0} loaded", newDriverResource.DisplayName);

                    }
                }
            }
            catch (Exception ex)
            {
                InfWizardStatus.Log(CategoryType.DriverResource,
                                    StatusType.Warning,
                                    "Failed loading driver resources from executing assembly: {0}",
                                    ex.Message);
            }
        }

        public static bool LoadResources()
        {
            mDriverResourceList = new DriverResourceList();

            DirectoryInfo diApp = new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath));
            DirectoryInfo diCurrent = new DirectoryInfo(Environment.CurrentDirectory);
            System.Collections.Generic.List<String> tempResPathList = new System.Collections.Generic.List<string>();

            tempResPathList.Add(diApp.FullName);
            tempResPathList.Add(diApp.FullName + Path.DirectorySeparatorChar + "DriverResources");
            if (diApp.FullName != diCurrent.FullName)
            {
                tempResPathList.Add(diCurrent.FullName);
                tempResPathList.Add(diCurrent.FullName + Path.DirectorySeparatorChar + "DriverResources");
            }

            foreach (string resPath in tempResPathList)
                loadResourcesFromPath(resPath);

            loadResourcesFromAssembly(Assembly.GetExecutingAssembly());


            if (mDriverResourceList.Count == 0)
            {
                InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Warning, "no resources found");
                InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Warning, resInfWizard.DRIVERRESOURCE_NOTFOUND);
                return false;
            }

            return true;
        }

        public static bool Check()
        {
            bool check = false;
            if (mDriverResourceList != null)
            {
                if (mDriverResourceList.Count > 0)
                {
                    foreach (DriverResource driverResource in mDriverResourceList)
                    {
                        if (!driverResource.Strings.ContainsKey(DrvResKey.Description.ToString()))
                            continue;
                        if (!driverResource.Strings.ContainsKey(DrvResKey.DisplayName.ToString()))
                            continue;
                        check = true;
                        break;
                    }
                }
            }
            return check;
        }
    }
}