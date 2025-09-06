using UnityEngine;

public enum SkillType
{
    gravity, power,
    fallback
}

[CreateAssetMenu(fileName = "LevelUpSO", menuName = "")]
public class LevelUpSO : ScriptableObject
{
    public SkillType skillType;
    public string desc;
    //public int curlevel = 0;
    public int maxlevel;
}
