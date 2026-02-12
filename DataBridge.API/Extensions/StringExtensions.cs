namespace DataBridge.API.Extensions
{
    public static  class StringExtensions
    {
        public static string AddDbNameToConnection(this string connection, string dbName)
        {
            return string.Format(connection, dbName);
        }
    }
}
