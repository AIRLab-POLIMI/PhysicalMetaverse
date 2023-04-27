using UnityEngine;

[CreateAssetMenu(fileName = "Byte SO", menuName = "Scriptable Objects/Variables/Values/Byte")]

public class ByteSO : ValueSO<byte>
{
    [SerializeField] private string Description;

    public string description => Description;
}
