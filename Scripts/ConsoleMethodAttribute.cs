using System;


namespace IngameDebugConsole
{
    /// <summary>
    /// Marks a <c>public static</c> method as a console command, registering it automatically at startup.
    /// The declaring class must also be <c>public</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ConsoleMethodAttribute : Attribute
    {
        /// <summary>Registers a console command with an optional list of human-readable parameter names shown in the help signature.</summary>
        public ConsoleMethodAttribute(string command, string description, params string[] parameterNames)
        {
            Command = command;
            Description = description;
            ParameterNames = parameterNames;
        }


        /// <summary>The command token the user types in the console (e.g. <c>"cube"</c>).</summary>
        public string Command { get; }

        /// <summary>Short description shown next to the command in help output.</summary>
        public string Description { get; }

        /// <summary>Optional display names for each parameter, shown in the command signature (e.g. <c>[Vector3 position]</c>).</summary>
        public string[] ParameterNames { get; }
    }
}