using System;
namespace googlewallet.Droid
{
	public static class EnvironmentVariable
	{
        public static void Set(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Machine);
        }
    }
}

