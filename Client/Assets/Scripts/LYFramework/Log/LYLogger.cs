namespace LYFramework.Log
{
    public static class LYLogger
    {
        static ILogHelper m_LogHelper;
        
        public static void SetLogHelper(ILogHelper logHelper)
        {
            m_LogHelper = logHelper;
        }

        public static void Log(string message)
        {
            m_LogHelper?.Log(message);
        }

        public static void Info(string message)
        {
            m_LogHelper?.Info(message);
        }

        public static void Warning(string message)
        {
            m_LogHelper?.Warning(message);
        }

        public static void Error(string message)
        {
            m_LogHelper?.Error(message);
        }
    }
}