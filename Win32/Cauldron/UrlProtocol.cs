﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cauldron.Net
{
    /// <summary>
    /// Provides methods for handling the registration of an Application to a URI Scheme and helper methods
    /// for handling the uri protocol
    /// </summary>
    public static class UrlProtocol
    {
        /// <summary>
        /// Registers the application to a URI scheme.
        /// </summary>
        /// <param name="urlProtocol">The application uri e.g. exampleApplication://</param>
        /// <exception cref="UnauthorizedAccessException">Process elevation required</exception>
        public static void Register(string urlProtocol)
        {
            var key = urlProtocol.Split(':').First();
            var location = Assembly.GetEntryAssembly().Location;
            var applicationPath = Path.Combine(Path.GetDirectoryName(location), Path.GetFileName(location).Replace(".vshost.exe", ".exe"));

            if (Registry.ClassesRoot.OpenSubKey(key, false) == null)
            {
                // Add the values to the registry
                try
                {
                    StartRegistration(key, applicationPath);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException("Process Elevation required to execute this method");
                }
                return;
            }

            var command = Registry.ClassesRoot?
                .OpenSubKey(key, false)?
                .OpenSubKey("shell", false)?
                .OpenSubKey("open", false)?
                .OpenSubKey("command", false);

            if (command == null)
            {
                try
                {
                    StartRegistration(key, applicationPath);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException("Process Elevation required to execute this method");
                }
                return;
            }

            var value = command.GetValue("") as string;
            var path = $"\"{location}\" \"%1\"";

            if (value != path)
            {
                try
                {
                    Registry.ClassesRoot
                        .OpenSubKey(key, false)
                        .OpenSubKey("shell", false)
                        .OpenSubKey("open", false)
                        .OpenSubKey("command", true)
                        .SetValue("", path);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException("Process Elevation required to execute this method");
                }
            }
        }

        /// <summary>
        /// Registers the application to a URI scheme using runas to elevate the process if required.
        /// Not that this can still throw a <see cref="UnauthorizedAccessException"/> if the elevated process is also not authorized.
        /// </summary>
        /// <param name="urlProtocol">The application uri e.g. exampleApplication://</param>
        /// <exception cref="UnauthorizedAccessException">Process elevation required</exception>
        public static void RegisterElevated(string urlProtocol)
        {
            try
            {
                UrlProtocol.Register(urlProtocol);
            }
            catch (UnauthorizedAccessException)
            {
                var location = Assembly.GetEntryAssembly().Location;

                var processInfo = new ProcessStartInfo();
                processInfo.Verb = "runas";
                processInfo.FileName = Path.Combine(Path.GetDirectoryName(location), Path.GetFileName(location).Replace(".vshost.exe", ".exe"));
                processInfo.Arguments = "registerUriScheme " + urlProtocol;
                Process.Start(processInfo);
            }
        }

        /// <summary>
        /// Returns true if the uri requires registration; otherwise false.
        /// </summary>
        /// <param name="urlProtocol">The application uri e.g. exampleApplication://</param>
        /// <returns>Returns true if the uri requires registration; otherwise false.</returns>
        public static bool RequiresRegistration(string urlProtocol)
        {
            var key = urlProtocol.Split(':').First();
            var location = Assembly.GetEntryAssembly().Location;
            var applicationPath = Path.Combine(Path.GetDirectoryName(location), Path.GetFileName(location).Replace(".vshost.exe", ".exe"));

            if (Registry.ClassesRoot.OpenSubKey(key, false) == null)
                return true;

            var command = Registry.ClassesRoot?
                .OpenSubKey(key, false)?
                .OpenSubKey("shell", false)?
                .OpenSubKey("open", false)?
                .OpenSubKey("command", false);

            if (command == null)
                return true;

            var value = command.GetValue("") as string;
            var path = $"\"{location}\" \"%1\"";

            if (value != path)
                return true;

            return false;
        }

        private static void StartRegistration(string name, string location)
        {
            var subKey = Registry.ClassesRoot.CreateSubKey(name);
            var defaultIcon = subKey.CreateSubKey("DefaultIcon");
            var command = subKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
            subKey.SetValue("", $"URL:{name}");
            subKey.SetValue("URL Protocol", "");
            defaultIcon.SetValue("", $"{location},0");
            command.SetValue("", $"\"{location}\" \"%1\"");
        }
    }
}