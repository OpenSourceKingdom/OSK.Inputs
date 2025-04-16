namespace OSK.Inputs.Models.Runtime;
public class ActiveInputScheme(int userId, string inputDefinitionName, string deviceName, string schemeName)
{
    public string InputDefinitionName => inputDefinitionName;

    public string DeviceName => deviceName;

    public string ActiveInputSchemeName => schemeName;

    public int UserId => userId;
}
