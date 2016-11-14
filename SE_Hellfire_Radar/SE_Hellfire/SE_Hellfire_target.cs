#region dontCopy1
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
using VRage.ModAPI;

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
        IMyTimerBlock hf_Timer;
        IMyFunctionalBlock hf_;

        //Lists  
        List<IMyTerminalBlock> systems = new List<IMyTerminalBlock>();

        //strings  
        string targetStatus = "none";

        //constants  
        float hellfireLength = 30;

        //Vectors  
        Vector3D LastShipPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D LastTargetPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D targetPosition = new Vector3D(0.0, 0.0, 0.0);
        Vector3D probe;

        IMyEntity target = null;

        string helpString = "Use the numkeys to choose\na target from the list\nPress 1 to continue";
        
        private float radius1;
        private float sample;
        private bool chooseTarget;

        Program()
        {

            hf_Remote = GridTerminalSystem.GetBlockWithName("hf_remote") as IMyRemoteControl;
            hf_Status = GridTerminalSystem.GetBlockWithName("hf_status") as IMyTextPanel;
            hf_Timer = GridTerminalSystem.GetBlockWithName("hf_timer") as IMyTimerBlock;
            thisBlock = GridTerminalSystem.GetBlockWithName("hf_programmable") as IMyProgrammableBlock;

            systems.InsertRange(systems.Count, new List<IMyTerminalBlock> { hf_Status, hf_Remote, thisBlock});

            LogsDisplay("Hellfire target system: starting", false);
            getStatus();
        }

        void Main(string Argument)
        {
            switch (Argument)
            {
                case "radar":
                    getRadarImage();
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
                    getRemote();
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

        private void getRemote()
        {
            LogsDisplay("+++" + hf_Remote.CustomInfo + "+++");
            LogsDisplay(hf_Remote.DetailedInfo);
            LogsDisplay("Position:" + getEnemyPosition().ToString());
        }


        private Vector3D getEnemyPosition()
        {
            LogsDisplay("Probing environment...");

            int x = 1;
            Vector3D result = hf_Remote.GetFreeDestination(probe + hf_Remote.WorldMatrix.Forward, radius1, sample);

            if (result.Equals(probe)) {
                LogsDisplay("Probe " + x + ": Found nothing");
            } else {
                
                LogsDisplay("Probe " + x + ": " + result + "\non " + probe);
                for (int i = 0; i < 10; i++)
                {
                    if (result.X < probe.X || result.Y < probe.Y || result.Z < probe.Z)
                    {
                        probe.X = probe.X - (probe.X - result.X);
                        probe.Y = probe.Y - (probe.Y - result.Y);
                        probe.Z = probe.Z - (probe.Z - result.Z);

                        result = hf_Remote.GetFreeDestination(probe, radius1, sample);
                        LogsDisplay("Probe " + x + ": " + result + "\non " + probe);
                    }
                    // need to find a good break condition so that size of the target bounding sphere is proben and not more:
                    // First probe finds right/left boundary
                    // Second probe might find the other one, if it has a too similar one it needs to look further and so forth.
                    // Probably (well most likely) the current implementation does not work this way anyways....

                }
            }

            return result;
        }


        void LogsDisplay(string status)
        {
            LogsDisplay(status, true);
        }
        void LogsDisplay(string status, bool append)
        {
            hf_Status.WritePrivateText(status + "\n", append);
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

        // STOP COMPY
        #region dontCopy2
        void Echo(string message)
        {
            LogsDisplay(message);
        }

        #endregion

    }
}
