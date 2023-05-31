using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace chicory
{
    internal class PowerDllHelper
    {
        // берем что нужно из powrprof.dll 
        [DllImport("powrprof.dll")]
        private static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            ref IntPtr ActivePolicyGuid
        );
        
        [DllImport("powrprof.dll")]
        private static extern uint PowerSetActiveScheme(
            IntPtr UserRootPowerKey,
            ref Guid SchemeGuid
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint AcValueIndex
        );
        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint DcValueIndex
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadACValue(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            ref int Type,
            ref int Buffer,
            ref uint BufferSize
        );

        // функция для записи значений спящего режима в реестр
        public static void SetPowerSettings(Guid GUID_SLEEP_SUBGROUP, Guid GUID_STANDBY_TIMEOUT, uint ACIndexValue, uint DCIndexValue)
        {
            IntPtr ptr = IntPtr.Zero;

            if (PowerDllHelper.PowerGetActiveScheme(IntPtr.Zero, ref ptr) == 0)
            {
                Guid scheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));

                if (PowerDllHelper.PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref GUID_SLEEP_SUBGROUP, ref GUID_STANDBY_TIMEOUT, ACIndexValue) == 0)
                    PowerDllHelper.PowerSetActiveScheme(IntPtr.Zero, ref scheme);

                if (PowerDllHelper.PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref GUID_SLEEP_SUBGROUP, ref GUID_STANDBY_TIMEOUT, DCIndexValue) == 0)
                    PowerDllHelper.PowerSetActiveScheme(IntPtr.Zero, ref scheme);

                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
