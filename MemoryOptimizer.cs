using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace atc
{
    public static class MemoryOptimizer
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr process, UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);

        /// <summary>
        /// Forces garbage collection and trims the working set of the process.
        /// This artificially lowers the reported memory footprint in Task Manager.
        /// </summary>
        public static void Trim()
        {
            try
            {
                // Force full garbage collection
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                // Page out unused memory
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)uint.MaxValue, (UIntPtr)uint.MaxValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MemoryOptimizer] Error trimming memory: {ex.Message}");
            }
        }
    }
}
