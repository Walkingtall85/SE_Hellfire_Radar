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

namespace SE_Hellfire
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


        IMyCameraBlock hf_Camera;
        IMySensorBlock hf_Sensor;
        IMyRadioAntenna antenna;
        IMyRemoteControl hf_Remote;
        IMyPistonBase hf_PistonSupport;
        IMySmallMissileLauncher hf_MisslileLauncher;

        IMyTimerBlock hf_statusTimer;
        IMyTimerBlock hf_velocityTimer;


        IMyTextPanel hf_DisplayLog;
        IMyTextPanel hf_DisplayStatus;

        List<IMyTerminalBlock> hf_Launchers = new List<IMyTerminalBlock>();

        // Rotors  
        IMyMotorAdvancedStator hf_BackupRotorSupport;
        IMyMotorAdvancedStator hf_RotorSupport;
        IMyMotorAdvancedStator hf_ActiveRotorSupport;
        IMyMotorAdvancedStator hf_RotorHinge;
        IMyMotorAdvancedStator hf_BackupRotorHinge;
        IMyMotorAdvancedStator hf_ActiveRotorHinge;

        //strings  
        string rotorStatusHinge = "functional";
        string rotorStatusSupport = "functional";
        string targetStatus = "none";
        string hellfireStatus = "none";
        string hingeStatus = "none";
        string weaponStatus = "none";
        string weaponCount = "none";
        string weaponEnabled = "none";

        string hf_help = "Arguments: close, open, angle\n status, lock, lockon, fire, help";

        //floats  
        float angleHingeRotor;
        float angleSupportRotor;
        float angleSupportPiston;
        float angleDifference;

        int launcherCount = 0;
        int launcherTotal = 0;
        int launcherEnabled = 0;
        int launcherDisabled = 0;

        //constants  
        const double hellfireLength = 45;
        const double safetyeAngle = 50;
        const float hellfireVelocity = 2.25f;
        const float open = 90;
        const float closed = -8;

        //boool  
        bool safety = true;

        //Lists    
        List<IMyFunctionalBlock> hf_Systems = new List<IMyFunctionalBlock>();

        //Vectors  
        Vector3D LastShipPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D LastTargetPos = new Vector3D(0.0, 0.0, 0.0);
        Vector3D targetPosition = new Vector3D(0.0, 0.0, 0.0);

        IMyEntity target = null;


        Program()
        {
            hf_DisplayStatus = GridTerminalSystem.GetBlockWithName("hf_displayStatus") as IMyTextPanel;
            hf_DisplayLog = GridTerminalSystem.GetBlockWithName("hf_displayLog") as IMyTextPanel;

            LogDisplay("Hellfire booting sequence", false);
            LogDisplay("Checking components");

            //hf_Camera = GridTerminalSystem.GetBlockWithName("hf_camera") as IMyCameraBlock;  
            //hf_Sensor = GridTerminalSystem.GetBlockWithName("hf_sensor") as IMySensorBlock;  
            //hf_Remote = GridTerminalSystem.GetBlockWithName("hf_remote") as IMyRemoteControl;  

            hf_BackupRotorSupport = GridTerminalSystem.GetBlockWithName("hf_supportRotor_b") as IMyMotorAdvancedStator;
            hf_ActiveRotorSupport = GridTerminalSystem.GetBlockWithName("hf_supportRotor_a") as IMyMotorAdvancedStator;
            hf_RotorSupport = GridTerminalSystem.GetBlockWithName("hf_supportRotor") as IMyMotorAdvancedStator;
            hf_PistonSupport = GridTerminalSystem.GetBlockWithName("hf_supportPiston") as IMyPistonBase;

            hf_BackupRotorHinge = GridTerminalSystem.GetBlockWithName("hf_rotor_b") as IMyMotorAdvancedStator;
            hf_RotorHinge = GridTerminalSystem.GetBlockWithName("hf_rotor_a") as IMyMotorAdvancedStator;
            hf_ActiveRotorHinge = GridTerminalSystem.GetBlockWithName("hf_rotor_a") as IMyMotorAdvancedStator;

            hf_statusTimer = GridTerminalSystem.GetBlockWithName("hf_status_Timer") as IMyTimerBlock;
            hf_velocityTimer = GridTerminalSystem.GetBlockWithName("hf_velocity_Timer") as IMyTimerBlock;

            hf_Systems.InsertRange(hf_Systems.Count, new List<IMyFunctionalBlock> { hf_ActiveRotorHinge, hf_ActiveRotorSupport, hf_PistonSupport, hf_statusTimer, hf_velocityTimer });

            LogDisplay("Checking weaponsystems");
            GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(hf_Launchers);

            // Delete this once the real total is known  
            launcherTotal = hf_Launchers.Count;

            if (Diagnostics())
            {
                hellfireStatus = "standby";
            }
            else
            {
                hellfireStatus = "malfunction";
            }

            LogDisplay(hellfireStatus);
            RefreshStatus();
        }



        void Main(string Argument)
        {
            string[] arguments = Argument.Split(':');

            switch (arguments[0])
            {
                case "close":
                    LogDisplay("closing system...");
                    closeHellfire();
                    break;
                case "open":
                    LogDisplay("opening system...");
                    openHellfire();
                    break;
                case "angle":
                    float angle = 0;
                    if (float.TryParse(arguments[1], out angle))
                    {
                        setAngle(angle);
                    }
                    else
                    {
                        LogDisplay("Error: Illegal angle - " + arguments[1]);
                    }
                    break;
                case "velocity":
                    AdjustVelocity();
                    break;
                case "unlock":
                    LockSystem(false);
                    break;
                case "lock":
                    LockSystem(true);
                    break;
                case "lockon":
                    LockOn();
                    break;
                case "fire":
                    FireHellfire();
                    break;
                case "status":
                    Diagnostics();
                    break;
                case "update":
                    RefreshStatus();
                    break;
                case "cls":
                    LogDisplay("cls...", false);
                    break;
                case "help":
                case "h":
                    LogDisplay(hf_help);
                    break;
                case "getAngle":
                    getAngle();
                    break;
                default:

                    break;
            }

        }

        public void openHellfire()
        {

            LogDisplay("Checking Current position");
            if (hf_ActiveRotorHinge.Angle < closed && hf_ActiveRotorHinge.Angle > open)
            {
                hingeStatus = "opening";
                LogDisplay("Adjusting opening position");
                hf_ActiveRotorHinge.SetValueFloat("LowerLimit", open);

                LogDisplay("Changeing Velocity");
                hf_ActiveRotorHinge.SetValueFloat("Velocity", hellfireVelocity);

                LogDisplay("Opening in Progress");
                AdjustVelocity();

            }
            else
            {
                LogDisplay("Error: System is already open");
            }
        }

        public void closeHellfire()
        {
            if (hf_ActiveRotorHinge.Angle > closed)
            {
                hf_ActiveRotorHinge.SetValueFloat("velocity", -hellfireVelocity);
                //adjustVelocity(closed); 
            }
            else
            {
                LogDisplay("Error: System is already closed");
            }
        }

        public void LockSystem(bool locking)
        {
            if ((locking && !hf_ActiveRotorHinge.IsLocked) || (!locking && hf_ActiveRotorHinge.IsLocked))
            {
                hf_ActiveRotorHinge.ApplyAction("Force weld");
                LogDisplay("Switching Lock");
            }
        }

        public void LockOn()
        {
            float targetDistance = getTargetDistance();
            float angle = open;
            angle = (float)(Math.Acos(hellfireLength / targetDistance) * (180 / Math.PI));
            setAngle(angle);
        }

        public void setAngle(float angle)
        {

            if (angle <= open && angle >= closed)
            {
                DisplayDebug("Seting Angle to " + angle);
                angle = DegreeToRadian(angle);
                if (hf_ActiveRotorHinge.DisplayNameText.Equals("hf_rotor_a") && hf_ActiveRotorHinge.Angle < angle)
                {

                } 
            }
            else
            {
                LogDisplay("Error: Illegal angle - " + angle);
            }
        }

        private void DisplayDebug(string v)
        {
            
        }

        public void getAngle()
        {
            LogDisplay(hf_ActiveRotorHinge.Angle.ToString());
        }

        public void HellFireAngle(float angle)
        {
            LogDisplay("Changeing angle from " + RadianToDegree(hf_ActiveRotorHinge.Angle) + " to " + angle);
            angle = DegreeToRadian(angle);

            if (hf_ActiveRotorHinge.Angle < angle)
            {
                hf_ActiveRotorHinge.SetValueFloat("MaxLimit", angle);
                hf_ActiveRotorHinge.SetValueFloat("Velocity", hellfireVelocity);
                hf_velocityTimer.ApplyAction("TriggerNow");
            }
            else if (hf_ActiveRotorHinge.Angle > angle)
            {
                hf_ActiveRotorHinge.SetValueFloat("MinLimit", angle);
                hf_ActiveRotorHinge.SetValueFloat("Velocity", -hellfireVelocity);
                hf_velocityTimer.ApplyAction("TriggerNow");
            }
            else
            {
                hf_ActiveRotorHinge.SetValueFloat("Velocity", 0f);
            }
        }

        public void AdjustVelocity()
        {
            float difference;
            if (hingeStatus.Equals("closing") && hf_ActiveRotorHinge.Angle < hf_ActiveRotorHinge.GetValueFloat("UpperLimit"))
            {
                difference = hf_ActiveRotorHinge.GetValueFloat("UpperLimit") - hf_ActiveRotorHinge.Angle;
                angleDifference = RadianToDegree(difference);
                if (difference < 0.05 && hf_ActiveRotorHinge.Velocity > (hellfireVelocity / 2))
                {
                    hf_ActiveRotorHinge.SetValueFloat("Velocity", (hellfireVelocity / 2));
                }

            }
            else if ((hingeStatus.Equals("opening") && hf_ActiveRotorHinge.Angle > hf_ActiveRotorHinge.GetValueFloat("LowerLimit")))
            {
                difference = hf_ActiveRotorHinge.GetValueFloat("LowerLimit") - hf_ActiveRotorHinge.Angle;
                angleDifference = RadianToDegree(difference);
                if (difference < 0.05 && hf_ActiveRotorHinge.Velocity > (hellfireVelocity / 2))
                {
                    hf_ActiveRotorHinge.SetValueFloat("Velocity", (hellfireVelocity / 2));
                }
            }
            else
            {
                hf_velocityTimer.ApplyAction("Stop");
                LogDisplay("Stopping Hinge " + hingeStatus + " - " + RadianToDegree(hf_ActiveRotorHinge.Angle));
            }
        }

        public bool WeaponDiagnostic()
        {
            bool status = true;
            int count = 0;
            int enabled = 0;
            int disabled = 0;
            //LogDisplay("Checking Weaponsystems"); 
            for (int i = 0; i < hf_Launchers.Count; i++)
            {
                if (hf_Launchers[i].IsFunctional)
                {
                    if (hf_Launchers[i].IsWorking)
                    {
                        enabled++;
                    }
                    else
                    {
                        disabled++;
                    }
                }
                else
                {
                    LogDisplay(hf_Launchers[i].DisplayNameText + "is offline");
                    status = false;
                }
                count++;
            }

            if (!status)
            {
                LogDisplay("All weapon systems - online");
            }

            weaponEnabled = (enabled + "/" + disabled);
            weaponCount = (count + "/" + launcherTotal);
            launcherCount = count;
            RefreshStatus();

            return status;
        }


        public bool WeaponStatus()
        {
            bool status = true;
            if (hf_ActiveRotorHinge.Angle > safetyeAngle && safety)
            {
                safety = false;
                for (int i = 0; i < hf_Launchers.Count; i++)
                {
                    hf_Launchers[0].ApplyAction("OnOff_On");
                }
                weaponStatus = "Ready to fire";
            }
            else if (hf_ActiveRotorHinge.Angle < safetyeAngle && !safety)
            {
                status = false;
                safety = true;
                for (int i = 0; i < hf_Launchers.Count; i++)
                {
                    hf_Launchers[0].ApplyAction("OnOff_Off");
                }
                weaponStatus = "Safety enabled";
            }
            RefreshStatus();
            return status;
        }

        void FireHellfire()
        {
            if (!safety || WeaponStatus())
            {
                for (int i = 0; i < hf_Launchers.Count; i++)
                {
                    hf_Launchers[i].ApplyAction("shootOnce");
                }
            }
            else
            {
                LogDisplay("Warning: Hellfire system is not\n in shooting position");
            }

        }

        void LogDisplay(string status)
        {
            LogDisplay(status, true);
        }

        void LogDisplay(string status, bool append)
        {
            hf_DisplayLog.WritePrivateText(status + "\n", append);
        }

        void RefreshStatus()
        {
            hf_DisplayStatus.WritePrivateText("Hellfire System:\n", false);
            hf_DisplayStatus.WritePrivateText("Status:" + hellfireStatus + "\n", true);
            hf_DisplayStatus.WritePrivateText("Weapons:" + weaponStatus + "\n", true);
            hf_DisplayStatus.WritePrivateText("Count/Total:" + weaponCount + "\n", true);
            hf_DisplayStatus.WritePrivateText("Enabled/Disabled:" + weaponEnabled + "\n", true);
            hf_DisplayStatus.WritePrivateText("Angle:" + RadianToDegree(hf_ActiveRotorHinge.LowerLimit) + "/" + RadianToDegree(hf_ActiveRotorHinge.Angle) + "/" + RadianToDegree(hf_ActiveRotorHinge.UpperLimit) + "\n", true);
            hf_DisplayStatus.WritePrivateText("Difference: " + hingeStatus + " - " + angleDifference, true);
            // add additional blocks for target ID etc  
        }


        bool Diagnostics()
        {
            bool status = true;

            LogDisplay("Running full diagnostics");

            if (!SystemDiagnostics() & !WeaponDiagnostic())
            {
                status = false;
            }

            return status;
        }

        bool SystemDiagnostics()
        {
            bool status = true;
            for (int i = 0; i < hf_Systems.Count; i++)
            {
                if (hf_Systems[i] != null)
                {
                    if (hf_Systems[i].IsWorking)
                    {
                        LogDisplay(hf_Systems[i].DisplayNameText + " - online");
                    }
                    else
                    {
                        LogDisplay(hf_Systems[i].DisplayNameText + " - offline");
                        status = false;
                    }
                }
                else
                {
                    LogDisplay(hf_Systems[i].DisplayNameText + " - cannot be found");
                    status = false;
                }
            }

            return status;
        }



        void CheckRotorSupport()
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
                    if (!hf_ActiveRotorSupport.IsAttached)
                    {
                        if (hf_BackupRotorSupport.IsFunctional && hf_BackupRotorSupport.IsAttached)
                        {

                        }
                    }
                    rotorStatusSupport = "Functional";
                }
            }
        }

        void CheckRotorHinge()
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
            IMyTextPanel hellfireSystemStatus = GridTerminalSystem.GetBlockWithName("hf_system_Display") as IMyTextPanel;

        }


        float RotorAngleCalculation()
        {
            float angle = 90f;
            double targetDistance = getTargetDistance();

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


        Vector3D getHellfirePosition()
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


        float getTargetDistance()
        {
            float distance = 150;
            if (target != null)
            {
                distance = (float)Math.Sqrt(LastShipPos.X * LastTargetPos.X + LastShipPos.Y * LastTargetPos.Y + LastShipPos.Z * LastTargetPos.Z);
            }
            return distance;
        }


        private float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        private float RadianToDegree(float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        //stop coping here
        #endregion
    }


}
