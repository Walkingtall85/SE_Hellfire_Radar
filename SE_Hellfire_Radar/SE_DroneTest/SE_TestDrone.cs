#region notcopy1
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;
using VRage.ModAPI;
using VRage.Game.ModAPI.Ingame;

namespace SE_TestDrone
{
    class Program
    {
        IMyGridTerminalSystem GridTerminalSystem;

        internal interface IMyTimerBlock : IMyFunctionalBlock
        {
        };

        /// <summary>
        /// Start Coping here
        /// </summary>
        #endregion
        #region copy


        IMyLaserAntenna test_Laser;
        IMyRemoteControl test_Remote;
        IMySensorBlock test_Sensor;
        IMyBeacon test_Beacon;

        IMyTimerBlock test_Timer;

        IMyCameraBlock test_Camera;
        IMyTextPanel test_Status;
        IMyTextPanel test_Data;

        List<IMyFunctionalBlock> test_FuncSystems = new List<IMyFunctionalBlock>();
        List<IMyShipController> test_CtrlSystems = new List<IMyShipController>();

        string currentState = "none";
        string data = "booting";

        bool moveTo = false;
        bool idling = false;
        bool firstRun = true;

        int minOffset = 25;
        int maxOffset = 50;

        double distanceTarget = 0;
        double stoppingdistance = 5;
        double safetyDistance = 800;

        Random r = new Random();

        Vector3D target;
        Vector3D moveTarget;
        Vector3D currentPosition;

        // For debug reasons in older version! Actually is:  
        // void Program();  
        public Program()
        {
            Echo("Starting...");

            test_Status = GridTerminalSystem.GetBlockWithName("test_status") as IMyTextPanel;
            test_Data = GridTerminalSystem.GetBlockWithName("test_data") as IMyTextPanel;

            if (test_Status != null && test_Data != null)
            {
                LogDisplay("Starting test systems", false);
                test_Data.WritePrivateText(data, false);
            }
            else
            {
                Echo("Display not found!");
            }

            Echo("Adding components...");
            test_Camera = GridTerminalSystem.GetBlockWithName("test_camera") as IMyCameraBlock;
            test_Laser = GridTerminalSystem.GetBlockWithName("test_laser") as IMyLaserAntenna;
            test_Sensor = GridTerminalSystem.GetBlockWithName("test_sensor") as IMySensorBlock;
            test_Beacon = GridTerminalSystem.GetBlockWithName("test_beacon") as IMyBeacon;

            test_Remote = GridTerminalSystem.GetBlockWithName("test_remote") as IMyRemoteControl;

            test_Timer = GridTerminalSystem.GetBlockWithName("test_timer") as IMyTimerBlock;


            Echo("Creating Lists...");
            test_FuncSystems.InsertRange(test_FuncSystems.Count, new List<IMyFunctionalBlock> { test_Camera, test_Laser, test_Sensor, test_Timer });
            test_CtrlSystems.InsertRange(test_CtrlSystems.Count, new List<IMyShipController> { test_Remote });

            Echo("Running Diagnostics...");
            if (Diagnostics())
            {
                target = getPosition();
                LogDisplay("CurrentPosition: " + target.ToString());
                minOffset = minOffset * 100;
                maxOffset = maxOffset * 100;
                Echo("Initialized - Waiting for Commands");
            }
            else
            {
                Echo("ERROR: Some of the diagnostics failed!");
            }
        }


        void Main(string Argument)
        {

            switch (Argument)
            {
                case "idle":
                    currentState = Argument;
                    idling = true;
                    Execute();
                    break;
                case "moveTo":
                    currentState = Argument;
                    idling = false;
                    break;
                case "stop":
                    currentState = Argument;
                    idling = false;
                    moveTo = false;
                    Stop();
                    break;
                case "info":
                    LogDisplay("+++ Retrieving Info +++");
                    LogDisplay(currentState + ": " + getPosition() + "\nTarget:" + moveTarget);
                    break;
                case "return":
                    LogDisplay("Returning Home");
                    currentState = Argument;
                    ReturnHome();
                    break;
                default:
                    Execute();
                    UpdateData();
                    break;
            }
        }

        private void UpdateData()
        {
            data = String.Format("Status: {0}[{1}]\nLoc: {2}\nTar: {3}\nDes: {4}", currentState, distanceTarget, currentPosition, moveTarget, target);
            test_Data.WritePrivateText(data, false);
        }

        /// <summary> 
        /// This is not properly implemented yet, as of now it only tries to find and get near the laser antenna 
        /// </summary> 
        /// This method will provide the means to find a docking station and automatically dock 
        private void ReturnHome()
        {
            if (test_Laser.IsPermanent)
            {
                Vector3D homeBase;
                if (Vector3D.TryParse(test_Laser.DisplayNameText, out homeBase))
                {
                    LogDisplay("Retrieving GPS and setting target (home)");
                    moveTarget = homeBase;
                    // add method manageing moveto 
                    moveTo = true;
                    idling = false;
                    Execute();
                }
                else
                {
                    LogDisplay("Something failed while retrieving GPS");
                }
            }
            else
            {
                LookingForReception();
                // then try again 
            }
        }

