﻿using Cauldron.Interception;
using NLog;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Cauldron.Interceptors
{
    /// <summary>
    /// Intercepts the method and logs its execution time.
    /// <para/>
    /// This interceptor is using NLog. NLog configuration will affect this interceptor.
    /// </summary>
    [InterceptorOptions(AlwaysCreateNewInstance = true)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PerformanceLoggerAttribute : Attribute, IMethodInterceptor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string methodName;
        private Stopwatch stopwatch;

        /// <exclude/>
        public void OnEnter(Type declaringType, object instance, MethodBase methodbase, object[] values)
        {
            this.methodName = methodbase.Name;
            stopwatch = Stopwatch.StartNew();
        }

        /// <exclude/>
        public bool OnException(Exception e) => true;

        /// <exclude/>
        public void OnExit()
        {
            stopwatch.Stop();
            logger.Log(LogLevel.Info, $"{methodName} took {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}