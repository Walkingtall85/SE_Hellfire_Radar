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
//using VRage.ModAPI;

namespace SE_DroneTest
{
    public class Program
    {

        IMyGridTerminalSystem GridTerminalSystem;

        internal interface IMyTimerBlock : IMyFunctionalBlock
        {
        }

        /// <summary>
        /// Start Coping here
        /// </summary>
        #region copy
       
        IMyRadioAntenna test_Antenna;
        IMyCameraBlock test_Camera;
        IMyLaserAntenna test_Laser;
        IMyRemoteControl test_Remote;
        IMySensorBlock test_Sensor;

        IMyTimerBlock sensor_timer;
        IMyTimerBlock test_timer;

        IMyProgrammableBlock thisBlock;
        IMyTextPanel test_Status;

        IMyProgrammableBlock debug;

        List<IMyFunctionalBlock> test_FuncSystems = new List<IMyFunctionalBlock>();
        List<IMyShipController> test_CtrlSystems = new List<IMyShipController>();

        bool isTimer = false;
        bool isSearching = true;

        string currentState = "none";

        public Program()
        {
            test_Status = GridTerminalSystem.GetBlockWithName("test_status") as IMyTextPanel;

            if (test_Status != null)
            {
                LogDisplay("Starting test systems");
            }

            test_FuncSystems.InsertRange(test_FuncSystems.Count, new List<IMyFunctionalBlock> { test_Antenna, test_Camera, test_Laser, test_Sensor, sensor_timer, test_timer });
            test_CtrlSystems.InsertRange(test_CtrlSystems.Count, new List<IMyShipController> { test_Remote });

            test_Antenna = GridTerminalSystem.GetBlockWithName("test_antenna") as IMyRadioAntenna;
            test_Camera = GridTerminalSystem.GetBlockWithName("test_camera") as IMyCameraBlock;
            test_Laser = GridTerminalSystem.GetBlockWithName("test_laser") as IMyLaserAntenna;
            test_Sensor = GridTerminalSystem.GetBlockWithName("test_sensor") as IMySensorBlock;

            test_Remote = GridTerminalSystem.GetBlockWithName("test_remote") as IMyRemoteControl;

            sensor_timer = GridTerminalSystem.GetBlockWithName("sensor_timer") as IMyTimerBlock;
            test_timer = GridTerminalSystem.GetBlockWithName("test_timer") as IMyTimerBlock;

            thisBlock = GridTerminalSystem.GetBlockWithName("test_programmable") as IMyProgrammableBlock;
            debug = GridTerminalSystem.GetBlockWithName("dt_debug") as IMyProgrammableBlock;


        }

        void Main(string Argument)
        {
            if (!Argument.Equals(currentState) && isTimer)
            {
                SensorTest(false);
            }

            switch (Argument)
            {
                case "camera":
                    currentState = Argument;
                    cameraTest();
                    break;
                case "remote":
                    currentState = Argument;
                    RemoteTest();
                    break;
                case "antenna":
                    currentState = Argument;
                    AntennaTest();
                    break;
                case "player":
                    currentState = Argument;
                    PlayerTest();
                    break;
                case "sensor":
                    SensorTest();
                    break;
                case "sensor_on":
                    currentState = "sensor";
                    SensorTest(true);
                    break;
                case "sensor_off":
                    currentState = "none";
                    SensorTest(false);
                    break;
                case "diagnostics":
                    Diagnostics();
                    currentState = "none";
                    break;
                case "shutdown":
                    currentState = "none";
                    break;
                default:
                    LogDisplay("ERROR: Unknown Command");
                    break;
            }
        }

        private void SensorTest(bool sensorOn)
        {
            if (sensorOn)
            {
                LogDisplay("+++ Activating Sensor +++");
                LogDisplay("Scanning in Progress");
                isTimer = true;
                isSearching = true;

            } else
            {
                LogDisplay("+++ Deactivating Sensor +++");
                isTimer = false;
            }

            SensorTest();
            
        }

        private void SensorTest()
        {
            var target = test_Sensor.LastDetectedEntity;

            if (target == null)
            {
                //LogDisplay(".");
            }
            
        }

        private void PlayerTest()
        {
            LogDisplay("+++ Camera +++");
            LogDisplay(test_Camera.CustomInfo);
        }

        private void AntennaTest()
        {
            LogDisplay("+++ Camera +++");
            LogDisplay(test_Camera.CustomInfo);
        }

        private void RemoteTest()
        {
            LogDisplay("+++ Camera +++");
            LogDisplay(test_Camera.CustomInfo);
        }

        private void cameraTest()
        {
            LogDisplay("+++ Camera +++");
            LogDisplay(test_Camera.CustomInfo);
            //test_Camera.
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

        private void DebugWrite(string message)
        {
            if (!debug.TryRun(message))
            {
                LogDisplay("DebugError: " + message);
            }
        }

        #endregion

    }

}

