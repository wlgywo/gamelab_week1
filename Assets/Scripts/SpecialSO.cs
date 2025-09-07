using UnityEngine;
public enum SpecialType
{
    partner, expTwice, knockBack, blood, quickMode,
    Marble
    ,Test
}

[CreateAssetMenu(fileName = "SpecialSO", menuName = "")]
public class SpecialSO : ScriptableObject
{
    public SpecialType specialType;
    public string desc;
}
