using UnityEngine;
public enum SpecialType
{
    partner, expTwice, knockBack, blood, quickMode
    ,Test
}

[CreateAssetMenu(fileName = "SpecialSO", menuName = "")]
public class SpecialSO : ScriptableObject
{
    public SpecialType specialType;
    public string desc;
}
