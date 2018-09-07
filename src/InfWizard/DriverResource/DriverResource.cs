using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace InfWizard
{
    public class DriverResource
    {
        private Dictionary<string, byte[]> mDriverFiles;
        private Dictionary<string, string> mStrings;
        ResourceReader mResourceReader;

        public string DisplayName
        {
            get
            {
                return Strings[DrvResKey.DisplayName.ToString()];
            }
        }
        public string Description
        {
            get
            {
                return Strings[DrvResKey.Description.ToString()];
            }
        }
        public override string ToString()
        {
            return DisplayName;
        }
        public DriverResource(Stream stream) { LoadResource(stream); }

        public DriverResource(string file)
        {
            FileStream stream = File.OpenRead(file);
            LoadResource(stream);
            //stream.Close();
        }

        public Dictionary<string, byte[]> Files
        {
            get
            {
                if (!ReferenceEquals(mDriverFiles, null)) return mDriverFiles;
                InfWizardStatus.Log(CategoryType.DriverResource, StatusType.Info, "loading driver resources {0}..", DisplayName);
                mDriverFiles = new Dictionary<string, byte[]>();
                foreach (DictionaryEntry entry in mResourceReader)
                {
                    if (entry.Value is byte[])
                        mDriverFiles.Add((string) entry.Key, (byte[]) entry.Value);
                }
                return mDriverFiles;
            }
        }

        public Dictionary<string, string> Strings
        {
            get
            {
                if (!ReferenceEquals(mStrings, null)) return mStrings;
                
                mStrings = new Dictionary<string, string>();
                foreach (DictionaryEntry entry in mResourceReader)
                {
                    if (entry.Value is string)
                        mStrings.Add((string)entry.Key, (string)entry.Value);
                }               
                
                return mStrings;
            }
        }

        private void LoadResource(Stream file)
        {
            mResourceReader = new ResourceReader(file);
        }
    }


    public class DriverResourceList : List<DriverResource>
    {
        public new bool Add(DriverResource driverResource)
        {
            bool isEqual = false;

            foreach (DriverResource resource in this)
            {
                Dictionary<String,String> origResString = resource.Strings;
                Dictionary<String,String> newResString = resource.Strings;

                if (resource.DisplayName == driverResource.DisplayName)
                {
                    isEqual = true;
                    foreach (string key in origResString.Keys)
                    {
                        string newValue;
                        if (!newResString.TryGetValue(key, out newValue))
                        {
                            isEqual = false;
                            break;
                        }
                        if (origResString[key] != newValue)
                        {
                            isEqual = false;
                            break;
                        }
                    }
                    if (isEqual) break;
                }
            }

            if (isEqual) return false;

            base.Add(driverResource);
            return true;
        }
    }
}