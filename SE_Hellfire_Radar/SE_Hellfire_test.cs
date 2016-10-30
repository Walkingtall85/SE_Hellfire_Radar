using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using Sandbox.Common.ObjectBuilders;
using VRageMath;
using VRage.ModAPI;

namespace SE_Hellfire
{
    public class HellFire_test
    {
        IMyGridTerminalSystem GridTerminalSystem;
        /// <summary>
        /// START COPYING HERE
        /// </summary>

        IMyProgrammableBlock thisBlock;
        IMyCameraBlock hf_Scanner;
        IMySensorBlock hf_Sensor;
        IMyRadioAntenna hf_Antenna;
        IMyLaserAntenna hf_Laser;
        IMyRemoteControl hf_Remote;
        IMyTextPanel hf_Status;
        //IMyTimerBlock hf_timer;  
        IMyFunctionalBlock hf_;
        IMyLargeTurretBase hf_TurretA;
        IMyLargeTurretBase hf_TurretB;

        //Lists  
        List<IMyFunctionalBlock> systems = new List<IMyFunctionalBlock>();

        //strings  
        string targetStatus = "none";
        string hellfireStatus = "none";
        string helpString = "Commands:\n antenna, camera, laser,\n sensor, azimut, status,\n turret, clear, help";
        string azimuthA = "0";
        string azimuthB = "0";


        //constants  
        float hellfireLength = 30;

        //Vectors  
        Vector3D LastShipPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D LastTargetPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D targetPosition = new Vector3D(0.0, 0.0, 0.0);

        IMyEntity target = null;


        public void Program()
        {
            hf_Scanner = GridTerminalSystem.GetBlockWithName("HF_camera") as IMyCameraBlock;
            hf_Sensor = GridTerminalSystem.GetBlockWithName("HF_sensor") as IMySensorBlock;
            hf_Remote = GridTerminalSystem.GetBlockWithName("HF_remote") as IMyRemoteControl;
            hf_Antenna = GridTerminalSystem.GetBlockWithName("HF_antenna") as IMyRadioAntenna;
            hf_Laser = GridTerminalSystem.GetBlockWithName("HF_laser") as IMyLaserAntenna;
            hf_Status = GridTerminalSystem.GetBlockWithName("HF_status") as IMyTextPanel;
            thisBlock = GridTerminalSystem.GetBlockWithName("HF_Programmable") as IMyProgrammableBlock;

            hf_TurretA = GridTerminalSystem.GetBlockWithName("HF_TurretA") as IMyLargeTurretBase;
            hf_TurretB = GridTerminalSystem.GetBlockWithName("HF_TurretB") as IMyLargeTurretBase;

            systems.InsertRange(systems.Count, new List<IMyFunctionalBlock> { hf_Antenna, hf_Laser,
hf_Scanner, hf_Sensor, hf_Status, hf_TurretA, hf_TurretB});

            statusDisplay("Hellfire Sustem: starting", false);
            getStatus();
        }

        void Main(string Argument)
        {
            switch (Argument)
            {
                case "antenna":
                    getAntenna();
                    break;
                case "camera":
                    getCamera();
                    break;
                case "laser":
                    getLaser();
                    break;
                case "sensor":
                    getSensor();
                    break;
                case "azimut":
                    getAzimut();
                    break;
                case "status":
                    getStatus();
                    break;
                case "turret":
                    getTurret();
                    break;
                case "cls":
                case "clear":
                    statusDisplay("cls...", false);
                    break;
                case "help":
                case "/h":
                case "h":
                default:
                    statusDisplay(helpString);
                    break;
            }
        }

        private void getTurret()
        {
            lock (hf_TurretA)
            {
                if (!azimuthA.Equals(hf_TurretA.Azimuth.ToString()))
                {
                    statusDisplay("+++" + " TURRET A " + "+++");
                    azimuthA = hf_TurretA.Azimuth.ToString();
                    statusDisplay(azimuthA);
                    statusDisplay(hf_TurretA.DetailedInfo);
                }
            }

            lock (hf_TurretB)
            {
                if (!azimuthB.Equals(hf_TurretB.Azimuth.ToString()))
                {
                    statusDisplay("+++" + " TURRET B " + "+++");
                    azimuthB = hf_TurretB.Azimuth.ToString();
                    statusDisplay(azimuthB);
                    statusDisplay(hf_TurretB.DetailedInfo);
                }
            }

        }

        private void getAzimut()
        {
            statusDisplay("+++" + " NOT IMPLEMENTED " + "+++");
        }

        private void getRemote()
        {
            statusDisplay("+++" + hf_Remote.CustomInfo + "+++");
            statusDisplay(hf_Remote.DetailedInfo);
        }

        private void getSensor()
        {
            statusDisplay("+++" + hf_Sensor.CustomInfo + "+++");
            statusDisplay(hf_Sensor.DetailedInfo);
            statusDisplay(hf_Sensor.LastDetectedEntity.GetPosition().ToString());
        }

        private void getLaser()
        {
            statusDisplay("+++" + hf_Laser.CustomInfo);
            statusDisplay(hf_Laser.DetailedInfo);
            //statusDisplay(hf_Laser.);  
        }

        private void getCamera()
        {
            statusDisplay("+++" + hf_Scanner.CustomInfo + "+++");
            statusDisplay(hf_Scanner.DetailedInfo);
            //statusDisplay(hf_Scanner.);  
        }

        private void getAntenna()
        {
            statusDisplay("+++" + hf_Antenna.CustomInfo + "+++");
            statusDisplay(hf_Antenna.DetailedInfo);
            //statusDisplay(hf_Antenna.);  
        }

        void statusDisplay(string status)
        {
            statusDisplay(status, true);
        }
        void statusDisplay(string status, bool append)
        {
            hf_Status.WritePrivateText(status + "\n", append);
        }

        void getStatus()
        {
            statusDisplay("Running diagnostics...");

            if (Diagnostics())
            {
                hellfireStatus = "standby";
            }
            else
            {
                hellfireStatus = "malfunction";
            }

            statusDisplay("Hellfire systems: " + hellfireStatus);
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
                statusDisplay(tempStatus);
            }

            if (hf_Remote.IsFunctional)
            {
                tempStatus = ("Status: " + hf_Remote.CustomName + " is okay");
            }
            else
            {
                tempStatus = ("Status: " + hf_Remote.CustomName + " has an error");
                temp = false;
            }
            statusDisplay(tempStatus);

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
            Vector3D hellfirePosition = GridTerminalSystem.GetBlockWithName("Hellfire").GetPosition();
            return hellfirePosition;
        }

        Vector3D GetTargetPosition()
        {
            Vector3D position = new Vector3D(0, 0, 0);
            if (hf_Sensor.IsActive)
            {
                if (hf_Sensor.LastDetectedEntity != null && hf_Sensor.LastDetectedEntity != target)
                {
                    target = hf_Sensor.LastDetectedEntity;
                    LastTargetPos = target.GetPosition(); //reset last pos to fix vel later 
                }
            }
            if (target != null)
            {
                position = target.GetPosition();
                LastTargetPos = position;
            }
            return position; //just aim ahead if nothing found yet 
        }


        float GetTargetDistance()
        {
            float distance = 150;
            if (target != null)

                distance = (float)Math.Sqrt(LastShipPos.X * LastTargetPos.X + LastShipPos.Y * LastTargetPos.Y + LastShipPos.Z * LastTargetPos.Z);
            return distance;
        }

        // STOP COMPY

    }
}
