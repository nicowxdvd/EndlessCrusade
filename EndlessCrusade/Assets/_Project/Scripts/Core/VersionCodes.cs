using System;

namespace EC.Core
{
    public static class VersionCodes
    {
        public static bool TryParse(string version, out int major, out int minor, out int patch)
        {
            major = minor = patch = 0;
            if (string.IsNullOrEmpty(version))
                return false;
            var parts = version.Split('.');
            return parts.Length == 3
                && int.TryParse(parts[0], out major)
                && int.TryParse(parts[1], out minor)
                && int.TryParse(parts[2], out patch)
                && major >= 0 && minor >= 0 && minor <= 99 && patch >= 0 && patch <= 99;
        }

        public static int ToCode(string version)
        {
            if (!TryParse(version, out var major, out var minor, out var patch))
                throw new FormatException("Versión inválida: " + version + ". Formato esperado: mayor.menor.parche");
            return major * 10000 + minor * 100 + patch;
        }
    }
}
