﻿namespace RomanticWeb
{
    public interface ICache
    {
        bool Contains(string key);

        T Retrieve<T>(string key);

        void Store(string key,object data);
    }
}