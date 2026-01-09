namespace HizkitzaServer.util;

using System.Windows.Forms;

public sealed class KeyPress
{
    private static bool right, left, up, down, esc, m, z;

    // Key Events //

    public static void KeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Right) right = true;
        if (e.KeyCode == Keys.Left) left = true;
        if (e.KeyCode == Keys.Up) up = true;
        if (e.KeyCode == Keys.Down) down = true;
        if (e.KeyCode == Keys.Escape) esc = true;
        if (e.KeyCode == Keys.M) m = true;
        if (e.KeyCode == Keys.Z) z = true;
    }

    public static void KeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Right) right = false;
        if (e.KeyCode == Keys.Left) left = false;
        if (e.KeyCode == Keys.Up) up = false;
        if (e.KeyCode == Keys.Down) down = false;
        if (e.KeyCode == Keys.Escape) esc = false;
        if (e.KeyCode == Keys.M) m = false;
        if (e.KeyCode == Keys.Z) z = false;
    }

    // Getters //

    public static bool Right()
    {
        return right;
    }

    public static bool Left()
    {
        return left;
    }

    public static bool Up()
    {
        return up;
    }

    public static bool Down()
    {
        return down;
    }

    public static bool Esc()
    {
        return esc;
    }

    public static bool M()
    {
        return m;
    }

    public static bool Z()
    {
        return z;
    }
}
