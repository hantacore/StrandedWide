using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StrandedWideMod_Harmony
{
    internal static class CustomLogger
    {
        static string filePath = "";
        static StreamWriter logTarget = null;

        public static void InitCustomLogger(string directory)
        {
            filePath = Path.Combine(directory, "StrandedWideLog.log");

            Debug.Log("Stranded Wide (Harmony edition) : placing custom log file in : " + filePath);

            if (File.Exists(filePath))
            {
                File.Delete (filePath);
            }
            logTarget = File.AppendText(filePath);
        }

        internal static void Log(string message)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }

                logTarget.WriteLine(message);
                logTarget.Flush();
            }
            catch (Exception ex)
            {
                Debug.Log("Stranded Wide (Harmony edition) : error while writing custom log : " + ex + Environment.NewLine + message);
            }
            finally
            {
                
            }
        }
    }
}
