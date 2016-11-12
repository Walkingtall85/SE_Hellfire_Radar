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

        int minOffset = 5;
        int maxOffset = 10;

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
                // this does not get the current position of the drone, AT ALL! (seams to return 0,0,0)
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
            //holy crap remove this start shit
            Start();
            switch (Argument)
            {
                case "idle":
                    currentState = Argument;
                    idling = true;
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
                default:
                    Idle();
                    break;
            }
        }

        private void Idle()
        {
            if (!moveTo && idling)
            {
                moveTarget = getIdlePoint();
                test_Remote.ClearWaypoints();
                test_Remote.AddWaypoint(moveTarget, "idling");
                test_Remote.SetAutoPilotEnabled(true);
                LogDisplay("Target Pos:" + moveTarget.ToString());
                idling = false;
            } else
            {
                currentPosition = getPosition();
                if (currentPosition.Equals(moveTarget))
                {
                    idling = true;
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
            return test_Remote.Position;
        }

        private void Stop()
        {
            test_Remote.SetAutoPilotEnabled(false);
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
