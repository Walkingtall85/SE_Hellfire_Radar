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

        List<IMyFunctionalBlock> test_FuncSystems = new List<IMyFunctionalBlock>();
        List<IMyShipController> test_CtrlSystems = new List<IMyShipController>();

        string currentState = "none";

        bool moveTo;
        bool idling;
        bool firstRun = true;

        int minOffset = 50;
        int maxOffset = 100;

        Random r = new Random();

        Vector3D target;
        Vector3D moveTarget;
        Vector3D currentPosition;

        // For debug reasons in older version! Actually is: 
        // void Program(); 
        public void Start()
        {
            Echo("Starting...");

            test_Status = GridTerminalSystem.GetBlockWithName("test_status") as IMyTextPanel;

            if (test_Status != null)
            {
                LogDisplay("Starting test systems", false);
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

            test_Status = GridTerminalSystem.GetBlockWithName("test_status") as IMyTextPanel;

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
            //holy crap remove this first run shit back in frankfurt 
            if (firstRun)
            {
                firstRun = false;
                Start();
            }
            else
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
                        break;
                }
            }
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
                if (Vector3D.TryParse(test_Laser.DisplayNameText, out homeBase)) {
                    LogDisplay("Retrieving GPS and setting target (home)");
                    moveTarget = homeBase;
                    // add method manageing moveto
                    moveTo = true;
                    idling = false;
                    Execute();
                } else
                {
                    LogDisplay("Something failed while retrieving GPS");
                }
            } else
            {
                LookingForReception();
                // then try again
            }
        }

        // Turn to look for a laser antenna (should probably memorize the last position the gps got cut off and move there after trying to turning)
        // Maybe even ask others for reception?
        private void LookingForReception()
        {
            throw new NotImplementedException();
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
                LogDisplay(currentState + ": " + moveTarget);
                test_Remote.ClearWaypoints();
                test_Remote.AddWaypoint(moveTarget, "idling");
                test_Remote.SetAutoPilotEnabled(true);
                LogDisplay("Target Pos:" + moveTarget.ToString());
                test_Timer.ApplyAction("OnOff_On");
                test_Timer.ApplyAction("Start");
                idling = false;
                moveTo = true;
            }
            else if (moveTo)
            {
                currentPosition = getPosition();
                if (currentPosition.Equals(moveTarget))
                {
                    LogDisplay(currentState + ": " + moveTarget);
                    idling = true;
                    moveTo = false;
                }
            }
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