using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.ModAPI;
using VRage.Game.ModAPI.Ingame;

namespace RotationController
{
    class Program
    {
        IMyGridTerminalSystem GridTerminalSystem;

        internal interface IMyTimerBlock : IMyFunctionalBlock
        {
        }

        string Storage;

        public void Echo(string message)
        {

        }

        /// <summary>
        /// Start Copy here
        /// </summary>

        string programName = "Rotation System";
        string systemStatus = "none";
        string hingeStatus = "none";
        string statusReport = "updating...";

        const char green = '\uE001';
        const char blue = '\uE002';
        const char red = '\uE003';
        const char yellow = '\uE004';

        IMyMotorAdvancedStator hf_RotorHinge;
        IMyTimerBlock hf_statusTimer;
        IMyTextPanel hf_DisplayLog;
        IMyTextPanel hf_DisplayStatus;

        List<IMyTerminalBlock> systems = new List<IMyTerminalBlock>();
        List<string> logfile = new List<string>();

        int targetDistance = 0;

        const float opened = 90.0f;
        const float closed = -8.0f;
        const float hellfireLength = 45;
        const float hellfireVelocity = 2.25f;
        const float threshold = 0.1f;

        const float rocketSpeed = 100.0f;

        float currentAngle;

        private bool backupHinge = false;


        public Program()
        {
            hf_DisplayStatus = GridTerminalSystem.GetBlockWithName("hf_displayStatus") as IMyTextPanel;
            hf_DisplayLog = GridTerminalSystem.GetBlockWithName("hf_displayLog") as IMyTextPanel;

            if (hf_DisplayLog == null)
            {
                Echo("No display 'hf_displayStatus' found");
            } else
            {
                LogDisplay(programName + " is booting up" + false);
            }

            hf_RotorHinge = GridTerminalSystem.GetBlockWithName("hf_rotor_a") as IMyMotorAdvancedStator;
            hf_statusTimer = GridTerminalSystem.GetBlockWithName("hf_status_Timer") as IMyTimerBlock;

            systems.InsertRange(systems.Count, new List<IMyFunctionalBlock> { hf_RotorHinge, hf_statusTimer });

            getStatus();
            updateStatus();
        }


        public void Main(string argument)
        {

            if (int.TryParse(argument, out targetDistance))
            {
                setHingeRotationtoDistance(targetDistance);
            } else
            {
                switch (argument)
                {
                    case "close":
                        closeHellfire();
                        break;
                    case "open":
                        openHellfire();
                        break;
                    case "test":
                        break;
                    case "update":
                        updateStatus();
                        break;
                    default:
                        LogDisplay("Wrong argument: " + argument);
                        break;
                }
            }
            

        }

        private void setHingeRotationtoDistance(int targetDistance)
        {
            double theta;
            theta = Math.Acos(hellfireLength / targetDistance);
            LogDisplay("Getting target distance: " + targetDistance + "\nCalculating Rotation: " + theta);
            setRotation((float)theta);
        }


        private void setRotation(float angle){
            angle = checkAngle(angle);
            currentAngle = hf_RotorHinge.Angle;
            if (Math.Abs(Math.Abs(angle) - Math.Abs(currentAngle)) > threshold)
            {
                changeRotation(angle);
            }
        }

        private void changeRotation(float angle)
        {
            LogDisplay("Changing rotation from " + currentAngle + " to " + angle);
            if(currentAngle > angle)
            {
                setUpperLimit(angle);
                setVelocity("open");
            } else
            {
                hf_RotorHinge.SetValueFloat(getLimitProprtyID("LowerLimit"), angle);
                setVelocity("closing");
            }
        }

        // set the correct velocity (open = backup + / open != backup -
        private void setVelocity(string direction)
        {

        }

        // Handles the relative upper limit (lower limit for !backup)
        public void setUpperLimit(float angle)
        {
            if (angle > opened)
            {
                LogDisplay("Error: UpperLimit is too high");
            } else
            {
                if (!backupHinge)
                {
                    hf_RotorHinge.SetValueFloat(getLimitProprtyID("LowerLimit"), -angle);
                }
                else
                {
                    hf_RotorHinge.SetValueFloat(getLimitProprtyID("UpperLimit"), angle);
                }
            }

            
        }

        // Handles the relative upper limit (upper limit for !backup)
        public void setLowerLimit(float angle)
        {
            if (angle < closed)
            {

                LogDisplay("Error: LowerLimit is too small");
            }
            else
            {
                if (!backupHinge)
                {
                    hf_RotorHinge.SetValueFloat(getLimitProprtyID("UpperLimit"), -angle);
                }
                else
                {
                    hf_RotorHinge.SetValueFloat(getLimitProprtyID("LowerLimit"), angle);
                }
            }

        }

        // this just totally breaks the whole system - rewrite reason might be the backup / !backup logic
        public void openHellfire()
        {
            LogDisplay("Checking Current position");
            hingeStatus = getHingeStatus();

            if (!hingeStatus.Equals("closing"))
            {
                setUpperLimit(opened);

                setHingeVelocity("opening", hellfireVelocity);

                LogDisplay("Opening in Progress");
            }
            else
            {
                LogDisplay("Error: Command can not be executed - open");
            }
        }


