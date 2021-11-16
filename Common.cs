public static class Common
{
    public static string AsString(this byte[] bytes)
    {
        return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }
}

