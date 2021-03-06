﻿#region dontCopy1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace SE_Hellfire_target
{
    public class Program
    {
        IMyGridTerminalSystem GridTerminalSystem;

        internal interface IMyTimerBlock : IMyFunctionalBlock
        {
            bool IsCountingDown { get; }
            float TriggerDelay { get; set; }
        };
        /// <summary>
        /// START COPYING HERE
        /// </summary>
        #endregion
        IMyProgrammableBlock thisBlock;
        IMyRemoteControl hf_Remote;
        IMyTextPanel hf_Status;
        IMyTextPanel hf_Notes;
        IMyTimerBlock hf_Timer;
        IMyFunctionalBlock hf_;

        //Lists     
        List<IMyTerminalBlock> systems = new List<IMyTerminalBlock>();

        //strings     
        string targetStatus = "none";
        string indicator = "--";

        //constants     
        float hellfireLength = 30;

        //Vectors     
        Vector3D LastShipPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D LastTargetPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D targetPosition = new Vector3D(0.0, 0.0, 0.0);

        Vector3D offset = new Vector3D(0.0, 0.0, 0.0);
        Vector3D testProbe = new Vector3D(0.0, 0.0, 0.0);
        Vector3D test1 = new Vector3D(0.0, 0.0, 0.0);


        Vector3D testTarget = new Vector3D(0, 0, 1000);

        Vector3D probe;
        Vector3D sample;

        List<Vector3D> targets = new List<Vector3D>();

        int probeSteps = 1;
        private int steps = 10;

        IMyEntity target = null;

        string helpString = "Use the numkeys to choose\na target from the list\nPress 1 to continue";

        private float radius1 = 1000.0f;
        private float sampleSize = 0.01f;
        private bool chooseTarget;
        private bool detected;
        double tick = 1;

        Program()
        {
            Echo("starting...");
            hf_Remote = GridTerminalSystem.GetBlockWithName("hf_remote") as IMyRemoteControl;
            hf_Status = GridTerminalSystem.GetBlockWithName("hf_status") as IMyTextPanel;
            hf_Notes = GridTerminalSystem.GetBlockWithName("hf_notes") as IMyTextPanel;
            hf_Timer = GridTerminalSystem.GetBlockWithName("hf_timer") as IMyTimerBlock;
            thisBlock = GridTerminalSystem.GetBlockWithName("hf_programmable") as IMyProgrammableBlock;

            systems.InsertRange(systems.Count, new List<IMyTerminalBlock> { hf_Status, hf_Remote, thisBlock });

            LogsDisplay("Hellfire target test: system starting", false);
            hf_Notes.WritePrivateText("Notes:\n", false);
            NotesDisplay("pos: " + hf_Remote.GetPosition() + "\nwm:" + hf_Remote.WorldMatrix.Forward +
"\n\n Probe: " + (hf_Remote.GetPosition() + hf_Remote.WorldMatrix.Forward) + "\n\ntarget1" + testTarget + "\n\nTest:\n");
            getStatus();
        }

        void Main(string Argument)
        {
            switch (Argument)
            {
                case "scan":
                    ScanArea();
                    break;
                case "update":
                    getUpdate();
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    getTarget();
                    break;
                case "list":
                    getList();
                    break;
                case "remote":
                    getRemote(false);
                    break;
                case "probe":
                    getEnemyPosition();
                    break;
                case "cls":
                case "clear":
                    LogsDisplay("cls...", false);
                    break;
                case "help":
                case "/h":
                case "h":
                    LogsDisplay(helpString);
                    break;
                default:
                    // update?   
                    break;
            }
        }

        private void getList()
        {
            //throw new NotImplementedException();   
        }

        private void getRadarImage()
        {
            //throw new NotImplementedException();   
        }

        private void getTarget()
        {
            //throw new NotImplementedException();   
        }

        private void getUpdate()
        {
            //throw new NotImplementedException();   
        }

        private void getRemote(bool scan)
        {
            LogsDisplay(testTarget.ToString(), scan);
            LogsDisplay(hf_Remote.WorldMatrix.Forward.ToString());
            probe = hf_Remote.GetPosition() + hf_Remote.WorldMatrix.Forward;
            Vector3D probe1 = hf_Remote.GetFreeDestination(probe, radius1, 0.01f);
            detected = (probe != probe1);

            //LogsDisplay("pos: " + hf_Remote.GetPosition() + "\n\n mag:" + probe.Length() + "\n wm:" +   
            //hf_Remote.WorldMatrix.Forward + "\n p:" + probe + "\n 1:" + probe1 + "\n\n" + "Detected: " + detected +   
            //"\n\nTest:\n");  
            LogsDisplay("pos: " + hf_Remote.GetPosition() + "\np:" + probe + "\n 1:" + probe1 + "\n\n" + "Detected: " + detected +
"\n\nTest:\n");

            //getEnemyPosition();   
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void ScanArea()
        {
            LogsDisplay("Scanning for " + testTarget.ToString() + tick, false);
            getRemote(true);
            int x = 0; int y = 0;
            for (int i = 0; i <= 5; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    x = i * steps;
                    y = j * steps;
                    if (x == 0 && y == 0)
                    {
                        offset = hf_Remote.WorldMatrix.Forward;
                    }
                    else if (y == 0)
                    {
                        offset = hf_Remote.WorldMatrix.Right + x * hf_Remote.WorldMatrix.Forward + 1000;
                    }
                    else if (x == 0)
                    {
                        offset = hf_Remote.WorldMatrix.Down + y * hf_Remote.WorldMatrix.Forward + 1000;
                    }
                    else
                    {
                        offset = hf_Remote.WorldMatrix.Right + x * hf_Remote.WorldMatrix.Down * y * hf_Remote.WorldMatrix.Forward + 1000;
                    }

                    offset.Normalize();

                    testProbe = hf_Remote.GetPosition() + offset;
                    test1 = hf_Remote.GetFreeDestination(testProbe, radius1, sampleSize);
                    if (test1 != testProbe)
                    {
                        indicator = "++";
                        LogsDisplay("match(" + i + "|" + j + ")\n   " + testProbe + "\n   " + test1 + "\nX:" + offset + "\n");
                    }
                    else
                    {
                        indicator = "--";
                    }
                    //LogsDisplay(indicator + " " + testProbe + "\n     " + test1 + "\n   X:" + offset + "\n");  
                }
            }
            tick++;
        }


        private void getEnemyPosition()
        {
            LogsDisplay("Probing environment... step " + probeSteps);

            for (int i = 0; i < 10; i++)
            {

                sample = hf_Remote.GetFreeDestination(probe + hf_Remote.WorldMatrix.Forward, radius1, 0.0f);

                if (sample != probe)
                {
                    // this returns the position of a bounding sphere with an offset of its size   
                    targets.Add(sample);
                }


            }

            probeSteps++;
        }


        void LogsDisplay(string status)
        {
            LogsDisplay(status, true);
        }
        void LogsDisplay(string status, bool append)
        {
            hf_Status.WritePrivateText(status + "\n", append);
        }

        void NotesDisplay(string status)
        {
            hf_Notes.WritePrivateText(status + "\n", true);
        }

        void getStatus()
        {
            LogsDisplay("Running diagnostics...");

            if (Diagnostics())
            {
                LogsDisplay("All systems functional");
            }
            else
            {
                LogsDisplay("There has been an error with the system");
            }
        }

        public bool Diagnostics()
        {
            bool temp = true;
            string tempStatus;
            for (int i = 0; i < systems.Count; i++)
            {
                if (systems[i].IsFunctional)
                {
                    tempStatus = ("Status: " + systems[i].CustomName + " is okay");
                }
                else
                {
                    tempStatus = ("Status: " + systems[i].CustomName + " has an error");
                    temp = false;
                }
                LogsDisplay(tempStatus);
            }

            return temp;
        }



        float RotorAngleCalculation()
        {
            float angle = 90f;
            double targetDistance = GetTargetDistance();

            if (targetDistance > 0)
            {
                targetStatus = "engaged";
                double radian = (Math.Atan2(targetDistance, hellfireLength));
                angle = (float)(radian * (180 / Math.PI));
            }
            else
            {
                targetStatus = "searching";
            }

            return angle;
        }


        Vector3D GetHellfirePosition()
        {
            Vector3D position = hf_Remote.GetPosition();
            return position;
        }

        Vector3D GetTargetPosition()
        {
            Vector3D position = new Vector3D(0, 0, 0);


            return position; //just aim ahead if nothing found yet    
        }


        float GetTargetDistance()
        {
            float distance = 150;
            float altDistance = 150;
            if (target != null)
            {
                // alt:   
                altDistance = (float)(LastShipPos - LastTargetPos).Length();
                distance = (float)Math.Sqrt(LastShipPos.X * LastTargetPos.X + LastShipPos.Y * LastTargetPos.Y + LastShipPos.Z * LastTargetPos.Z);
            }

            LogsDisplay("Distance: " + distance + "\nAlt. Distance: " + altDistance);
            return distance;
        }




        #region endcopy
        public void Echo(string test)
        {
            LogsDisplay(test);
        }

        

    }
}
#endregion