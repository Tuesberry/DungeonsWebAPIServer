﻿namespace TuesberryAPIServer.Services
{
    public class MemoryDbKeyMaker
    {
        const string loginUID = "UID_";
        const string userLockKey = "ULock_";

        public static string MakeUIDKey(string id)
        {
            return loginUID + id;
        }

        public static string MakeUserLockKey(string id) 
        {  
            return userLockKey + id; 
        }
    }
}
