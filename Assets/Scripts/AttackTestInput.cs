using UnityEngine;
using FreedomGrind.Combat;

public class AttackTestInput : MonoBehaviour
{
    [SerializeField] private AttackModule _attack;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            _attack.TryAttack(Vector2.right);
    }
}
