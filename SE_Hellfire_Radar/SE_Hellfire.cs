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
using Sandbox.ModAPI.Weapons;
using Sandbox.Common.ObjectBuilders;
using VRageMath;
using VRage.ModAPI;

namespace SE_Hellfire
{
    public class HellFire
    {
        IMyGridTerminalSystem GridTerminalSystem;
        IMyCameraBlock hf_Scanner;
        IMySensorBlock hf_Sensor;
        IMyRadioAntenna antenna;
        IMyRemoteControl hf_Remote;
        IMyPistonBase hf_PistonSupport;
        IMyTextPanel hf_Status;
        IMySmallMissileLauncher hf_MisslileLauncher;

        // Rotors
        IMyMotorAdvancedStator hf_BackupRotorSupport;
        IMyMotorAdvancedStator hf_RotorSupport;
        IMyMotorAdvancedStator hf_BackupRotorHinge;
        IMyMotorAdvancedStator hf_rotorHinge;

        //strings
        string rotorStatusHinge = "functional";
        string rotorStatusSupport = "functional";
        string targetStatus = "none";
        string hellfireStatus = "none";

        //floats
        float angleHingeRotor;
        float angleSupportRotor;
        float angleSupportPiston;

        //constants
        float hellfireLength = 30;

        //Vectors
        Vector3D LastShipPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D LastTargetPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D targetPosition = new Vector3D(0.0, 0.0, 0.0);

        IMyEntity target = null;


        void Program()
        {
            hf_Scanner = GridTerminalSystem.GetBlockWithName("HF_scanner") as IMyCameraBlock;
            hf_Sensor = GridTerminalSystem.GetBlockWithName("HF_sensor") as IMySensorBlock;
            hf_Remote = GridTerminalSystem.GetBlockWithName("HF_remote") as IMyRemoteControl;

            hf_BackupRotorSupport = GridTerminalSystem.GetBlockWithName("HF_supportRotor_b") as IMyMotorAdvancedStator;
            hf_RotorSupport = GridTerminalSystem.GetBlockWithName("HF_supportRotor_a") as IMyMotorAdvancedStator;
            hf_PistonSupport = GridTerminalSystem.GetBlockWithName("HF_supportPiston_a") as IMyPistonBase;

            hf_BackupRotorHinge = GridTerminalSystem.GetBlockWithName("HF_support_b") as IMyMotorAdvancedStator;
            hf_rotorHinge = GridTerminalSystem.GetBlockWithName("HF_support_a") as IMyMotorAdvancedStator;

            hf_Status = GridTerminalSystem.GetBlockWithName("HF_status") as IMyTextPanel;

            if (Diagnostics())
            {
                hellfireStatus = "standby";
            } else
            {
                hellfireStatus = "malfunction";

            }

            hf_MisslileLauncher.ApplyAction("shootOnce");

            statusDisplay(hellfireStatus);
        }

        

        void Main(string Argument)
        {
            switch (Argument)
            {
                case "close":

                    break;
                case "open":
                    //GridTerminalSystem. hf_rotorHinge.


                    break;
                default:
                   
                    break;
            }




            var launchers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(launchers);

            foreach (IMySmallMissileLauncher item in launchers)
            {

            }
        }


        void statusDisplay(string status)
        {
            hf_Status.WritePublicText(status + "/n", true);
        }

        bool Diagnostics()
        {

            return true;
        }



        void checkRotorSupport()
        {
            if (!hf_RotorSupport.IsWorking || !hf_RotorSupport.IsAttached)
            {
                if (!hf_BackupRotorSupport.IsWorking || !hf_BackupRotorSupport.IsAttached)
                {
                    rotorStatusSupport = "Malfunctioning";
                }
                else
                {
                    rotorStatusSupport = "Backup";
                }
            }
            else
            {
                if (!rotorStatusSupport.Equals("Functional"))
                {
                    if(!hf_RotorSupport.IsAttached)
                    {
                        if(hf_BackupRotorSupport.IsFunctional && hf_BackupRotorSupport.IsAttached)
                        {
                            
                        }
                    }
                    rotorStatusSupport = "Functional";
                }
            }
        }

        void checkRotorHinge()
        {

            if (!hf_rotorHinge.IsWorking || !hf_rotorHinge.IsAttached)
            {
                if (!hf_BackupRotorSupport.IsWorking || !hf_BackupRotorSupport.IsAttached)
                {
                    rotorStatusHinge = "Malfunctioning";
                }
                else
                {
                    rotorStatusHinge = "Backup";
                }
            }
            else
            {
                rotorStatusHinge = "Functional";
            }

        }
 

        void SystemstatusUpdate()
        {
            IMyTextPanel hellfireSystemStatus = GridTerminalSystem.GetBlockWithName("HF_system_Display") as IMyTextPanel;

        }


        float RotorAngleCalculation()
        {
            float angle = 90f;
            double targetDistance = GetTargetDistance();

            if (targetDistance > 0)
            {
                targetStatus = "engaged";
                double radian = (Math.Atan2(targetDistance, hellfireLength));
                angle = (float) (radian * (180 / Math.PI));
            } else
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
            Vector3D position = new Vector3D(0,0,0);
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
            if(target != null)

            distance = (float)Math.Sqrt(LastShipPos.X * LastTargetPos.X + LastShipPos.Y * LastTargetPos.Y + LastShipPos.Z * LastTargetPos.Z);
        return distance;
        }

    }
}
