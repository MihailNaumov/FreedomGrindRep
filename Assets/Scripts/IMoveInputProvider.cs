using UnityEngine;

public interface IMoveInputProvider // ��� ��������� ����� ������ ���� �������� ������ (�� ������, ����� ������ � �.�.) ����� ������������ � MoveModule ���������.
{
    Vector2 GetInput();
}