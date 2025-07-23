using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.items;
    }

    public static string FixJsonArray(string json)
    {
        return "{\"items\":" + json + "}";
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
