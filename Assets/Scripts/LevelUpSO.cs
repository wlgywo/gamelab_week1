using UnityEngine;

public enum SkillType
{
    gravity, power, speed, hp, attackSpeed, critical
    ,fallback
}

[CreateAssetMenu(fileName = "LevelUpSO", menuName = "")]
public class LevelUpSO : ScriptableObject
{
    public SkillType skillType;
    public string desc;
    public int maxlevel;
}
