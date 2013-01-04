using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Hanasu.Background_Tasks
{
    public static class BGTaskManager
    {
        public static void UnregisterBackgroundTask(string taskName)
        {
            var reg = GetRegisteredBackgroundTask(taskName);

            if (reg != null)
                reg.Unregister(true);
            else
                throw new Exception("Unable to find task: " + taskName);
            
        }

        public static bool BackgroundTaskIsRegistered(string taskName)
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == taskName)
                {
                    // 
                    // The task is already registered.
                    // 

                    return true;
                }
            }
            return false;
        }

        public static IBackgroundTaskRegistration GetRegisteredBackgroundTask(string taskName)
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == taskName)
                {
                    // 
                    // The task is already registered.
                    // 

                    return cur.Value;
                }
            }
            return null;
        }


        //modified from from: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/jj553413.aspx

        //
        // Register a background task with the specified taskEntryPoint, name, trigger,
        // and condition (optional).
        //
        // taskEntryPoint: Task entry point for the background task.
        // taskName: A name for the background task.
        // trigger: The trigger for the background task.
        // condition: Optional parameter. A conditional event that must be true for the task to fire.
        //
        public static BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint,
                                                                        string taskName,
                                                                        IBackgroundTrigger trigger,
                                                                        params IBackgroundCondition[] conditions)
        {
            //
            // Check for existing registrations of this background task.
            //

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == taskName)
                {
                    // 
                    // The task is already registered.
                    // 

                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }


            //
            // Register the background task.
            //

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (conditions != null)
            {
                foreach (var condition in conditions)
                    builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();

            return task;
        }
    }
}