        // Turn to look for a laser antenna (should probably memorize the last position the gps got cut off and move there after trying to turning) 
        // Maybe even ask others for reception? 
        private void LookingForReception()
        {
            Echo("not Implemented");
        }

        private void Stop()
        {
            test_Timer.ApplyAction("OnOff_Off");
            test_Timer.ApplyAction("Stop");
            test_Remote.SetAutoPilotEnabled(false);
        }


        private void Execute()
        {
            if (!moveTo && idling)
            {
                moveTarget = getIdlePoint();
                MoveTo(moveTarget);
                LogDisplay(currentState + ": " + moveTarget);
                test_Timer.ApplyAction("OnOff_On");
                test_Timer.ApplyAction("Start");
                idling = false;
                moveTo = true;
            }
            else if (moveTo)
            {
                currentPosition = getPosition();
                distanceTarget = euclidianDistance(currentPosition, moveTarget);

                if (distanceTarget <= stoppingdistance)
                {
                    LogDisplay("Reached desitination");
                    LogDisplay(currentState + ": " + moveTarget);
                    idling = true;
                    moveTo = false;
                } else if (distanceTarget >= safetyDistance)
                {
                    LogDisplay("Reached desitination boundaries");
                    LogDisplay(currentState + ": " + moveTarget);
                    idling = true;
                    moveTo = false;
                }
            }
            else
            {
                LogDisplay(".", true);
            }
        }

        private void MoveTo(object movetarget)
        {
            test_Remote.ClearWaypoints();
            test_Remote.AddWaypoint(moveTarget, currentState);
            test_Remote.SetAutoPilotEnabled(true);
            LogDisplay("Target Pos[" + currentState + "]:" + moveTarget.ToString());
        }

        private double euclidianDistance(Vector3D vector1, Vector3D vector2)
        {
            double distance;
            Vector3D difference = Vector3D.Subtract(vector1, vector2);
            distance = Math.Sqrt(Math.Pow(difference.X, 2) + Math.Pow(difference.X, 2) + Math.Pow(difference.X, 2));
            return distance;
            //Math.Sqrt(Math.Pow((vector1.X - vector2.X), 2) + Math.Pow((vector1.Y - vector2.Y), 2) + Math.Pow((vector1.Z - vector2.Z), 2)); 

        }

        private Vector3D getIdlePoint()
        {
            Vector3D result = target;
            result.X += (r.Next(minOffset, maxOffset) / 100);
            result.Z += (r.Next(minOffset, maxOffset) / 100);
            result.Y += (r.Next(minOffset, maxOffset) / 100);
            return result;
        }

        private Vector3D getPosition()
        {
            return test_Remote.GetPosition();
        }

        bool Diagnostics()
        {
            bool status = true;

            LogDisplay("Running full diagnostics");

            if (!SystemDiagnostics() & !CtrlDiagnostics())
            {
                status = false;
            }

            return status;
        }

        bool SystemDiagnostics()
        {
            bool status = true;
            for (int i = 0; i < test_FuncSystems.Count; i++)
            {
                if (test_FuncSystems[i] != null)
                {
                    if (test_FuncSystems[i].IsWorking)
                    {
                        LogDisplay(test_FuncSystems[i].DisplayNameText + " - online");
                    }
                    else
                    {
                        LogDisplay(test_FuncSystems[i].DisplayNameText + " - offline");
                        status = false;
                    }
                }
                else
                {
                    LogDisplay(test_FuncSystems[i].DisplayNameText + " - cannot be found");
                    status = false;
                }
            }

            return status;
        }

        bool CtrlDiagnostics()
        {
            bool status = true;
            for (int i = 0; i < test_CtrlSystems.Count; i++)
            {
                if (test_CtrlSystems[i] != null)
                {
                    if (test_CtrlSystems[i].IsWorking)
                    {
                        LogDisplay(test_CtrlSystems[i].DisplayNameText + " - online");
                    }
                    else
                    {
                        LogDisplay(test_CtrlSystems[i].DisplayNameText + " - offline");
                        status = false;
                    }
                }
                else
                {
                    LogDisplay(test_CtrlSystems[i].DisplayNameText + " - cannot be found");
                    status = false;
                }
            }

            return status;
        }

        void LogDisplay(string status)
        {
            LogDisplay(status, true);
        }

        void LogDisplay(string status, bool append)
        {
            test_Status.WritePrivateText(status + "\n", append);
        }

        // STOP COPY HERE
        #endregion
        #region notcopy2

        // helperclass - do not copy
        void Echo(string echo)
        {
            LogDisplay("HALLO: " + echo);
        }
    }

    class Agent
    {
        IMyGridTerminalSystem GridTerminalSystem;
    }
}
#endregion