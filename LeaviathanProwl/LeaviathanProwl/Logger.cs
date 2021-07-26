using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace LeaviathanProwl
{
    public class Logger
    {
        public static void LogAllFields(object obj)
        {
            LogInfo("");
            LogInfo("{0}", obj);
            if (obj != null)
                foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    LogInfo("{0}\t{1}\t{2}", fieldInfo.FieldType.Name, fieldInfo.Name, fieldInfo.GetValue(obj));
            LogInfo("");
        }

        public static void LogInfo(Creature creature, string prefix = "")
        {
            Vector3 lastPosition = Vector3.zero;
            ResourceTracker tracker = null;
            if (LeviathanInfo.IsActive(creature))
            {
                lastPosition = creature.transform.position;
                tracker = creature.GetComponent<ResourceTracker>();
            }

            LogInfo("{3} {2} {1} {0}", lastPosition, prefix, GetPrivate<string>(tracker, "uniqueId"), creature);
        }

        public static void LogInfo(String format, params object[] args)
        {
            LogInfo(false, format, args);
        }

        public static void LogInfo(bool error, String format, params object[] args)
        {
            string message = string.Format("[LeaviathanProwl] " + (error ? "!!! ERROR !!! " : "") + format, args);
            if (error)
                ErrorMessage.AddMessage(message);
            Console.WriteLine(message);
        }

        public static T GetPrivate<T>(object instance, String name)
        {
            if (instance == null)
                return default;

            FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)field.GetValue(instance);
        }
    }
}
