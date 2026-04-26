using System.Runtime.InteropServices;

namespace Faryma.Composer.Desktop.Utils
{
    internal static partial class NativeMethods
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();
    }
}
