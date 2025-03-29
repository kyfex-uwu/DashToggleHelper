using Microsoft.Xna.Framework;
using MoreDasheline;

namespace Celeste.Mod.DashToggleHelper;

public class MoreDashelineIntegration {
    public static Color getColor(int color) {
        return MoreDashelineModule.GetDashColor(color, false);
    }
}