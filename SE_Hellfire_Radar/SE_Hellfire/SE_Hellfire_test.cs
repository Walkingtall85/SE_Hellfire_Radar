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

namespace SE_Hellfire_test
{
    public class Program
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
        private Vector3D probe;
        private float radius1;
        private float sample;

        Program()
        {
            hf_Scanner = GridTerminalSystem.GetBlockWithName("hf_camera") as IMyCameraBlock;
            hf_Sensor = GridTerminalSystem.GetBlockWithName("hf_sensor") as IMySensorBlock;
            hf_Remote = GridTerminalSystem.GetBlockWithName("hf_remote") as IMyRemoteControl;
            hf_Antenna = GridTerminalSystem.GetBlockWithName("hf_antenna") as IMyRadioAntenna;
            hf_Laser = GridTerminalSystem.GetBlockWithName("hf_laser") as IMyLaserAntenna;
            hf_Status = GridTerminalSystem.GetBlockWithName("hf_status") as IMyTextPanel;
            thisBlock = GridTerminalSystem.GetBlockWithName("hf_programmable") as IMyProgrammableBlock;

            hf_TurretA = GridTerminalSystem.GetBlockWithName("hf_TurretA") as IMyLargeTurretBase;
            hf_TurretB = GridTerminalSystem.GetBlockWithName("hf_TurretB") as IMyLargeTurretBase;

            systems.InsertRange(systems.Count, new List<IMyFunctionalBlock> { hf_Antenna, hf_Laser,
hf_Scanner, hf_Sensor, hf_Status, hf_TurretA, hf_TurretB});

            LogsDisplay("Hellfire System: starting", false);
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
                default:
                    LogsDisplay(helpString);
                    break;
            }
        }

        private void getTurret()
        {
            lock (hf_TurretA)
            {
                if (!azimuthA.Equals(hf_TurretA.Azimuth.ToString()))
                {
                    LogsDisplay("+++" + " TURRET A " + "+++");
                    azimuthA = hf_TurretA.Azimuth.ToString();
                    LogsDisplay(azimuthA);
                    LogsDisplay(hf_TurretA.DetailedInfo);
                }
            }

            lock (hf_TurretB)
            {
                if (!azimuthB.Equals(hf_TurretB.Azimuth.ToString()))
                {
                    LogsDisplay("+++" + " TURRET B " + "+++");
                    azimuthB = hf_TurretB.Azimuth.ToString();
                    LogsDisplay(azimuthB);
                    LogsDisplay(hf_TurretB.DetailedInfo);
                }
            }

        }

        private void getAzimut()
        {
            LogsDisplay("+++" + " NOT IMPLEMENTED " + "+++");
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

        private void getSensor()
        {
            LogsDisplay("+++" + hf_Sensor.CustomInfo + "+++");
            LogsDisplay(hf_Sensor.DetailedInfo);
            LogsDisplay(hf_Sensor.LastDetectedEntity.GetPosition().ToString());
        }

        private void getLaser()
        {
            LogsDisplay("+++" + hf_Laser.CustomInfo);
            LogsDisplay(hf_Laser.DetailedInfo);
            //statusDisplay(hf_Laser.);  
        }

        private void getCamera()
        {
            LogsDisplay("+++" + hf_Scanner.CustomInfo + "+++");
            LogsDisplay(hf_Scanner.DetailedInfo);
            //statusDisplay(hf_Scanner.);  
        }

        private void getAntenna()
        {
            LogsDisplay("+++" + hf_Antenna.CustomInfo + "+++");
            LogsDisplay(hf_Antenna.DetailedInfo);
            //statusDisplay(hf_Antenna.);  
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
                hellfireStatus = "standby";
            }
            else
            {
                hellfireStatus = "malfunction";
            }

            LogsDisplay("Hellfire systems: " + hellfireStatus);
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

            if (hf_Remote.IsFunctional)
            {
                tempStatus = ("Status: " + hf_Remote.CustomName + " is okay");
            }
            else
            {
                tempStatus = ("Status: " + hf_Remote.CustomName + " has an error");
                temp = false;
            }
            LogsDisplay(tempStatus);

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

    }
}
