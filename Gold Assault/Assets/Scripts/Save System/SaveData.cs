using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    private static SaveData _current;
    public static SaveData current
    {
        get
        {
            if (_current == null)
            {
                _current = new SaveData();
            }

            return _current;
        }
        set
        {
            _current = value;
        }
    }

    public PlayerProfile profile;

    public int[] inventory = { 0, 0, 0, 0, 0 };

    public float sensitivity = 1f;

    // public List<ToyData> toys;
    // public int toyCars;
    // public int toyDolls;
}
