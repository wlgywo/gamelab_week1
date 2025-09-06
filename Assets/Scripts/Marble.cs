using System.Collections;
using UnityEngine;

public class Marble : MonoBehaviour
{
    private SphereCollider sphereCol;
    [SerializeField] private Renderer render;

    [SerializeField] private int hp = 200;
    [SerializeField] private int maxHp = 200;
    public int index = 0;

    public bool isDamaged = false;
    public float invincibleTimer = 0.1f;

    private void Awake()
    {
        sphereCol = GetComponent<SphereCollider>();
    }

    public void SetIndex(int index)
    {
        this.index = index;

        GetComponent<Spawner>().mapDirect = (MapDirect)index;
    }

    public void Damage(int damage)
    {
        if (isDamaged) return;

        isDamaged = true;
        Debug.Log("수정 다친다..");

        hp -= damage;

        InGameManager.Instance.SetMarbleUIHp((float)hp / maxHp, index);

        if(hp < 0)
        {
            InGameManager.Instance.Special(); // Special 스킬 획득
            SpawnManager.Instance.DestroyMarble(index);

            sphereCol.enabled = false;
            render.enabled = false;
            Debug.Log("수정 파괴");
            Destroy(this);
        }

        StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        yield return new WaitForSeconds(invincibleTimer);
        isDamaged = false;
    }
}
