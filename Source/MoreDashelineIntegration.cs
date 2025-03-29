using Microsoft.Xna.Framework;

namespace Celeste.Mod.DashToggleHelper;

public class MoreDashelineIntegration {
    public static Color getColor(int color) {
        return MoreDasheline.MoreDashelineModule.GetDashColor(color, false);
    }
}