using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.ModAPI;

namespace SE_Hellfire_Radar
{
    class SE_debug
    {
        IMyGridTerminalSystem GridTerminalSystem;
        string[] Storage;

        /// <summary>
        /// Start coping
        /// </summary>

        IMyTextPanel debugScreen;

        string help = ("This program will show the output \n" +
            "of any argument that gets handed to it.\n" +
            "as text on a display with the name Debug\n" +
            "It also takes certains arguments as commands:\n" +
            "-help  Shows this text\n" +
            "-up    Scrolls down by one line\n" +
            "-down  Scrolls up by one line\n" +
            "-next  Goes to the next page\n" +
            "-prev  Goes to the previous page\n"
            );

        //the logfile
        List<string> log = new List<string>();

        //int
        int linesOnScreen = 0;
        int currentPageNumber = 1;
        int currentTopLine = 0;

        //constants
        const string debugScreenName = "debug";
        const char lineBreak = '\n';
        const int lineBreakCount = 20;
        const int linesPerPage = 10;
        const int maximumPages = 50;

        void Program()
        {
            debugScreen = GridTerminalSystem.GetBlockWithName(debugScreenName) as IMyTextPanel;

            if (debugScreen != null)
            {
                debugScreen.ShowPrivateTextOnScreen();
                debugScreen.WritePrivateTitle("DEBUG");

                string test = debugScreen.GetValueColor("FontColor") + " " + debugScreen.GetValueColor("BackGroundColor");
                Echo(test);

                if (Storage != null)
                {
                    debugScreen.WritePrivateText("Restoring Logfile", true);
                    LoadStorage();
                }
            } else
            {
                Echo("Error: Debugscreen '" + debugScreenName + "' not found");
            }
        }

        void Save()
        {
            Storage = log.ToArray();
        }

        void Main(string Argument)
        {
            switch (Argument)
            {
                case "-up":
                    ScrollUp();
                    break;
                case "-down":
                    ScrollDown();
                    break;
                case "-del":
                    Delete();
                    break;
                case "-help":
                    Help();
                    break;
                case "-next":
                    NextPage();
                    break;
                case "-prev":
                    PreviousPage();
                    break;
                default:
                    WriteLine(Argument);
                    break;
            }
        }

        private void ScrollUp()
        {
            if (currentTopLine - 1 >= 0)
            {
                currentTopLine--;
            } else
            {
                currentTopLine += linesPerPage;
                PreviousPage();
            }
        }

        private void ScrollDown()
        {
            if (currentTopLine + 1 < linesPerPage)
            {
                currentTopLine++;
            }
            else
            {
                currentTopLine = 0;
                NextPage();
            }
        }

        private void PreviousPage()
        {
            if(currentPageNumber - 1 >= 1)
            {
                currentPageNumber--;
                UpdateScreen();
            }
        }

        private void NextPage()
        {
            if (currentPageNumber + 1 <= maximumPages)
            {
                currentPageNumber++;
                UpdateScreen();
            }
        }

        /// <summary>
        /// Just deletes the whole log
        /// </summary>
        private void Delete()
        {
            log.Clear();
            log.Add("cls..");
            UpdateScreen();
        }


        /// <summary>
        /// This will produce some bullshit ...
        /// </summary>
        private void Help()
        {
            WriteToLog(help);
        }

        private void WriteLine(string argument)
        {
            StringBuilder line = new StringBuilder(argument);

            for (int i = 0; i < line.Length; i = i + lineBreakCount)
            {
                line.Append(lineBreak, i);
            }

            WriteToLog(line.ToString());

            UpdateScreen();
        }

        /// <summary>
        /// Draws a new screen with the given variables of current pagenumber and the last pagenumber
        /// </summary>
        private void UpdateScreen()
        {
            debugScreen.WritePrivateText("Debug - Page " + currentPageNumber + "/" + getLastPage(), false);
            for(int i = (currentPageNumber * linesPerPage + currentTopLine); (i < (currentPageNumber * linesPerPage) + linesPerPage) && i < log.Count; i++)
            {
                debugScreen.WritePrivateText(log[i], true);
            }
        }

        /// <summary>
        /// Returns the number of pages int division of the number of lines in the log by the maximal lines per page
        /// Probably should be revisited if there is an easier or cleaner way
        /// </summary>
        /// <returns>log.length/linesPerpage</returns>
        private int getLastPage()
        {
            return (log.Count/linesPerPage);
        }


        /// <summary>
        /// Writes a new string to the log
        /// </summary>
        /// <param name="line">the new line that is added to the log list</param>
        private void WriteToLog(string line)
        {
            string[] lines = line.Split('\n');

            if (lines.Length + log.Count < maximumPages * linesPerPage)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    log.Add(lines[i]);
                }
            }
            else
            {
                WriteToLog("Maximum logsize reached, please delete log");
            }
        }


        private void LoadStorage()
        {
            for (int i = 0; i < Storage.Length; i++)
            {
                log.Add(Storage[i]);
            }
        }




        // stop copying
        void Echo(string echo)
        {
            WriteLine("HALLO: " + echo);
        }
        
    }
}
