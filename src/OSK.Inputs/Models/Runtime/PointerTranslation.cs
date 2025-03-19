namespace OSK.Inputs.Models.Runtime;
public class PointerTranslation(float x, float y)
{
    public static PointerTranslation None = new PointerTranslation(0, 0);

    public float X => x;

    public float Y => y;
}
