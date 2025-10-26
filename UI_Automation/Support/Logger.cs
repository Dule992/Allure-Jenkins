using NUnit.Framework;

public class Logger
{
    public static void Log(string message)
    {
        TestContext.WriteLine(message);
    }
}
