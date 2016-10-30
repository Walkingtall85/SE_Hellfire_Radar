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

        List<IMyTerminalBlock> hf_Launchers = new List<IMyTerminalBlock>();

        // Rotors
        IMyMotorAdvancedStator hf_BackupRotorSupport;
        IMyMotorAdvancedStator hf_rotorSupport;
        IMyMotorAdvancedStator hf_ActiveRotorSupport;
        IMyMotorAdvancedStator hf_rotorHinge;
        IMyMotorAdvancedStator hf_BackupRotorHinge;
        IMyMotorAdvancedStator hf_ActiveRotorHinge;

        //strings
        string rotorStatusHinge = "functional";
        string rotorStatusSupport = "functional";
        string targetStatus = "none";
        string hellfireStatus = "none";

        string hf_help = "Arguments: close, open, angle\n status, lock, lockon, fire, help";

        //floats
        float angleHingeRotor;
        float angleSupportRotor;
        float angleSupportPiston;
        
        //constants
        float hellfireLength = 30;
        float deactivateAngle = 50;

        //Lists  
        List<IMyFunctionalBlock> hf_Systems = new List<IMyFunctionalBlock>();

        //Vectors
        Vector3D LastShipPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D LastTargetPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D targetPosition = new Vector3D(0.0, 0.0, 0.0);

        IMyEntity target = null;


        void Program()
        {
            statusDisplay("Hellfire booting sequence", false);
            statusDisplay("Checking components");
            hf_Scanner = GridTerminalSystem.GetBlockWithName("HF_scanner") as IMyCameraBlock;
            hf_Sensor = GridTerminalSystem.GetBlockWithName("HF_sensor") as IMySensorBlock;
            hf_Remote = GridTerminalSystem.GetBlockWithName("HF_remote") as IMyRemoteControl;

            hf_BackupRotorSupport = GridTerminalSystem.GetBlockWithName("HF_supportRotor_b") as IMyMotorAdvancedStator;
            hf_ActiveRotorSupport = GridTerminalSystem.GetBlockWithName("HF_supportRotor_a") as IMyMotorAdvancedStator;
            hf_rotorSupport = GridTerminalSystem.GetBlockWithName("HF_supportRotor_a") as IMyMotorAdvancedStator;
            hf_PistonSupport = GridTerminalSystem.GetBlockWithName("HF_supportPiston_a") as IMyPistonBase;

            hf_BackupRotorHinge = GridTerminalSystem.GetBlockWithName("HF_support_b") as IMyMotorAdvancedStator;
            hf_rotorHinge = GridTerminalSystem.GetBlockWithName("HF_support_a") as IMyMotorAdvancedStator;
            hf_ActiveRotorHinge = GridTerminalSystem.GetBlockWithName("HF_support_a") as IMyMotorAdvancedStator;

            hf_Status = GridTerminalSystem.GetBlockWithName("HF_status") as IMyTextPanel;

            hf_Systems.InsertRange(hf_Systems.Count, new List<IMyFunctionalBlock> { hf_ActiveRotorHinge, hf_ActiveRotorSupport, hf_PistonSupport });

            statusDisplay("Checking weaponsystems");
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(hf_Launchers);      

            if (Diagnostics())
            {
                hellfireStatus = "standby";
            } else
            {
                hellfireStatus = "malfunction";

            }

            statusDisplay(hellfireStatus);
        }

        

        void Main(string Argument)
        {
            string[] arguments = Argument.Split(':');

            switch (arguments[0])
            {
                case "close":

                    break;
                case "open":
                    //GridTerminalSystem. hf_rotorHinge.
                    break;
                case "angle":
                    int angle = 0;
                    if (int.TryParse(arguments[1], out angle))
                    {
                        setAngle(angle);
                    }
                    else
                    {
                        statusDisplay("Error: Illegal angle - " + arguments[1]);
                    }
                    break;
                case "lock":
                    lockSystem();
                    break;
                case "lockon":
                    lockOn();
                    break;
                case "fire":
                    fireHellfire();
                    break;
                case "status":
                    Diagnostics();
                    break;
                case "help":
                case "h":
                    statusDisplay(hf_help);
                    break;
                default:
                   
                    break;
            }

        }

        private void lockSystem()
        {
            throw new NotImplementedException();
        }

        private void lockOn()
        {
            throw new NotImplementedException();
        }

        private void setAngle(int angle)
        {
            if (angle > 90 || angle < -10)
            {
                statusDisplay("Error: Illegal angle - " + angle);
            }
            else
            {
                if (hf_ActiveRotorHinge.Angle > angle)
                {
                    hf_ActiveRotorHinge.SetValueFloat("velocity", -1.0f);
                }
                else if (hf_ActiveRotorHinge.Velocity > 0)
                {

                }
                else
                {

                }
            }
        }

        void fireHellfire()
        {
            if (hf_ActiveRotorHinge.Angle >= deactivateAngle)
            {
                for (int i = 0; i < hf_Launchers.Count; i++)
                {
                    hf_Launchers[i].ApplyAction("shootOnce");
                }
            } else
            {
                statusDisplay("Warning: Hellfire system is not\n in shooting position");
            }

        }

        void statusDisplay(string status)
        {
            statusDisplay(status, true);
        }

        void statusDisplay(string status, bool append)
        {
            hf_Status.WritePublicText(status + "/n", append);
        }

        bool Diagnostics()
        {
            bool status = true;

            statusDisplay("Running diagnostics");

            for(int i = 0; i < hf_Systems.Count; i++)
            {
                if (hf_Systems[i] != null)
                {
                    if (hf_Systems[i].IsWorking)
                    {
                        statusDisplay(hf_Systems[i].DisplayNameText + " - online");
                    }
                    else
                    {
                        statusDisplay(hf_Systems[i].DisplayNameText + " - offline");
                        status = false;
                    }
                } else
                {
                    statusDisplay(hf_Systems[i].DisplayNameText + " - cannot be found");
                    status = false;
                }
            } 

            return status;
        }



        void checkRotorSupport()
        {
            if (!hf_ActiveRotorSupport.IsWorking || !hf_ActiveRotorSupport.IsAttached)
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
                    if(!hf_ActiveRotorSupport.IsAttached)
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

            if (!hf_ActiveRotorHinge.IsWorking || !hf_ActiveRotorHinge.IsAttached)
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
