namespace OSK.Inputs.Models.Runtime;
public class ActiveInputScheme(string inputDefinitionName, string controllerName, string schemeName)
{
    public string InputDefinitionName => inputDefinitionName;

    public string ControllerName => controllerName;

    public string ActiveInputSchemeName => schemeName;
}
