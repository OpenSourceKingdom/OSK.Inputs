namespace OSK.Inputs.Models.Runtime;
public class ActiveInputScheme(int userId, string inputDefinitionName, string controllerName, string schemeName)
{
    public string InputDefinitionName => inputDefinitionName;

    public string ControllerName => controllerName;

    public string ActiveInputSchemeName => schemeName;

    public int UserId => userId;
}
