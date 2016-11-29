using System;
using System.Collections.Generic;
using System.IO;

namespace AcutisProcessingDevelopment
{
    static class UpdateManager
    {
        //Dictionary<string, DateTime> lastWriteTimes = new Dictionary<string, DateTime>();
     
        public static int CheckForNewFileWrites(Dictionary<string,DateTime> LastWriteTimes, string[] AKVAconnectExportFilesFolderPath)
        {
            int retVal = 0;
            // creates a backup of the last dictionary and recreates the a new one with the current file write time in it.
            // returns the count of changes if any of the files have been written to. Returns -1 if errors
            try
            {
                Dictionary<string, DateTime> previousWriteTimes = new Dictionary<string, DateTime>(LastWriteTimes);     // make a copy of the existing "lastWriteTimes" dictionary.

                LastWriteTimes.Clear();                                     // clear the contents of the dictionary
                foreach (string file in AKVAconnectExportFilesFolderPath)
                {
                    DateTime dt = File.GetLastWriteTime(file);              // get the last write time from the file
                    LastWriteTimes.Add(file, dt);                           // update the dictionary with all files
                }

                    try
                    {
                        foreach (var key in previousWriteTimes.Keys)
                        {
                            if (!previousWriteTimes[key].Equals(LastWriteTimes[key]))
                                retVal += 1;        // +1 to change count
                        }
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
            }
            catch (Exception)
            {
                return -1;
            }

            return retVal;
        }
    }
}
