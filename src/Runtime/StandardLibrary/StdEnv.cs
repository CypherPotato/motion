namespace Motion.Runtime.StandardLibrary;

internal class StdEnv : IMotionLibrary
{
    public string? Namespace => "env";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("get-vars", () =>
        {
            return Environment.GetEnvironmentVariables();
        });
        context.Methods.Add("get-var", (string varname) =>
        {
            return Environment.GetEnvironmentVariable(varname);
        });
    }
}
