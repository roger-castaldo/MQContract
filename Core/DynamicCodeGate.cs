using System.Runtime.CompilerServices;

namespace MQContract
{
    internal static class DynamicCodeGate
    {
        public static bool? TestOverride;

        public static bool IsSupported =>
            TestOverride ?? RuntimeFeature.IsDynamicCodeSupported;
    }
}