        // this just totally breaks the whole system - rewrite reason might be the backup / !backup logic
        public void closeHellfire()
        {
            hingeStatus = getHingeStatus();

            if (!hingeStatus.Equals("closing"))
            {
                LogDisplay("Closing in Progress");

                setLowerLimit(closed);

                setHingeVelocity("closing", hellfireVelocity);
            }
            else
            {
                LogDisplay("Error: Command can not be executed - close");
            }
        }

        // could probably be much easier
        private void setHingeVelocity(string option, float newVelocity)
        {
            if(option.Equals("closing"))
            {
                if (backupHinge)
                {
                    newVelocity = -hellfireVelocity;
                }
            } else if (option.Equals("opening"))
            {
                if (!backupHinge)
                {
                    newVelocity = -hellfireVelocity;
                }
            } else
            {
                LogDisplay("Velocity: Command not regocnized '" + option + "' - Emergency stop");
                newVelocity = 0f;
            }

            hf_RotorHinge.SetValueFloat("Velocity", newVelocity);
        }

        private float checkAngle(float angle)
        {
            if (angle > opened)
            {
                LogDisplay("Warning: angle is too large (" + angle + ")\n Setting angle to " + opened);
                angle = opened;
            } else if (angle < closed)
            {
                LogDisplay("Warning: angle is too small (" + angle + ")\n Setting angle to " + closed);
                angle = closed;
            }
            
            if (backupHinge)
            {
                angle = -angle;
            }

            return angle;
        }

        public string getLimitProprtyID(string limit)
        {
            string result = limit;
            if (backupHinge)
            {
                if(limit.Equals("UpperLimit"))
                {
                    limit = "LowerLimit";
                } else if (limit.Equals("LowerLimit"))
                {
                    limit = "UpperLimit";
                } 
            }
            return limit;
        
        }

        void getStatus()
        {
            LogDisplay("Running diagnostics...");

            if (Diagnostics())
            {
                systemStatus = "Ready";
            }
            else
            {
                systemStatus = "Malfunction";
            }

            LogDisplay(programName + " systems: " + systemStatus);
        }


        /// <summary>
        /// Need to be adjusted to support +/- angle for the backup system
        /// </summary>
        /// <returns></returns>
        string getHingeStatus()
        {
            LogDisplay("Rotor status:");
            string result = "none";
            if (hf_RotorHinge.Velocity > 0)
            {
                if (hf_RotorHinge.Angle < opened)
                {
                    result = "opening";
                }
                else if (hf_RotorHinge.Angle >= opened)
                {
                    result = "malfunction";
                }
            }
            else if (hf_RotorHinge.Velocity < 0)
            {
                if (hf_RotorHinge.Angle > closed)
                {
                    result = "closing";
                }
                else if (hf_RotorHinge.Angle <= closed)
                {
                    result = "malfunction";
                }
            }
            else
            {
                result = "malfunction";
            }

            LogDisplay(result + ": vel = " + hf_RotorHinge.Velocity + " rot: " + hf_RotorHinge.Angle);
            return result;
        }


        public bool Diagnostics()
        {
            bool result = true;
            string tempStatus;
            for (int i = 0; i < systems.Count; i++)
            {
                if (systems[i].IsFunctional && systems[i].IsWorking)
                {
                    tempStatus = (green + " " + systems[i].CustomName + " is okay");
                } else if (systems[i].IsWorking) {
                    tempStatus = (yellow + " " + systems[i].CustomName + " is okay");
                }
                else
                {
                    tempStatus = (red + " " + systems[i].CustomName + " has an error");
                    result = false;
                }

                LogDisplay(tempStatus);
            }
            return result;
        }

        /// <summary>
        /// Needs adjustment for backup rotor support
        /// </summary>
        private void updateStatus()
        {
            hf_DisplayStatus.WritePrivateText("Hellfire System Status (" + systemStatus + "):", false);
            hf_DisplayStatus.WritePrivateText("Active Rotor: " + hf_RotorHinge.CustomName);
            statusReport = String.Format("Functional: {0}\n Rotation: {1}\n Velocity: {2}\n Lower Limit: {3}\n Upper Limit: {4} ", hf_RotorHinge.IsFunctional, hf_RotorHinge.Angle, hf_RotorHinge.Velocity, hf_RotorHinge.LowerLimit, hf_RotorHinge.UpperLimit);
            hf_DisplayStatus.WritePrivateText(statusReport);
        }

        void LogDisplay(string status)
        {
            LogDisplay(status, true);
        }

        //create handling of multiple lines
        void LogDisplay(string status, bool append)
        {
            if (append)
            {
                logfile.Add(status);
            } else
            {
                logfile.Clear();
                logfile.Add(status + "\n");
            }
            hf_DisplayLog.WritePrivateText(status + "\n", append);
            updateLog();
        }

        // create a handling of the display so that the most current infos are shown and the display autoscrolls
        void updateLog()
        {
            for (int i = logfile.Count - 16; i < logfile.Count; i++)
            {
                hf_DisplayLog.WritePrivateText(logfile[i] + "\n", true);
            }
        }

        /// <summary>
        /// Stop Copy here
        /// </summary>
        #region StopCopy

        #endregion
    }
}
