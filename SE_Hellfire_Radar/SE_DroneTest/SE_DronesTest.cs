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

        IMyProgrammableBlock thisBlock;
        IMyTextPanel test_Status;

        IMyProgrammableBlock debug;

        List<IMyFunctionalBlock> test_FuncSystems = new List<IMyFunctionalBlock>();
        List<IMyShipController> test_CtrlSystems = new List<IMyShipController>();

        public Program()
        {
            test_Status = GridTerminalSystem.GetBlockWithName("test_status") as IMyTextPanel;

            if (test_Status != null)
            {
                LogDisplay("Starting test systems");
            }

            test_FuncSystems.InsertRange(test_FuncSystems.Count, new List<IMyFunctionalBlock> { test_Antenna, test_Camera, test_Laser, test_Sensor });
            test_CtrlSystems.InsertRange(test_CtrlSystems.Count, new List<IMyShipController> { test_Remote });

            test_Antenna = GridTerminalSystem.GetBlockWithName("test_antenna") as IMyRadioAntenna;
            test_Camera = GridTerminalSystem.GetBlockWithName("test_camera") as IMyCameraBlock;
            test_Laser = GridTerminalSystem.GetBlockWithName("test_laser") as IMyLaserAntenna;
            test_Sensor = GridTerminalSystem.GetBlockWithName("test_sensor") as IMySensorBlock;


            test_Remote = GridTerminalSystem.GetBlockWithName("test_remote") as IMyRemoteControl;



            thisBlock = GridTerminalSystem.GetBlockWithName("test_programmable") as IMyProgrammableBlock;
            debug = GridTerminalSystem.GetBlockWithName("dt_debug") as IMyProgrammableBlock;


        }

        void Main(string Argument)
        {
            switch (Argument)
            {
                case "camera":
                    cameraTest();
                    break;
                case "remote":
                    RemoteTest();
                    break;
                case "antenna":
                    antennaTest();
                    break;
                case "player":
                    playerTest();
                    break;
                case "sensor":
                    sensorTest();
                    break;
                default:
                    
                    break;
            }
        }

        private void sensorTest()
        {
            LogDisplay("+++ Sensor +++");
            LogDisplay(test_Sensor.cas);
        }

        private void playerTest()
        {
            LogDisplay("+++ Camera +++");
            LogDisplay(test_Camera.CustomInfo);
        }

        private void antennaTest()
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

